using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.threerings.util;

namespace ThreeRingsSharp.XansData.Extensions {

	/// <summary>
	/// Extends <see cref="DeepObject"/> to add a typed Clone method.
	/// </summary>
	public static class DeepObjectExtensions {

		/// <summary>
		/// Clones this <see cref="DeepObject"/> into a new instance of the given type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="original"></param>
		/// <returns></returns>
		public static T Clone<T>(this T original) where T : DeepObject => original.clone() as T;

		/// <summary>
		/// Clones this <see cref="DeepObject"/> into a new instance of the given type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="original"></param>
		/// <returns></returns>
		public static T CloneAs<T>(this DeepObject original) where T : DeepObject {
			return original.clone() as T;
		}
		

	}
}
