using com.threerings.math;
using com.threerings.opengl.model.config;
using System.Collections.Generic;
using static com.threerings.opengl.model.config.ArticulatedConfig;

namespace ThreeRingsSharp.XansData {

	/// <summary>
	/// Intended to represent a bone in a mesh. It is incredibly similar to <see cref="Node"/>, with the exception that its matrix is premultiplied for exporting.
	/// </summary>
	public class Armature {

		/// <summary>
		/// The name of this <see cref="Armature"/>.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The <see cref="Node"/> that this <see cref="Armature"/> was created from.
		/// </summary>
		public Node BaseNode { get; }

		/// <summary>
		/// A reference to <see cref="BaseNode"/>.invRefTransform, which is the inverse reference transformation of this <see cref="Armature"/>.
		/// </summary>
		public Transform3D InverseReferenceTransform => BaseNode.invRefTransform;

		/// <summary>
		/// A reference to <see cref="BaseNode"/>.transform, which is the main transformation of this <see cref="Armature"/>.
		/// </summary>
		public Transform3D Transform => BaseNode.transform;

		/// <summary>
		/// The index of this bone in the parent model's bone name list.
		/// </summary>
		public int Index { get; internal set; } = int.MinValue;

		/// <summary>
		/// The <see cref="Armature"/> that contains this instance, or <see langword="null"/> if this is a root instance.<para/>
		/// Setting this will update the children of the applicable objects (remove this from the children of the old parent (if applicable), add this to the children of the new parent (if applicable)) automatically.
		/// </summary>
		public Armature Parent {
			get => _Parent;
			set {
				if (value == _Parent) return;

				if (_Parent != null) {
					_Parent.RemoveChild(this);
				}
				if (value != null) {
					value.AddChild(this);
				}
				_Parent = value;
			}
		}

		#region Hierarchy Control

		/// <summary>
		/// The internal reference of the parent. Do not change this internally, and instead change <see cref="Parent"/>, as it properly updates the object hierarchy.
		/// </summary>
		private Armature _Parent = null;

		/// <summary>
		/// All <see cref="Armature"/>s that are first-level descendants of this object (this does NOT include nested objects).
		/// </summary>
		public IReadOnlyList<Armature> Children => _Children.AsReadOnly();

		/// <summary>
		/// An internal reference to the children of this <see cref="Armature"/>.
		/// </summary>
		private readonly List<Armature> _Children = new List<Armature>();

		/// <summary>
		/// Adds the given child to this <see cref="Armature"/>'s children.
		/// </summary>
		/// <param name="child"></param>
		protected internal void AddChild(Armature child) {
			if (!_Children.Contains(child)) {
				_Children.Add(child);
			}
		}

		/// <summary>
		/// Removes the given child from this <see cref="Armature"/>'s children.
		/// </summary>
		/// <param name="child"></param>
		protected internal void RemoveChild(Armature child) {
			if (_Children.Contains(child)) {
				_Children.Remove(child);
			}
		}

		#endregion

		protected Armature(Node node) {
			Name = node.name;
			BaseNode = node;
		}

		/// <summary>
		/// Given a <see cref="Node"/> from an <see cref="ArticulatedConfig"/>, this will translates the <see cref="Node"/> and all its <see cref="Node.children"/> into <see cref="Armature"/>s.
		/// </summary>
		/// <param name="rootNode"></param>
		/// <returns></returns>
		public static Armature ConstructHierarchyFromNode(Node rootNode) {
			Armature fromRoot = new Armature(rootNode);
			IterateChildren(fromRoot, rootNode);
			return fromRoot;
		}

		private static void IterateChildren(Armature parent, Node currentNode) {
			foreach (Node node in currentNode.children) {
				Armature armature = new Armature(node) {
					Parent = parent
				};
				IterateChildren(armature, node);
			}
		}

		/// <summary>
		/// Gets the <see cref="Index"/> property of the children of this armature, adding <paramref name="offset"/> to their value.
		/// </summary>
		/// <param name="offset"></param>
		/// <returns></returns>
		public int[] GetChildIndices(int offset = 0) {
			int[] indices = new int[Children.Count];
			for (int idx = 0; idx < indices.Length; idx++) {
				indices[idx] = Children[idx].Index + offset;
			}
			return indices;
		}

		/// <summary>
		/// Recursively get the children of this node.
		/// </summary>
		/// <returns></returns>
		public List<Armature> GetDescendants() {
			List<Armature> descendants = new List<Armature>();
			GetDescendants(descendants);
			return descendants;
		}

		private void GetDescendants(List<Armature> parentList) {
			foreach (Armature node in Children) {
				parentList.Add(node);
				node.GetDescendants(parentList);
			}
		}
	}
}
