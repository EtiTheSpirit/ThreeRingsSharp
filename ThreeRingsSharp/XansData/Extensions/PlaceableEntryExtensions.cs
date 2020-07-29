using com.threerings.math;
using com.threerings.tudey.config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static com.threerings.tudey.data.TudeySceneModel;

namespace ThreeRingsSharp.XansData.Extensions {
	public static class PlaceableEntryExtensions {

		/// <summary>
		/// Returns a <see cref="Transform3D"/> designed for export from TR#.
		/// </summary>
		/// <param name="entry"></param>
		/// <param name="originalImpl"></param>
		/// <param name="result"></param>
		public static void GetExportTransform(this PlaceableEntry entry, PlaceableConfig.Original originalImpl, out Transform3D result) {
			throw new NotImplementedException();
		}

	}
}
