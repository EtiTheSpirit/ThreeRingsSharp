namespace ThreeRingsSharp.XansData {

	/// <summary>
	/// Represents an <see langword="object"/> or <see langword="struct"/> that can be cloned into a new instance.
	/// </summary>
	/// <typeparam name="T">The type of the object that will be returned by the <see cref="Clone"/> method.</typeparam>
	public interface ICloneable<T> {

		/// <summary>
		/// Clone this object into a new instance of the same type. All of the data in the new object will be identical to the original object, but it will be under a different reference.
		/// </summary>
		/// <returns></returns>
		T Clone();

	}
}
