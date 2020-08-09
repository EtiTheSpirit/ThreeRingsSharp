using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// Represents an entire secene.
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
