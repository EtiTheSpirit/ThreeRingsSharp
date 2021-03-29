using com.threerings.config;
using com.threerings.math;
using com.threerings.opengl.model.config;
using System.Collections.Generic;
using System.IO;
using ThreeRingsSharp.DataHandlers.AnimationHandlers;
using ThreeRingsSharp.DataHandlers.Parameters;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.Utility.Interface;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.XansData.Extensions;
using static com.threerings.opengl.model.config.ArticulatedConfig;
using static com.threerings.opengl.model.config.ModelConfig;

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

			// ArticulatedConfig has a lot of steps.
			SKAnimatorToolsProxy.IncrementEnd(4);

			ArticulatedConfig model = (ArticulatedConfig)baseModel.implementation;
			SetupCosmeticInformation(model, dataTreeParent);

			MeshSet meshes = model.skin;
			VisibleMesh[] renderedMeshes = meshes.visible;
			Dictionary<string, Armature> allInstantiatedArmatures = new Dictionary<string, Armature>();

			List<Model3D> allModelsAndNodes = new List<Model3D>();

			// 1
			SKAnimatorToolsProxy.IncrementProgress();

			int idx = 0;
			string depth1Name = ResourceDirectoryGrabber.GetDirectoryDepth(sourceFile);
			string fullDepthName = ResourceDirectoryGrabber.GetDirectoryDepth(sourceFile, -1);

			SKAnimatorToolsProxy.IncrementEnd(renderedMeshes.Length);
			foreach (VisibleMesh mesh in renderedMeshes) {
				string meshTitle = "-Skin-Mesh[" + idx + "]";

				Model3D meshToModel = GeometryConfigTranslator.GetGeometryInformation(mesh.geometry, fullDepthName + meshTitle, model.root);
				meshToModel.Name = depth1Name + meshTitle;
				if (globalTransform != null) meshToModel.Transform.composeLocal(globalTransform);

				(List<string> textureFiles, string active) = ModelPropertyUtility.FindTexturesAndActiveFromDirects(baseModel, mesh.texture);
				meshToModel.Textures.SetFrom(textureFiles);
				meshToModel.ActiveTexture = active;

				if (meshToModel.Mesh.HasBoneData) {
					XanLogger.WriteLine("Model has bone data, setting that up.", XanLogger.TRACE);
					// meshToModel.Mesh.SetBones(model.root);
					// ^ now called by GetGeometryInformation
					foreach (KeyValuePair<string, Armature> boneNamesToBones in meshToModel.Mesh.AllBones) {
						allInstantiatedArmatures[boneNamesToBones.Key] = boneNamesToBones.Value;
					}
					allModelsAndNodes.Add(meshToModel);
				}
				modelCollection.Add(meshToModel);
				idx++;

				SKAnimatorToolsProxy.IncrementProgress();
			}
			// 2
			SKAnimatorToolsProxy.IncrementProgress();

			SKAnimatorToolsProxy.IncrementEnd(GetNodeCount(model.root));
			Dictionary<string, Model3D> nodeModels = new Dictionary<string, Model3D>();
			RecursivelyIterateNodes(baseModel, model, sourceFile, model.root, modelCollection, globalTransform, globalTransform, nodeModels, fullDepthName);
			allModelsAndNodes.AddRange(nodeModels.Values);

			SKAnimatorToolsProxy.SetProgressState(ProgressBarState.ExtraWork);
			SKAnimatorToolsProxy.IncrementEnd(model.attachments.Length);

			foreach (Attachment attachment in model.attachments) {
				List<Model3D> attachmentModels = ConfigReferenceUtil.HandleConfigReference(sourceFile, attachment.model, modelCollection, dataTreeParent, globalTransform);
				if (attachmentModels == null) {
					SKAnimatorToolsProxy.IncrementProgress();
					continue; // A lot of attachments have null models and I'm not sure why.
				}

				// NEW BEHAVIOR: Is the model root-less but rigged?
				// Set its root to *this* model
				foreach (Model3D mdl in attachmentModels) {
					if (mdl.Mesh != null && mdl.Mesh.UsesExternalRoot) {
						mdl.Mesh.SetBones(model.root);
					}
				}

				SKAnimatorToolsProxy.IncrementEnd(attachmentModels.Count);
				foreach (Model3D referencedModel in attachmentModels) {
					referencedModel.Transform.composeLocal(attachment.transform);
					if (allInstantiatedArmatures.ContainsKey(attachment.node ?? string.Empty)) {
						referencedModel.AttachmentNode = allInstantiatedArmatures[attachment.node];
						XanLogger.WriteLine("Attached [" + referencedModel.Name + "] to [" + attachment.node + "]", XanLogger.TRACE);
					} else {
						// New catch case: This might actually be the name of a model!
						if (nodeModels.ContainsKey(attachment.node ?? string.Empty)) {
							// Indeed it is!

							referencedModel.AttachmentModel = nodeModels[attachment.node];
							referencedModel.AttachmentModel.Transform.setScale(1f); // TODO: Is this okay?

							if (referencedModel.Transform.getType() < Transform3D.AFFINE) {
								float scale = referencedModel.Transform.getScale();
								referencedModel.Transform.set(new Transform3D(new Vector3f(), Quaternion.IDENTITY, scale));
							} else {
								Vector3f scale = referencedModel.Transform.extractScale();
								referencedModel.Transform.set(new Transform3D(new Vector3f(), Quaternion.IDENTITY, scale));
							}


							XanLogger.WriteLine("Attached [" + referencedModel.Name + "] to [" + attachment.node + "]", XanLogger.TRACE);
						} else {
							XanLogger.WriteLine("Attachment wanted to attach to node or model [" + attachment.node + "] but it does not exist!");
						}
					}
					SKAnimatorToolsProxy.IncrementProgress();
				}
				SKAnimatorToolsProxy.IncrementProgress();
			}
			
			SKAnimatorToolsProxy.SetProgressState(ProgressBarState.OK);
			// 3
			SKAnimatorToolsProxy.IncrementProgress();

			SKAnimatorToolsProxy.IncrementEnd(model.animationMappings.Length);
			foreach (AnimationMapping animationMapping in model.animationMappings) {
				ConfigReference animationRef = animationMapping.animation;
				if (animationRef.IsFileReference()) {
					object animationObj = animationRef.ResolveFile();
					if (animationObj is AnimationConfig animation) {
						SKAnimatorToolsProxy.SetProgressState(ProgressBarState.ExtraWork);
						AnimationConfigHandler.HandleAnimationImplementation(animationRef, animationMapping.name, animation, animation.implementation, allModelsAndNodes, model);
						SKAnimatorToolsProxy.SetProgressState(ProgressBarState.OK);
					}
				}
				SKAnimatorToolsProxy.IncrementProgress();
			}

			// 4
			SKAnimatorToolsProxy.IncrementProgress();
		}

		/// <summary>
		/// A utility function that iterates through all of the nodes recursively, as some may store mesh data.<para/>
		/// This does not decode the rig. It strictly grabs meshes out of the rig.
		/// </summary>
		/// <param name="baseModel">The <see cref="ModelConfig"/> that contained this <see cref="ArticulatedConfig"/>.</param>
		/// <param name="model">A reference to the <see cref="ArticulatedConfig"/> that contains these nodes.</param>
		/// <param name="sourceFile">The file where the <see cref="ArticulatedConfig"/> is stored.</param>
		/// <param name="parent">The parent node to iterate through.</param>
		/// <param name="models">The <see cref="List{T}"/> of all models ripped from the source .dat file in this current chain (which may include references to other .dat files)</param>
		/// <param name="latestTransform">The latest transform that has been applied. This is used for recursive motion since nodes inherit the transform of their parent.</param>
		/// <param name="initialTransform"></param>
		/// <param name="nodeModels">A lookup from a <see cref="Node"/> to a <see cref="Model3D"/>, which does include potentially empty models for standard, non-mesh nodes.</param>
		/// <param name="fullDepthName">The complete path to this model from rsrc, rsrc included.</param>
		public static void RecursivelyIterateNodes(ModelConfig baseModel, ArticulatedConfig model, FileInfo sourceFile, Node parent, List<Model3D> models, Transform3D latestTransform, Transform3D initialTransform, Dictionary<string, Model3D> nodeModels, string fullDepthName) {
			foreach (Node node in parent.children) {
				// Transform3D newTransform = latestTransform;
				if (node is MeshNode meshNode) {
					VisibleMesh mesh = meshNode.visible;
					if (mesh != null) {
						// "Let's use a node designed to store meshes for something that doesn't contain meshes!"
						//		-- Some knucklehead at OOO.
						// ...No offense.

						string meshTitle = "-MeshNodes[\"" + node.name + "\"]";

						Model3D meshToModel = GeometryConfigTranslator.GetGeometryInformation(mesh.geometry, fullDepthName + meshTitle);
						meshToModel.Name = ResourceDirectoryGrabber.GetDirectoryDepth(sourceFile) + meshTitle;
						meshToModel.RawName = node.name;

						(List<string> textureFiles, string active) = ModelPropertyUtility.FindTexturesAndActiveFromDirects(baseModel, mesh.texture);
						meshToModel.Textures.SetFrom(textureFiles);
						meshToModel.ActiveTexture = active;

						/*
						meshToModel.Textures.SetFrom(ModelPropertyUtility.FindTexturesFromDirects(baseModel));
						meshToModel.ActiveTexture = mesh.texture;
						*/

						// Modify the transform that it already has to the node's transform.						
						meshToModel.Transform.composeLocal(latestTransform);
						meshToModel.Transform.composeLocal(node.transform);

						nodeModels[node.name] = meshToModel;
						models.Add(meshToModel);
					}
				} else {
					string meshTitle = "-Nodes[\"" + node.name + "\"]";
					Model3D emptyModel = Model3D.NewEmpty();
					emptyModel.Name = ResourceDirectoryGrabber.GetDirectoryDepth(sourceFile) + meshTitle;
					emptyModel.Transform.composeLocal(latestTransform);
					emptyModel.Transform.composeLocal(node.transform);

					nodeModels[node.name] = emptyModel;
					models.Add(emptyModel);
				}

				//VertexGroup group = new VertexGroup();
				//group.Name = node.name;

				SKAnimatorToolsProxy.IncrementProgress();
				if (node.children.Length > 0) {
					RecursivelyIterateNodes(baseModel, model, sourceFile, node, models, latestTransform.compose(node.transform), initialTransform, nodeModels, fullDepthName);
				}
			}
		}

		/// <summary>
		/// Gets the node count.
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static int GetNodeCount(Node parent) {
			int count = 0;
			foreach (Node child in parent.children) {
				count++;
				count += GetNodeCount(child);
			}
			return count;
		}
	}
}
