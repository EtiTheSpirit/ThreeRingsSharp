using OOOReader.Reader;
using OOOReader.Utility.Mathematics;
using OOOReader.Utility.ShallowImpl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utilities;
using System.Diagnostics;
using OOOReader.Utility.TudeyScene;

namespace ThreeRingsSharp.ConfigHandlers.TudeyScenes.Entries {

	/// <summary>
	/// The base type for something that's in a TudeySceneModel. This is comparable to TudeySceneModel$Entry.
	/// </summary>
	public class SceneEntry {

		/// <summary>
		/// The <see cref="ConfigReference"/> pointing to the model this entry uses.
		/// </summary>
		public virtual ConfigReference Reference { get; set; }

		/// <summary>
		/// Whether or not this entry is missing necessary data. If this is true, <see cref="Reference"/> is either <see langword="null"/> or empty.
		/// </summary>
		public bool IsEmpty => Reference?.IsEmpty ?? true;

		/// <summary>
		/// The location of this object in 3D space.
		/// </summary>
		public Transform3D Transform { get; set; }

		/// <summary>
		/// Converts a shadowed <c>com.threerings.tudey.data.TudeySceneModel$PlaceableEntry</c> into an instance of this class.
		/// </summary>
		/// <param name="scene"></param>
		/// <param name="shadowEntry"></param>
		public SceneEntry(ShadowClass scene, ShadowClass shadowEntry) {
			shadowEntry.AssertIsInstanceOf("com.threerings.tudey.data.TudeySceneModel$PlaceableEntry");
			Reference = new ConfigReference(shadowEntry["placeable"]);
			Transform = Transform3D.FromShadow(shadowEntry["transform"]);
		}

		/// <summary>
		/// Creates a <see cref="SceneEntry"/> by decoding a <see cref="CoordIntEntry"/>'s data.
		/// </summary>
		/// <param name="scene"></param>
		/// <param name="entry"></param>
		public SceneEntry(ShadowClass scene, Tile entry) {
			object tile = scene["_tileConfigs"]![entry.TileID]["tile"];
			if (tile is ShadowClass sTile) {
				Reference = new ConfigReference(sTile);
				ShadowClass? ptr = Reference.Resolve();
				if (ptr != null && ptr.TryGetField("implementation", out ShadowClass? impl)) {
					bool success = impl!.TryGetField("width", out int width);
					success = impl!.TryGetField("height", out int height) && success;
					if (success) {
						Transform = entry.ComputeTransform(width, height);
						return;
					}
				}
				Transform = entry.ComputeTransform(1, 1);
			} else {
				Debug.WriteLine("Failed to acquire tile (unexpected instance of " + (tile?.GetType().Name ?? "null") + ")");
				Reference = ConfigReference.Empty;
			}
		}

	}
}
