using com.threerings.config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.Utility {
	public class ReferenceRoot {

		private string Name = null;

		public string getName() => Name;

		public ReferenceRoot(string name) => Name = name;

		public static implicit operator ReferenceRoot(ManagedConfig cfg) => new ReferenceRoot(cfg.getName());

		public static implicit operator ReferenceRoot(ParameterizedConfig cfg) => new ReferenceRoot(cfg.getName());

		public static implicit operator ReferenceRoot(ConfigReference cfg) => new ReferenceRoot(cfg.getName());

	}
}
