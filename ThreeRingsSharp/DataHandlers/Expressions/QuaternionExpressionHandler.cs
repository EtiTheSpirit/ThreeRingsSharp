using com.threerings.expr;
using com.threerings.math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;

namespace ThreeRingsSharp.DataHandlers.Expressions {
	public static class QuaternionExpressionHandler {

		/// <summary>
		/// Given a <see cref="Quaternion"/> and an optional frame number (if this expression is part of an animation), this will attempt to compute its value. 
		/// Most expression types are supported, but some are not.
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="frameNumber"></param>
		/// <returns></returns>
		public static Quaternion Compute(this QuaternionExpression expression, int frameNumber = 0) {

			if (expression is QuaternionExpression.Constant constExpr) {
				return constExpr.value;
			} else if (expression is QuaternionExpression.Angles angleExpr) {
				return new Quaternion().fromAngles(
					angleExpr.x.Compute(frameNumber),
					angleExpr.y.Compute(frameNumber),
					angleExpr.z.Compute(frameNumber)
				);
			}

			XanLogger.WriteLine($"Cannot parse expression type {expression.GetType().FullName} at this time.", XanLogger.TRACE);
			return Quaternion.IDENTITY;
		}

	}
}
