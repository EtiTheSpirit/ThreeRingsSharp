using Newtonsoft.Json;
using System.Collections.Generic;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {
	public class GLTFSkin : GLTFObject {

		/// <summary>
		/// The index of the accessor containing the floating-point 4x4 inverse-bind matrices. The default is that each matrix is a 4x4 identity matrix, which implies that inverse-bind matrices were pre-applied.
		/// </summary>
		[JsonProperty("inverseBindMatrices")] public int InverseBindMatrices = -1;

		/// <summary>
		/// The index of the node used as a skeleton root.	
		/// </summary>
		[JsonProperty("skeleton")] public int Skeleton = -1;

		/// <summary>
		/// Indices of skeleton nodes, used as joints in this skin.	
		/// </summary>
		[JsonProperty("joints")] public List<int> Joints = new List<int>();

		/// <summary>
		/// The user-defined name of this object.	
		/// </summary>
		[JsonProperty("name")] public string Name = "null";

		#region Newtonsoft Field Write Conditions

		public bool ShouldSerializeSkeleton() => Skeleton >= 0;
		public bool ShouldSerializeInverseBindMatrices() => InverseBindMatrices >= 0;

		#endregion
	}
}
