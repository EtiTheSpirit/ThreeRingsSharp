using com.threerings.math;
using com.threerings.tudey.config;
using com.threerings.tudey.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.DataHandlers.Scene.Data;
using static com.threerings.tudey.data.TudeySceneModel;

namespace ThreeRingsSharp.XansData.Extensions {
	public static class TileEntryExtensions {
		/// <summary>
		/// Populates <paramref name="result"/> with the location data from the given <see cref="TileEntry"/> and <see cref="ShallowTileConfig"/> (instead of by trying to reference the original implementation)
		/// </summary>
		/// <param name="entry"></param>
		/// <param name="shallowTile"></param>
		/// <param name="result"></param>
		public static void GetTransformFromShallow(this TileEntry entry, ShallowTileConfig shallowTile, out Transform3D result) {
			result = new Transform3D();
			int x = entry.getLocation().x;
			int y = entry.getLocation().y;
			int w = entry.GetWidth(shallowTile);
			int h = entry.GetHeight(shallowTile);
			TudeySceneMetrics.getTileTransform(w, h, x, y, entry.elevation, entry.rotation, result);
		}

		/// <summary>
		/// Gets the width of this tile, but factors in the rotation of the tile too.
		/// </summary>
		/// <returns></returns>
		public static int GetWidth(this TileEntry entry, ShallowTileConfig shallowTile) => TudeySceneMetrics.getTileWidth(shallowTile.Width, shallowTile.Height, entry.rotation);

		/// <summary>
		/// Gets the height of this tile, but factors in the rotation of the tile too.
		/// </summary>
		/// <returns></returns>
		public static int GetHeight(this TileEntry entry, ShallowTileConfig shallowTile) => TudeySceneMetrics.getTileHeight(shallowTile.Width, shallowTile.Height, entry.rotation);

	}
}
