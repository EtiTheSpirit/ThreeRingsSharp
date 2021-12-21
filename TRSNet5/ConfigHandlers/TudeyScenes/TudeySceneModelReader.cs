using OOOReader.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utilities;

namespace ThreeRingsSharp.ConfigHandlers.TudeyScenes {
	public static class TudeySceneModelReader {

		// we getting into big boi territory
		// L A R G E   B O I   T E R R I T O R Y
		// H  O  L  Y     S  H  I  T

		

		public static void ReadData(ReadFileContext ctx, ShadowClass tudeySceneModel) {
			tudeySceneModel.AssertIsInstanceOf("com.threerings.tudey.data.TudeySceneModel");
			ShadowClass coordIntMapTiles = tudeySceneModel["_tiles"]!;
			CoordIntMap tiles = new CoordIntMap(coordIntMapTiles);
			List<CoordIntEntry> entries = tiles.CoordEntrySet();

		}

	}
}
