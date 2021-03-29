using com.threerings.expr;
using com.threerings.expr.util;
using java.lang;
using java.lang.reflect;
using java.util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ThreeRingsSharp.Utility {

	/// <summary>
	/// Provides an means of lazily implementing <see cref="Scope"/>.<para/>
	/// A <see cref="Scope"/> is basically a glorified reflection accessor.
	/// </summary>
	public class SimpleReferenceScope : Scope {

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
			try {
				return ScopeUtil.get(ReferenceObject, str ?? "", c);
			} catch { }
			return null;
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

		public SimpleReferenceScope() {
			ReferenceObject = null;
		}

		public SimpleReferenceScope(object reference) {
			ReferenceObject = reference;
		}

	}
}
