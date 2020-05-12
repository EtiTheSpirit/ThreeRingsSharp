using java.lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.XansData;

namespace ThreeRingsSharp.Utility {

	/// <summary>
	/// A utility dedicated to taking in Java classnames and breaking them down into parts.<para/>
	/// This also offers extension methods to string, for instance, 
	/// </summary>
	public static class ClassNameStripper {

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
		/// Returns a <see cref="string"/> that contains everything after the last index of <c>.</c>. This does not filter out subclasses (e.g. <c>MyClass0$MySubclass0</c>) and will return the string containing the $ and everything.<para/>
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

	}
}
