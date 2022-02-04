using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using XDataTree.Data;
using XDataTree.TreeElements;
using XDataTree.TreeElements.Interop;

namespace XDataTree {

	/// <summary>
	/// Represents an element in a data tree in its most minimal form.
	/// </summary>
	public abstract class TreeElement {

		private const string MODIFY_TOOLTIP = "Double-click this option to modify it.";

		/// <summary>
		/// All child nodes of this <see cref="TreeElement"/>.
		/// </summary>
		protected readonly List<TreeElement> Children = new List<TreeElement>();

		/// <summary>
		/// If this <see cref="TreeElement"/> has associated properties (mostly used for a second data tree view),
		/// then this is the element of the actual properties window.
		/// </summary>
		public virtual TreeElement Properties { get; }

		/// <summary>
		/// The icon displayed on this tree element.
		/// </summary>
		public SilkImage Icon { get; protected set; }

		/// <summary>
		/// The text displayed on this element.
		/// </summary>
		public abstract string Text { get; protected set; }

		/// <summary>
		/// The tooltip to display on this when the mouse hovers over it.
		/// </summary>
		public virtual string Tooltip { get; set; }

		/// <summary>
		/// Whether or not this element in the data tree can have objects inside of it.
		/// </summary>
		public abstract bool CanContainChildren { get; protected set; }

		/// <summary>
		/// Whether or not this tree element can be edited in some fashion. This causes its text to display in blue with an underline.
		/// </summary>
		public virtual bool IsEditable { get; protected set; } = false;

		/// <summary>
		/// Only available after calling <see cref="ConvertToNode"/>, this is the equivalent <see cref="TreeNode"/> for use in a WinForms based application.
		/// </summary>
		public TreeNode? EquivalentNode { get; private set; }

		/// <summary>
		/// Construct a new <see cref="TreeElement"/> using the given icon.
		/// </summary>
		/// <param name="image"></param>
		public TreeElement(SilkImage image) {
			Icon = image;
			Properties = new RootSubstituteElement();
			Tooltip = string.Empty;
		}

		/// <summary>
		/// Explicitly for <see cref="RootSubstituteElement"/>
		/// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		internal TreeElement() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		/// <summary>
		/// Returns a list of every tree element within this one. Returns an empty array if <see cref="CanContainChildren"/> is <see langword="false"/>.
		/// </summary>
		/// <returns></returns>
		public virtual TreeElement[] GetChildren() => CanContainChildren ? Children.ToArray() : Array.Empty<TreeElement>();

		/// <summary>
		/// Clears all child elements from this instance, if supported.
		/// </summary>
		public virtual void ClearAllChildren() {
			if (CanContainChildren) {
				Children.Clear();
			}
		}

		/// <summary>
		/// Adds the given <see cref="TreeElement"/> as a child of this.
		/// </summary>
		/// <param name="child"></param>
		/// <exception cref="NotSupportedException">If <see cref="CanContainChildren"/> is <see langword="false"/></exception>
		public void Add(TreeElement child) {
			if (!CanContainChildren) throw new NotSupportedException();
			Children.Add(child);
		}

		/// <summary>
		/// Removes the given <see cref="TreeElement"/> from this element's child list.
		/// </summary>
		/// <param name="child"></param>
		/// <exception cref="NotSupportedException">If <see cref="CanContainChildren"/> is <see langword="false"/></exception>
		public void Remove(TreeElement child) {
			if (!CanContainChildren) throw new NotSupportedException();
			Children.Remove(child);
		}

		/// <summary>
		/// Checks whether or not the given <see cref="TreeElement"/> is a child of this.
		/// </summary>
		/// <param name="child"></param>
		/// <exception cref="NotSupportedException">If <see cref="CanContainChildren"/> is <see langword="false"/></exception>
		public bool Contains(TreeElement child) {
			if (!CanContainChildren) throw new NotSupportedException();
			return Children.Contains(child);
		}

		/// <summary>
		/// Executes when this tree element is clicked on or highlighted.
		/// </summary>
		public virtual void OnSelected(SynchronizationContext? synchronizationContext, object mainWindow) { }

		/// <summary>
		/// Executes when this tree element is deselected, either by something else being clicked on or it being unhighlighted.
		/// </summary>
		public virtual void OnDeselected(SynchronizationContext? synchronizationContext, object mainWindow) { }

		/// <summary>
		/// Executes when this element is double clicked on or the enter key is pressed while it's selected.
		/// </summary>
		public virtual void OnActivated(SynchronizationContext? synchronizationContext, object mainWindow) { }

		/// <summary>
		/// Converts this <see cref="TreeElement"/> into a <see cref="TreeNode"/>. If this has any child elements, they are converted as well.
		/// </summary>
		/// <remarks>
		/// This sets the <see cref="TreeNode.Tag"/> property to its equivalent <see cref="TreeElement"/>. Likewise, this populates
		/// </remarks>
		/// <returns></returns>
		public virtual EventHandlingTreeNode ConvertToNode() {
			TreeElement[] children = GetChildren();
			if (children.Length == 0) {
				EventHandlingTreeNode instance = new EventHandlingTreeNode(Text, (int)Icon, (int)Icon) {
					OnActivated = OnActivated,
					OnDeselected = OnDeselected,
					OnSelected = OnSelected,
					Tag = this,
				};
				if (IsEditable) {
					instance.ForeColor = System.Drawing.Color.Blue;
					instance.ToolTipText = MODIFY_TOOLTIP;
				}
				if (!string.IsNullOrWhiteSpace(Tooltip)) {
					instance.ToolTipText = Tooltip;
				}
				EquivalentNode = instance;
				return instance;
			} else {
				TreeNode[] nodeChildren = new TreeNode[children.Length];
				for (int idx = 0; idx < children.Length; idx++) {
					nodeChildren[idx] = children[idx].ConvertToNode();
				}
				EventHandlingTreeNode instance = new EventHandlingTreeNode(Text, (int)Icon, (int)Icon, nodeChildren) {
					OnActivated = OnActivated,
					OnDeselected = OnDeselected,
					OnSelected = OnSelected,
					Tag = this,
				};
				if (IsEditable) {
					instance.ForeColor = System.Drawing.Color.Blue;
					instance.ToolTipText = MODIFY_TOOLTIP;
				}
				if (!string.IsNullOrWhiteSpace(Tooltip)) {
					instance.ToolTipText = Tooltip;
				}
				EquivalentNode = instance;
				return instance;
			}
		}

	}
}
