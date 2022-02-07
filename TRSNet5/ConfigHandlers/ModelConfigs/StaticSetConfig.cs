using OOOReader.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.ConfigHandlers.Common;
using ThreeRingsSharp.Utilities;
using ThreeRingsSharp.Utilities.DataTree;
using ThreeRingsSharp.Utilities.Parameters;
using ThreeRingsSharp.Utilities.Parameters.Implementation;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.XansData.Extensions;
using ThreeRingsSharp.XansData.XDataTreeExtension;
using XDataTree.Data;
using XDataTree.TreeElements;

namespace ThreeRingsSharp.ConfigHandlers.ModelConfigs {
	public static class StaticSetConfig {

		public static void ReadData(ReadFileContext ctx, ShadowClass modelConfig) {
			ShadowClass staticSetImpl = ModelConfig.GetConfigFromFileSC(modelConfig, "com.threerings.opengl.model.config.StaticSetConfig");
			//ShadowClass[] visibleMeshes = staticImpl["meshes"]!["visible"]!;
			Dictionary<object, object> meshes = staticSetImpl["meshes"]!;

			#region Data Tree
			GenericElement staticSetTreeNode = MasterDataExtractor.SetupBaseInformation(modelConfig, ctx.Push(ctx.File.Name, SilkImage.ModelSet));

			KeyValueContainerElement treeMaterials = new KeyValueContainerElement("Materials", SilkImage.Texture);
			KeyValueContainerElement treeVariantRefs = new KeyValueContainerElement("Variants", SilkImage.Object);
			List<string> treeTextures = new List<string>();
			Dictionary<string, string> treeVariants = new Dictionary<string, string>();
			if (staticSetImpl["materialMappings"] is ShadowClass[] mtMaps) {
				foreach (ShadowClass mtlClass in mtMaps!) {
					treeTextures.Add(mtlClass["texture"]!);
				}
			}
			foreach (KeyValuePair<object, object> meshInfo in meshes) {
				ShadowClass meshSet = (ShadowClass)meshInfo.Value;
				treeVariants[(string)meshInfo.Key] = "Meshes: " + meshSet["visible"]!.Length;
			}
			treeMaterials.SetToEnumerable(treeTextures, new SilkImage[] { SilkImage.Texture }, true);
			treeVariantRefs.SetToEnumerable(treeVariants, new SilkImage[] { SilkImage.Triangle }, true);

			staticSetTreeNode.Properties.Add(new StaticSetConfigVariantElement("Target Model", meshes.FirstOrDefault().Key?.ToString() ?? "null", true, SilkImage.Reference) {
				ValueHolder = staticSetImpl
			});
			staticSetTreeNode.Properties.Add(treeMaterials);
			staticSetTreeNode.Properties.Add(treeVariantRefs);
			staticSetTreeNode.Properties.Add(ModelConfig.SetupParametersForProperties(modelConfig));
			staticSetTreeNode.Properties.Add(ctx.CurrentSceneTransform.ToKeyValueContainer());
			#endregion

			int idx = 0;
			string fullDepthName = RsrcDirectoryTool.GetDirectoryDepth(ctx.File, -1);
			foreach (KeyValuePair<object, object> meshInfo in meshes) {
				string meshTitle = $"-Variant[{meshInfo.Key}]";
				ShadowClass[] visible = (ShadowClass[])((ShadowClass)meshInfo.Value)["visible"]!;
				foreach (ShadowClass visMesh in visible) {
					meshTitle += $"-Submesh[{idx}]";
					Model3D meshToModel = GeometryConfigTranslator.ToModel3D(ctx, visMesh["geometry"], fullDepthName + meshTitle, ctx.CurrentAttachmentNode?.BaseNode);
					meshToModel.Transform *= ctx.CurrentSceneTransform;
					ctx.RegisterStaticSetVariantModel(staticSetImpl, meshInfo.Key.ToString() ?? "null", meshToModel);

					// TODO: Textures
					(List<string> textureFiles, string active, Choice? defaultContainer) = TextureHelper.FindTexturesAndActiveFromDirects(modelConfig, (string)visMesh["texture"]!);
					meshToModel.Textures.SetFrom(textureFiles);
					meshToModel.ActiveTexture = active;
					meshToModel.ActiveTextureChoice = defaultContainer;
					ctx.AllModels.Add(meshToModel);

					idx++;
				}
			}
			ctx.Pop();
		}

	}
}
