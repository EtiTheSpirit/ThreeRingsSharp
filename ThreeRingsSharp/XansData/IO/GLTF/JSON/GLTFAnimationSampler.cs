using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {
	public class GLTFAnimationSampler : GLTFObject {

		/// <summary>
		/// The index of an accessor containing keyframe input values, e.g., time.
		/// </summary>
		[JsonProperty("input")] public int Input;

		/// <summary>
		/// Interpolation algorithm.
		/// </summary>
		[JsonProperty("interpolation")] public string Interpolation = GLTFAnimationInterpolation.LINEAR;

		/// <summary>
		/// The index of an accessor containing keyframe output values.
		/// </summary>
		[JsonProperty("output")] public int Output;

	}
}
