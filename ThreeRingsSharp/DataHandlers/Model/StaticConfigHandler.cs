using com.threerings.math;
using com.threerings.opengl.model.config;
using System.Collections.Generic;
using System.IO;
using ThreeRingsSharp.DataHandlers.Properties;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.Utility.Interface;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.XansData.Extensions;
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

			SKAnimatorToolsProxy.IncrementEnd(renderedMeshes.Length);
			int idx = 0;
			string depth1Name = ResourceDirectoryGrabber.GetDirectoryDepth(sourceFile);
			string fullDepthName = ResourceDirectoryGrabber.GetDirectoryDepth(sourceFile, -1);
			foreach (VisibleMesh mesh in renderedMeshes) {
				string meshTitle = "-Mesh[" + idx + "]";


				Model3D meshToModel = GeometryConfigTranslator.GetGeometryInformation(mesh.geometry, fullDepthName + meshTitle);
				meshToModel.Name = depth1Name + meshTitle;
				meshToModel.Transform = meshToModel.Transform.compose(globalTransform).compose(new Transform3D(meshes.bounds.getCenter(), Quaternion.IDENTITY, 1f));
				//meshToModel.Textures.SetFrom(ModelConfigHandler.GetTexturesFromModel(sourceFile, model));
				//meshToModel.Textures.SetFrom(ModelPropertyUtility.FindTexturesFromDirects(baseModel));
				//meshToModel.Textures.SetFrom(new List<string>() { mesh.texture });
				//meshToModel.ActiveTexture = mesh.texture;

				(List<string> textureFiles, string active) = ModelPropertyUtility.FindTexturesAndActiveFromDirects(baseModel, mesh.texture);
				meshToModel.Textures.SetFrom(textureFiles);
				meshToModel.ActiveTexture = active;

				modelCollection.Add(meshToModel);
				idx++;
				SKAnimatorToolsProxy.IncrementProgress();
			}
		}
	}
}
