using com.threerings.config;

namespace ThreeRingsSharp.Logging {
	public class ReferenceRoot {

		private string Name = null;

		public string getName() => Name;

		public ReferenceRoot(string name) => Name = name;

		public static implicit operator ReferenceRoot(ManagedConfig cfg) => new ReferenceRoot(cfg.getName());

		public static implicit operator ReferenceRoot(ParameterizedConfig cfg) => new ReferenceRoot(cfg.getName());

		public static implicit operator ReferenceRoot(ConfigReference cfg) => new ReferenceRoot(cfg.getName());

	}
}
