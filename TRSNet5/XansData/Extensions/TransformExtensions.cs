using System;
using OOOReader.Utility.Mathematics;
using ThreeRingsSharp.XansData.Structs;

namespace ThreeRingsSharp.XansData.Extensions {
	public static class TransformExtensions {

		/// <summary>
		/// Returns the translation, rotation, and scale (in this order) of this <see cref="Transform3D"/> as float arrays.<para/>
		/// <list type="number">
		/// <item>
		/// <term>float Translation[3]</term>
		/// <description>{ x, y, z }</description>
		/// </item>
		/// <item>
		/// <term>float Quaternion[4]</term>
		/// <description>{ x, y, z, w }</description>
		/// </item>
		/// <item>
		/// <term>float Scale[3]</term>
		/// <description>{ x, y, z }</description>
		/// </item>
		/// </list>
		/// </summary>
		/// <param name="trs">The <see cref="Transform3DRef"/> to extract the components from.</param>
		/// <returns></returns>
		public static (float[], float[], float[]) GetAllComponents(this Transform3D trs) {
			Vector3f translation = trs.Translation;
			Quaternion rotation = trs.Rotation;
			Vector3f scale = trs.Scale;
			return (
				new float[] { translation.X, translation.Y, translation.Z },
				new float[] { rotation.X, rotation.Y, rotation.Z, rotation.W },
				new float[] { scale.X, scale.Y, scale.Z }
			);
		}

		/// <summary>
		/// Returns the components of this <see cref="Transform3D"/> in column-major order.
		/// </summary>
		/// <returns></returns>
		[Obsolete("For glTF writing, use the Write extension.")]
		public static float[] GetMatrixComponents(this Transform3D trs) => GetMatrixComponents(trs.Matrix);

		/// <summary>
		/// Returns the components of this <see cref="Matrix4f"/> in column-major order.
		/// </summary>
		/// <returns></returns>
		[Obsolete("For glTF writing, use the Write extension.")]
		public static float[] GetMatrixComponents(this Matrix4f mtx) {
			/*
			// Row Major
			return new float[] {
				mtx.m00, mtx.m10, mtx.m20, mtx.m30,
				mtx.m01, mtx.m11, mtx.m21, mtx.m31,
				mtx.m02, mtx.m12, mtx.m22, mtx.m32,
				mtx.m03, mtx.m13, mtx.m23, mtx.m33
			};
			*/
			return new float[] {
				mtx.M00, mtx.M01, mtx.M02, mtx.M03,
				mtx.M10, mtx.M11, mtx.M12, mtx.M13,
				mtx.M20, mtx.M21, mtx.M22, mtx.M23,
				mtx.M30, mtx.M31, mtx.M32, mtx.M33
			};
		}
	}
}
