using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XDataTree.Data;

namespace XDataTree.TreeElements {

	/// <summary>
	/// A container for one or more <see cref="KeyValueElement"/>s.
	/// </summary>
	public class KeyValueContainerElement : TreeElement {

		/// <inheritdoc/>
		public override string Text { get; protected set; }

		/// <inheritdoc/>
		public override bool CanContainChildren {
			get => true;
			protected set => throw new NotSupportedException();
		}

		/// <summary>
		/// Create a new <see cref="KeyValueContainerElement"/>, which is intended to contain one or more <see cref="KeyValueElement"/>s.
		/// </summary>
		/// <param name="text">The text to display on this container.</param>
		/// <param name="icon">The icon to use for this container.</param>
		public KeyValueContainerElement(string text, SilkImage icon = SilkImage.Generic) : base(icon) {
			Text = text;
		}

		/// <summary>
		/// Adds the given key/value pair to this container. Converts the pair into a <see cref="KeyValueElement"/>.
		/// </summary>
		/// <param name="key">The key for this value.</param>
		/// <param name="value">The value to display. The <see cref="object.ToString()"/> method will be used on this.</param>
		/// <param name="icon">The icon to use for this value. It is recommended that this is uniform for all entries, but it is not required.</param>
		public void Add(string key, object? value, SilkImage icon = SilkImage.Value) => Add(new KeyValueElement(key, value?.ToString() ?? "null", false, icon));

		/// <summary>
		/// Directly adds the given <see cref="KeyValueElement"/> to this container.
		/// </summary>
		/// <param name="kvElement">The element to add to this container.</param>
		public void Add(KeyValueElement kvElement) {
			Children.Add(kvElement);
		}

		/// <summary>
		/// Removes the first element that has the given key.
		/// </summary>
		/// <param name="key">The key to search for.</param>
		public void Remove(string key) {
			TreeElement? obj = Children.FirstOrDefault(child => {
				if (child is KeyValueElement kve) {
					return kve.Key == key;
				}
				return false;
			});
			if (obj != null) {
				Children.Remove(obj);
			}
		}

		/// <summary>
		/// Directly removes the given <see cref="KeyValueElement"/> from this container.
		/// </summary>
		/// <param name="element"></param>
		public void Remove(KeyValueElement element) {
			Children.Remove(element);
		}

		/// <summary>
		/// Sets the contents of this container to an <see cref="IEnumerable"/>. The child <see cref="KeyValueElement"/> will use <c>[n]</c> as their keys, and ToString() of each array element as values.
		/// If this is an <see cref="IEnumerable"/> of <see cref="KeyValuePair{TKey, TValue}"/> (or, a <see cref="Dictionary{TKey, TValue}"/>) then this will treat it accordingly.
		/// </summary>
		/// <param name="enumerable">The object array to use in this container.</param>
		/// <param name="imageOverrides">If defined, this is a 1:1 override of the images used on a given value object. That is, <paramref name="imageOverrides"/>[0] is the image used on <paramref name="enumerable"/>[0], <paramref name="imageOverrides"/>[1] for <paramref name="enumerable"/>[1], and so on for each index. If the length of this is 1, then that icon is used for all elements.</param>
		/// <param name="keyless">If true, keys will be blank strings instead of the array index.</param>
		public void SetToEnumerable(IEnumerable enumerable, SilkImage[]? imageOverrides = null, bool keyless = false) {
			Children.Clear();
			SilkImage icon = SilkImage.Value;
			if (imageOverrides != null && imageOverrides.Length == 1) {
				icon = imageOverrides[0];
			}
			int idx = 0;
			foreach (object o in enumerable) {
				if (imageOverrides != null && imageOverrides.Length != 1) {
					icon = imageOverrides[idx];
				}
				KeyValueElement kve;
				if (o is KeyValuePair<object, object> kvp) {
					kve = new KeyValueElement(keyless ? null : kvp.Key.ToString() ?? "null", kvp.Value?.ToString() ?? "null", false, icon);
				} else {
					kve = new KeyValueElement(keyless ? null : $"[{idx}]", o?.ToString() ?? "null", false, icon);
				}
				Add(kve);
				idx++;
			}
		}

		/// <summary>
		/// Sets the contents of this container to a dictionary. The child <see cref="KeyValueElement"/> will use the result of <see cref="object.ToString()"/> for both the key and value.
		/// </summary>
		/// <param name="dictionary">The dictionary to use in this container.</param>
		/// <param name="imageOverrides">If defined, this is a 1:1 override of the images used on a given value object. That is, <paramref name="imageOverrides"/>[key0] is the image used on <paramref name="dictionary"/>[key0], <paramref name="imageOverrides"/>[key1] for <paramref name="dictionary"/>[key1], and so on for each index.</param>
		/// <param name="strictImageOverrides">If true, and if a key exists in <paramref name="dictionary"/> but not <paramref name="imageOverrides"/>, then an exception will be raised instead of defaulting to <see cref="SilkImage.Value"/></param>
		public void SetToDictionary(Dictionary<object, object?> dictionary, Dictionary<object, SilkImage>? imageOverrides = null, bool strictImageOverrides = false) {
			Children.Clear();
			SilkImage icon = SilkImage.Value;
			foreach (KeyValuePair<object, object?> data in dictionary) {
				if (imageOverrides != null) {
					if (!imageOverrides.TryGetValue(data.Key, out icon)) {
						if (strictImageOverrides) throw new KeyNotFoundException($"{nameof(imageOverrides)} does not contain key {data.Key}!");
						icon = SilkImage.Value;
					}
				}
				KeyValueElement kve = new KeyValueElement(data.Key.ToString() ?? "null", data.Value?.ToString() ?? "null", false, icon);
				Add(kve);
			}
		}

	}
}
