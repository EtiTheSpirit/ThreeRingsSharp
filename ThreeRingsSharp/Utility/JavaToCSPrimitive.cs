using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.Logging {

	/// <summary>
	/// A utility class that translates between java classes and C# classes of various primitive types.
	/// </summary>
	public static class JavaToCSPrimitive {

		/// <summary>
		/// Given a java primitive type <strong>class instance</strong> (<see cref="java.lang.Boolean"/>, for instance) this will turn it into its C# primitive type equivalent.
		/// </summary>
		/// <remarks>
		/// Since <see cref="java.lang.String"/> is not classified as a primitive in the JVM, it will not be converted in this method.
		/// </remarks>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static object AsCSPrimitive(object obj) {
			if (obj is java.lang.Boolean @bool) {
				return @bool.booleanValue();
			} else if (obj is java.lang.Character @char) {
				return @char.charValue();
			} else if (obj is java.lang.Byte @byte) {
				return @byte.byteValue();
			} else if (obj is java.lang.Short @short) {
				return @short.shortValue();
			} else if (obj is java.lang.Integer @int) {
				return @int.intValue();
			} else if (obj is java.lang.Long @long) {
				return @long.longValue();
			} else if (obj is java.lang.Float @float) {
				return @float.floatValue();
			} else if (obj is java.lang.Double @double) {
				return @double.doubleValue();
			}
			return obj;
		}

		/// <summary>
		/// Given a C# primitive type, this will turn it into its Java primitive type.
		/// </summary>
		/// <remarks>
		/// Since <see cref="java.lang.String"/> is not classified as a primitive in the JVM, it will not be converted in this method.
		/// </remarks>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static object AsJavaPrimitive(object obj) {
			if (obj is bool @bool) {
				return new java.lang.Boolean(@bool);
			} else if (obj is char @char) {
				return new java.lang.Character(@char);
			} else if (obj is byte @byte) {
				return new java.lang.Byte(@byte);
			} else if (obj is short @short) {
				return new java.lang.Short(@short);
			} else if (obj is int @int) {
				return new java.lang.Integer(@int);
			} else if (obj is long @long) {
				return new java.lang.Long(@long);
			} else if (obj is float @float) {
				return new java.lang.Float(@float);
			} else if (obj is double @double) {
				return new java.lang.Double(@double);
			}
			return obj;
		}

	}
}
