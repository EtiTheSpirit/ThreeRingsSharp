using Newtonsoft.Json;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// Represents a subsection of a complete data buffer.
	/// </summary>
	public class GLTFBufferView : GLTFObject {

		/// <summary>
		/// The ID of the buffer to browse.
		/// </summary>
		[JsonProperty("buffer")] public int Buffer = 0;

		/// <summary>
		/// The length of the view in bytes (how many bytes this view "contains")
		/// </summary>
		[JsonProperty("byteLength")] public int ByteLength = 0;

		/// <summary>
		/// The offset of the first byte in this view
		/// </summary>
		[JsonProperty("byteOffset")] public int ByteOffset = 0;

		// <summary>
		// The spacing between the bytes in this view. For types larger than one byte, this represents the spacing between the first bytes (e.g. if I have a buffer of int32s, the stride here would be 4, NOT 1)
		// </summary>
		// public int? byteStride = null;

	}
}
