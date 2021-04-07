using com.threerings.expr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;

namespace ThreeRingsSharp.DataHandlers.Expressions {
	public static class BooleanExpressionHandler {

		/// <summary>
		/// Given a <see cref="BooleanExpression"/> and an optional frame number (if this expression is part of an animation), this will attempt to compute its value. 
		/// Most expression types are supported, but some are not.
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="frameNumber"></param>
		/// <returns></returns>
		public static bool Compute(this BooleanExpression expression, int frameNumber = 0) {
			if (expression is BooleanExpression.Constant constExpr) {
				return constExpr.value;
			} else if (expression is BooleanExpression.And andExpr) {
				bool a = andExpr.firstOperand.Compute(frameNumber);
				bool b = andExpr.secondOperand.Compute(frameNumber);
				return a && b;

			} else if (expression is BooleanExpression.Or orExpr) {
				bool a = orExpr.firstOperand.Compute(frameNumber);
				bool b = orExpr.secondOperand.Compute(frameNumber);
				return a || b;

			} else if (expression is BooleanExpression.Xor xorExpr) {
				bool a = xorExpr.firstOperand.Compute(frameNumber);
				bool b = xorExpr.secondOperand.Compute(frameNumber);
				return (a || b) && (a != b); // A or B, but not both.

			} else if (expression is BooleanExpression.Not notExpr) {
				return !notExpr.operand.Compute(frameNumber);
			} else if (expression is BooleanExpression.BooleanEquals eqExpr) {
				return eqExpr.firstOperand.Compute(frameNumber) == eqExpr.secondOperand.Compute(frameNumber);
			} else if (expression is BooleanExpression.StringEquals strExpr) {
				return strExpr.firstOperand.Compute(frameNumber) == strExpr.secondOperand.Compute(frameNumber);
			//} else if (expression is BooleanExpression.Parsed parsedExpr) {
			//} else if (expression is BooleanExpression.Reference refExpr) {
			} else if (expression is BooleanExpression.FloatEquals feqExpr) {
				return feqExpr.firstOperand.Compute(frameNumber) == feqExpr.secondOperand.Compute(frameNumber);

			} else if (expression is BooleanExpression.FloatGreater fgtExpr) {
				return fgtExpr.firstOperand.Compute(frameNumber) > fgtExpr.secondOperand.Compute(frameNumber);

			} else if (expression is BooleanExpression.FloatGreaterEquals fgeExpr) {
				return fgeExpr.firstOperand.Compute(frameNumber) >= fgeExpr.secondOperand.Compute(frameNumber);

			} else if (expression is BooleanExpression.FloatLess fltExpr) {
				return fltExpr.firstOperand.Compute(frameNumber) < fltExpr.secondOperand.Compute(frameNumber);

			} else if (expression is BooleanExpression.FloatLessEquals fleExpr) {
				return fleExpr.firstOperand.Compute(frameNumber) <= fleExpr.secondOperand.Compute(frameNumber);
			}

			XanLogger.WriteLine($"Cannot parse expression type {expression.GetType().FullName} at this time.", XanLogger.TRACE);
			return false;
		}

	}
}
