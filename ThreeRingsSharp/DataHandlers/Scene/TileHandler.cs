﻿using static com.threerings.tudey.data.TudeySceneModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.Utility.Interface;
using System.IO;
using com.threerings.math;
using ThreeRingsSharp.DataHandlers.Model;
using com.threerings.tudey.data;
using ThreeRingsSharp.DataHandlers.Scene.Data;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.XansData.XML.ConfigReferences;
using com.threerings.tudey.util;

namespace ThreeRingsSharp.DataHandlers.Scene {

	/// <summary>
	/// Handles instances of <see cref="TileEntry"/>.
	/// </summary>
	public class TileHandler : Singleton<TileHandler>, IEntryHandler, IDataTreeInterface<TileEntry> {

		public void SetupCosmeticInformation(TileEntry data, DataTreeObject dataTreeParent) {
			if (dataTreeParent == null) return;

			// This method returns a property, but this is just a stock object (it was created this way in TudeySceneConfigBrancher)
			List<DataTreeObject> values = dataTreeParent.Properties[dataTreeParent.FindSimpleProperty("Entries")];
			DataTreeObject prop = values.First();
			DataTreeObject container = new DataTreeObject() {
				Text = data.tile.getName(),
				// ImageKey = SilkImage.Tile
			};


			Coord location = data.getLocation();
			Transform3D trs = new Transform3D(new Vector3f(location.x, data.elevation, location.y), new Quaternion().fromAngleAxis((float)(data.rotation * Math.PI / 2), Vector3f.UNIT_Y), 1f);
			//container.AddSimpleProperty("Elevation", data.elevation);
			//container.AddSimpleProperty("Rotation", data.rotation);
			container.AddSimpleProperty("Transform", trs, SilkImage.Matrix);
			container.AddSimpleProperty("Tile Reference", data.tile?.getName(), SilkImage.Reference);
			prop.AddSimpleProperty("Entry", container, SilkImage.Tile);
		}

		public void HandleEntry(FileInfo sourceFile, Entry entry, List<Model3D> modelCollection, DataTreeObject dataTreeParent = null, Transform3D globalTransform = null) {
			TileEntry tile = (TileEntry)entry;
			//Transform3D trs = new Transform3D();
			//tile.getTransform(tile.getConfig(scene.getConfigManager()), trs);

			SetupCosmeticInformation(tile, dataTreeParent);
			try {
				ShallowTileConfig tileCfg = ConfigReferenceBootstrapper.References["tile"][tile.tile.getName()];
				Coord location = tile.getLocation();
				Transform3D trs = new Transform3D(new Vector3f(location.x, tile.elevation, location.y), new Quaternion().fromAngleAxis((float)(tile.rotation * Math.PI / 2), Vector3f.UNIT_Y), 1f);
				

				ConfigReferenceUtil.HandleConfigReferenceFromDirectPath(sourceFile, tileCfg.ModelPath, modelCollection, dataTreeParent, globalTransform.compose(trs), extraData: new Dictionary<string, dynamic> { ["TargetModel"] = tileCfg.TargetModel });
			} catch (KeyNotFoundException) {
				XanLogger.WriteLine("Unable to find data: " + tile.tile.getName());
			}
		}
		
	}
}