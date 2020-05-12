using com.threerings.opengl.geometry.config;
using com.threerings.opengl.model.config;
using com.threerings.util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.DataHandlers.Model.ModelConfigHandlers;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.Utility.Interface;
using ThreeRingsSharp.XansData;
using static com.threerings.opengl.model.config.ModelConfig;

namespace ThreeRingsSharp.DataHandlers.Model.StaticConfigHandlers {
	public class StaticConfigHandler : IModelDataHandler, IDataTreeInterface<StaticConfig> {

		/// <summary>
		/// A reference to the singleton instance of <see cref="StaticConfigHandler"/>.
		/// </summary>
		public static StaticConfigHandler Instance { get; } = new StaticConfigHandler();

		/// <summary>
		/// Sets up the cosmetic data for this model, or, what's displayed in the GUI for the program.
		/// </summary>
		/// <param name="model">The <see cref="StaticConfig"/> to pull data from.</param>
		/// <param name="dataTreeParent">This is the instance in the data tree that represents this object in the hierarchy.</param>
		public void SetupCosmeticInformation(StaticConfig model, DataTreeObject dataTreeParent) {
			if (dataTreeParent == null) return;

			if (model.meshes != null && model.meshes.visible != null) {
				List<object> subModels = new List<object>();
				int idx = 0;
				foreach (VisibleMesh mesh in model.meshes.visible) {
					subModels.Add(new DataTreeObjectProperty("Mesh " + idx, SilkImage.Triangle));
					idx++;
				}
				dataTreeParent.AddSimpleProperty("Geometry", subModels.ToArray(), SilkImage.Variant, SilkImage.Value, false);
			}
		}

		public void HandleModelConfig(FileInfo sourceFile, ModelConfig baseModel, ref List<Model3D> modelCollection, DataTreeObject dataTreeParent = null) {
			ModelConfigHandler.SetupCosmeticInformation(baseModel, dataTreeParent);
			StaticConfig model = (StaticConfig)baseModel.implementation;
			SetupCosmeticInformation(model, dataTreeParent);

			//Model3D mdl = new Model3D();
			MeshSet meshes = model.meshes;
			VisibleMesh[] renderedMeshes = meshes.visible;

			int idx = 0;
			foreach (VisibleMesh mesh in renderedMeshes) {
				Model3D meshToModel = GeometryConfigTranslator.GetGeometryInformation(mesh.geometry);
				meshToModel.Name = ResourceDirectoryGrabber.GetFormattedPathFromRsrc(sourceFile) + "-Mesh[" + idx + "]";
				modelCollection.Add(meshToModel);
				idx++;
			}
		}
	}
}
