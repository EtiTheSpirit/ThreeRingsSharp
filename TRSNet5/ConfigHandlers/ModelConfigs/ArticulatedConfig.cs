using OOOReader.Reader;
using OOOReader.Utility.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.ConfigHandlers.Common;
using ThreeRingsSharp.Utilities;
using ThreeRingsSharp.Utilities.Parameters;
using ThreeRingsSharp.Utilities.Parameters.Implementation;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.XansData.Extensions;
using XDataTree.Data;
using XDataTree.TreeElements;

namespace ThreeRingsSharp.ConfigHandlers.ModelConfigs {
	public static class ArticulatedConfig {

		public static void ReadData(ReadFileContext ctx, ShadowClass modelConfig) {
			ShadowClass articulatedImpl = ModelConfig.GetConfigFromFileSC(modelConfig, "com.threerings.opengl.model.config.ArticulatedConfig");

			#region Data Tree
			GenericElement articulatedTreeNode = MasterDataExtractor.SetupBaseInformation(modelConfig, ctx.Push(ctx.File.Name, SilkImage.Articulated));

			KeyValueContainerElement treeMaterials = new KeyValueContainerElement("Materials", SilkImage.Texture);
			KeyValueContainerElement treeAnimationRefs = new KeyValueContainerElement("Animations", SilkImage.Animation);
			List<string> treeTextures = new List<string>();
			List<string> treeAnimations = new List<string>();
			if (articulatedImpl["materialMappings"] is ShadowClass[] mtMaps) {
				foreach (ShadowClass mtlClass in mtMaps) {
					treeTextures.Add(mtlClass["texture"]!);
				}
			}
			if (articulatedImpl["animationMappings"] is ShadowClass[] scArray) {
				foreach (ShadowClass animClass in scArray) {
					treeAnimations.Add(animClass["name"]!);
				}
			}
			treeMaterials.SetToEnumerable(treeTextures, new SilkImage[] { SilkImage.Texture }, true);
			treeAnimationRefs.SetToEnumerable(treeAnimations, new SilkImage[] { SilkImage.Animation }, true);
			articulatedTreeNode.Properties.Add(treeMaterials);
			articulatedTreeNode.Properties.Add(treeAnimationRefs);
			articulatedTreeNode.Properties.Add(ModelConfig.SetupParametersForProperties(modelConfig));
			#endregion

			ShadowClass[] skinnedVisibleMeshes = articulatedImpl["skin"]!["visible"]!;
			// string depth1Name = RsrcDirectoryTool.GetDirectoryDepth(ctx.File);
			string fullDepthName = RsrcDirectoryTool.GetDirectoryDepth(ctx.File, -1);
			int idx = 0;

			foreach (ShadowClass mesh in skinnedVisibleMeshes) {
				string meshTitle = $"-Skin-Mesh[{idx}]";
				ShadowClass geometryConfig = mesh["geometry"]!;
				// At the very least its guaranteed to be a Stored implementation.
				// This offers a lot of data right away
				
				Model3D meshToModel = GeometryConfigTranslator.ToModel3D(geometryConfig, fullDepthName + meshTitle, new Armature.Node(articulatedImpl["root"]));
				meshToModel.Transform.ComposeSelf(ctx.CurrentSceneTransform);

				(List<string> textureFiles, string active, Choice? defaultContainer) = TextureHelper.FindTexturesAndActiveFromDirects(modelConfig, (string)mesh["texture"]!);
				meshToModel.Textures.SetFrom(textureFiles);
				meshToModel.ActiveTexture = active;
				meshToModel.ActiveTextureChoice = defaultContainer;

				if (meshToModel.Mesh!.HasBoneData) {
					Debug.WriteLine("Model has bone data, setting that up.");
					foreach (KeyValuePair<string, Armature> boneNamesToBones in meshToModel.Mesh.AllBones) {
						ctx.AllArmatures[boneNamesToBones.Key] = boneNamesToBones.Value;
					}
					ctx.AllModelsAndNodes.Add(meshToModel);
				}
				ctx.AllModels.Add(meshToModel);

				idx++;
			}

			if (articulatedImpl["root"] is ShadowClass root) {
				RecursivelyIterateNodes(ctx, modelConfig, articulatedImpl, root, ctx.CurrentSceneTransform, ctx.CurrentSceneTransform, fullDepthName);
			}

			if (articulatedImpl["attachments"] is ShadowClass[] attachments) {
				foreach (ShadowClass attachment in attachments) {
					if (attachment["model"] != null) {
						ConfigReference cfgRef = new ConfigReference(attachment["model"]);
						MasterDataExtractor.ExtractFrom(ctx, cfgRef);
					}
				}
			}
			ctx.Pop(); // :b:op
		}

		/// <summary>
		/// A utility function that iterates through all of the nodes recursively, as some may store mesh data.<para/>
		/// This does not decode the rig. It strictly grabs meshes out of the rig.
		/// </summary>
		/// <param name="ctx">The read-file context storing all ongoing data.</param>
		/// <param name="baseModelConfig">The <see cref="ShadowClass"/> that contained this <see cref="ArticulatedConfig"/>.</param>
		/// <param name="articulatedConfigModel">A reference to the <see cref="ArticulatedConfig"/> that contains these nodes.</param>
		/// <param name="parentNode">The parent node to iterate through.</param>
		/// <param name="latestTransform">The latest transform that has been applied. This is used for recursive motion since nodes inherit the transform of their parent.</param>
		/// <param name="initialTransform"></param>
		/// <param name="fullDepthName">The complete path to this model from rsrc, rsrc included.</param>
		public static void RecursivelyIterateNodes(ReadFileContext ctx, ShadowClass baseModelConfig, ShadowClass articulatedConfigModel, ShadowClass parentNode ,Transform3D latestTransform, Transform3D initialTransform, string fullDepthName) {
			foreach (ShadowClass node in parentNode["children"]!) {
				// Transform3D newTransform = latestTransform;

				string nodeName = node["name"]!;
				Transform3D nodeTransform = new Transform3D(node["transform"]!);
				if (node.IsA("com.threerings.opengl.model.config.ArticulatedConfig$MeshNode")) {
					ShadowClass? mesh = node["visible"];
					if (mesh != null) {
						// "Let's use a node designed to store meshes for something that doesn't contain meshes!"
						//		-- Some knucklehead at OOO.
						// ...No offense. Just very annoying that a MeshNode is allocated only to have no mesh.

						
						string meshTitle = "-MeshNodes[\"" + nodeName + "\"]";

						Model3D meshToModel = GeometryConfigTranslator.ToModel3D(mesh["geometry"], fullDepthName + meshTitle, new Armature.Node(node));
						meshToModel.Transform.ComposeSelf(latestTransform);
						meshToModel.Transform.ComposeSelf(nodeTransform);
						meshToModel.RawName = nodeName;

						(List<string> textureFiles, string active, Choice? defaultContainer) = TextureHelper.FindTexturesAndActiveFromDirects(baseModelConfig, (string)mesh["texture"]!);
						meshToModel.Textures.SetFrom(textureFiles);
						meshToModel.ActiveTexture = active;
						meshToModel.ActiveTextureChoice = defaultContainer;

						// nodeModels[nodeName] = meshToModel;
						ctx.AllModelsAndNodes.Add(meshToModel);
						ctx.AllModels.Add(meshToModel);
					}
				} else {
					string meshTitle = "-Nodes[\"" + nodeName + "\"]";
					Model3D emptyModel = Model3D.NewEmpty();
					emptyModel.Name = RsrcDirectoryTool.GetDirectoryDepth(ctx.File) + meshTitle;
					emptyModel.Transform.ComposeSelf(latestTransform);
					emptyModel.Transform.ComposeSelf(nodeTransform);

					ctx.AllModelsAndNodes.Add(emptyModel);
					ctx.AllModels.Add(emptyModel);
				}

				if (node["children"]!.Length > 0) {
					RecursivelyIterateNodes(ctx, baseModelConfig, articulatedConfigModel, node, latestTransform.Compose(nodeTransform), initialTransform, fullDepthName);
				}
			}
		}

	}
}
