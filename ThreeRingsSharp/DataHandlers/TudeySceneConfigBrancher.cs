using com.threerings.math;
using com.threerings.tudey.data;
using static com.threerings.tudey.data.TudeySceneModel;
using java.util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.Utility.Interface;
using ThreeRingsSharp.XansData;
using com.threerings.config;
using ThreeRingsSharp.DataHandlers.Model;
using ThreeRingsSharp.DataHandlers.Scene;

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
			XanLogger.WriteLine("Iterating through scene entries...", true);

			string implName = (ClassNameStripper.GetWholeClassName(scene.getClass()) ?? scene.getClass().getTypeName()).Replace("$", "::");
			if (currentDataTreeObject != null) {
				if (useImplementation) {
					currentDataTreeObject.Text = implName;
				} else {
					currentDataTreeObject.Text = sourceFile.Name;
				}
			}

			Collection entries = scene.getEntries();
			foreach (object entryObj in entries.toArray()) {
				// Now each entry will be one of three types (at least, in the context that we care about)
				Entry entry = (Entry)entryObj;
				if (entry is TileEntry) {
					TileHandler.Instance.HandleEntry(sourceFile, entry, models, currentDataTreeObject, transform);
				//} else if (entry is PlaceableEntry) {
					//PlaceableHandler.Instance.HandleEntry(sourceFile, entry, models, currentDataTreeObject, transform);
				}
			}

		}

	}
}
