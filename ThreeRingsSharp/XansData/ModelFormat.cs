using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData {

	/// <summary>
	/// Represents a format type for 3D models.
	/// </summary>
	public enum ModelFormat {

		/// <summary>
		/// Format: WaveFront OBJ Format
		/// </summary>
		[Obsolete] OBJ,

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
			[".obj"] = ModelFormat.OBJ,
			[".glb"] = ModelFormat.GLTF,
		};

	}
}
