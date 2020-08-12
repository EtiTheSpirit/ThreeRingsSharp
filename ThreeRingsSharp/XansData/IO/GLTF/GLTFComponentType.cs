using ThreeRingsSharp.XansData.Extensions;
using ThreeRingsSharp.XansData.IO.GLTF.JSON;

namespace ThreeRingsSharp.XansData.IO.GLTF {

	/// <summary>
	/// Represents a component type in glTF for use in accessors.
	/// </summary>
	public static class GLTFComponentType {

		/// <summary>
		/// A <see langword="sbyte"/> value.
		/// </summary>
		[Size(1)] public const int BYTE = 5120;

		/// <summary>
		/// A <see langword="byte"/> value.
		/// </summary>
		[Size(1)] public const int UNSIGNED_BYTE = 5121;

		/// <summary>
		/// A <see langword="short"/> value.
		/// </summary>
		[Size(2)] public const int SHORT = 5122;

		/// <summary>
		/// A <see langword="ushort"/> value.
		/// </summary>
		[Size(2)] public const int UNSIGNED_SHORT = 5123;

		/// <summary>
		/// An <see langword="int"/> value.
		/// </summary>
		[Size(4)] public const int UNSIGNED_INT = 5125;

		/// <summary>
		/// A <see langword="float"/> value.
		/// </summary>
		[Size(4)] public const int FLOAT = 5126;

	}

	/// <summary>
	/// Represents a type in glTF for use in accessors.
	/// </summary>
	public static class GLTFValueType {

		/// <summary>
		/// A scalar value, which contains a single component.
		/// </summary>
		[Size(1)] public const string SCALAR = "SCALAR";

		/// <summary>
		/// A Vector2 value, which contains 2 components.
		/// </summary>
		[Size(2)] public const string VEC2 = "VEC2";

		/// <summary>
		/// A Vector3 value, which contains 3 components.
		/// </summary>
		[Size(3)] public const string VEC3 = "VEC3";

		/// <summary>
		/// A Vector4 value, which contains 4 components.
		/// </summary>
		[Size(4)] public const string VEC4 = "VEC4";

		/// <summary>
		/// A MAT2 value (2x2 transformation matrix), which contains 4 components.
		/// </summary>
		[Size(4)] public const string MAT2 = "MAT2";

		/// <summary>
		/// A MAT3 value (3x3 transformation matrix), which contains 9 components.
		/// </summary>
		[Size(9)] public const string MAT3 = "MAT3";

		/// <summary>
		/// A MAT4 value (4x4 transformation matrix), which contains 16 components.
		/// </summary>
		[Size(16)] public const string MAT4 = "MAT4";

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

	/// <summary>
	/// Represents what part of a node's transform a given <see cref="GLTFAnimationChannel"/> affects.
	/// </summary>
	public static class GLTFAnimationPath {

		/// <summary>
		/// Represents that this <see cref="GLTFAnimationChannel"/> affects the translation of the given node.
		/// </summary>
		public const string TRANSLATION = "translation";

		/// <summary>
		/// Represents that this <see cref="GLTFAnimationChannel"/> affects the rotation of the given node.
		/// </summary>
		public const string ROTATION = "rotation";

		/// <summary>
		/// Represents that this <see cref="GLTFAnimationChannel"/> affects the scale of the given node.
		/// </summary>
		public const string SCALE = "scale";

		/// <summary>
		/// Represents that this <see cref="GLTFAnimationChannel"/> affects the weights of the given node.
		/// </summary>
		public const string WEIGHTS = "weights";

	}
}
