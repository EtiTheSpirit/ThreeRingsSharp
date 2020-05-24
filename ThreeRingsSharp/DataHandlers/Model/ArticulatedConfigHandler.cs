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
using ThreeRingsSharp.XansData.Extensions;
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
			string depth1Name = ResourceDirectoryGrabber.GetDirectoryDepth(sourceFile);
			string fullDepthName = ResourceDirectoryGrabber.GetDirectoryDepth(sourceFile, -1);
			foreach (VisibleMesh mesh in renderedMeshes) {
				string meshTitle = "-Skin-Mesh[" + idx + "]";

				Model3D meshToModel = GeometryConfigTranslator.GetGeometryInformation(mesh.geometry, fullDepthName + meshTitle);
				meshToModel.Name = depth1Name + meshTitle;
				if (globalTransform != null) meshToModel.Transform = meshToModel.Transform.compose(globalTransform);
				meshToModel.Textures.SetFrom(ModelConfigHandler.GetTexturesFromModel(sourceFile, model));
				meshToModel.ActiveTexture = mesh.texture;

				/*
				string tex = sourceFile.Directory.FullName.Replace("\\", "/") + "/" + mesh.texture;
				meshToModel.Textures.Add(tex);
				XanLogger.WriteLine($"Added texture {tex} to model {meshToModel.Name}");
				*/

				modelCollection.Add(meshToModel);
				idx++;
			}

			//foreach (Attachment attachment in model.attachments) {
				// TODO: Make attachments move to where they're supposed to.
				// ConfigReferenceUtil.HandleConfigReference(sourceFile, attachment.model, modelCollection, dataTreeParent, globalTransform);
			//}

			RecursivelyIterateNodes(model, sourceFile, model.root, modelCollection, globalTransform, fullDepthName);
		}

		/// <summary>
		/// A utility function that iterates through all of the nodes recursively, as some may store mesh data.
		/// </summary>
		/// <param name="model">A reference to the <see cref="ArticulatedConfig"/> that contains these nodes.</param>
		/// <param name="sourceFile">The file where the <see cref="ArticulatedConfig"/> is stored.</param>
		/// <param name="parent">The parent node to iterate through.</param>
		/// <param name="models">The <see cref="List{T}"/> of all models ripped from the source .dat file in this current chain (which may include references to other .dat files)</param>
		/// <param name="latestTransform">The latest transform that has been applied. This is used for recursive motion since nodes inherit the transform of their parent.</param>
		/// <param name="fullDepthName">The complete path to this model from rsrc, rsrc included.</param>
		private void RecursivelyIterateNodes(ArticulatedConfig model, FileInfo sourceFile, Node parent, List<Model3D> models, Transform3D latestTransform, string fullDepthName) {
			foreach (Node node in parent.children) {
				// Transform3D newTransform = latestTransform;

				if (node is MeshNode meshNode) {
					VisibleMesh mesh = meshNode.visible;
					Transform3D modifiedTransform = node.invRefTransform.invert().compose(node.transform);
					//Transform3D modifiedTransform = node.transform.compose(node.invRefTransform.invert());
					string meshTitle = "-Nodes[\"" + node.name + "\"]";

					Model3D meshToModel = GeometryConfigTranslator.GetGeometryInformation(mesh.geometry, fullDepthName + meshTitle);
					meshToModel.Name = ResourceDirectoryGrabber.GetDirectoryDepth(sourceFile) + meshTitle;
					meshToModel.Transform = meshToModel.Transform.compose(latestTransform).compose(modifiedTransform);
					meshToModel.Textures.SetFrom(ModelConfigHandler.GetTexturesFromModel(sourceFile, model));
					meshToModel.ActiveTexture = mesh.texture;

					/*
					string tex = sourceFile.Directory.FullName.Replace("\\", "/") + "/" + mesh.texture;
					meshToModel.Textures.Add(tex);
					XanLogger.WriteLine($"Added texture {tex} to model {meshToModel.Name}");
					*/

					models.Add(meshToModel);
				}

				//VertexGroup group = new VertexGroup();
				//group.Name = node.name;

				if (node.children.Length > 0) {
					RecursivelyIterateNodes(model, sourceFile, node, models, latestTransform, fullDepthName);
				}
			}
		}
	}
}
