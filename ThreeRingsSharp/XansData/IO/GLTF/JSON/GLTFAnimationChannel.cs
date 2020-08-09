using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// Represents an animation sampler.
	/// </summary>
	public class GLTFAnimationChannel : GLTFObject {

		[JsonProperty("sampler")] public int Sampler;

		[JsonProperty("target")] public GLTFAnimationChannelTarget Target;

	}
}
