using com.threerings.config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.DataHandlers.Properties;
using static ThreeRingsSharp.DataHandlers.Properties.WrappedDirect;

namespace ThreeRingsSharp.Utility {
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
					WrappedDirect wDir = new WrappedDirect(cfg, direct);
					args.put(param.name, wDir.GetValue());
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
			// Me @ OOO for making me write this method: https://www.youtube.com/watch?v=y2weNM4JtME

			string firstKey = null;
			object firstValue = null;
			List<object> subsequentValues = new List<object>();

			object[] keys = args.keySet().toArray();
			foreach (object key in keys) {
				if (firstKey == null) {
					firstKey = (string)key;
					firstValue = args.get(key);
				} else {
					subsequentValues.AddRange(new object[] { key, args.get(key) });
				}
			}

			return new ConfigReference(name, firstKey, firstValue, subsequentValues.ToArray());
		}

	}
}
