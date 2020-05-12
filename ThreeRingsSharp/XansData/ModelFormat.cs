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
		/// The Autodesk FBX Format
		/// </summary>
		FBX,

		/// <summary>
		/// WaveFront OBJ Format
		/// </summary>
		OBJ,

		/// <summary>
		/// Graphics Library Transmission Format
		/// </summary>
		GLTF,
	}
	
	public class ModelFormatUtil {
	#pragma warning disable CS0612 // Type or member is obsolete
		public static readonly IReadOnlyDictionary<string, ModelFormat> ExtensionToFormatBindings = new Dictionary<string, ModelFormat>() {
			[".fbx"] = ModelFormat.FBX,
			[".obj"] = ModelFormat.OBJ,
			[".gltf"] = ModelFormat.GLTF,
		};

	}
}
