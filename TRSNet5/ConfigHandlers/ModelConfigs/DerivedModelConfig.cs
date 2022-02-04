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
	public static class DerivedModelConfig {

		public static void ReadData(ReadFileContext ctx, ShadowClass modelConfig) {
			ShadowClass derivedImpl = ModelConfig.GetConfigFromFileSC(modelConfig, "com.threerings.opengl.model.config.ModelConfig$Derived");
			ShadowClass? modelCfgRef = derivedImpl["model"];
			ConfigReference? cfgRefImpl = null;
			if (modelCfgRef != null) {
				cfgRefImpl = new ConfigReference(modelCfgRef);
			}

			#region Data Tree
			GenericElement derivedTreeNode = MasterDataExtractor.SetupBaseInformation(modelConfig, ctx.Push(ctx.File.Name, SilkImage.Derived));
			if (modelCfgRef != null) {
				derivedTreeNode.Properties.Add(new KeyValueElement("Model Reference", cfgRefImpl!.Name, false, SilkImage.ModelSet));
			} else {
				derivedTreeNode.Properties.Add(new KeyValueElement("Model Reference", "N/A", false, SilkImage.ModelSet));
			}

			derivedTreeNode.Properties.Add(ModelConfig.SetupParametersForProperties(modelConfig));
			#endregion

			if (cfgRefImpl != null) {
				MasterDataExtractor.ExtractFrom(ctx, cfgRefImpl);
			}
			ctx.Pop();
		}

	}
}
