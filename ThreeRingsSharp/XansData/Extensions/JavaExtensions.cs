using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.Extensions {

	/// <summary>
	/// Utilities go to between <see cref="Type"/> and <see cref="java.lang.Class"/>
	/// </summary>
	public static class JavaExtensions {

		private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// Equivalent to java's <c>System.currentTimeMillis</c>
		/// </summary>
		public static long CurrentTimeMillis => (long)(DateTime.UtcNow - Epoch).TotalMilliseconds;

		private static readonly Dictionary<java.lang.Class, Type> EquivalentCache = new Dictionary<java.lang.Class, Type>();

		/// <summary>
		/// Returns the C# <see cref="Type"/> that represents this <paramref name="class"/>.
		/// </summary>
		/// <param name="class"></param>
		/// <returns></returns>
		public static Type EquivalentType(this java.lang.Class @class) {
			if (EquivalentCache.ContainsKey(@class)) {
				return EquivalentCache[@class];
			}


			//Type.GetType()
			string className = @class.getCanonicalName();
			// Different from C#: inner classes still use a dot.
			// In java's raw form, a $ is used.
			// In C#, a + is used.
			// Now this is fine and all, issue is I have to do two searches.
			Type t = Type.GetType(className);
			if (t == null) {
				// Okay, null. Try this.
				if (className.Contains(".")) {
					int lastPeriodIndex = className.LastIndexOf('.');
					string innerName = className.Substring(lastPeriodIndex + 1);
					className = className.Substring(0, lastPeriodIndex);
					t = Type.GetType(className + "+" + innerName);
				}
			}
			EquivalentCache[@class] = t;
			return t;
		}

		/// <summary>
		/// Returns <see langword="true"/> if this class is equivalent to the given C# type, and <see langword="false"/> if it is not.<para/>
		/// This should be used when the instance is a java type via some form of reflection or is passed in as a literal class.
		/// </summary>
		/// <param name="javaClass"></param>
		/// <param name="objectType"></param>
		/// <returns></returns>
		public static bool IsA(this java.lang.Class javaClass, Type objectType) {
			return javaClass.EquivalentType()?.Equals(objectType) ?? false;
		}

		/// <summary>
		/// Returns <see langword="true"/> if this object's type is equivalent to the given class as a C# type, and <see langword="false"/> if they are not.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="classEquivalent"></param>
		/// <returns></returns>
		public static bool IsA(this object obj, java.lang.Class classEquivalent) {
			Type objectType = obj.GetType();
			Type eqType = classEquivalent.EquivalentType();
			return objectType.Equals(eqType);
		}

	}
}
