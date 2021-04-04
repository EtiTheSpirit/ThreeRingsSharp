using com.threerings.expr;
using com.threerings.math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;

namespace ThreeRingsSharp.DataHandlers.AnimationHandlers.Expressions {
	public static class Vector3fExpressionHandler {

		public static Vector3f Compute(Vector3fExpression expression, int frameNumber) {

			if (expression is Vector3fExpression.Constant constExpr) {
				return constExpr.value;
			} else if (expression is Vector3fExpression.Cartesian cartExpr) {
				return new Vector3f(
					FloatExpressionHandler.Compute(cartExpr.x, frameNumber),
					FloatExpressionHandler.Compute(cartExpr.y, frameNumber),
					FloatExpressionHandler.Compute(cartExpr.z, frameNumber)
				);
			}

			XanLogger.WriteLine($"Cannot parse expression type {expression.GetType().FullName} at this time.", XanLogger.TRACE);
			return Vector3f.ZERO;
		}

	}
}
