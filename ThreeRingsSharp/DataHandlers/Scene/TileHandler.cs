using static com.threerings.tudey.data.TudeySceneModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.Utility.Interface;
using System.IO;
using com.threerings.math;
using ThreeRingsSharp.DataHandlers.Model;
using com.threerings.tudey.data;
using ThreeRingsSharp.DataHandlers.Scene.Data;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.XansData.XML.ConfigReferences;
using com.threerings.tudey.util;
using ThreeRingsSharp.XansData.Extensions;
using System.Diagnostics;
using com.threerings.tudey.config;

namespace ThreeRingsSharp.DataHandlers.Scene {

	/// <summary>
	/// Handles instances of <see cref="TileEntry"/>.
	/// </summary>
	public class TileHandler : Singleton<TileHandler>, IEntryHandler, IDataTreeInterface<TileEntry> {

		public void SetupCosmeticInformation(TileEntry data, DataTreeObject dataTreeParent) {
			if (dataTreeParent == null) return;

			// This method returns a property, but this is just a stock object (it was created this way in TudeySceneConfigBrancher)
			List<DataTreeObject> values = dataTreeParent.Properties[dataTreeParent.FindSimpleProperty("Entries")];
			DataTreeObject prop = values.First();
			DataTreeObject existingTileCtr = prop.FindSimpleProperty("Tiles");
			if (existingTileCtr != null) {
				// Yes, there is a reason you call find twice. The implicit cast from property to object on existingTileCtr creates a new object
				// as such, casting it back for use in this lookup is a different key.
				existingTileCtr = prop.Properties[prop.FindSimpleProperty("Tiles")].FirstOrDefault();
			}
			DataTreeObject tilesContainer = existingTileCtr ?? new DataTreeObject() {
				ImageKey = SilkImage.Tile
			};

			if (existingTileCtr == null) {
				// We made a new one. Add it.
				prop.AddSimpleProperty("Tiles", tilesContainer);
			}

			DataTreeObject individualTileDataContainer = new DataTreeObject() {
				Text = data.tile.getName(),
				// ImageKey = SilkImage.Tile
			};


			Coord location = data.getLocation();
			//Transform3D trs = new Transform3D(new Vector3f(location.x, data.elevation, location.y), new Quaternion().fromAngleAxis((float)(data.rotation * Math.PI / 2), Vector3f.UNIT_Y), 1f);
			individualTileDataContainer.AddSimpleProperty("Elevation", data.elevation);
			individualTileDataContainer.AddSimpleProperty("Rotation (Deg)", data.rotation * 90);
			individualTileDataContainer.AddSimpleProperty("Coordinate", $"[{location.x}, {location.y}]", SilkImage.Matrix);
			individualTileDataContainer.AddSimpleProperty("Tile Reference", data.tile?.getName(), SilkImage.Reference);
			tilesContainer.AddSimpleProperty("Entry", individualTileDataContainer, SilkImage.Tile);
		}

		public void HandleEntry(FileInfo sourceFile, Entry entry, List<Model3D> modelCollection, DataTreeObject dataTreeParent = null, Transform3D globalTransform = null) {
			TileEntry tile = (TileEntry)entry;
			//Transform3D trs = new Transform3D();
			//tile.getTransform(tile.getConfig(scene.getConfigManager()), trs);

			SetupCosmeticInformation(tile, dataTreeParent);
			// TODO: Some config references have SK data that I need. I need to do a hybrid of my shallow implementation on
			// top of the bootstrapper that loads the actual filtered game data so that I can grab everything.
			TileConfig[] tileCfgs = ConfigReferenceBootstrapper.ConfigReferences["tile"].OfType<TileConfig>().ToArray();
			TileConfig tileCfg = tileCfgs.GetEntryByName(tile.tile.getName());
			if (tileCfg == null) {
				XanLogger.WriteLine($"Unable to find data for tile [{tile.tile.getName()}]!");
				return;
			}

			
			// First things first: Tiles are offset and in the wrong position. Fix it.

			TileConfig.Original originalImpl;
			do {
				if (tileCfg == null) {
					XanLogger.WriteLine("ALERT: A tile was null!", false, System.Drawing.Color.Red);
					return;
				}
				if (tileCfg.getConfigManager() != null) {
					originalImpl = tileCfg.getOriginal(tileCfg.getConfigManager());
					break;
				} else {
					if (tileCfg.implementation is TileConfig.Original original) {
						originalImpl = original;
						break;
					} else if (tileCfg.implementation is TileConfig.Derived derived) {
						tileCfg = tileCfgs.GetEntryByName(derived.tile.getName());
					} else {
						originalImpl = null;
						break;
					}
				}
			} while (true);

			//tile.GetExportTransform(originalImpl, out Transform3D transform);
			// All transforms are relative to the center of the object. the origin of the actual models is in their lower-back-left bounds.
			// I need to add a flag to tell the exporter to move the geometry based on bounds center.
			Transform3D transform = new Transform3D();
			tile.getTransform(originalImpl, transform);
			string relativeModelPath = originalImpl.model.getName();
			XanLogger.WriteLine("Grabbing tile [" + tile.tile.getName() + "] at " + relativeModelPath, true);
			// , true, new Dictionary<string, dynamic>() { ["SceneObjectFlag"] = true, ["SceneObjectTransform"] = transform }
			List<Model3D> acquiredModels = ConfigReferenceUtil.HandleConfigReference(sourceFile, originalImpl.model, modelCollection, dataTreeParent, globalTransform.compose(transform));
			foreach (Model3D model in acquiredModels) {
				model.Mesh.SetOffsetToAABBCenter();
			}
		}
		
	}
}
