using OOOReader.Reader;
using OOOReader.Utility.ShallowImpl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.ConfigHandlers.TudeyScenes.Entries;
using ThreeRingsSharp.Utilities;
using XDataTree.Data;
using XDataTree.TreeElements;

namespace ThreeRingsSharp.ConfigHandlers.TudeyScenes {
	public static class TudeySceneModelReader {

		// we getting into big boi territory
		// L A R G E   B O I   T E R R I T O R Y
		// H  O  L  Y     S  H  I  T



		public static void ReadData(ReadFileContext ctx, ShadowClass tudeySceneModel) {
			tudeySceneModel.AssertIsInstanceOf("com.threerings.tudey.data.TudeySceneModel");
			ShadowClass coordIntMapTiles = tudeySceneModel["_tiles"]!;
			CoordIntMap tiles = new CoordIntMap(coordIntMapTiles);
			List<CoordIntEntry> entries = tiles.GetCoordEntries();
			ShadowClass[] otherEntries = tudeySceneModel["_entries"]!;

			#region Data Tree
			MasterDataExtractor.SetupBaseInformation(tudeySceneModel, ctx.Push(ctx.File.Name, SilkImage.Scene));
			ctx.Push("Scene Objects", SilkImage.Tile);
			#endregion

			List<SceneEntry> objects = new List<SceneEntry>();
			foreach (CoordIntEntry entry in entries) {
				objects.Add(new SceneEntry(tudeySceneModel, entry));
			}

			foreach (ShadowClass placeableOrOther in otherEntries) {
				if (placeableOrOther.IsA("com.threerings.tudey.data.TudeySceneModel$PlaceableEntry")) {
					objects.Add(new SceneEntry(tudeySceneModel, placeableOrOther));
				} else {
					Debug.WriteLine("Unable to handle instance of " + placeableOrOther.Signature + " at this time.");
				}
			}

			foreach (SceneEntry entry in objects) {
				if (entry.IsEmpty) continue;
				ShadowClass ptr = entry.Reference!.Resolve()!;
				if (ptr.TryGetField("implementation", out ShadowClass? impl)) {
					if (impl!.TryGetField("model", out ShadowClass? cfgRefTile)) {
						ConfigReference reference = new ConfigReference(cfgRefTile!);
						ctx.CurrentSceneTransform.ComposeSelf(entry.Transform);
						MasterDataExtractor.ExtractFrom(ctx, reference);
						ctx.CurrentSceneTransform.ComposeSelf(entry.Transform.Invert());
					}
				}
			}

			ctx.Pop();
			ctx.Pop();
		}

	}
}
