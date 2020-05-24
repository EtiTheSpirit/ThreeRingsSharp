using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.XansData.Extensions;

namespace ThreeRingsSharp.XansData.Structs {

	/// <summary>
	/// Represents a vertex group, which is used for rigged models. All stored vertices simply represent a vertex that has a weight in a given bone group. As such, if a vertex has a weight value less than 1, it will be a duplicate of at least one other vertex in one other vertex group.
	/// </summary>
	public class VertexGroup : ICloneable<VertexGroup> {

		/// <summary>
		/// All of the vertices stored in this VertexGroup. These ARE unique objects (not unique positions in space) since this is a representation of bone data. If you need to access the full geometry (and not every vertex used by a given bone), do it through <see cref="MeshData.Vertices"/>, since these vertices are not affected by transforms which may drastically malform the output.
		/// </summary>
		public List<Vertex> Vertices { get; set; } = new List<Vertex>();

		/// <summary>
		/// The indices in this vertex group. These refer to the triangles in this <see cref="VertexGroup"/>, but are the indices used by the whole model otherwise.<para/>
		/// Please note that certain indices in this array may be duplicated (or, the same exact index will appear up to 4 times in a row). If this is the case, ignore subsequent instances of the same number. For instance, this array may contain <c>1 2 2 2 3 3 4</c>, and should turn into <c>1 2 3 4</c> after you trim it yourself.
		/// </summary>
		public List<short> Indices { get; set; } = new List<short>();

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

		/// <summary>
		/// Clones this <see cref="VertexGroup"/> into a new instance. All data in the new instance is separate from the old instance.
		/// </summary>
		/// <returns></returns>
		public VertexGroup Clone() {
			return new VertexGroup {
				Name = Name,
				Vertices = Vertices.ShallowClone().ToList(),
				Indices = Indices.ShallowClone().ToList()
			};
		}

	}
}
