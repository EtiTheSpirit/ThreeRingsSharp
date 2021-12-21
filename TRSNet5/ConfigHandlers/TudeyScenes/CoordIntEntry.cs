using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.ConfigHandlers.TudeyScenes {
	public class CoordIntEntry {

		/// <summary>
		/// The coordinate this entry is bound to.
		/// </summary>
		public Coord Key { get; } = new Coord();

		/// <summary>
		/// The value at this entry.
		/// </summary>
		public int Value => _values[_index];

		/// <summary>
		/// The values array of this entry. Defaults to an uninitialized array, as it is set to an int[] reference by <see cref="CoordIntMap.CoordEntrySet"/>.
		/// As such, using <see cref="SetValue"/> will actually reflect to the original object in the <see cref="CoordIntMap"/> that created this.
		/// </summary>
		internal int[] _values;
		internal int _index = 0;

		/// <summary>
		/// Initialize a new, blank <see cref="CoordIntEntry"/> at the origin with a value of 0.
		/// </summary>
#pragma warning disable CS8618
		internal CoordIntEntry() { }
#pragma warning restore CS8618

		/// <summary>
		/// Sets the value associated with this entry.
		/// </summary>
		/// <param name="value">The new value to associated with this entry.</param>
		/// <returns>The previous value prior to it being changed.</returns>
		public int SetValue(int value) {
			int oldValue = _values[_index];
			_values[_index] = value;
			return oldValue;
		}

		public override bool Equals(object? obj) {
			if (ReferenceEquals(obj, this)) return true;
			if (obj is CoordIntEntry entry) return entry.Key == Key && entry.Value == Value;
			return false;
		}

		public override int GetHashCode() {
			return Key.GetHashCode() ^ Value;
		}

		public override string ToString() {
			return Key.ToString() + ": " + Value;
		}

		public static bool operator ==(CoordIntEntry? left, CoordIntEntry? right) {
			return left?.Equals(right) ?? false;
		}

		public static bool operator !=(CoordIntEntry? left, CoordIntEntry? right) => !(left == right);

	}
}
