using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// Represents a single piece of geometry.
	/// </summary>
	public class GLTFPrimitive : GLTFObject {

		/// <summary>
		/// Information about this geometry.
		/// </summary>
		[JsonProperty("attributes")] public GLTFPrimitiveAttribute Attributes = new GLTFPrimitiveAttribute();

		/// <summary>
		/// The indices used to determine triangles in this geometry.
		/// </summary>
		[JsonProperty("indices")] public int Indices;
		
		/// <summary>
		/// The material index of this primitive.
		/// </summary>
		[JsonProperty("material")] public int Material = -1;

		// This is referenced by newtonsoft during runtime.
		public bool ShouldSerializeMaterial() => Material >= 0;
		
	}
}
