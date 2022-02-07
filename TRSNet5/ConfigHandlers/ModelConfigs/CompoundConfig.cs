using OOOReader.Reader;
using OOOReader.Utility.Mathematics;
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
	public static class CompoundConfig {

		public static void ReadData(ReadFileContext ctx, ShadowClass modelConfig) {
			ShadowClass compoundImpl = ModelConfig.GetConfigFromFileSC(modelConfig, "com.threerings.opengl.model.config.CompoundConfig");
			ShadowClass[] models = compoundImpl["models"]!;

			#region Data Tree
			GenericElement mergedTreeNode = MasterDataExtractor.SetupBaseInformation(modelConfig, ctx.Push(ctx.File.Name, SilkImage.ModelSet));

			KeyValueContainerElement treeMaterials = new KeyValueContainerElement("Materials", SilkImage.Texture);
			// KeyValueContainerElement treeMeshRefs = new KeyValueContainerElement("Meshes", SilkImage.Object);

			List<string> treeTextures = new List<string>();
			// List<string> treeMeshes = new List<string>();
			if (compoundImpl.TryGetField("materialMappings", out ShadowClass[]? mtMaps, true)) {
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
			#endregion

			foreach (ShadowClass component in models) {
				if (component.IsA("com.threerings.opengl.model.config.CompoundConfig$ComponentModel") && component.TryGetFieldAs("model", "com.threerings.config.ConfigReference", out ShadowClass? mdlRef, true) && mdlRef != null) {
					Transform3D offset;
					if (component.TryGetFieldAs("transform", "com.threerings.math.Transform3D", out ShadowClass? shdTransform, true) && shdTransform != null) {
						offset = Transform3D.FromShadow(shdTransform!);
					} else {
						offset = new Transform3D();
					}
					ctx.ComposeTransform(offset);
					ConfigReference cfgRef = new ConfigReference(mdlRef!);
					MasterDataExtractor.ExtractFrom(ctx, cfgRef);
					ctx.ComposeTransform(offset.Invert());
				}
			}

			ctx.Pop();
		}

	}
}
