using java.util;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.Structs {

	/// <summary>
	/// Represents a vertex, which contains a <see cref="Vector3"/> Point, a <see cref="float"/> Weight, a <see cref="Vector3"/> Normal, and a <see cref="Vector2"/> UV coordinate.
	/// </summary>
	public struct Vertex : IEquatable<Vertex>, ICloneable<Vertex> {

		/// <summary>
		/// The location of this <see cref="Vertex"/> in 3D space.
		/// </summary>
		public Vector3 Point;

		/// <summary>
		/// The weight of this <see cref="Vertex"/> in the context of bone groups.
		/// </summary>
		public float Weight;

		/// <summary>
		/// The normal of this <see cref="Vertex"/>.
		/// </summary>
		public Vector3 Normal;

		/// <summary>
		/// The UV coordinate of this <see cref="Vertex"/>.
		/// </summary>
		public Vector2 UV;

		/// <summary>
		/// Construct a new <see cref="Vertex"/> with the given X, Y, and Z coordinates and weight.
		/// </summary>
		/// <param name="x">The X component of this Vector3.</param>
		/// <param name="y">The Y component of this Vector3.</param>
		/// <param name="z">The Z component of this Vector3.</param>
		/// <param name="weight">The weight of this vertex in the context of any associated bone data.</param>
		/// <param name="normal">The normal of this vertex.</param>
		/// <param name="uv">The UV coordinate of this vertex.</param>
		public Vertex(float x = 0f, float y = 0f, float z = 0f, float weight = 1f, Vector3 normal = default, Vector2 uv = default) {
			Point = new Vector3(x, y, z);
			Weight = weight;
			Normal = normal;
			UV = uv;
		}

		/// <summary>
		/// Construct a new <see cref="Vertex"/> from the given point and weight.
		/// </summary>
		/// <param name="point">The location of this <see cref="Vertex"/> in 3D space.</param>
		/// <param name="weight">The weight of this <see cref="Vertex"/> in the context of any associated bone data.</param>
		/// <param name="normal">The normal of this vertex.</param>
		/// <param name="uv">The UV coordinate of this vertex.</param>
		public Vertex(Vector3 point, float weight = 1f, Vector3 normal = default, Vector2 uv = default) {
			Point = point;
			Weight = weight;
			Normal = normal;
			UV = uv;
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

		public Vertex Clone() {
			return new Vertex() {
				Point = Point.Clone(),
				Weight = Weight,
				Normal = Normal.Clone(),
				UV = UV.Clone()
			};
		}
	}
}
