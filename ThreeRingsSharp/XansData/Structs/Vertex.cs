using java.util;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.Structs {

	/// <summary>
	/// Represents a vertex, which is effectively identical to a <see cref="Vector3"/> with a Weight value.
	/// </summary>
	public struct Vertex : IEquatable<Vertex> {

		/// <summary>
		/// The location of this <see cref="Vertex"/> in 3D space.
		/// </summary>
		public Vector3 Point;

		/// <summary>
		/// The weight of this <see cref="Vertex"/> in the context of bone groups.
		/// </summary>
		public float Weight;

		/// <summary>
		/// Construct a new <see cref="Vertex"/> with the given X, Y, and Z coordinates and weight.
		/// </summary>
		/// <param name="x">The X component of this Vector3.</param>
		/// <param name="y">The Y component of this Vector3.</param>
		/// <param name="z">The Z component of this Vector3.</param>
		/// <param name="weight">The weight of this vertex in the context of any associated bone data.</param>
		public Vertex(float x = 0f, float y = 0f, float z = 0f, float weight = 1f) {
			Point = new Vector3(x, y, z);
			Weight = weight;
		}

		/// <summary>
		/// Construct a new <see cref="Vertex"/> from the given point and weight.
		/// </summary>
		/// <param name="point">The location of this <see cref="Vertex"/> in 3D space.</param>
		/// <param name="weight">The weight of this <see cref="Vertex"/> in the context of any associated bone data.</param>
		public Vertex(Vector3 point, float weight = 1f) {
			Point = point;
			Weight = weight;
		}

		/// <summary>
		/// Returns a list of <see cref="Vertex"/>s composed of the given float array, taking each value out in triplets. The weight of each <see cref="Vertex"/> will be set to <paramref name="defaultWeight"/>.<para/>
		/// Throws <see cref="DataMisalignedException"/> if the float array's length is not divisible by three.
		/// </summary>
		/// <param name="values">The float array to be translated into a <see cref="Vertex"/> array.</param>
		/// <param name="defaultWeight">The weight to give to each vertex, which is used in the context of bone information.</param>
		/// <returns></returns>
		public static Vertex[] FromFloatArray(float[] values, float defaultWeight = 1f) {
			int lenDiv3 = values.Length / 3;
			if (values.Length % 3 != 0) throw new DataMisalignedException("Failed to convert float[] to Vertex[] -- Float array does not have a length divisible by 3!");
			Vertex[] vecs = new Vertex[lenDiv3];
			for (int idx = 0; idx < values.Length; idx += 3) {
				vecs[idx / 3] = new Vertex(values[idx], values[idx + 1], values[idx + 2], defaultWeight);
			}
			return vecs;
		}

		/// <summary>
		/// Returns a list of <see cref="Vertex"/>s composed of the given float array, taking each value out in triplets.<para/>
		/// Throws <see cref="DataMisalignedException"/> if the float array's length is not divisible by three, or if the length of the weight array isn't the same as the result <see cref="Vertex"/> array.
		/// </summary>
		/// <param name="values">The float array to be translated into a <see cref="Vertex"/> array.</param>
		/// <param name="weights">A list of the weights to give the resulting <see cref="Vertex"/> instances.</param>
		/// <returns></returns>
		public static Vertex[] FromFloatArray(float[] values, float[] weights) {
			int lenDiv3 = values.Length / 3;
			if (values.Length % 3 != 0) throw new DataMisalignedException("Failed to convert float[] to Vertex[] -- Float array does not have a length divisible by 3!");
			if (weights.Length != lenDiv3) throw new DataMisalignedException("Failed to convert float[] to Vertex[] -- Weight array does not have the same length as the result Vertex array."); 
			Vertex[] vecs = new Vertex[lenDiv3];
			for (int idx = 0; idx < values.Length; idx += 3) {
				vecs[idx / 3] = new Vertex(values[idx], values[idx + 1], values[idx + 2], weights[idx / 3]);
			}
			return vecs;
		}

		#region Equality: Vertex to Vertex
		public bool Equals(Vertex other) {
			if (ReferenceEquals(this, other)) return true;
			return other.Point == Point && other.Weight == Weight;
		}

		public static bool operator ==(Vertex left, Vertex right) => left.Equals(right);

		public static bool operator !=(Vertex left, Vertex right) => !(left == right);
		#endregion

		#region Equality: Vertex to Vector3
		// Note: Cases where Vector3 is the left operand are defined in Vector3.
		public static bool operator ==(Vertex left, Vector3 right) => left.Point.Equals(right);

		public static bool operator !=(Vertex left, Vector3 right) => !(left == right);
		#endregion

		#region Stock Object Overrides
		public override bool Equals(object obj) => obj is Vertex other ? Equals(other) : ReferenceEquals(this, obj);

		public override int GetHashCode() {
			return HashCode.Combine(Point, Weight);
		}

		public override string ToString() {
			return $"[Point={Point}, Weight={Weight}f]";
		}
		#endregion
	}
}
