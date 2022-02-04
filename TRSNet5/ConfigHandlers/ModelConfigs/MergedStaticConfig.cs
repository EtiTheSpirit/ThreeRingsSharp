using OOOReader.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.ConfigHandlers.Common;
using ThreeRingsSharp.Utilities;
using ThreeRingsSharp.Utilities.DataTree;
using XDataTree.Data;
using XDataTree.TreeElements;

namespace ThreeRingsSharp.ConfigHandlers.ModelConfigs {
	public static class MergedStaticConfig {

		public static void ReadData(ReadFileContext ctx, ShadowClass modelConfig) {
			ShadowClass mergedImpl = ModelConfig.GetConfigFromFileSC(modelConfig, "com.threerings.opengl.model.config.MergedStaticConfig");
			ShadowClass[] models = mergedImpl["models"]!;

			#region Data Tree
			GenericElement mergedTreeNode = MasterDataExtractor.SetupBaseInformation(modelConfig, ctx.Push(ctx.File.Name, SilkImage.MergedStatic));

			KeyValueContainerElement treeMaterials = new KeyValueContainerElement("Materials", SilkImage.Texture);
			// KeyValueContainerElement treeMeshRefs = new KeyValueContainerElement("Meshes", SilkImage.Object);

			List<string> treeTextures = new List<string>();
			// List<string> treeMeshes = new List<string>();
			if (mergedImpl["materialMappings"] is ShadowClass[] mtMaps) {
				foreach (ShadowClass mtlClass in mtMaps!) {
					treeTextures.Add(mtlClass["texture"]!);
				}
			}
			// for (int index = 0; index < visibleMeshes.Length; index++) { 
			// treeMeshes.Add($"Mesh #{index}");
			// }
			treeMaterials.SetToEnumerable(treeTextures, new SilkImage[] { SilkImage.Texture }, true);
			// treeMeshRefs.SetToEnumerable(treeMeshes, new SilkImage[] { SilkImage.Triangle }, true);
			mergedTreeNode.Properties.Add(treeMaterials);
			//staticTreeNode.Properties.Add(treeMeshRefs);
			mergedTreeNode.Properties.Add(new KeyValueElement("Models", models.Length.ToString(), false, SilkImage.Static));
			mergedTreeNode.Properties.Add(ModelConfig.SetupParametersForProperties(modelConfig));
			mergedTreeNode.Properties.Add(ctx.CurrentSceneTransform.ToKVC());
			#endregion

			foreach (ShadowClass component in models) {
				ConfigReference cfgRef = new ConfigReference(component["model"]!);
				MasterDataExtractor.ExtractFrom(ctx, cfgRef);
			}
			ctx.Pop();

		}

	}
}
