using com.threerings.expr;
using com.threerings.math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;

namespace ThreeRingsSharp.DataHandlers.Expressions {

	/// <summary>
	/// A tool to assist in the parsing of <see cref="Transform3DExpression"/>.
	/// </summary>
	public static class Transform3DExpressionHandler {

		/// <summary>
		/// Given a <see cref="Transform3DExpression"/> and an optional frame number (if this expression is part of an animation), this will attempt to compute its value. 
		/// Most expression types are supported, but some are not.
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="keyframeNumber"></param>
		/// <returns></returns>
		public static Transform3D Compute(this Transform3DExpression expression, int keyframeNumber = 0) {
			if (expression is Transform3DExpression.Constant constantExpr) {
				return constantExpr.value;
			} else if (expression is Transform3DExpression.NonUniform nonUniformExpr) {

				Vector3f translation = nonUniformExpr.translation.Compute(keyframeNumber);
				Quaternion rotation = nonUniformExpr.rotation.Compute(keyframeNumber);
				Vector3f scale = nonUniformExpr.scale.Compute(keyframeNumber);

				return new Transform3D(translation, rotation, scale);
			} else if (expression is Transform3DExpression.Uniform uniformExpr) {
				Vector3f translation = uniformExpr.translation.Compute(keyframeNumber);
				Quaternion rotation = uniformExpr.rotation.Compute(keyframeNumber);
				float scale = uniformExpr.scale.Compute(keyframeNumber);

				return new Transform3D(translation, rotation, scale);
			}

			XanLogger.WriteLine($"Cannot parse expression type {expression.GetType().FullName} at this time.", XanLogger.TRACE);
			return new Transform3D();
		}
	}
}
