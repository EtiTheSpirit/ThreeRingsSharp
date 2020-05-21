using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// Represents raw mesh data, which stores triangles, normals, uvs, and indices, alongside other mesh-related data.
	/// </summary>
	public class GLTFMesh {

		/// <summary>
		/// The index of this mesh in glTF data.
		/// </summary>
		[JsonIgnore] public int ThisIndex = 0;

		[JsonProperty("primitives")] public List<GLTFPrimitive> Primitives = new List<GLTFPrimitive>(1);

		[JsonProperty("name")] public string Name = "Mesh";

		//public List<float> weights = new List<float>();
	}
}
