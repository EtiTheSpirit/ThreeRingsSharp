using OOOReader.Reader;
using OOOReader.Utility.Mathematics;
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
	public static class ViewerAffecterConfig {

		private const string VFC_BASE_CLASS = "com.threerings.opengl.scene.config.ViewerEffectConfig$";

		public static void ReadData(ReadFileContext ctx, ShadowClass modelConfig) {
			ShadowClass viewerConfig = ModelConfig.GetConfigFromFileSC(modelConfig, "com.threerings.opengl.scene.config.ViewerAffecterConfig");

			ShadowClass? vEffectCfg = viewerConfig["effect"];

			#region Data Tree (start)
			GenericElement sceneTreeNode = MasterDataExtractor.SetupBaseInformation(modelConfig, ctx.Push(ctx.File.Name, SilkImage.CameraBolt));
			string type = "null";
			if (vEffectCfg != null) {
				type = vEffectCfg.Signature.Replace(VFC_BASE_CLASS, string.Empty);
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

			string? tip = null;
			bool isSkybox = false;
			if (vEffectCfg.IsA(VFC_BASE_CLASS + "AmbientLightOffset")) {
				icon = SilkImage.Missing;
			} else if (vEffectCfg.IsA(VFC_BASE_CLASS + "BackgroundColor")) {
				icon = SilkImage.Missing; // TODO: Can this be supported? This is an issue because most 3d apps use background color for ambient color too,
										  // so it might not translate nicely
			} else if (vEffectCfg.IsA(VFC_BASE_CLASS + "Particles")) {
				icon = SilkImage.Missing;
			} else if (vEffectCfg.IsA(VFC_BASE_CLASS + "RenderEffect")) {
				icon = SilkImage.Missing;
			} else if (vEffectCfg.IsA(VFC_BASE_CLASS + "Skybox")) {
				icon = SilkImage.Sky;
				isSkybox = true;
			} else if (vEffectCfg.IsA(VFC_BASE_CLASS + "Sound")) {
				icon = SilkImage.SoundWarning;
				tip = "The glTF 2.0 specification does not support sounds (nor do any mainstream extensions to glTF), so this can't be exported.";
				// glTF does not support sound :(
			}

			#region Data Tree (final)
			kve = new KeyValueElement("Type", type, false, icon);
			if (tip != null) kve.Tooltip = tip;
			sceneTreeNode.Properties.Add(kve);
			#endregion

			if (isSkybox) {
				Vector3f offset = new Vector3f(vEffectCfg["translationOrigin"]);
				//Vector3f translationScale = new Vector3f(vEffectCfg["translationScale"]);
				Transform3D offsetTransform = new Transform3D(offset, Quaternion.NewIdentity());

				// Note from old program: This is for parallax scale, not actual model scale
				//Vector3f originalScale = new Vector3f(ctx.CurrentTranslationScale);
				//ctx.CurrentTranslationScale.MultSelf(translationScale);
				ctx.ComposeTransform(offsetTransform);

				// do stuff here
				ShadowClass? model = vEffectCfg["model"];
				if (model != null) {
					ConfigReference cfgRef = new ConfigReference(model);
					MasterDataExtractor.ExtractFrom(ctx, cfgRef);
				}
				//

				//ctx.CurrentTranslationScale.SetTo(originalScale);
			}

			ctx.Pop(); // :b:op
		}

	}
}
