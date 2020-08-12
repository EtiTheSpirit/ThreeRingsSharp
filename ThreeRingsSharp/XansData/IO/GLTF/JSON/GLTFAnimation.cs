using Newtonsoft.Json;
using System.Collections.Generic;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// Represents an animation object in a GLTF file.
	/// </summary>
	public class GLTFAnimation : GLTFObject {

		[JsonProperty("name")] public string Name = "NULL";

		[JsonProperty("channels")] public List<GLTFAnimationChannel> Channels = new List<GLTFAnimationChannel>();

		[JsonProperty("samplers")] public List<GLTFAnimationSampler> Samplers = new List<GLTFAnimationSampler>();

	}
}
