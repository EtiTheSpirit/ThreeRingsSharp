using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XDataTree.TreeElements.Interop;

namespace XDataTree.TreeElements {

	/// <summary>
	/// A <see cref="TreeElement"/> that serves as a temporary "root node". It only supports child objects. Attempting to index any other properties or convert it to a node
	/// will raise <see cref="NotSupportedException"/>
	/// </summary>
	public class RootSubstituteElement : TreeElement {
		public RootSubstituteElement() : base() { }

		/// <summary>
		/// Cannot be used in <see cref="RootSubstituteElement"/> instances.
		/// Both <see langword="get"/> and <see langword="set"/> raise <see cref="NotSupportedException"/>.
		/// </summary>
		public override string Text {
			get => throw new NotSupportedException();
			protected set => throw new NotSupportedException();
		}

		/// <inheritdoc cref="Text"/>
		public override string Tooltip {
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		/// <summary>
		/// In <see cref="RootSubstituteElement"/>, this is always <see langword="true"/>.
		/// </summary>
		public override bool CanContainChildren {
			get => true;
			protected set => throw new NotSupportedException();
		}

		/// <summary>
		/// In <see cref="RootSubstituteElement"/>, this is always <see langword="false"/>.
		/// </summary>
		public override bool IsEditable {
			get => false;
			protected set => throw new NotSupportedException();
		}


		public override TreeElement Properties => throw new NotSupportedException();

		public override EventHandlingTreeNode ConvertToNode() {
			throw new NotSupportedException();
		}

		/// <summary>
		/// Adds all children of this to the given <see cref="TreeView"/>.
		/// </summary>
		public void AddToTreeView(TreeView treeView) {
			foreach (TreeElement child in GetChildren()) {
				treeView.Nodes.Add(child.ConvertToNode());
			}
		}

		/// <summary>
		/// Whether or not this element is empty.
		/// </summary>
		public bool Empty => Children.Count == 0;
	}
}
