using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.Structs {
	/// <summary>
	/// Stores three <see langword="float"/> values that represent a coordinate in 3D space.<para/>
	/// Unlike <see cref="System.Numerics.Vector2"/>, this does not contain any vector math methods. This is strictly for data storage.
	/// </summary>
	public struct Vector2 {

		/// <summary>
		/// The X component of this <see cref="Vector2"/> which generally represents left or right positions relative to the world.
		/// </summary>
		public float X;

		/// <summary>
		/// The Y component of this <see cref="Vector2"/> which generally represents upward or downward positions relative to the world.
		/// </summary>
		public float Y;

		/// <summary>
		/// Construct a new Vector3 with the given X, Y, and Z coordinates.
		/// </summary>
		/// <param name="x">The X component of this Vector3.</param>
		/// <param name="y">The Y component of this Vector3.</param>
		public Vector2(float x = 0, float y = 0) {
			X = x;
			Y = y;
		}

		/// <summary>
		/// Returns a list of <see cref="Vector2"/>s composed of the given float array, taking each value out in pairs.<para/>
		/// Throws <see cref="DataMisalignedException"/> if the float array's length is not divisible by two.
		/// </summary>
		/// <param name="values">The float array to be translated into a <see cref="Vector2"/> array.</param>
		/// <returns></returns>
		public static Vector2[] FromFloatArray(float[] values) {
			int lenDiv2 = values.Length / 2;
			if (values.Length % 2 != 0) throw new DataMisalignedException("Failed to convert float[] to Vector2[] -- Float array does not have a length divisible by 2!");
			Vector2[] vecs = new Vector2[lenDiv2];
			for (int idx = 0; idx < values.Length; idx += 2) {
				vecs[idx / 2] = new Vector2(values[idx], values[idx + 1]);
			}
			return vecs;
		}

		/// <summary>
		/// Converts this <see cref="Vector2"/> into a string where each component is separated by a single space: <c>X Y</c>
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			return $"{X} {Y}";
		}
	}
}
