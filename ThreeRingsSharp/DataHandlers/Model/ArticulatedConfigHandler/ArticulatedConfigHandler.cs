using com.threerings.math;
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
using static com.threerings.opengl.model.config.ArticulatedConfig;
using static com.threerings.opengl.model.config.ModelConfig;
using static com.threerings.opengl.model.config.ModelConfig.Imported;

namespace ThreeRingsSharp.DataHandlers.Model.ArticulatedConfigHandlers {
	class ArticulatedConfigHandler : IModelDataHandler, IDataTreeInterface<ArticulatedConfig> {

		/// <summary>
		/// A reference to the singleton instance of <see cref="ArticulatedConfigHandler"/>.
		/// </summary>
		public static ArticulatedConfigHandler Instance { get; } = new ArticulatedConfigHandler();

		public void SetupCosmeticInformation(ArticulatedConfig model, DataTreeObject dataTreeParent) {
			if (dataTreeParent == null) return;

			AnimationMapping[] animMaps = model.animationMappings;
			object[] animations = new object[animMaps.Length];
			for (int idx = 0; idx < animations.Length; idx++) {
				animations[idx] = animMaps[idx].name;
			}
			dataTreeParent.AddSimpleProperty("Animations", animations, SilkImage.Value, SilkImage.Animation, false);
		}

		public void HandleModelConfig(FileInfo sourceFile, ModelConfig baseModel, List<Model3D> modelCollection, DataTreeObject dataTreeParent = null, Transform3D globalTransform = null) {
			// ModelConfigHandler.SetupCosmeticInformation(baseModel, dataTreeParent);
			ArticulatedConfig model = (ArticulatedConfig)baseModel.implementation;
			SetupCosmeticInformation(model, dataTreeParent);

			MeshSet meshes = model.skin;
			VisibleMesh[] renderedMeshes = meshes.visible;

			int idx = 0;
			foreach (VisibleMesh mesh in renderedMeshes) {
				Model3D meshToModel = GeometryConfigTranslator.GetGeometryInformation(mesh.geometry);
				meshToModel.Name = ResourceDirectoryGrabber.GetDirectoryDepth(sourceFile) + "-SKIN-Mesh[" + idx + "]";
				if (globalTransform != null) meshToModel.Transform = meshToModel.Transform.compose(globalTransform);
				modelCollection.Add(meshToModel);
				idx++;
			}

			RecursivelyIterateNodes(model.root, sourceFile, modelCollection, globalTransform);
		}

		private void RecursivelyIterateNodes(Node parent, FileInfo sourceFile, List<Model3D> models, Transform3D globalTransform) {
			foreach (Node node in parent.children) {
				if (node is MeshNode meshNode) {
					VisibleMesh mesh = meshNode.visible;
					Model3D meshToModel = GeometryConfigTranslator.GetGeometryInformation(mesh.geometry);
					meshToModel.Name = ResourceDirectoryGrabber.GetDirectoryDepth(sourceFile) + "-Nodes[\"" + node.name + "\"]";
					if (globalTransform != null) meshToModel.Transform = meshToModel.Transform.compose(globalTransform);
					meshToModel.Transform = meshToModel.Transform.compose(node.invRefTransform.compose(node.transform));
					models.Add(meshToModel);
				}
				if (node.children.Length > 0) {
					RecursivelyIterateNodes(node, sourceFile, models, globalTransform);
				}
			}
		}
	}
}
