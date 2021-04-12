using com.threerings.expr;
using java.io;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;

namespace ThreeRingsSharp.DataHandlers.Expressions {
	public class ExpressionParserProxy {

		/// <summary>
		/// Given an expression type <typeparamref name="TExpr"/> and its own parsed expression type <typeparamref name="TParsed"/>, this will attempt to index a field named <c>expression</c>, and then call the static method <c>parseExpression</c> of <typeparamref name="TExpr"/>.
		/// </summary>
		/// <typeparam name="TExpr"></typeparam>
		/// <typeparam name="TParsed"></typeparam>
		/// <param name="parsedExpr"></param>
		/// <returns></returns>
		public static TExpr GetExpression<TExpr, TParsed>(TParsed parsedExpr) where TParsed : TExpr {
			// This is a really terrible hack in general, but I'm at the point where I'm not too bothered.
			// This is scalable, for one, and while using reflection is a cardinal sin, the method is protected anyway. I could do an IL edit.
			// Meh.
			string expression = (string)ReflectionHelper.Get(parsedExpr, "expression");
			return (TExpr)ReflectionHelper.Call(typeof(TExpr), "parseExpression", null, expression);
		}

		public static float Parse(FloatExpression.Parsed parsedExpr) {
			return GetExpression<FloatExpression, FloatExpression.Parsed>(parsedExpr).Compute();
		}

		public static bool Parse(BooleanExpression.Parsed parsedExpr) {
			return GetExpression<BooleanExpression, BooleanExpression.Parsed>(parsedExpr).Compute();
		}

		public static string Parse(StringExpression.Parsed parsedExpr) {
			return GetExpression<StringExpression, StringExpression.Parsed>(parsedExpr).Compute();
		}

	}
}
