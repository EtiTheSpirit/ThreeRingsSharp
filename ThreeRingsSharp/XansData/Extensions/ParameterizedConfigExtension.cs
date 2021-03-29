using com.threerings.config;
using System;
using ThreeRingsSharp.DataHandlers.Parameters;
using ThreeRingsSharp.Utility;
using static ThreeRingsSharp.DataHandlers.Parameters.XDirect;

namespace ThreeRingsSharp.XansData.Extensions {

	/// <summary>
	/// Appends an extension to <see cref="ParameterizedConfig"/> that allows applying the provided <see cref="ArgumentMap"/> to it.
	/// </summary>
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
							XChoice cho = new XChoice(config, choice);
							foreach (XDirect dir in cho.Directs) {
								if (dir.Name == strKey) {
									dir.GetValuePointer().Value = args.getOrDefault(key, null);
								}
							}
						}
					} else {
						Parameter param = config.getParameter(strKey);
						if (param is Parameter.Direct direct) {
							XDirect dir = new XDirect(config, direct);
							object newValue = args.getOrDefault(key, null);
							DirectPointer ptr = dir.GetValuePointer();
							try {
								ptr.Value = newValue;
							} catch (Exception) {
								XanLogger.WriteLine($"A Direct [{ptr.DereferencedPath}] attempted to have its value set to {newValue}, but it failed! This data will not apply properly.");
							}
						}
					}
				}
			}
		}

	}
}
