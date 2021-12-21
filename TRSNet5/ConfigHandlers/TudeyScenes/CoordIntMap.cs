using OOOReader.Reader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.ConfigHandlers.TudeyScenes {
	public class CoordIntMap {

		/// <summary>
		/// I'll be honest with you, I have no clue what this does.
		/// </summary>
		/// <remarks>
		/// I'm Commander Shepard, and this is my favorite comment in the codebase.
		/// </remarks>
		public int Granularity { get; private set; }
		public int Empty { get; private set; }
		public int Mask { get; private set; }
		public int Size { get; private set; }
		public Coord IndexerCoordinate { get; private set; }

		internal int _modCount;
		internal Dictionary<Coord, Cell> _cells = new Dictionary<Coord, Cell>();

		public CoordIntMap(int granularity = 3, int empty = -1) {
			Granularity = granularity;
			Empty = empty;
			IndexerCoordinate = new Coord();
			Mask = (1 << Granularity) - 1;
		}

		public CoordIntMap(ShadowClass shadow) {
			shadow.AssertIsInstanceOf("com.threerings.tudey.util.CoordIntMap");
			Granularity = shadow.GetNumericField<int>("_granularity");
			Empty = shadow.GetNumericField<int>("_empty");
			Size = shadow.GetNumericField<int>("_size");
			IndexerCoordinate = new Coord((ShadowClass)shadow["_coord"]!);
			foreach (KeyValuePair<object, object> cellData in shadow["_cells"]!) {
				_cells.Add(new Coord((ShadowClass)cellData.Key), new Cell(this, (ShadowClass)cellData.Value));
			}
			Mask = (1 << Granularity) - 1;
		}

		/// <summary>
		/// Translates the contents of this <see cref="CoordIntMap"/> into a list of <see cref="CoordIntEntry"/> elements. These elements store values by reference
		/// and so modifications made to these elements will affect this <see cref="CoordIntMap"/>.
		/// </summary>
		/// <returns></returns>
		public List<CoordIntEntry> CoordEntrySet() {
			// The original java impl's iterator (anonymous) seemed to grab all of the values out of the next entry in Cells
			// It would iterate through these, and then the first value it found that was not equal to Empty it would do some mathy stuff to the mask and granularity etc
			// It seems the intent of the iterator was to not create a list of cloned cells for return.
			// This method also protected against concurrent modification in the original java impl (attempting to modify the parent CoordIntMap while this was iterating would
			// raise a ConcurrentModificationException).

			// I don't think it's much of an issue to create a CoordIntEntry list.
			CoordIntMapEnumerator enumerator = new CoordIntMapEnumerator(this);
			List<CoordIntEntry> list = new List<CoordIntEntry>();
			while (enumerator.MoveNext()) {
				list.Add(enumerator.Current);
			}
			return list;
		}

		#region Indexing and Modification

		private void UpdateCoordinate(int x, int y) => IndexerCoordinate.Set(x >> Granularity, y >> Granularity);

		public int Get(int x, int y) {
			Cell? cell = GetCell(x, y);
			return cell?.Get(x & Mask, y & Mask) ?? Empty;
		}

		public int Put(int x, int y, int value) {
			if (value == Empty) return Remove(x, y);
			UpdateCoordinate(x, y);
			Cell? cell = GetCell();
			if (cell == null) {
				cell = new Cell(this);
				_cells[new Coord(IndexerCoordinate)] = cell; 
			}

			int oldValue = cell.Put(x & Mask, y & Mask, value);
			if (oldValue == Empty) {
				Size++;
			}
			return oldValue;
		}

		public int SetBits(int x, int y, int bits) {
			if (bits == 0) return Get(x, y);
			UpdateCoordinate(x, y);
			Cell? cell = GetCell();
			if (cell == null) {
				cell = new Cell(this);
				_cells[new Coord(IndexerCoordinate)] = cell;
			}

			int oldValue = cell.SetBits(x & Mask, y & Mask, bits);
			if (oldValue == Empty) {
				Size++;
			}
			return oldValue;
		}

		public int Remove(int x, int y) {
			UpdateCoordinate(x, y);
			Cell? cell = GetCell();
			if (cell == null) return Empty;
			int oldValue = cell.Remove(x & Mask, y & Mask);
			if (oldValue != Empty) {
				Size--;
				if (cell.Size == 0) {
					_cells.Remove(IndexerCoordinate);
				}
			}
			return oldValue;
		}

		public bool ContainsKey(int x, int y) {
			Cell? cell = GetCell(x, y);
			return (cell?.Get(x & Mask, y & Mask) ?? Empty) != Empty;
		}

		public bool ContainsKey(Coord key) {
			Cell? cell = GetCell(key);
			return (cell?.Get(key.X & Mask, key.Y & Mask) ?? Empty) != Empty;
		}

		public bool ContainsValue(int value) {
			return _cells.FirstOrDefault(kvp => kvp.Value.ContainsValue(value)).Value != null;
		}

		public void Clear() {
			_cells.Clear();
			Size = 0;
			_modCount++;
		}

		#endregion

		#region Cell Acquisition

		/// <summary>
		/// Returns the cell at the given coordinate.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public Cell? GetCell(int x, int y) {
			UpdateCoordinate(x, y);
			return GetCell();
		}

		/// <summary>
		/// Returns the cell at the given coordinate.
		/// </summary>
		/// <param name="location"></param>
		/// <returns></returns>
		public Cell? GetCell(Coord location) {
			if (_cells.TryGetValue(location, out Cell? cell)) {
				return cell!;
			}
			return null;
		}

		/// <summary>
		/// Returns the current cell that <see cref="IndexerCoordinate"/> points to.
		/// </summary>
		/// <returns></returns>
		private Cell? GetCell() => GetCell(IndexerCoordinate);

		#endregion

		#region Iteration

		/// <summary>
		/// An iterator class for <see cref="CoordIntMap"/> that translates all contained values into <see cref="CoordIntEntry"/> objects.
		/// </summary>
		private sealed class CoordIntMapEnumerator : IEnumerator<CoordIntEntry> {

			private CoordIntMap _super;
			private IEnumerator<KeyValuePair<Coord, Cell>> _coordIntMapEnumerator;
			private KeyValuePair<Coord, Cell> _currentIntMapEntry;
			private int _index = 0;
			private CoordIntEntry _current;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
			public CoordIntMapEnumerator(CoordIntMap parent) {
				_super = parent;
				_coordIntMapEnumerator = parent._cells.GetEnumerator();
			}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

			public CoordIntEntry Current => _current;

			object IEnumerator.Current => _current;

			public bool MoveNext() {
				while (_coordIntMapEnumerator.MoveNext()) {
					_currentIntMapEntry = _coordIntMapEnumerator.Current;
					int[] values = _currentIntMapEntry.Value.Values;
					//for (int idx = 0; idx < values.Length; idx++) {
					while (_index < values.Length) {
						int value = values[_index];
						if (value != _super.Empty) {
							Coord coord = _currentIntMapEntry.Key;
							_current = new CoordIntEntry(); // I am going to do this due to how enumerables work. Don't want this deviating from expectation.
							_current.Key.Set(
								coord.X << _super.Granularity | (_index & _super.Mask),
								coord.Y << _super.Granularity | (_index >> _super.Granularity)
							);
							_current._values = values; // Self-reminder: This is a reference.
							// I've been dealing with a lot of Lua (from which setting tables is by value, so a duplicate of the table is actually created)
							// This means that editing _current via its .SetValue method will affect the entry in this CoordIntMap
							_current._index = _index;
							_index++;
							return true;
						}
						_index++; // Emulate the _idx++ of the for loop (which I replaced with a while loop)
					}
					_index = 0;
				}
				return false;
			}

			public void Reset() {
				_coordIntMapEnumerator.Reset();
				_index = 0;
				_current._index = 0;
				_current._values = new int[1];
			}

			public void Dispose() {
				_coordIntMapEnumerator.Dispose();
			}
		}

		#endregion

	}
}
