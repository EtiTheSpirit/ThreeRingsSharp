using Newtonsoft.Json;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// Represents an image, usually for a texture.
	/// </summary>
	public class GLTFImage : GLTFObject {

		/// <summary>
		/// The path to the image. If this is set (not <see langword="null"/>), <see cref="BufferView"/> and <see cref="MimeType"/> will be ignored and this will be used instead.
		/// </summary>
		[JsonProperty("uri")] public string? URI = null;

		/// <summary>
		/// The location of this image in the buffer.
		/// </summary>
		[JsonProperty("bufferView")] public int BufferView;

		/// <summary>
		/// The type of the embedded data.
		/// </summary>
		[JsonProperty("mimeType")] public string MimeType = "image/png";

		#region Newtonsoft Field Write Conditions
		// These are referenced by newtonsoft during runtime.
		// Format: ShouldSerialize...
		// Replace ... with the name of the field.

		public bool ShouldSerializeBufferView() => URI == null;

		public bool ShouldSerializeMimeType() => ShouldSerializeBufferView();

		public bool ShouldSerializeURI() => !ShouldSerializeBufferView();
		#endregion

	}
}
