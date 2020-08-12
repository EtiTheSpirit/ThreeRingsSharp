﻿using com.threerings.math;
using System;
using ThreeRingsSharp.XansData.Structs;

namespace ThreeRingsSharp.XansData.Extensions {
	public static class TransformExtensions {

		/// <summary>
		/// Returns all components of this transform in the order of <c>translation, rotation, scale (as Vector3f), scale (as float)</c>.
		/// </summary>
		/// <param name="trs"></param>
		/// <returns></returns>
		public static (Vector3f, Quaternion, Vector3f) GetAllTransforms(this Transform3D trs) {
			// Translation will never fail, it directoy references m30, m31, and m32.
			Vector3f translation = trs.extractTranslation();

			// Neither will scale.
			Vector3f scale = trs.extractScale();

			// Rotation is the odd one out that will throw an exception.
			Quaternion rotation;
			try {
				rotation = trs.extractRotation(); // Try to get it out of the matrix (if applicable)
			} catch (SingularMatrixException) {
				// Give up if the rotation can't be extracted this way, just use the old value.
				rotation = trs.getRotation();
			}

			return (translation ?? new Vector3f(), rotation ?? Quaternion.IDENTITY, scale ?? new Vector3f(trs.getScale(), trs.getScale(), trs.getScale()));
		}

		/// <summary>
		/// Returns the components of this <see cref="Transform3D"/>'s matrix in column-major order. If it does not have a matrix, one will be created via the promotion of a duplicate <see cref="Transform3D"/>.
		/// </summary>
		/// <returns></returns>
		public static float[] GetMatrixComponents(this Transform3D trs) {
			return trs.Clone().promote(Transform3D.GENERAL).getMatrix().GetMatrixComponents();
		}


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
		/// <param name="trs">The <see cref="Transform3D"/> to extract the components from.</param>
		/// <returns></returns>
		public static (float[], float[], float[]) GetAllComponents(this Transform3D trs) {
			Vector3f translation = trs.extractTranslation() ?? Vector3f.ZERO;
			Quaternion rotation = trs.extractRotation() ?? Quaternion.IDENTITY;
			Vector3f scale = trs.extractScale() ?? new Vector3f(trs.getScale(), trs.getScale(), trs.getScale());
			return (
				new float[] { translation.x, translation.y, translation.z },
				new float[] { rotation.x, rotation.y, rotation.z, rotation.w },
				new float[] { scale.x, scale.y, scale.z }
			);
		}

		/// <summary>
		/// Returns the components of this <see cref="Matrix4f"/> in column-major order.
		/// </summary>
		/// <returns></returns>
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
				mtx.m00, mtx.m01, mtx.m02, mtx.m03,
				mtx.m10, mtx.m11, mtx.m12, mtx.m13,
				mtx.m20, mtx.m21, mtx.m22, mtx.m23,
				mtx.m30, mtx.m31, mtx.m32, mtx.m33
			};
		}

		/// <summary>
		/// Rotates the given <see cref="Transform3D"/> on the given <paramref name="axis"/> by the given <paramref name="rotation"/> (which is in radians)<para/>
		/// Returns the same <see cref="Transform3D"/> that this was called on for ease in chaining.
		/// </summary>
		/// <param name="trs">The translation to alter.</param>
		/// <param name="axis">The axis to rotate on.</param>
		/// <param name="rotation">The amount of radians to rotate.</param>
		/// <returns></returns>
		public static Transform3D RotateOnAxis(this Transform3D trs, Axis axis, float rotation) {
			return trs.composeLocal(new Transform3D(new Vector3f(), new Quaternion().fromAngleAxis(rotation, Vector3.FromAxis(axis))));
		}

		/// <summary>
		/// Rotates the given <see cref="Transform3D"/> on the given <paramref name="axis"/> by the given <paramref name="rotation"/> (which is in degrees)<para/>
		/// Returns the same <see cref="Transform3D"/> that this was called on for ease in chaining.
		/// </summary>
		/// <param name="trs">The translation to alter.</param>
		/// <param name="axis">The axis to rotate on.</param>
		/// <param name="rotation">The amount of radians to rotate.</param>
		/// <returns></returns>
		public static Transform3D RotateOnAxisDegrees(this Transform3D trs, Axis axis, float rotation) => RotateOnAxis(trs, axis, (float)(Math.PI / 180) * rotation);

		/// <summary>
		/// Clones this <see cref="Transform3D"/> into a new instance with the same data.
		/// </summary>
		/// <param name="trs"></param>
		/// <returns></returns>
		public static Transform3D Clone(this Transform3D trs) => new Transform3D(trs);

		/// <summary>
		/// Returns a new <see cref="Quaternion"/> which is <paramref name="quat"/> but rotated so that 
		/// </summary>
		/// <param name="quat"></param>
		/// <param name="targetUpAxis"></param>
		/// <returns></returns>
		public static Quaternion RotateToUpAxis(this Quaternion quat, Axis targetUpAxis) {
			if (targetUpAxis == Axis.NegativeY) return quat.mult(new Quaternion().fromAngleAxis((float)Math.PI, new Vector3f(0, 0, 1)));

			if (targetUpAxis == Axis.PositiveX) return quat.mult(new Quaternion().fromAngleAxis(-(float)Math.PI / 2f, new Vector3f(0, 0, 1)));
			if (targetUpAxis == Axis.NegativeX) return quat.mult(new Quaternion().fromAngleAxis((float)Math.PI / 2f, new Vector3f(0, 0, 1)));

			if (targetUpAxis == Axis.PositiveZ) return quat.mult(new Quaternion().fromAngleAxis(-(float)Math.PI / 2f, new Vector3f(1, 0, 0)));
			if (targetUpAxis == Axis.NegativeZ) return quat.mult(new Quaternion().fromAngleAxis((float)Math.PI / 2f, new Vector3f(1, 0, 0)));

			return new Quaternion(quat); // pos y and default
		}

	}
}
