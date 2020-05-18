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
using System.Runtime.CompilerServices;
using ThreeRingsSharp.DataHandlers.Model;

namespace ThreeRingsSharp.DataHandlers.Scene {

	/// <summary>
	/// Handles instances of <see cref="PlaceableEntry"/>.
	/// </summary>
	public class PlaceableHandler : Singleton<PlaceableHandler>, IEntryHandler, IDataTreeInterface<PlaceableEntry> {

		public void SetupCosmeticInformation(PlaceableEntry data, DataTreeObject dataTreeParent) {
			if (dataTreeParent == null) return;
			Transform3D transform = GetTransform(data);

			dataTreeParent.AddSimpleProperty("Transform", transform.toString(), SilkImage.Matrix);
			dataTreeParent.AddSimpleProperty("Object", data.placeable.getName(), SilkImage.Reference);
		}

		public void HandleEntry(FileInfo sourceFile, Entry entry, List<Model3D> modelCollection, DataTreeObject dataTreeParent = null, Transform3D globalTransform = null) {
			PlaceableEntry placeable = (PlaceableEntry)entry;

			// TODO: Why the hell does referencing the transform field treat it like a method?
			// Is this some really stupid inheritence problem? I didn't even know this was possible.
			// EDIT: Yeah it is. Entry has a method called "transform", but placeable has a field called "transform". Great.
			// This may have been caused by the transpiler, which is to be expected. I'm actually quite suprised that I've not run into any errors until now.
			ConfigReferenceUtil.HandleConfigReference(sourceFile, placeable.placeable, modelCollection, dataTreeParent, globalTransform.compose(GetTransform(placeable)));
		}

		/// <summary>
		/// Since Java allows for something that C# doesn't (a subclass having a field with the same name as a method in the parent class), this uses reflection to get the <c>transform</c> *field* of a <see cref="PlaceableEntry"/>.<para/>
		/// Thanks, Java.
		/// </summary>
		/// <param name="entry">The <see cref="PlaceableEntry"/> to get the transform of.</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Transform3D GetTransform(PlaceableEntry entry) {
			return typeof(PlaceableEntry).GetField("transform").GetValue(entry) as Transform3D;
		}

	}
}
