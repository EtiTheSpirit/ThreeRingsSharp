using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// Represents an image sampler, which determines how an image is applied as a texture.
	/// </summary>
	public class GLTFSampler {

		[JsonProperty("magFilter")] public int MagFilter = 9729;

		[JsonProperty("minFilter")] public int MinFilter = 9987;

		[JsonProperty("wrapS")] public int WrapS = 10497;

		[JsonProperty("wrapT")] public int WrapT = 10497;

	}
}
