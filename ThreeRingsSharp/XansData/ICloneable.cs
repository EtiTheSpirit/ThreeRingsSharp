using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData {

	/// <summary>
	/// Represents an <see langword="object"/> or <see langword="struct"/> that can be cloned into a new instance with all-new data.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ICloneable<T> {

		/// <summary>
		/// Clone this object into a new instance of the same type where all contained data is completely separate from the original object.
		/// </summary>
		/// <returns></returns>
		T Clone();

	}
}
