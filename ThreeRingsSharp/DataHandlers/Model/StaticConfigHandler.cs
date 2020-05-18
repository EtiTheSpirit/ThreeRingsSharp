using com.threerings.math;
using com.threerings.opengl.geometry.config;
using com.threerings.opengl.model.config;
using com.threerings.util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.Utility.Interface;
using ThreeRingsSharp.XansData;
using static com.threerings.opengl.model.config.ModelConfig;

namespace ThreeRingsSharp.DataHandlers.Model {
	public class StaticConfigHandler : Singleton<StaticConfigHandler>, IModelDataHandler, IDataTreeInterface<StaticConfig> {

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

		public void HandleModelConfig(FileInfo sourceFile, ModelConfig baseModel, List<Model3D> modelCollection, DataTreeObject dataTreeParent = null, Transform3D globalTransform = null, Dictionary<string, dynamic> extraData = null) {
			// ModelConfigHandler.SetupCosmeticInformation(baseModel, dataTreeParent);
			StaticConfig model = (StaticConfig)baseModel.implementation;
			SetupCosmeticInformation(model, dataTreeParent);

			//Model3D mdl = new Model3D();
			MeshSet meshes = model.meshes;
			VisibleMesh[] renderedMeshes = meshes.visible;

			int idx = 0;
			foreach (VisibleMesh mesh in renderedMeshes) {
				Model3D meshToModel = GeometryConfigTranslator.GetGeometryInformation(mesh.geometry);
				meshToModel.Name = ResourceDirectoryGrabber.GetDirectoryDepth(sourceFile) + "-Mesh[" + idx + "]";
				if (globalTransform != null) meshToModel.Transform = meshToModel.Transform.compose(globalTransform);
				meshToModel.Transform = meshToModel.Transform.compose(new Transform3D(meshes.bounds.getCenter(), Quaternion.IDENTITY).promote(4));

				modelCollection.Add(meshToModel);
				idx++;
			}
		}
	}
}
