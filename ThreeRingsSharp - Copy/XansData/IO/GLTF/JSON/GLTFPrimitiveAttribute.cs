using Newtonsoft.Json;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// Represents information about a model, namely, what <see cref="GLTFAccessor"/> ids contain the applicable data.
	/// </summary>
	public class GLTFPrimitiveAttribute : GLTFObject {

		/// <summary>
		/// The id of the <see cref="GLTFAccessor"/> containing the vertices for this primitive.
		/// </summary>
		[JsonProperty("POSITION")] public int Position;

		/// <summary>
		/// The id of the <see cref="GLTFAccessor"/> containing the normals for this primitive.
		/// </summary>
		[JsonProperty("NORMAL")] public int Normal = -1;

		/// <summary>
		/// XYZW vertex tangents where the w component is a sign value (-1 or +1) indicating handedness of the tangent basis.
		/// </summary>
		[JsonProperty("TANGENT")] public int Tangent = -1;

		/// <summary>
		/// The id of the <see cref="GLTFAccessor"/> containing the UVs for this primitive.
		/// </summary>
		[JsonProperty("TEXCOORD_0")] public int TexCoord0 = -1;

		/// <summary>
		/// The id of the <see cref="GLTFAccessor"/> containing the alternate UVs for this primitive. It will likely go unused.
		/// </summary>
		[JsonProperty("TEXCOORD_1")] public int TexCoord1 = -1;

		/// <summary>
		/// The id of the <see cref="GLTFAccessor"/> containing the vertex colors for this primitive, either RGB (VEC3) or RGBA (VEC4)
		/// </summary>
		[JsonProperty("COLOR_0")] public int Color = -1;

		/// <summary>
		/// The ID of the <see cref="GLTFAccessor"/> containing the joints for this skinned primitive.
		/// </summary>
		[JsonProperty("JOINTS_0")] public int Joints = -1;

		/// <summary>
		/// The ID of the <see cref="GLTFAccessor"/> containing the weights for this skinned primitive.
		/// </summary>
		[JsonProperty("WEIGHTS_0")] public int Weights = -1;

		#region Newtonsoft Field Write Conditions
		// These are referenced by newtonsoft during runtime.
		// Format: ShouldSerialize...
		// Replace ... with the name of the field.

		public bool ShouldSerializeNormal() => Normal >= 0;

		public bool ShouldSerializeTangent() => Tangent >= 0;

		public bool ShouldSerializeTexCoord0() => TexCoord0 >= 0;

		public bool ShouldSerializeTexCoord1() => TexCoord1 >= 0;

		public bool ShouldSerializeColor() => Color >= 0;

		public bool ShouldSerializeJoints() => Joints >= 0;

		public bool ShouldSerializeWeights() => Weights >= 0;

		#endregion

	}
}
