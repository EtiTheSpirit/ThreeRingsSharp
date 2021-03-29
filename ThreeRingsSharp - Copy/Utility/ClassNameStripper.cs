using java.lang;
using System.Linq;
using ThreeRingsSharp.XansData.Extensions;

namespace ThreeRingsSharp.Utility {

	/// <summary>
	/// A utility dedicated to taking in Java classnames and breaking them down into parts.
	/// </summary>
	public static class JavaClassNameStripper {

		/// <summary>
		/// Returns a <see cref="string"/> array of the class name. This array always has a very specific structure:<para/>
		///	[0]=Class Name<para/>
		///	[1]=Subclass Name<para/>
		///	If a class does not have a subclass (denoted with a $ in Java classnames), the array will have a Length of 1. Returns <see langword="null"/> if the string does not have a locatable instance of '<c>.</c>'
		/// </summary>
		/// <param name="cls">The <see cref="Class"/> to get the name of.</param>
		/// <returns></returns>
		public static string[] GetSplitClassName(Class cls) => GetSplitClassName(cls.getTypeName());

		/// <summary>
		/// Returns a <see cref="string"/> that contains everything after the last index of <c>.</c> (this cuts off the package, namely). This does not filter out subclasses (e.g. <c>MyClass0$MySubclass0</c>) and will return the string containing the $ and everything.<para/>
		/// Returns <see langword="null"/> if the string does not have a locatable instance of '<c>.</c>'
		/// </summary>
		/// <param name="cls">The <see cref="Class"/> to get the name of.</param>
		/// <returns></returns>
		public static string GetWholeClassName(Class cls) => GetWholeClassName(cls.getTypeName());

		/// <summary>
		/// An alias method that calls <see cref="GetSplitClassName(string)"/> and returns the first index (the base class name), or <see langword="null"/> if '<c>.</c>' could not be found in the <see cref="string"/>.
		/// </summary>
		/// <param name="cls">The <see cref="Class"/> to get the name of.</param>
		/// <returns></returns>
		public static string GetBaseClassName(Class cls) => GetBaseClassName(cls.getTypeName());

		/// <summary>
		/// Returns a <see cref="string"/> array of the class name. This array always has a very specific structure:<para/>
		///	[0]=Class Name<para/>
		///	[1]=Subclass Name<para/>
		///	If a class does not have a subclass (denoted with a $ in Java classnames), the array will have a Length of 1. Returns <see langword="null"/> if the string does not have a locatable instance of '<c>.</c>'
		/// </summary>
		/// <param name="className">The classname to parse.</param>
		/// <returns></returns>
		public static string[] GetSplitClassName(string className) {
			string clipped = className.AfterLastIndexOf(".");
			if (clipped == null) return null;

			string[] retn;
			if (clipped.Contains('$')) {
				retn = clipped.Split('$');
			} else {
				retn = new string[] { clipped };
			}
			return retn;
		}

		/// <summary>
		/// Returns a <see cref="string"/> that contains everything after the last index of <c>.</c>. This does not filter out subclasses (e.g. <c>MyClass0$MySubclass0</c>) and will return the string containing the $ and everything.<para/>
		/// Returns <see langword="null"/> if the string does not have a locatable instance of '<c>.</c>'
		/// </summary>
		/// <param name="className">The classname to parse.</param>
		/// <returns></returns>
		public static string GetWholeClassName(string className) => className.AfterLastIndexOf(".");

		/// <summary>
		/// An alias method that calls <see cref="GetSplitClassName(string)"/> and returns the first index (the base class name), or <see langword="null"/> if '<c>.</c>' could not be found in the <see cref="string"/>.
		/// </summary>
		/// <param name="className">The classname to parse.</param>
		/// <returns></returns>
		public static string GetBaseClassName(string className) {
			string[] cls = GetSplitClassName(className);
			if (cls == null) return null;
			return cls[0];
		}

		/// <summary>
		/// Given a classname with signatures included, this will remove the signature.<para/>
		/// This only works on classes that extend <see cref="java.lang.Object"/>, NOT primitive types!
		/// </summary>
		/// <param name="className"></param>
		/// <returns></returns>
		public static string RemoveSignature(string className) {
			while (className.First() == '[') {
				// This assumes we aren't using primitives. This will brick under primitives.
				// Objects use Lsomething.something.classhere
				// Arrays use [ at the start, multidimensional arrays use multiple [s.
				// Primitives are just a letter (e.g. B for bool) and that's why it'll brick, because
				// [B; will be used to denote an array of bools. No classname there!
				className = className.Substring(1);
			}
			className = className.Substring(1);

			return className.Substring(0, className.Length - 1); // Get rid of the trailing ;
		}

	}
}
