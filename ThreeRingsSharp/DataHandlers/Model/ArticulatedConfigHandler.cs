using com.threerings.math;
using com.threerings.opengl.model.config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.Utility.Interface;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.XansData.Structs;
using static com.threerings.opengl.model.config.ArticulatedConfig;
using static com.threerings.opengl.model.config.ModelConfig;
using static com.threerings.opengl.model.config.ModelConfig.Imported;

namespace ThreeRingsSharp.DataHandlers.Model {
	class ArticulatedConfigHandler : Singleton<ArticulatedConfigHandler>, IModelDataHandler, IDataTreeInterface<ArticulatedConfig> {

		public void SetupCosmeticInformation(ArticulatedConfig model, DataTreeObject dataTreeParent) {
			if (dataTreeParent == null) return;

			AnimationMapping[] animMaps = model.animationMappings;
			object[] animations = new object[animMaps.Length];
			for (int idx = 0; idx < animations.Length; idx++) {
				animations[idx] = animMaps[idx].name;
			}
			dataTreeParent.AddSimpleProperty("Animations", animations, SilkImage.Value, SilkImage.Animation, false);
		}

		public void HandleModelConfig(FileInfo sourceFile, ModelConfig baseModel, List<Model3D> modelCollection, DataTreeObject dataTreeParent = null, Transform3D globalTransform = null, Dictionary<string, dynamic> extraData = null) {
			// ModelConfigHandler.SetupCosmeticInformation(baseModel, dataTreeParent);
			ArticulatedConfig model = (ArticulatedConfig)baseModel.implementation;
			SetupCosmeticInformation(model, dataTreeParent);

			MeshSet meshes = model.skin;
			VisibleMesh[] renderedMeshes = meshes.visible;

			int idx = 0;
			foreach (VisibleMesh mesh in renderedMeshes) {
				Model3D meshToModel = GeometryConfigTranslator.GetGeometryInformation(mesh.geometry);
				meshToModel.Name = ResourceDirectoryGrabber.GetDirectoryDepth(sourceFile) + "-Skin-Mesh[" + idx + "]";
				if (globalTransform != null) meshToModel.Transform = meshToModel.Transform.compose(globalTransform);
				meshToModel.Textures.Add(sourceFile.Directory.FullName.Replace("\\", "/") + "/" + mesh.texture);

				modelCollection.Add(meshToModel);
				idx++;
			}

			foreach (Attachment attachment in model.attachments) {
				//List<Model3D> attachmentMdls = 
				ConfigReferenceUtil.HandleConfigReference(sourceFile, attachment.model, modelCollection, dataTreeParent, globalTransform);
			}

			RecursivelyIterateNodes(sourceFile, model.root, modelCollection, globalTransform);
		}

		/// <summary>
		/// A utility function that iterates through all of the nodes recursively, as some may store mesh data.
		/// </summary>
		/// <param name="sourceFile">The file where the <see cref="ArticulatedConfig"/> is stored.</param>
		/// <param name="parent">The parent node to iterate through.</param>
		/// <param name="models">The <see cref="List{T}"/> of all models ripped from the source .dat file in this current chain (which may include references to other .dat files)</param>
		/// <param name="latestTransform">The latest transform that has been applied. This is used for recursive motion since nodes inherit the transform of their parent.</param>
		private void RecursivelyIterateNodes(FileInfo sourceFile, Node parent, List<Model3D> models, Transform3D latestTransform) {
			foreach (Node node in parent.children) {
				// Transform3D newTransform = latestTransform;

				if (node is MeshNode meshNode) {
					VisibleMesh mesh = meshNode.visible;
					Transform3D modifiedTransform = node.invRefTransform.invertLocal().compose(node.transform);

					Model3D meshToModel = GeometryConfigTranslator.GetGeometryInformation(mesh.geometry);
					meshToModel.Name = ResourceDirectoryGrabber.GetDirectoryDepth(sourceFile) + "-Nodes[\"" + node.name + "\"]";
					meshToModel.Transform = meshToModel.Transform.compose(latestTransform).compose(modifiedTransform);
					meshToModel.Textures.Add(sourceFile.Directory.FullName.Replace("\\", "/") + "/" + mesh.texture);

					models.Add(meshToModel);
				}

				//VertexGroup group = new VertexGroup();
				//group.Name = node.name;

				if (node.children.Length > 0) {
					RecursivelyIterateNodes(sourceFile, node, models, latestTransform);
				}
			}
		}
	}
}
