using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.Structs {

	/// <summary>
	/// Represents a vertex group, which is used for rigged models.
	/// </summary>
	public class VertexGroup {

		/// <summary>
		/// All of the vertices stored in this VertexGroup. These ARE unique since this is a representation of bone data. If you need to access the full geometry (and not every vertex used by a given bone), do it through <see cref="Model3D.Vertices"/>, since these vertices are not affected by transforms which may drastically malform the output.
		/// </summary>
		public List<Vertex> Vertices { get; set; } = new List<Vertex>();

		/// <summary>
		/// The indices in this vertex group. An index refers to a vertex in an associated <see cref="Model3D"/>.
		/// </summary>
		[Obsolete("Use Vertices instead.")] public List<short> Indices { get; set; } = new List<short>();

		/// <summary>
		/// An array of all of the weights that are in this vertex group. These values have a 1:1 correspondence with <see cref="Indices"/>.
		/// </summary>
		[Obsolete("Use Vertices instead.")] public List<float> Weights { get; set; } = new List<float>();

		/// <summary>
		/// The name of this vertex group, which should be identical to the node it corresponds to.
		/// </summary>
		public string Name { get; set; } = "err_no_name";

		/// <summary>
		/// Construct a new <see cref="VertexGroup"/> with default properties.
		/// </summary>
		public VertexGroup() { }

		/// <summary>
		/// Construct a new <see cref="VertexGroup"/> with the given name.
		/// </summary>
		/// <param name="name"></param>
		public VertexGroup(string name) {
			Name = name;
		}

	}
}
