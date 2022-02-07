using OOOReader.Reader;
using System;
using System.Collections.Generic;
using OOOReader.Utility.Mathematics;

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
		public Transform3D InverseReferenceTransform => BaseNode.InverseReferenceTransform;
		/*
		public Transform3D InverseReferenceTransform {
			get {
				Transform3D? parentTransform = Parent?.Transform;
				if (parentTransform != null) {
					return parentTransform.Compose(Transform).Invert();
				}
				return Transform3D.NewIdentity();
			}
		}
		*/

		/// <summary>
		/// A reference to <see cref="BaseNode"/>.transform, which is the main transformation of this <see cref="Armature"/>.
		/// </summary>
		public Transform3D Transform => BaseNode.Transform;

		/// <summary>
		/// The index of this bone in the parent model's bone name list.
		/// </summary>
		public int Index { get; internal set; } = int.MinValue;

		/// <summary>
		/// The <see cref="Armature"/> that contains this instance, or <see langword="null"/> if this is a root instance.<para/>
		/// Setting this will update the children of the applicable objects (remove this from the children of the old parent (if applicable), add this to the children of the new parent (if applicable)) automatically.
		/// </summary>
		public Armature? Parent {
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
		private Armature? _Parent = null;

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
			Name = node.Name;
			BaseNode = node;
			node.RealArmature = this;
		}

		/// <summary>
		/// Given a <see cref="Node"/> from an ArticulatedConfig, this will translates the <see cref="Node"/> and all its <see cref="Node.Children"/> into <see cref="Armature"/>s.<para/>
		/// If the given <see cref="Node"/> has already been transformed using this method, it will return the previously created <see cref="Armature"/>.
		/// </summary>
		/// <param name="rootNode"></param>
		/// <returns></returns>
		public static Armature ConstructHierarchyFromNode(Node rootNode) {
			if (rootNode.RealArmature != null) {
				return rootNode.RealArmature;
			}
			Armature fromRoot = new Armature(rootNode);
			IterateChildren(fromRoot, rootNode);
			return fromRoot;
		}

		private static void IterateChildren(Armature parent, Node currentNode) {
			foreach (Node node in currentNode.Children) {
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

		#region Nodes

		/// <summary>
		/// An intermediary stage from <see cref="ShadowClass"/> to <see cref="RealArmature"/> that is easier to modify in early stages of conversion.
		/// </summary>
		public class Node {
			/// <summary>
			/// The name of this node.
			/// </summary>
			public string Name { get; set; }

			/// <summary>
			/// The transform of this node.
			/// </summary>
			public Transform3D Transform { get; set; }

			/// <summary>
			/// The nodes within this node.
			/// </summary>
			public Node[] Children { get; set; }

			/// <summary>
			/// The inverse reference transform of this node.
			/// </summary>
			//[Obsolete("This is now computed on the fly in an Armature as it requires a hierarchy reference.")]
			public Transform3D InverseReferenceTransform { get; set; }

			/// <summary>
			/// The <see cref="Armature"/> that has been created from this <see cref="Node"/>, granted that such has been done.
			/// </summary>
			public Armature? RealArmature { get; internal set; }

			/// <summary>
			/// Construct a new node with the given name, transform, and children.
			/// </summary>
			/// <param name="name"></param>
			/// <param name="transform"></param>
			/// <param name="children"></param>
			public Node(string name, Transform3D transform, Node[]? children = null) {
				Name = name;
				Transform = transform;
				Children = children ?? Array.Empty<Node>();
				InverseReferenceTransform = new Transform3D();
			}

			/// <summary>
			/// Construct a new node from the given shadow of an ArticulatedConfig.Node instance.
			/// </summary>
			/// <param name="articulatedConfigNode"></param>
			public Node(ShadowClass articulatedConfigNode) {
				Name = articulatedConfigNode["name"]!;
				
				ShadowClass? transform = (ShadowClass?)articulatedConfigNode["transform"];
				ShadowClass? invRefTransform = (ShadowClass?)articulatedConfigNode["invRefTransform"];

				Transform = transform != null ? Transform3D.FromShadow(transform) : new Transform3D();
				InverseReferenceTransform = invRefTransform != null ? Transform3D.FromShadow(invRefTransform) : new Transform3D();

				ShadowClass[]? children = (ShadowClass[]?)articulatedConfigNode["children"];
				if (children != null && children.Length > 0) {
					Children = new Node[children.Length];
					for (int i = 0; i < Children.Length; i++) {
						Children[i] = new Node(children[i]);
					}
				} else {
					Children = Array.Empty<Node>();
				}
			}
		}

		#endregion
	}
}
