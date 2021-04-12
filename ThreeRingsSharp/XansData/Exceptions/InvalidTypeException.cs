using System;

namespace ThreeRingsSharp.XansData.Exceptions {

	/// <summary>
	/// An exception thrown when a method that handles an arbitrary class type receives a type that is invalid for the given context (but is otherwise within constraints defined by the language)
	/// </summary>
	public class InvalidTypeException : Exception {

		/// <inheritdoc cref="InvalidTypeException"/>
		public InvalidTypeException() : base() { }

		/// <inheritdoc cref="InvalidTypeException"/>
		/// <param name="message">A message to include with the error.</param>
		public InvalidTypeException(string message) : base(message) { }
	}
}
