using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
