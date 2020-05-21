using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// Represents information about a model, namely, what <see cref="GLTFAccessor"/> ids contain the applicable data.
	/// </summary>
	public class GLTFPrimitiveAttribute {

		/// <summary>
		/// The id of the <see cref="GLTFAccessor"/> containing the vertices for this primitive.
		/// </summary>
		[JsonProperty("POSITION")] public int Position;

		/// <summary>
		/// The id of the <see cref="GLTFAccessor"/> containing the normals for this primitive.
		/// </summary>
		[JsonProperty("NORMAL")] public int Normal;

		//public int TANGENT;

		/// <summary>
		/// The id of the <see cref="GLTFAccessor"/> containing the UVs for this primitive.
		/// </summary>
		[JsonProperty("TEXCOORD_0")] public int TexCoord0;

		[JsonProperty("JOINTS_0")] public int Joints0 = -1;

		[JsonProperty("WEIGHTS_0")] public int Weights0 = -1;

		// This is referenced by newtonsoft during runtime.
		public bool ShouldSerializeJoints0() => Joints0 >= 0;

		// This is referenced by newtonsoft during runtime.
		public bool ShouldSerializeWeights0() => Weights0 >= 0;

	}
}
