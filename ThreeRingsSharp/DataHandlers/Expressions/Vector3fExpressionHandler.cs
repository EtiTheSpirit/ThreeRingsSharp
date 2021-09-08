using com.threerings.expr;
using com.threerings.math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Logging;

namespace ThreeRingsSharp.DataHandlers.Expressions {
	public static class Vector3fExpressionHandler {

		/// <summary>
		/// Given a <see cref="Vector3fExpression"/> and an optional frame number (if this expression is part of an animation), this will attempt to compute its value. 
		/// Most expression types are supported, but some are not.
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="frameNumber"></param>
		/// <returns></returns>
		public static Vector3f Compute(this Vector3fExpression expression, int frameNumber = 0) {

			if (expression is Vector3fExpression.Constant constExpr) {
				return constExpr.value;
			} else if (expression is Vector3fExpression.Cartesian cartExpr) {
				return new Vector3f(
					cartExpr.x.Compute(frameNumber),
					cartExpr.y.Compute(frameNumber),
					cartExpr.z.Compute(frameNumber)
				);		
			}

			XanLogger.WriteLine($"Cannot parse expression type {expression.GetType().FullName} at this time.", XanLogger.TRACE);
			return Vector3f.ZERO;
		}

	}
}
