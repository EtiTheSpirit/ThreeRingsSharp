using com.threerings.expr;
using com.threerings.math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;

namespace ThreeRingsSharp.DataHandlers.AnimationHandlers.Expressions {

	/// <summary>
	/// A tool to assist in the parsing of <see cref="Transform3DExpression"/>.
	/// </summary>
	public static class Transform3DExpressionHandler {

		public static Transform3D Compute(Transform3DExpression expression, int keyframeNumber) {
			if (expression is Transform3DExpression.Constant constantExpr) {
				return constantExpr.value;
			} else if (expression is Transform3DExpression.NonUniform nonUniformExpr) {

				Vector3f translation = Vector3fExpressionHandler.Compute(nonUniformExpr.translation, keyframeNumber);
				Quaternion rotation = QuaternionExpressionHandler.Compute(nonUniformExpr.rotation, keyframeNumber);
				Vector3f scale = Vector3fExpressionHandler.Compute(nonUniformExpr.scale, keyframeNumber);

				return new Transform3D(translation, rotation, scale);
			} else if (expression is Transform3DExpression.Uniform uniformExpr) {
				Vector3f translation = Vector3fExpressionHandler.Compute(uniformExpr.translation, keyframeNumber);
				Quaternion rotation = QuaternionExpressionHandler.Compute(uniformExpr.rotation, keyframeNumber);
				float scale = FloatExpressionHandler.Compute(uniformExpr.scale, keyframeNumber);

				return new Transform3D(translation, rotation, scale);
			}

			XanLogger.WriteLine($"Cannot parse expression type {expression.GetType().FullName} at this time.", XanLogger.TRACE);
			return new Transform3D();
		}
	}
}
