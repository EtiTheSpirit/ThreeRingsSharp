using com.samskivert.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using java.lang;

namespace ThreeRingsSharp.ILPatches {
	public class DummyLoggerFactory : Logger.Factory {
		public Logger getLogger(string str) {
			return new DummyLogger();
		}

		public Logger getLogger(Class c) {
			return new DummyLogger();
		}

		public void init() { }
	}
}
