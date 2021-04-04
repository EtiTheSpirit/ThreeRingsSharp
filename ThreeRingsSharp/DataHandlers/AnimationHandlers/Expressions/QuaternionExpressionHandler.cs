using com.threerings.expr;
using com.threerings.math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;

namespace ThreeRingsSharp.DataHandlers.AnimationHandlers.Expressions {
	public static class QuaternionExpressionHandler {

		public static Quaternion Compute(QuaternionExpression expression, int frameNumber) {

			if (expression is QuaternionExpression.Constant constExpr) {
				return constExpr.value;
			} else if (expression is QuaternionExpression.Angles angleExpr) {
				return new Quaternion().fromAngles(
					FloatExpressionHandler.Compute(angleExpr.x, frameNumber),
					FloatExpressionHandler.Compute(angleExpr.y, frameNumber),
					FloatExpressionHandler.Compute(angleExpr.z, frameNumber)
				);
			}

			XanLogger.WriteLine($"Cannot parse expression type {expression.GetType().FullName} at this time.", XanLogger.TRACE);
			return Quaternion.IDENTITY;
		}

	}
}
