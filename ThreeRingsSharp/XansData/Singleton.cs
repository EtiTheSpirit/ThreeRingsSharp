using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData {

	/// <summary>
	/// When extended, a property named <c>Instance</c> is statically provided for the given type that references an instance of said type.
	/// </summary>
	/// <typeparam name="T">The type of the singleton instance.</typeparam>
	public abstract class Singleton<T> where T : new() {

		/// <summary>
		/// A reference to the singleton instance of this class.
		/// </summary>
		public static T Instance { get; } = new T();
		// Note to self: Use { get; } = instance, NOT => instance. Lambda expressions call it every time, {get;} caches the value in this case.

	}
}
