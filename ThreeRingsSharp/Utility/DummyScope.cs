using com.threerings.expr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using java.lang;
using com.threerings.opengl.model.config;
using ThreeRingsSharp.XansData;
using com.threerings.expr.util;

namespace ThreeRingsSharp.Utility {

	/// <summary>
	/// Provides an means of lazily implementing <see cref="Scope"/>.<para/>
	/// A <see cref="Scope"/> is basically a glorified reflection accessor.
	/// </summary>
	public class DummyScope : Scope {

		private readonly List<ScopeUpdateListener> UpdateListeners = new List<ScopeUpdateListener>();

		private readonly string InstantiationTime = DateTime.Now.ToBinary().ToString();
		
		/// <summary>
		/// The object that this <see cref="Scope"/> is pointing to.
		/// </summary>
		public object ReferenceObject { get; }

		public void addListener(ScopeUpdateListener sul) {
			UpdateListeners.Add(sul);
		}

		public object get(string str, Class c) {
			return ScopeUtil.get(ReferenceObject, str, c);
		}

		public Scope getParentScope() {
			return null;
		}

		public string getScopeName() {
			return "DummyScope-" + InstantiationTime;
		}

		public void removeListener(ScopeUpdateListener sul) {
			UpdateListeners.Remove(sul);
		}

		public DummyScope() {
			ReferenceObject = null;
		}

		public DummyScope(object reference) {
			ReferenceObject = reference;
		}

	}
}
