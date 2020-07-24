using com.threerings.expr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using java.lang;
using com.threerings.opengl.model.config;
using ThreeRingsSharp.XansData;

namespace ThreeRingsSharp.Utility {

	/// <summary>
	/// Used for the parsing of <see cref="ConditionalConfig"/>s, this provides an object implementing <see cref="Scope"/> which returns null data.
	/// </summary>
	public class DummyScope : Singleton<DummyScope>, Scope {

		public void addListener(ScopeUpdateListener sul) { }

		public object get(string str, Class c) {
			return null;
		}

		public Scope getParentScope() {
			return null;
		}

		public string getScopeName() {
			return string.Empty;
		}

		public void removeListener(ScopeUpdateListener sul) { }
	}
}
