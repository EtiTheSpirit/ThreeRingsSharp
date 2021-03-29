using Newtonsoft.Json;
using System.Collections.Generic;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// Represents raw mesh data, which stores triangles, normals, uvs, and indices, alongside other mesh-related data.
	/// </summary>
	public class GLTFMesh : GLTFObject {

		[JsonProperty("primitives")] public List<GLTFPrimitive> Primitives = new List<GLTFPrimitive>(1);

		[JsonProperty("name")] public string Name = "Mesh";

		//public List<float> weights = new List<float>();
	}
}
