using com.threerings.config;
using com.threerings.opengl.model.config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.DataHandlers.Properties;
using ThreeRingsSharp.Utility;

namespace ThreeRingsSharp.XansData.Extensions {
	public static class ParameterizedConfigExtension {

		/// <summary>
		/// Applies arguments to a <see cref="ParameterizedConfig"/>. This <see cref="ParameterizedConfig"/> is expected to have come from a <see cref="ConfigReference"/>. This same <see cref="ConfigReference"/> is expected to be the source of <paramref name="args"/>.
		/// </summary>
		/// <param name="config"></param>
		/// <param name="args"></param>
		/// <param name="parentChoiceName">If non-null, this will traverse into a parameter with this name (which is presumably a Choice) and access the choice's directs to apply the args.</param>
		public static void ApplyArguments(this ParameterizedConfig config, ArgumentMap args, string parentChoiceName = null) {
			object[] keys = args.keySet().toArray();
			foreach (object key in keys) {
				if (key is string strKey) {
					//object impl = ReflectionHelper.Get(config, "implementation"); // TODO: Is this OK to use?
					if (parentChoiceName != null) {
						if (config.getParameter(parentChoiceName) is Parameter.Choice choice) {
							foreach (Parameter.Direct choiceDirect in choice.directs) {
								if (choiceDirect.name == strKey) {
									WrappedDirect.SetDataOn(config, choiceDirect.paths, args.get(key));
								}
							}
						}
					} else {
						Parameter param = config.getParameter(strKey);
						if (param is Parameter.Direct direct) {
							WrappedDirect.SetDataOn(config, direct.paths, args.get(key));
						}
					}
				}
			}
		}

	}
}
