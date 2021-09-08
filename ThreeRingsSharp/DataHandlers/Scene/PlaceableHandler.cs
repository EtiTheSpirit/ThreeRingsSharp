using com.threerings.math;
using com.threerings.tudey.config;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using ThreeRingsSharp.DataHandlers.Model;
using ThreeRingsSharp.Logging;
using ThreeRingsSharp.Logging.Interface;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.XansData.Extensions;
using ThreeRingsSharp.XansData.XML.ConfigReferences;
using static com.threerings.tudey.data.TudeySceneModel;

namespace ThreeRingsSharp.DataHandlers.Scene {

	/// <summary>
	/// Handles instances of <see cref="PlaceableEntry"/>.
	/// </summary>
	public class PlaceableHandler : Singleton<PlaceableHandler>, IEntryHandler, IDataTreeInterface<PlaceableEntry> {

		public void SetupCosmeticInformation(PlaceableEntry data, DataTreeObject dataTreeParent) {
			if (dataTreeParent == null) return;
			Transform3D transform = GetTransformField(data);

			// This method returns a property, but this is just a stock object (it was created this way in TudeySceneConfigBrancher)
			List<DataTreeObject> values = dataTreeParent.Properties[dataTreeParent.FindSimpleProperty("Entries")];
			DataTreeObject prop = values.First();
			DataTreeObject existingPlaceableCtr = prop.FindSimpleProperty("Placeable Objects");
			if (existingPlaceableCtr != null) {
				// Yes, there is a reason you call find twice. The implicit cast from property to object on existingTileCtr creates a new object
				// as such, casting it back for use in this lookup is a different key.
				existingPlaceableCtr = prop.Properties[prop.FindSimpleProperty("Placeable Objects")].FirstOrDefault();
			}
			DataTreeObject placeableContainer = existingPlaceableCtr ?? new DataTreeObject() {
				ImageKey = SilkImage.Variant
			};

			if (existingPlaceableCtr == null) {
				// We made a new one. Add it.
				prop.AddSimpleProperty("Placeable Objects", placeableContainer);
			}

			DataTreeObject individualPlacementCtr = new DataTreeObject() {
				Text = data.placeable.getName()
			};

			individualPlacementCtr.AddSimpleProperty("Transform", transform.toString(), SilkImage.Matrix);
			individualPlacementCtr.AddSimpleProperty("Reference", data.getReference()?.getName() ?? "null", SilkImage.Reference);
			placeableContainer.AddSimpleProperty("Entry", individualPlacementCtr);
		}

		public void HandleEntry(FileInfo sourceFile, Entry entry, List<Model3D> modelCollection, DataTreeObject dataTreeParent = null, Transform3D globalTransform = null) {
			PlaceableEntry placeable = (PlaceableEntry)entry;
			SetupCosmeticInformation(placeable, dataTreeParent);

			// TODO: Why the hell does referencing the transform field treat it like a method?
			// Is this some really stupid inheritence problem? I didn't even know this was possible.
			// EDIT: Yeah it is. Entry has a method called "transform", but placeable has a field called "transform". Great.
			// This may have been caused by the transpiler, which is to be expected. I'm actually quite suprised that I've not run into any errors until now.

			SKAnimatorToolsProxy.IncrementEnd();
			// TEST: Is this, by some slim chance, a file ref? (This can happen!)
			if (placeable.placeable.IsRealReference()) {
				PlaceableConfig[] placeableCfgs = ConfigReferenceBootstrapper.ConfigReferences["placeable"].OfType<PlaceableConfig>().ToArray();
				PlaceableConfig placeableCfg = placeableCfgs.GetEntryByName(placeable.placeable.getName());
				if (placeableCfg == null) {
					XanLogger.WriteLine($"Unable to find data for placeable [{placeable.placeable.getName()}]!");
					return;
				}

				PlaceableConfig.Original originalImpl;
				do {
					if (placeableCfg == null) {
						XanLogger.WriteLine("ALERT: A placeable was null!", color: System.Drawing.Color.Red);
						return;
					}
					if (placeableCfg.getConfigManager() != null) {
						originalImpl = placeableCfg.getOriginal(placeableCfg.getConfigManager());
						break;
					} else {
						if (placeableCfg.implementation is PlaceableConfig.Original org) {
							originalImpl = org;
							break;
						} else if (placeableCfg.implementation is PlaceableConfig.Derived der) {
							placeableCfg = placeableCfgs.GetEntryByName(der.placeable.getName());
						} else {
							originalImpl = null;
							break;
						}
					}
				} while (true);
				if (originalImpl != null) {
					Transform3D transform = GetTransformField(placeable);
					string relativeModelPath = originalImpl.model.getName();
					XanLogger.WriteLine("Grabbing placeable [" + placeable.placeable.getName() + "] at " + relativeModelPath, XanLogger.DEBUG);
					ConfigReferenceUtil.HandleConfigReference(sourceFile, originalImpl.model, modelCollection, dataTreeParent, globalTransform.compose(transform));
				} else {
					XanLogger.WriteLine($"Implementation for placeable [{placeable.placeable.getName()}] does not exist!");
				}
			} else {
				Transform3D trs = GetTransformField(placeable);
				XanLogger.WriteLine("Grabbing placeable at " + placeable.placeable.getName(), XanLogger.DEBUG);
				ConfigReferenceUtil.HandleConfigReference(sourceFile, placeable.placeable, modelCollection, dataTreeParent, globalTransform.compose(trs));
			}
			SKAnimatorToolsProxy.IncrementProgress();
		}

		/// <summary>
		/// Since Java allows for something that C# doesn't (a subclass having a field with the same name as a method in the parent class), this uses reflection to get the <c>transform</c> *field* of a <see cref="PlaceableEntry"/>.<para/>
		/// Thanks, Java.
		/// </summary>
		/// <param name="entry">The <see cref="PlaceableEntry"/> to get the transform of.</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Transform3D GetTransformField(PlaceableEntry entry) {
			return typeof(PlaceableEntry).GetField("transform").GetValue(entry) as Transform3D;
		}

	}
}
