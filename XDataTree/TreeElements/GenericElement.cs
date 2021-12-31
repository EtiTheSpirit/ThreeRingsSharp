using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XDataTree.Data;

namespace XDataTree.TreeElements {

	/// <summary>
	/// A generic <see cref="TreeElement"/> containing text and an icon. Can contain child nodes. 
	/// It is best compared to a folder, with no special functionality on its own.
	/// </summary>
	public class GenericElement : TreeElement {

		/// <inheritdoc/>
		public override string Text { get; protected set; }

		/// <inheritdoc/>
		public override bool CanContainChildren { get; protected set; } = true;

		/// <summary>
		/// A generic <see cref="TreeElement"/> containing text and an icon. Can contain child nodes.
		/// </summary>
		/// <param name="text">The text displayed on this node.</param>
		/// <param name="icon">The icon used for this node.</param>
		public GenericElement(string text, SilkImage icon = SilkImage.Generic) : base(icon) {
			Text = text;
		}

		/// <summary>
		/// Sets the text to the given <see cref="string"/>.
		/// </summary>
		/// <param name="text"></param>
		public void SetText(string text) => Text = text;

		/// <summary>
		/// Sets the icon to the given <see cref="SilkImage"/>.
		/// </summary>
		/// <param name="icon"></param>
		public void SetIcon(SilkImage icon) => Icon = icon;

		/// <summary>
		/// Changes whether or not child elements are allowed.
		/// </summary>
		/// <param name="allowChildren"></param>
		public void SetCanContainChildren(bool allowChildren) => CanContainChildren = allowChildren;
	}
}
