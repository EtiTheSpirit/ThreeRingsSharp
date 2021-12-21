using OOOReader.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.ConfigHandlers.TudeyScenes {

	/// <summary>
	/// Reimplementation of com.threerings.tudey.util.Coord
	/// </summary>
	public class Coord : IEquatable<Coord> {

		/// <summary>
		/// The encoded form of an empty coordinate, located at -32768, -32768
		/// </summary>
		public static readonly int EMPTY;

		static Coord() {
			unchecked {
				EMPTY = (int)0x80008000;
			}
		}

		/// <summary>
		/// The X component of this coordinate, which responds to left/right in the 3D scene.
		/// </summary>
		public short X { get; set; }

		/// <summary>
		/// The Y component of this coordinate, which corresponds to forward/backwards in the 3D scene (not up/down!).
		/// </summary>
		public short Y { get; set; }

		/// <summary>
		/// The encoded representation of this coordinate pair, where X occupies the upper two bytes and Y occupies the lower two bytes (assuming big endian; 0xXXXXYYYY)
		/// </summary>
		/// <remarks>
		/// Setting this will appropriately set X and Y.
		/// </remarks>
		public int Encoded {
			get => ((X & 0xFFFF) << 16) | (Y & 0xFFFF);
			set {
				unchecked {
					X = (short)(value >> 16);
					Y = (short)(value & 0xFFFF);
				}
			}
		}

		/// <summary>
		/// Create a new <see cref="Coord"/> at (0, 0)
		/// </summary>
		public Coord() { }

		/// <summary>
		/// Clone the other <see cref="Coord"/> into a new instance.
		/// </summary>
		/// <param name="other"></param>
		public Coord(Coord other) {
			X = other.X;
			Y = other.Y;
		}

		/// <summary>
		/// Create a new <see cref="Coord"/> at the given (x, y) coordinate pair.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public Coord(short x, short y) {
			X = x;
			Y = y;
		}

		/// <summary>
		/// Construct a coordinate from a <see cref="ShadowClass"/> representing a coordinate.
		/// </summary>
		/// <param name="shadow"></param>
		public Coord(ShadowClass shadow) {
			shadow.AssertIsInstanceOf("com.threerings.tudey.util.Coord");
			X = shadow.GetNumericField<short>("x");
			Y = shadow.GetNumericField<short>("y");
		}

		/// <summary>
		/// Encodes X and Y into a merged form. Note that only the lower 16 bits are used, so values greater than <see cref="ushort.MaxValue"/> will be incorrect.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public static int Encode(int x, int y) {
			// TODO: Deviate from OOO behavior? Passing in >ushort.MAX_VALUE in for X will cause serious problems for Y.
			// Org behavior: (x << 16) | (y & 0xFFFF);
			// For now, I'll leave it be.
			x <<= 16;
			y &= 0xFFFF;
			return x | y;
		}

		public static int DecodeX(int encoded) {
			return encoded >> 16;
		}

		public static int DecodeY(int encoded) {
			return encoded & 0xFFFF;
		}

		public Coord Set(Coord other) => Set(other.X, other.Y);

		public Coord Set(int[] values) => Set(values[0], values[1]);

		public Coord Set(int encoded) => Set(DecodeX(encoded), DecodeY(encoded));

		public Coord Set(int x, int y) {
			X = (short)x;
			Y = (short)y;
			return this;
		}

		#region Object Overrides

		public override bool Equals(object? obj) {
			if (ReferenceEquals(obj, this)) return true;
			if (obj is Coord coord) Equals(coord);
			return false;
		}

		public override int GetHashCode() {
			return Encoded;
		}

		public override string ToString() {
			return $"[{X}, {Y}]";
		}

		public static bool operator ==(Coord? left, Coord? right) {
			return left?.Equals(right) ?? false;
		}

		public static bool operator !=(Coord? left, Coord? right) => !(left == right);

		public bool Equals(Coord? other) {
			if (ReferenceEquals(other, this)) return true;
			if (other is Coord) return X == other.X && Y == other.Y;
			return false;
		}

		#endregion
	}
}
