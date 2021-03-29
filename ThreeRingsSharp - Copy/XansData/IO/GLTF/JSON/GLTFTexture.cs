using Newtonsoft.Json;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// Represents a texture.
	/// </summary>
	public class GLTFTexture : GLTFObject {

		/// <summary>
		/// The index of the sampler that determines how to apply this texture.
		/// </summary>
		[JsonProperty("sampler")] public int Sampler;

		/// <summary>
		/// The index of the image that contains the actual picture to apply.
		/// </summary>
		[JsonProperty("source")] public int Source;

	}
}
