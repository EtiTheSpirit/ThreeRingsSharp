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
using XDataTree.Data;
using XDataTree.TreeElements;

namespace ThreeRingsSharp.ConfigHandlers.ModelConfigs {
	public static class StaticConfig {

		public static void ReadData(ReadFileContext ctx, ShadowClass modelConfig) {
			ShadowClass staticImpl = ModelConfig.GetConfigFromFileSC(modelConfig, "com.threerings.opengl.model.config.StaticConfig");
			ShadowClass[] visibleMeshes = staticImpl["meshes"]!["visible"]!;

			#region Data Tree
			GenericElement staticTreeNode = MasterDataExtractor.SetupBaseInformation(modelConfig, ctx.Push(ctx.File.Name, SilkImage.Static));

			KeyValueContainerElement treeMaterials = new KeyValueContainerElement("Materials", SilkImage.Texture);
			// KeyValueContainerElement treeMeshRefs = new KeyValueContainerElement("Meshes", SilkImage.Object);

			List<string> treeTextures = new List<string>();
			// List<string> treeMeshes = new List<string>();
			if (staticImpl["materialMappings"] is ShadowClass[] mtMaps) {
				foreach (ShadowClass mtlClass in mtMaps!) {
					treeTextures.Add(mtlClass["texture"]!);
				}
			}
			// for (int index = 0; index < visibleMeshes.Length; index++) { 
				// treeMeshes.Add($"Mesh #{index}");
			// }
			treeMaterials.SetToEnumerable(treeTextures, new SilkImage[] { SilkImage.Texture }, true);
			// treeMeshRefs.SetToEnumerable(treeMeshes, new SilkImage[] { SilkImage.Triangle }, true);
			staticTreeNode.Properties.Add(treeMaterials);
			//staticTreeNode.Properties.Add(treeMeshRefs);
			staticTreeNode.Properties.Add(new KeyValueElement("Meshes", visibleMeshes.Length.ToString(), false, SilkImage.Triangle));
			staticTreeNode.Properties.Add(ModelConfig.SetupParametersForProperties(modelConfig));
			staticTreeNode.Properties.Add(ctx.CurrentSceneTransform.ToKVC());
			#endregion

			int idx = 0;
			string fullDepthName = RsrcDirectoryTool.GetDirectoryDepth(ctx.File, -1);
			foreach (ShadowClass visMesh in visibleMeshes) {
				string meshTitle = $"-Submesh[{idx}]";
				Model3D meshToModel = GeometryConfigTranslator.ToModel3D(ctx, visMesh["geometry"], fullDepthName + meshTitle, ctx.CurrentAttachmentNode?.BaseNode);
				meshToModel.Transform.ComposeSelf(ctx.CurrentSceneTransform);

				// TODO: Textures
				(List<string> textureFiles, string active, Choice? defaultContainer) = TextureHelper.FindTexturesAndActiveFromDirects(modelConfig, (string)visMesh["texture"]!);
				meshToModel.Textures.SetFrom(textureFiles);
				meshToModel.ActiveTexture = active;
				meshToModel.ActiveTextureChoice = defaultContainer;
				ctx.AllModels.Add(meshToModel);

				idx++;
			}
			ctx.Pop();
		}
	}
}
