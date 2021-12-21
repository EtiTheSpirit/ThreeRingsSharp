using OOOReader.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.ConfigHandlers.TudeyScenes {
	public class Cell {

		private readonly CoordIntMap _super;
		public int[] Values { get; private set; }
		public int Size { get; private set; }

		public Cell(CoordIntMap super) {
			_super = super;
			Values = new int[1 << super.Granularity << super.Granularity];
			Array.Fill(Values, super.Empty);
		}

		internal Cell(CoordIntMap super, ShadowClass shadow) {
			_super = super;
			Values = shadow.GetField<int[]>("_values")!;
			Size = Values.Count(value => value != _super!.Empty);
		}

		public bool ContainsValue(int value) => Values.Contains(value);

		public int Get(int x, int y) => Values[y << _super.Granularity | x];

		public int Put(int x, int y, int value) {
			int index = y << _super.Granularity | x;
			int oldValue = Values[index];
			Values[index] = value;
			if (oldValue == _super.Empty) {
				Size++;
			}
			_super._modCount++;
			return oldValue;
		}

		public int SetBits(int x, int y, int bits) {
			int index = y << _super.Granularity | x;
			int oldValue = Values[index];
			Values[index] |= bits;
			if (oldValue == _super.Empty) {
				Size++;
			}
			_super._modCount++;
			return oldValue;
		}

		public int Remove(int index) {
			int oldValue = Values[index];
			Values[index] = _super.Empty;
			Size--;
			_super._modCount++;
			return oldValue;
		}

		public int Remove(int x, int y) => Remove(y << _super.Granularity | x);

	}
}
