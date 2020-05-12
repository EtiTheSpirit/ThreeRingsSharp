using com.threerings.opengl.model.config;
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

namespace ThreeRingsSharp.DataHandlers.Model.StaticSetConfigHandler {
	class StaticSetConfigHandler : IModelDataHandler, IDataTreeInterface<StaticSetConfig> {

		/// <summary>
		/// A reference to the singleton instance of <see cref="StaticSetConfigHandler"/>.
		/// </summary>
		public static StaticSetConfigHandler Instance { get; } = new StaticSetConfigHandler();

		public void SetupCosmeticInformation(StaticSetConfig model, DataTreeObject dataTreeParent) {
			if (dataTreeParent == null) return;

			// So a note to self: Static sets have the 'meshes' container which is a map of other model files.
			// In SK Animator Tools V1 I was an idiot and thought the "model" property was the only key. This is false.
			// Instead, "model" represents the *default selection*. There may be more models.

			dataTreeParent.AddSimpleProperty("Default Model", model.model);

			List<object> objects = new List<object>();
			if (model.meshes != null) {
				object[] keys = model.meshes.keySet().toArray();
				int msIdx = 0;
				foreach (object key in keys) {
					MeshSet subModel = (MeshSet)model.meshes.get(key);
					VisibleMesh[] meshes = subModel.visible;
					DataTreeObject subModelRef = new DataTreeObject() {
						Text = key.ToString(),
						ImageKey = SilkImage.ModelSet
					};

					List<object> subModels = new List<object>();
					int idx = 0;
					foreach (VisibleMesh mesh in meshes) {
						subModels.Add(new DataTreeObjectProperty("Mesh " + idx, SilkImage.Triangle));
						idx++;
					}
					subModelRef.AddSimpleProperty("Geometry", subModels.ToArray(), SilkImage.Variant, SilkImage.Value, false);
					objects.Add(subModelRef);
					msIdx++;
				}
			}
			dataTreeParent.AddSimpleProperty("Referenced Meshes", objects.ToArray(), SilkImage.Reference, SilkImage.Reference, false);
		}

		public void HandleModelConfig(FileInfo sourceFile, ModelConfig baseModel, ref List<Model3D> modelCollection, DataTreeObject dataTreeParent = null) {
			ModelConfigHandler.SetupCosmeticInformation(baseModel, dataTreeParent);
			StaticSetConfig staticSet = (StaticSetConfig)baseModel.implementation;
			SetupCosmeticInformation(staticSet, dataTreeParent);

			if (staticSet.meshes != null) {
				object[] keys = staticSet.meshes.keySet().toArray();
				int msIdx = 0;
				foreach (object key in keys) {
					MeshSet subModel = (MeshSet)staticSet.meshes.get(key);
					VisibleMesh[] meshes = subModel.visible;
					int idx = 0;
					foreach (VisibleMesh mesh in meshes) {
						Model3D meshToModel = GeometryConfigTranslator.GetGeometryInformation(mesh.geometry);
						meshToModel.Name = ResourceDirectoryGrabber.GetFormattedPathFromRsrc(sourceFile) + "-MeshSets[" + msIdx + "].Mesh[" + idx + "]";
						modelCollection.Add(meshToModel);
						idx++;
					}
				}
			}
		}
	}
}
