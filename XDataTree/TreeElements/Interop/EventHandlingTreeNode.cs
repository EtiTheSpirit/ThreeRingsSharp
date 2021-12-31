using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XDataTree.TreeElements.Interop {

	/// <summary>
	/// An extension to <see cref="TreeNode"/> that provides methods that execute when this is selected, deselected, or double-clicked / return is pressed.
	/// </summary>
	public class EventHandlingTreeNode : TreeNode {
		public EventHandlingTreeNode() {}

		public EventHandlingTreeNode(string text) : base(text) {}

		public EventHandlingTreeNode(string text, TreeNode[] children) : base(text, children) {}

		public EventHandlingTreeNode(string text, int imageIndex, int selectedImageIndex) : base(text, imageIndex, selectedImageIndex) {}

		public EventHandlingTreeNode(string text, int imageIndex, int selectedImageIndex, TreeNode[] children) : base(text, imageIndex, selectedImageIndex, children) { }

		/// <summary>
		/// An action that will occur when this node is selected.
		/// </summary>
		public Action<SynchronizationContext?, object>? OnSelected { get; set; }

		/// <summary>
		/// An action that will occur when this node is deselected.
		/// </summary>
		public Action<SynchronizationContext?, object>? OnDeselected { get; set; }

		/// <summary>
		/// An action that will occur when this node is double clicked, or when the return key is pressed on it.
		/// </summary>
		public Action<SynchronizationContext?, object>? OnActivated { get; set; }

	}
}
