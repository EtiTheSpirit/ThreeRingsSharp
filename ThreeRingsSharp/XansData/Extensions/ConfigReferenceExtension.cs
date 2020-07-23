using com.google.inject;
using com.threerings.config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;

namespace ThreeRingsSharp.XansData.Extensions {
	public static class ConfigReferenceExtension {

		/// <summary>
		/// Returns <see langword="true"/> if this <see cref="ConfigReference"/> points to an actual config, and <see langword="false"/> if it does not (for instance, it points to a model file instead).
		/// </summary>
		/// <param name="cfgRef"></param>
		/// <returns></returns>
		public static bool IsRealReference(this ConfigReference cfgRef) {
			return !File.Exists(ResourceDirectoryGrabber.ResourceDirectoryPath + cfgRef.getName());
		}

		/// <summary>
		/// Sets the arguments of the given <see cref="ConfigReference"/>.
		/// </summary>
		/// <param name="cfgRef"></param>
		/// <param name="args"></param>
		public static void SetArguments(this ConfigReference cfgRef, ArgumentMap args) {
			ArgumentMap defArgs = cfgRef.getArguments();
			defArgs.clear();
			object[] keys = args.keySet().toArray();
			foreach (object key in keys) {
				defArgs.put(key, args.get(key));
			}
		}

	}
}
