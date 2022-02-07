using OOOReader.Reader;
using OOOReader.Utility.Mathematics;
using OOOReader.Utility.ShallowImpl;
using OOOReader.Utility.TudeyScene;
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
			SceneTileContainer sceneTiles = new SceneTileContainer(coordIntMapTiles);
			List<Tile> tiles = sceneTiles.GetAllTiles();

			#region Data Tree
			MasterDataExtractor.SetupBaseInformation(tudeySceneModel, ctx.Push(ctx.File.Name, SilkImage.Scene));
			ctx.Push("Scene Objects", SilkImage.Tile);
			#endregion

			List<SceneEntry> objects = new List<SceneEntry>();
			foreach (Tile tile in tiles) {
				objects.Add(new SceneEntry(tudeySceneModel, tile));
			}

			IEnumerable<ShadowClass?> otherEntries;
			object? otherEntriesObj = tudeySceneModel["_entries"];
			if (otherEntriesObj is ShadowClass[] scArray) {
				otherEntries = scArray;
			} else if (otherEntriesObj is Dictionary<object, object> objDict) {
				otherEntries = objDict.Values.Select(obj => obj as ShadowClass);
			} else {
				otherEntries = new List<ShadowClass?>();
				Debug.WriteLine("Unknown type for otherEntries: " + otherEntriesObj?.GetType()?.FullName ?? "null");
			}
			foreach (ShadowClass? placeableOrOther in otherEntries) {
				if (placeableOrOther != null) {
					if (placeableOrOther.IsA("com.threerings.tudey.data.TudeySceneModel$PlaceableEntry")) {
						objects.Add(new SceneEntry(tudeySceneModel, placeableOrOther));
					} else {
						Debug.WriteLine("Unable to handle instance of " + placeableOrOther.Signature + " at this time.");
					}
				}
			}

			int index = 0;
			foreach (SceneEntry entry in objects) {
				if (entry.IsEmpty) {
					Debug.WriteLine($"Entry #{index} was empty.");
					index++;
					continue;
				}
				ShadowClass? ptr = entry.Reference!.Resolve();
				if (ptr != null && ptr.TryGetField("implementation", out ShadowClass? impl)) {
					if (impl!.TryGetField("model", out ShadowClass? cfgRefTile)) {
						ConfigReference reference = new ConfigReference(cfgRefTile!);
						ctx.CurrentSceneTransform *= entry.Transform;
						MasterDataExtractor.ExtractFrom(ctx, reference);
						ctx.CurrentSceneTransform /= entry.Transform;
					}
				} else {
					Debug.WriteLine("Failed to resolve ConfigReference: " + entry.Reference.Name);
				}
				index++;
			}

			ctx.Pop();
			ctx.Pop();
		}

	}
}
