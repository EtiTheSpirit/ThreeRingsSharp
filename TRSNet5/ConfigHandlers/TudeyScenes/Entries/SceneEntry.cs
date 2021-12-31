using OOOReader.Reader;
using OOOReader.Utility.Mathematics;
using OOOReader.Utility.ShallowImpl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utilities;
using Transform3DImpl = OOOReader.Utility.Mathematics.Transform3D;
using QuaternionImpl = OOOReader.Utility.Mathematics.Quaternion;
using System.Diagnostics;

namespace ThreeRingsSharp.ConfigHandlers.TudeyScenes.Entries {

	/// <summary>
	/// The base type for something that's in a TudeySceneModel. This is identical to TudeySceneModel$Entry, minus the fact that it represents all types at once.
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

		/*
		/// <summary>
		/// The elevation of this tile, or <see cref="int.MinValue"/> for none.
		/// </summary>
		public int Elevation { get; set; }

		/// <summary>
		/// The X component of this entry's location in cartesian space (where Z is up/down).
		/// </summary>
		public float X { get; set; }

		/// <summary>
		/// The Y component of this entry's location in cartesian space (where Z is up/down).
		/// </summary>
		public float Y { get; set; }

		/// <summary>
		/// The rotation of this entry in radians.
		/// </summary>
		public float Rotation { get; set; }
		*/

		/// <summary>
		/// The location of this object in 3D space.
		/// </summary>
		public Transform3DImpl Transform { get; set; }

		/// <summary>
		/// Converts a shadowed <c>com.threerings.tudey.data.TudeySceneModel$Entry</c> into an instance of this class.
		/// </summary>
		/// <param name="scene"></param>
		/// <param name="shadowEntry"></param>
		public SceneEntry(ShadowClass scene, ShadowClass shadowEntry) {
			shadowEntry.AssertIsInstanceOf("com.threerings.tudey.data.TudeySceneModel$PlaceableEntry");
			Reference = new ConfigReference(shadowEntry["placeable"]);
			Transform = new Transform3DImpl(shadowEntry["transform"]);
		}

		/// <summary>
		/// Creates a <see cref="SceneEntry"/> by decoding a <see cref="CoordIntEntry"/>'s data.
		/// </summary>
		/// <param name="scene"></param>
		/// <param name="entry"></param>
		public SceneEntry(ShadowClass scene, CoordIntEntry entry) {
			float x = (float)entry.Key.X;
			float y = (float)entry.Key.Y;
			int value = entry.Value;
			float rotation = (float)(value & 0b0011) * MathF.PI * 1.5f;
			Transform = new Transform3DImpl(
				new Vector3f(
					(float)x + 0.5f,
					(float)y + 0.5f,
					(float)((value << 16) >> 18)
				),
				new QuaternionImpl(
					0f,
					MathF.Sin(rotation),
					0f,
					MathF.Cos(rotation)
				)
			);
			object tile = scene["_tileConfigs"]![value >> 16]["tile"];
			if (tile is ShadowClass sTile) {
				Reference = new ConfigReference(sTile);
			} else {
				Debug.WriteLine("Failed to acquire tile (unexpected instance of " + (tile?.GetType().Name ?? "null") + ")");
				Reference = ConfigReference.Empty;
			}
		}

	}
}
