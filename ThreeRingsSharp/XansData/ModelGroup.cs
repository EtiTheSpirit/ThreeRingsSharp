using System.Collections.Generic;

namespace ThreeRingsSharp.XansData {

	/// <summary>
	/// Represents a group of models. May represent a scene.
	/// </summary>
	public class ModelGroup {

		/// <summary>
		/// The models stored within this group.
		/// </summary>
		public List<Model3D> Models { get; set; } = new List<Model3D>();

		/// <summary>
		/// If <see langword="true"/>, this <see cref="ModelGroup"/> should be handled as a scene. This is mainly used for glTF exporting.
		/// </summary>
		public bool IsScene { get; set; } = false;

	}
}
