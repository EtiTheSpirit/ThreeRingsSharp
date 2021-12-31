using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using XDataTree.Data;

namespace XDataTree.TreeElements {

	/// <summary>
	/// A "key-value" element contains dedicated <see cref="Key"/> and <see cref="Value"/> properties that both enforce certain text-based parameters. 
	/// It cannot contain child nodes, but in turn, can optionally display its value as a child element. To create a list of many key/value pairs together,
	/// consider using <see cref="KeyValueContainerElement"/>.
	/// </summary>
	public class KeyValueElement : TreeElement {

		/// <summary>
		/// The object containing the possible values for this <see cref="KeyValueElement"/>, assuming it's editable.<para/>
		/// In most cases, this is something like a StaticSetConfig's ShadowClass.
		/// </summary>
		public object ValueHolder { get; set; }

		/// <summary>
		/// The key for this element. Displays as a prefix in <see cref="Text"/>: <c>"Key: "</c>
		/// </summary>
		/// <remarks>
		/// If <see cref="Value"/> is <see langword="null"/> or whitespace, then <see cref="Text"/> will point directly to this and nothing else (it will not have the trailing colon either).
		/// </remarks>
		public string? Key { get; set; }

		/// <summary>
		/// The value for this element. Displays as a suffix in <see cref="Text"/>: <c>"Value"</c>.
		/// </summary>
		/// <remarks>
		/// If <see cref="Key"/> is <see langword="null"/> or whitespace, then <see cref="Text"/> will point directly to this and nothing else.
		/// </remarks>
		public string? Value { get; set; }

		/// <inheritdoc/>
		/// <remarks>
		/// For <see cref="KeyValueElement"/>s, <see cref="Text"/>.<see langword="set"/> will throw a <see cref="NotImplementedException"/>.
		/// </remarks>
		public override string Text {
			get {
				bool hasKey = !string.IsNullOrWhiteSpace(Key);
				bool hasValue = !string.IsNullOrWhiteSpace(Value);
				if (hasKey && hasValue) {
					return $"{Key!}: {Value!}";
				} else if (hasKey) {
					return Key!;
				} else if (hasValue) {
					return Value!;
				}
				return string.Empty;
			}
			protected set => throw new NotSupportedException();
		}

		/// <inheritdoc/>
		public override bool CanContainChildren {
			get => false;
			protected set => throw new NotSupportedException();
		}

		/// <summary>
		/// Construct a new <see cref="KeyValueElement"/> with the given key and value, optionally defining it as editable.
		/// </summary>
		/// <param name="key">The key to display.</param>
		/// <param name="value">The value to display.</param>
		/// <param name="editable">Whether or not this is intended to be edited, which makes the text show in blue with an underline.</param>
		/// <param name="icon">The icon to use on this node.</param>
		public KeyValueElement(string? key, string? value, bool editable = false, SilkImage icon = SilkImage.Value) : base(icon) {
			IsEditable = editable;
			Key = key;
			Value = value;
		}
	}
}
