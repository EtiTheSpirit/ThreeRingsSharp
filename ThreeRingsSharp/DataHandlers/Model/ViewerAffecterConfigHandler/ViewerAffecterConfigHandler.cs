﻿using com.threerings.math;
using com.threerings.opengl.model.config;
using com.threerings.opengl.scene.config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.Utility.Interface;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.XansData.Exceptions;
using static com.threerings.opengl.scene.config.ViewerEffectConfig;

namespace ThreeRingsSharp.DataHandlers.Model.ViewerAffecterConfigHandlers {
	class ViewerAffecterConfigHandler : Singleton<ViewerAffecterConfigHandler>, IModelDataHandler, IDataTreeInterface<ViewerAffecterConfig> {

		public void SetupCosmeticInformation(ViewerAffecterConfig model, DataTreeObject dataTreeParent) {
			ViewerEffectConfig effect = model.effect;
			string cls = ClassNameStripper.GetWholeClassName(effect.getClass());
			if (cls == null) {
				XanLogger.WriteLine("WARNING: Attempt to get class of ViewerEffectConfig failed!");
				return;
			}

			DataTreeObjectProperty implementationPropKey = dataTreeParent.FindSimpleProperty("Implementation");
			DataTreeObject implementationProp = dataTreeParent.Properties[implementationPropKey].First();
			implementationProp.Text = cls.Replace("$", "::");
			if (effect is Skybox skybox) {
				implementationPropKey.ImageKey = SilkImage.Sky;
				dataTreeParent.AddSimpleProperty("Model Reference", skybox.model.getName(), SilkImage.Reference, SilkImage.ModelSet, false);
			}
		}

		public void HandleModelConfig(FileInfo sourceFile, ModelConfig baseModel, List<Model3D> modelCollection, DataTreeObject dataTreeParent = null, Transform3D globalTransform = null) {
			ViewerAffecterConfig vac = (ViewerAffecterConfig)baseModel.implementation;
			ViewerEffectConfig effect = vac.effect;
			SetupCosmeticInformation(vac, dataTreeParent);

			if (effect is Skybox skybox) {
				string filePathRelativeToRsrc = skybox.model.getName();
				if (filePathRelativeToRsrc.StartsWith("/")) filePathRelativeToRsrc = filePathRelativeToRsrc.Substring(1);
				FileInfo referencedModel = new FileInfo(ResourceDirectoryGrabber.ResourceDirectoryPath + filePathRelativeToRsrc);
				if (!referencedModel.Exists) {
					throw new ClydeDataReadException($"CompoundConfig at [{ResourceDirectoryGrabber.GetFormattedPathFromRsrc(sourceFile, false)}] attempted to reference [{filePathRelativeToRsrc}], but this file could not be found!");
				}

				// Note to self: You only use the x component on scale for a reason.
				// For some reason, skybox scale is internally stored as a Vector3. I assume this is because they thought they'd need to stretch skyboxes.
				// In all implementations from the scene viewer, it only uses the x component for a single-float scale.
				// Why? Ask OOO. This is how it needs to work in seemingly 100% of cases with SK stuff.
				Transform3D newTrs = new Transform3D(skybox.translationOrigin, Quaternion.IDENTITY, skybox.translationScale.x);
				newTrs = globalTransform.compose(newTrs);
				ClydeFileHandler.HandleClydeFile(referencedModel, modelCollection, false, dataTreeParent, false, newTrs);
			}
		}
	}
}
