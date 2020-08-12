using com.threerings.math;
using com.threerings.opengl.model.config;
using com.threerings.opengl.scene.config;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.Utility.Interface;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.XansData.Exceptions;
using static com.threerings.opengl.scene.config.ViewerEffectConfig;

namespace ThreeRingsSharp.DataHandlers.Model {

	/// <summary>
	/// Handles instance of <see cref="ViewerAffecterConfig"/> and their associated <see cref="ViewerEffectConfig"/>s.
	/// </summary>
	class ViewerAffecterConfigHandler : Singleton<ViewerAffecterConfigHandler>, IModelDataHandler, IDataTreeInterface<ViewerAffecterConfig> {

		public void SetupCosmeticInformation(ViewerAffecterConfig model, DataTreeObject dataTreeParent) {
			if (dataTreeParent == null) return;
			ViewerEffectConfig effect = model.effect;
			string cls = JavaClassNameStripper.GetWholeClassName(effect.getClass());
			if (cls == null) {
				XanLogger.WriteLine("WARNING: Attempt to get class of ViewerEffectConfig failed!");
				return;
			}

			DataTreeObjectProperty implementationPropKey = dataTreeParent.FindSimpleProperty("Implementation");
			DataTreeObject implementationProp = dataTreeParent.Properties[implementationPropKey].First();
			implementationProp.Text = cls.Replace("$", "::");
			if (effect is Skybox skybox) {
				dataTreeParent.ImageKey = SilkImage.Sky;
				string name = skybox.model?.getName();
				if (name == null && dataTreeParent.Parent != null && dataTreeParent.Parent.ImageKey == SilkImage.Schemed) {
					dataTreeParent.ImageKey = SilkImage.Scheme;
					dataTreeParent.AddSimpleProperty("Data Type", "Render Scheme", SilkImage.Scheme);
				} else {
					// Name may still be null here.
					SilkImage target = name == null ? SilkImage.Missing : SilkImage.ModelSet;
					dataTreeParent.AddSimpleProperty("Model Reference", name, SilkImage.Reference, target, false);
				}

				Transform3D newTrs = new Transform3D(skybox.translationOrigin, Quaternion.IDENTITY, skybox.translationScale.x);
				dataTreeParent.AddSimpleProperty("Transform", newTrs.toString(), SilkImage.Matrix);
			}
		}

		public void HandleModelConfig(FileInfo sourceFile, ModelConfig baseModel, List<Model3D> modelCollection, DataTreeObject dataTreeParent = null, Transform3D globalTransform = null, Dictionary<string, dynamic> extraData = null) {
			ViewerAffecterConfig vac = (ViewerAffecterConfig)baseModel.implementation;
			ViewerEffectConfig effect = vac.effect;
			SetupCosmeticInformation(vac, dataTreeParent);

			SKAnimatorToolsTransfer.IncrementEnd();
			if (effect is Skybox skybox) {
				string filePathRelativeToRsrc = skybox.model?.getName();
				if (filePathRelativeToRsrc != null) {
					// If this is null, it is okay!
					// Certain implementations, (for instance, schemed implementations) use this to define their render scheme.

					if (filePathRelativeToRsrc.StartsWith("/")) filePathRelativeToRsrc = filePathRelativeToRsrc.Substring(1);
					FileInfo referencedModel = new FileInfo(ResourceDirectoryGrabber.ResourceDirectoryPath + filePathRelativeToRsrc);
					if (!referencedModel.Exists) {
						throw new ClydeDataReadException($"ViewerEffectConfig::Skybox at [{ResourceDirectoryGrabber.GetFormattedPathFromRsrc(sourceFile, false)}] attempted to reference [{filePathRelativeToRsrc}], but this file could not be found!");
					}

					// Note to self: DO NOT USE SCALE.
					// The scale value of skyboxes is used for a parallax effect (the scale = "how much does the skybox move relative to the camera")
					// Applying this scale is not proper.
					Transform3D newTrs = new Transform3D(skybox.translationOrigin, Quaternion.IDENTITY);

					// Now one thing to note is that transforms do NOT affect skyboxes.
					// As such, the new translation should NOT be affected by the global transform.
					ClydeFileHandler.HandleClydeFile(referencedModel, modelCollection, false, dataTreeParent, false, newTrs);
				}
			}
			SKAnimatorToolsTransfer.IncrementProgress();
		}
	}
}
