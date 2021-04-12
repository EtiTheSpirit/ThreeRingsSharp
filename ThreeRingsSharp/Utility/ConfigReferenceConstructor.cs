using com.threerings.config;
using ThreeRingsSharp.DataHandlers.Parameters;
using ThreeRingsSharp.XansData.Extensions;

namespace ThreeRingsSharp.Utility {

	/// <summary>
	/// A utility to create <see cref="ConfigReference"/> instances.
	/// </summary>
	public static class ConfigReferenceConstructor {


		/// <summary>
		/// Creates a new <see cref="ConfigReference"/> pointing to the given <see cref="ParameterizedConfig"/> and automatically populates the <see cref="ConfigReference"/>'s arguments with the data defined by the <see cref="ParameterizedConfig"/>.
		/// </summary>
		/// <param name="cfg"></param>
		/// <returns></returns>
		public static ConfigReference MakeConfigReferenceTo(ParameterizedConfig cfg) {
			ArgumentMap args = new ArgumentMap();
			foreach (Parameter param in cfg.parameters) {
				if (param is Parameter.Direct direct) {
					//WrappedDirect wDir = new WrappedDirect(cfg, direct);
					XDirect dir = new XDirect(cfg, direct);
					args.put(param.name, dir.Value);
				}
			}

			return NewConfigReference(cfg.getName(), args);
		}

		/// <summary>
		/// Creates a new <see cref="ConfigReference"/> from the given name and <see cref="ArgumentMap"/><para/>
		/// For some reason, OOO doesn't offer a constructor to ConfigReference that takes in an <see cref="ArgumentMap"/>.<para/>
		/// ...And then they made all of the constructors that *can* populate args complete aids. I need to buy a punching bag.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static ConfigReference NewConfigReference(string name, ArgumentMap args) {
			ConfigReference newCfg = new ConfigReference(name);
			newCfg.SetArguments(args);
			return newCfg;
		}

	}
}
