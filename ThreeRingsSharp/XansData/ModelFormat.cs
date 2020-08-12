using System.Collections.Generic;

namespace ThreeRingsSharp.XansData {

	/// <summary>
	/// Represents a format type for 3D models.
	/// </summary>
	public enum ModelFormat {

		/// <summary>
		/// Format: Graphics Library Transmission Format
		/// </summary>
		GLTF,
	}

	public class ModelFormatUtil {
		/// <summary>
		/// A binding from <see cref="string"/> file extensions to <see cref="ModelFormat"/>s, e.g. the string <c>".glb"</c> corresponds to <see cref="ModelFormat.GLTF"/>.
		/// </summary>
		public static readonly IReadOnlyDictionary<string, ModelFormat> ExtensionToFormatBindings = new Dictionary<string, ModelFormat>() {
			[".glb"] = ModelFormat.GLTF,
		};

	}
}
