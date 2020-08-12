using System;
using System.Collections.Generic;
using System.Linq;
using ThreeRingsSharp.XansData.Extensions;

namespace ThreeRingsSharp.XansData.Structs {

	/// <summary>
	/// Represents a vertex group, which is used for rigged models. All stored vertices simply represent a vertex that has a weight in a given bone group. As such, if a vertex has a weight value less than 1, it will be a duplicate of at least one other vertex in one other vertex group.
	/// </summary>
	public class VertexGroup : ICloneable<VertexGroup> {

		/// <summary>
		/// All of the vertices stored in this VertexGroup, which all correspond to a specific bone. These ARE unique objects (not unique positions in space) since this is a representation of bone data. If you need to access the full geometry (and not every vertex used by a given bone), do it through <see cref="MeshData.Vertices"/>. Do note that Weights are stored within vertices.
		/// </summary>
		[Obsolete("Reference parent MeshData via IndexIndices.")] public List<Vertex> Vertices { get; set; } = new List<Vertex>();

		/// <summary>
		/// The indices in this vertex group. These form triangles in this <see cref="VertexGroup"/>.
		/// </summary>
		[Obsolete("Reference parent MeshData via IndexIndices.")] public List<ushort> Indices { get; set; } = new List<ushort>();

		/// <summary>
		/// The indices of elements in the parent <see cref="MeshData"/>. This is probably confusing to read, so think of it this way:<para/>
		/// I access a value out of this list. The returned value <c>int returnedValue = IndexIndices[x]</c> is used to index the parent's data arrays (Indices, Vertices, Bone stuff): <c>Parent.Indices[returnedValue]</c>. Make more sense?
		/// </summary>
		public List<int> IndexIndices { get; set; } = new List<int>();

		/// <summary>
		/// The MeshData that contains this <see cref="VertexGroup"/>.
		/// </summary>
		public MeshData Parent { get; }

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
				IndexIndices = IndexIndices.ShallowClone().ToList(),
				//Vertices = Vertices.ShallowClone().ToList(),
				//Indices = Indices.ShallowClone().ToList(),
			};
		}

	}
}
