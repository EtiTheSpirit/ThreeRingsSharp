using Newtonsoft.Json;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// Represents an animation sampler.
	/// </summary>
	public class GLTFAnimationChannel : GLTFObject {

		[JsonProperty("sampler")] public int Sampler;

		[JsonProperty("target")] public GLTFAnimationChannelTarget Target;

	}
}
