using com.threerings.expr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;

namespace ThreeRingsSharp.DataHandlers.AnimationHandlers.Expressions {
	public static class FloatExpressionHandler {

		public const float PI = 3.141592653f;

		/// <summary>
		/// Returns the sign of the sine wave (-1, 0, or 1)
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private static float Square(float value) {
			return Math.Sign(Math.Sin(value));
		}

		/// <summary>
		/// Uses absolute value of Saw to generate a triangle wave.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private static float Triangle(float value) {
			return Math.Abs(Saw(value));
		}

		/// <summary>
		/// Uses modulo to create a sawtooth wave with the same phase as a sine wave.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private static float Saw(float value) {
			return Math.Max(-1, Math.Min(1, value)) % PI;
		}
		public static float Compute(FloatExpression expression, int frameNumber) {

			if (expression is FloatExpression.Constant constExpr) {
				return constExpr.value;
			} else if (expression is FloatExpression.Add addExpr) {
				return Compute(addExpr.firstOperand, frameNumber) + Compute(addExpr.secondOperand, frameNumber);
			} else if (expression is FloatExpression.Subtract subExpr) {
				return Compute(subExpr.firstOperand, frameNumber) + Compute(subExpr.secondOperand, frameNumber);
			} else if (expression is FloatExpression.Multiply mulExpr) {
				return Compute(mulExpr.firstOperand, frameNumber) * Compute(mulExpr.secondOperand, frameNumber);
			} else if (expression is FloatExpression.Divide divExpr) {
				return Compute(divExpr.firstOperand, frameNumber) / Compute(divExpr.secondOperand, frameNumber);

			} else if (expression is FloatExpression.Clock clockExpr) {
				return frameNumber;
			} else if (expression is FloatExpression.Noise1 noise1Expr) {
				// Create lazy noise with a seed. I have no idea how this one works.
				Random RNG = new Random((int)(Compute(noise1Expr.operand, frameNumber) * 10000));
				return RNG.Next();
			} else if (expression is FloatExpression.Noise2 noise2Expr) {
				// Create lazy noise with a seed. I have no idea how this one works.
				Random RNG = new Random((int)(Compute(noise2Expr.firstOperand, frameNumber) * Compute(noise2Expr.secondOperand, frameNumber)));
				return RNG.Next();

			} else if (expression is FloatExpression.Cos cosineExpr) {
				return (float)Math.Cos(Compute(cosineExpr.operand, frameNumber));
			} else if (expression is FloatExpression.Sin sineExpr) {
				return (float)Math.Sin(Compute(sineExpr.operand, frameNumber));
			} else if (expression is FloatExpression.Square squareExpr) {
				return Square(Compute(squareExpr.operand, frameNumber));
			} else if (expression is FloatExpression.Triangle triExpr) {
				return Triangle(Compute(triExpr.operand, frameNumber));
			} else if (expression is FloatExpression.Saw sawExpr) {
				return Saw(Compute(sawExpr.operand, frameNumber));
			} else if (expression is FloatExpression.Tan tangentExpr) {
				return (float)Math.Tan(Compute(tangentExpr.operand, frameNumber));

			} else if (expression is FloatExpression.Exp exponentExpr) {
				return (float)Math.Exp(Compute(exponentExpr.operand, frameNumber));
			} else if (expression is FloatExpression.Negate negateExpr) {
				return Compute(negateExpr.operand, frameNumber) * -1;
			} else if (expression is FloatExpression.Pow powExpr) {
				return (float)Math.Pow(Compute(powExpr.firstOperand, frameNumber), Compute(powExpr.secondOperand, frameNumber));
			} else if (expression is FloatExpression.Remainder remainderExpr) {
				return (float)Math.IEEERemainder(Compute(remainderExpr.firstOperand, frameNumber), Compute(remainderExpr.secondOperand, frameNumber));
			}

			XanLogger.WriteLine($"Cannot parse expression type {expression.GetType().FullName} at this time.", XanLogger.TRACE);
			return 0;
		}
	}
}
