using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData {

	/// <summary>
	/// Provides methods that are nice for <see cref="List{T}"/>
	/// </summary>
	public static class EnumerableExtension {

		/// <summary>
		/// Sets the contents of the given <see cref="List{T}"/> to the given content.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="values"></param>
		public static void SetTo<T>(this List<T> list, IEnumerable<T> values) {
			list.Clear();
			list.Capacity = values.Count();
			for (int idx = 0; idx < values.Count(); idx++) {
				list.Add(values.ElementAt(idx));
			}
		}

	}
}
