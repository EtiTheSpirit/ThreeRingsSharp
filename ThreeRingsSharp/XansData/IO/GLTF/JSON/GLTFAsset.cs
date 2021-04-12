using Newtonsoft.Json;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// Represents raw asset data for this glTF file, such as the version it's using and the tool that created the file.
	/// </summary>
	public class GLTFAsset : GLTFObject {

		/// <summary>
		/// The version of glTF that this was designed with.
		/// </summary>
		[JsonProperty("version")] public const string VERSION = "2.0";

		/// <summary>
		/// The tool that this glTF file was generated with.
		/// </summary>
		[JsonProperty("generator")] public const string GENERATOR = AbstractModelExporter.TOOL;

	}
}
