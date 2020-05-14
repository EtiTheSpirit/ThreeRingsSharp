using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.Exceptions {

	/// <summary>
	/// An exception that is thrown when a critical read error occurs when trying to process files.
	/// </summary>
	public class ClydeDataReadException : Exception {
		public ClydeDataReadException() : base() { }

		public ClydeDataReadException(string message) : base(message) { }
	}
}
