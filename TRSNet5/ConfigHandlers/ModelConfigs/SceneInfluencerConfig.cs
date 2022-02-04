using OOOReader.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.ConfigHandlers.Common;
using ThreeRingsSharp.Utilities;
using XDataTree.Data;
using XDataTree.TreeElements;

namespace ThreeRingsSharp.ConfigHandlers.ModelConfigs {
	public static class SceneInfluencerConfig {

		private const string SIC_BASE_CLASS = "com.threerings.opengl.scene.config.SceenInfluenceConfig$";

		public static void ReadData(ReadFileContext ctx, ShadowClass modelConfig) {
			ShadowClass viewerConfig = ModelConfig.GetConfigFromFileSC(modelConfig, "com.threerings.opengl.scene.config.SceneInfluencerConfig");

			ShadowClass? vEffectCfg = viewerConfig["effect"];

			#region Data Tree (start)
			GenericElement sceneTreeNode = MasterDataExtractor.SetupBaseInformation(modelConfig, ctx.Push(ctx.File.Name, SilkImage.CameraBolt));
			string type = "null";
			if (vEffectCfg != null) {
				type = vEffectCfg.Signature.Replace(SIC_BASE_CLASS, string.Empty);
			}
			SilkImage icon = SilkImage.Value;
			KeyValueElement kve;
			#endregion

			if (vEffectCfg == null) {
				ctx.Pop();
				kve = new KeyValueElement("Type", type, false, SilkImage.Missing);
				sceneTreeNode.Properties.Add(kve);
				return;
			}

			if (vEffectCfg.IsA(SIC_BASE_CLASS + "AmbientLight")) {
				icon = SilkImage.Missing;
			} else if (vEffectCfg.IsA(SIC_BASE_CLASS + "Definer")) {
				icon = SilkImage.Missing;
			} else if (vEffectCfg.IsA(SIC_BASE_CLASS + "Fog")) {
				icon = SilkImage.Missing;
			} else if (vEffectCfg.IsA(SIC_BASE_CLASS + "Light")) {
				icon = SilkImage.Light;
			} else if (vEffectCfg.IsA(SIC_BASE_CLASS + "Projector")) {
				icon = SilkImage.Missing;
			}

			#region Data Tree (final)
			kve = new KeyValueElement("Type", type, false, icon);
			sceneTreeNode.Properties.Add(kve);
			#endregion

			ctx.Pop(); // :b:op
		}

	}
}
