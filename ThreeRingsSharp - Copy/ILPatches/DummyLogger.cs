using com.samskivert.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.ILPatches {

	/// <summary>
	/// Implements <see cref="Logger"/> and provides a log that does nothing.
	/// </summary>
	public class DummyLogger : Logger {
		protected override void doLog(int i, string str, Exception t) { }

		protected override bool shouldLog(int i) => false;
	}
}
