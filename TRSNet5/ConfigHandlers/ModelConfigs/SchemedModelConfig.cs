using OOOReader.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.ConfigHandlers.Common;
using ThreeRingsSharp.Utilities;
using ThreeRingsSharp.XansData.Extensions;
using XDataTree.Data;
using XDataTree.TreeElements;

namespace ThreeRingsSharp.ConfigHandlers.ModelConfigs {
	public static class SchemedModelConfig {

		public static void ReadData(ReadFileContext ctx, ShadowClass modelConfig) {
			ShadowClass schemedImpl = ModelConfig.GetConfigFromFileSC(modelConfig, "com.threerings.opengl.model.config.ModelConfig$Schemed");
			ShadowClass[] modelCfgRef = schemedImpl["models"]!;
			

			#region Data Tree
			GenericElement schemedTreeNode = MasterDataExtractor.SetupBaseInformation(modelConfig, ctx.Push(ctx.File.Name, SilkImage.Schemed));
			GenericElement schemesRoot = new GenericElement("Render Schemes", SilkImage.Scheme);
			KeyValueContainerElement kvc = new KeyValueContainerElement("Schemes", SilkImage.Object);
			foreach (ShadowClass sc in modelCfgRef) {
				kvc.Add("Scheme", ((string)sc["scheme"]!).DefaultIfNullOrWhitespace("(no name)"), SilkImage.Schemed);
			}
			schemesRoot.Add(new KeyValueElement("Current Scheme", ((string)modelCfgRef[0]["scheme"]!).DefaultIfNullOrWhitespace("(no name)"), false, SilkImage.SchemedModel)); // TODO: Editable.
			schemesRoot.Add(kvc);
			schemedTreeNode.Properties.Add(schemesRoot);
			schemedTreeNode.Properties.Add(ModelConfig.SetupParametersForProperties(modelConfig));

			#endregion

			foreach (ShadowClass sc in modelCfgRef) {
				if (sc["model"] is ShadowClass cfgRef) {
					ConfigReference realReference = new ConfigReference(cfgRef);
					MasterDataExtractor.ExtractFrom(ctx, realReference, $"Scheme={sc["scheme"]!}");
				}
			}

			ctx.Pop();
		}

	}
}
