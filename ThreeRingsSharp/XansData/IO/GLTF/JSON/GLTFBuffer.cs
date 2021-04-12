using Newtonsoft.Json;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// Represents a binary data buffer.
	/// </summary>
	public class GLTFBuffer : GLTFObject {

		/// <summary>
		/// The amount of bytes in this buffer.
		/// </summary>
		[JsonProperty("byteLength")] public int ByteLength = 0;

	}
}
