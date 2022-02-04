using Newtonsoft.Json;
using System.Collections.Generic;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// Represents an entire scene.
	/// </summary>
	public class GLTFScene : GLTFObject {

		/// <summary>
		/// The name of this <see cref="GLTFScene"/>.
		/// </summary>
		[JsonProperty("name")] public string Name = "scene";

		/// <summary>
		/// The IDs of the nodes included in this <see cref="GLTFScene"/>.
		/// </summary>
		[JsonProperty("nodes")] public List<int> Nodes = new List<int>();

	}
}
