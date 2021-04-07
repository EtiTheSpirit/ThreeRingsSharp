using com.threerings.expr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;

namespace ThreeRingsSharp.DataHandlers.Expressions {
	public static class StringExpressionHandler {

		/// <summary>
		/// Given a <see cref="StringExpression"/> and an optional frame number (if this expression is part of an animation), this will attempt to compute its value. 
		/// Most expression types are supported, but some are not.
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="frameNumber"></param>
		/// <returns></returns>
		public static string Compute(this StringExpression expression, int frameNumber = 0) {
			if (expression is StringExpression.Constant constExpr) {
				return constExpr.value;
			} else if (expression is StringExpression.Parsed parsedExpr) {
				return parsedExpr.expression; // This is presumably verbatim. I need to verify this.
			}

			XanLogger.WriteLine($"Cannot parse expression type {expression.GetType().FullName} at this time.", XanLogger.TRACE);
			return string.Empty;
		}

	}
}
