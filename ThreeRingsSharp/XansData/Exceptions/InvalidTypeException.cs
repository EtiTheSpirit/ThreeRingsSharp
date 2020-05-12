using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.Exceptions {

	/// <summary>
	/// An exception thrown when a method that handles an arbitrary class type receives a type that is invalid for the given context (but is otherwise within constraints defined by the language)
	/// </summary>
	public class InvalidTypeException : Exception { 
		public InvalidTypeException() : base() { }

		public InvalidTypeException(string message) : base(message) { }
	}
}
