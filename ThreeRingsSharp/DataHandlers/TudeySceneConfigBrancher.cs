﻿using com.threerings.math;
using com.threerings.tudey.data;
using System.Collections.Generic;
using System.IO;
using ThreeRingsSharp.DataHandlers.Scene;
using ThreeRingsSharp.Logging;
using ThreeRingsSharp.Logging.Interface;
using ThreeRingsSharp.XansData;
using static com.threerings.tudey.data.TudeySceneModel;

namespace ThreeRingsSharp.DataHandlers {

	/// <summary>
	/// A class that takes in a <see cref="TudeySceneModel"/> and handles the data appropriately.
	/// </summary>
	public class TudeySceneConfigBrancher {


		public static void SetupCosmeticData(TudeySceneModel scene, DataTreeObject dataTreeParent) {
			if (dataTreeParent == null) return;

			DataTreeObject entryContainer = new DataTreeObject {
				Text = scene.getEntries().size().ToString(),
				ImageKey = SilkImage.Value
			};
			dataTreeParent.AddSimpleProperty("Entries", entryContainer);
		}

		/// <summary>
		/// Sends an arbitrary <see cref="TudeySceneModel"/> into the data brancher and processes it.
		/// </summary>
		/// <param name="sourceFile">The file that the given <see cref="TudeySceneModel"/> came from.</param>
		/// <param name="scene">The <see cref="TudeySceneModel"/> itself.</param>
		/// <param name="models">A list containing every processed model from the entire hierarchy.</param>
		/// <param name="currentDataTreeObject">The current element in the data tree hierarchy to use.</param>
		/// <param name="useImplementation">If <see langword="false"/>, the name of the implementation will be displayed instead of the file name. Additionally, it will not have its implementation property.</param>
		/// <param name="transform">Intended to be used by reference loaders, this specifies an offset for referenced models. All models loaded by this method in the given chain / hierarchy will have this transform applied to them. If the value passed in is <see langword="null"/>, it will be substituted with a new <see cref="Transform3D"/>.</param>
		public static void HandleDataFrom(FileInfo sourceFile, TudeySceneModel scene, List<Model3D> models, DataTreeObject currentDataTreeObject = null, bool useImplementation = false, Transform3D transform = null) {
			SetupCosmeticData(scene, currentDataTreeObject);
			XanLogger.WriteLine("Iterating through scene entries...", XanLogger.DEBUG);

			SKAnimatorToolsProxy.IncrementEnd();
			string implName = (JavaClassNameStripper.GetWholeClassName(scene.getClass()) ?? scene.getClass().getTypeName()).Replace("$", "::");
			if (currentDataTreeObject != null) {
				if (useImplementation) {
					currentDataTreeObject.Text = implName;
				} else {
					currentDataTreeObject.Text = sourceFile.Name;
				}
			}

			object[] entries = scene.getEntries().toArray();
			SKAnimatorToolsProxy.IncrementEnd(entries.Length);
			foreach (object entryObj in entries) {
				// Now each entry will be one of three types (at least, in the context that we care about)
				Entry entry = (Entry)entryObj;
				if (entry is TileEntry) {
					TileHandler.Instance.HandleEntry(sourceFile, entry, models, currentDataTreeObject, transform);
				} else if (entry is PlaceableEntry) {
					PlaceableHandler.Instance.HandleEntry(sourceFile, entry, models, currentDataTreeObject, transform);
				}
				// Other entry types are more for game data and less for the visual scene (e.g. pathfinding nodes, area markers, etc.)
				SKAnimatorToolsProxy.IncrementProgress();
			}
			SKAnimatorToolsProxy.IncrementProgress();
		}
	}
}
