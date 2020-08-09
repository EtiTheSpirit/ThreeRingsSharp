using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.IO.GLTF {

	/// <summary>
	/// Represents a component type in glTF for use in accessors.
	/// </summary>
	public static class GLTFComponentType {

		/// <summary>
		/// A <see langword="sbyte"/> value.
		/// </summary>
		public const int BYTE = 5120;

		/// <summary>
		/// A <see langword="byte"/> value.
		/// </summary>
		public const int UNSIGNED_BYTE = 5121;

		/// <summary>
		/// A <see langword="short"/> value.
		/// </summary>
		public const int SHORT = 5122;

		/// <summary>
		/// A <see langword="ushort"/> value.
		/// </summary>
		public const int UNSIGNED_SHORT = 5123;

		/// <summary>
		/// An <see langword="int"/> value.
		/// </summary>
		public const int UNSIGNED_INT = 5125;

		/// <summary>
		/// A <see langword="float"/> value.
		/// </summary>
		public const int FLOAT = 5126;

	}

	/// <summary>
	/// Represents a type in glTF for use in accessors.
	/// </summary>
	public static class GLTFDataType {

		/// <summary>
		/// A scalar value, which contains a single component.
		/// </summary>
		public const string SCALAR = "SCALAR";

		/// <summary>
		/// A Vector2 value, which contains 2 components.
		/// </summary>
		public const string VEC2 = "VEC2";

		/// <summary>
		/// A Vector3 value, which contains 3 components.
		/// </summary>
		public const string VEC3 = "VEC3";

		/// <summary>
		/// A Vector4 value, which contains 4 components.
		/// </summary>
		public const string VEC4 = "VEC4";

		/// <summary>
		/// A MAT2 value (2x2 transformation matrix), which contains 4 components.
		/// </summary>
		public const string MAT2 = "MAT2";

		/// <summary>
		/// A MAT3 value (3x3 transformation matrix), which contains 9 components.
		/// </summary>
		public const string MAT3 = "MAT3";

		/// <summary>
		/// A MAT4 value (4x4 transformation matrix), which contains 16 components.
		/// </summary>
		public const string MAT4 = "MAT4";

	}

	/// <summary>
	/// Represents a method of interpolation for animations.
	/// </summary>
	public static class GLTFAnimationInterpolation {

		/// <summary>
		/// Represents linear motion.
		/// </summary>
		public const string LINEAR = "LINEAR";

		/// <summary>
		/// Represents constant interpolation, or, the latest keyframe is persistent until another keyframe overrides it later on.
		/// </summary>
		public const string STEP = "STEP";

		/// <summary>
		/// Represents a cubic spline interpolation pattern.
		/// </summary>
		public const string CUBICSPLINE = "CUBICSPLINE";

	}
}
