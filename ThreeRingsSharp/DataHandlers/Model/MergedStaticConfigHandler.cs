using com.threerings.math;
using com.threerings.opengl.model.config;
using System.Collections.Generic;
using System.IO;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.Utility.Interface;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.XansData.Exceptions;
using static com.threerings.opengl.model.config.CompoundConfig;

namespace ThreeRingsSharp.DataHandlers.Model {
	public class MergedStaticConfigHandler : Singleton<MergedStaticConfigHandler>, IModelDataHandler, IDataTreeInterface<MergedStaticConfig> {

		public void SetupCosmeticInformation(MergedStaticConfig model, DataTreeObject dataTreeParent) {
			ComponentModel[] componentModels = model.models;
			List<object> refs = new List<object>();
			foreach (ComponentModel mdl in componentModels) {
				refs.Add(mdl.model.getName());
			}
			dataTreeParent.AddSimpleProperty(componentModels.Length + " model references", refs.ToArray(), SilkImage.Reference, SilkImage.Reference, false);
		}

		public void HandleModelConfig(FileInfo sourceFile, ModelConfig baseModel, List<Model3D> modelCollection, DataTreeObject dataTreeParent = null, Transform3D globalTransform = null, Dictionary<string, dynamic> extraData = null) {
			MergedStaticConfig mergedStatic = (MergedStaticConfig)baseModel.implementation;

			ComponentModel[] componentModels = mergedStatic.models;
			SKAnimatorToolsProxy.IncrementEnd(componentModels.Length);
			foreach (ComponentModel model in componentModels) {
				string filePathRelativeToRsrc = model.model.getName();
				if (filePathRelativeToRsrc.StartsWith("/")) filePathRelativeToRsrc = filePathRelativeToRsrc.Substring(1);
				FileInfo referencedModel = new FileInfo(ResourceDirectoryGrabber.ResourceDirectoryPath + filePathRelativeToRsrc);
				if (!referencedModel.Exists) {
					throw new ClydeDataReadException($"CompoundConfig at [{ResourceDirectoryGrabber.GetFormattedPathFromRsrc(sourceFile, false)}] attempted to reference [{filePathRelativeToRsrc}], but this file could not be found!");
				}
				Transform3D newTrs = model.transform;
				newTrs = globalTransform.compose(newTrs);
				ClydeFileHandler.HandleClydeFile(referencedModel, modelCollection, false, dataTreeParent, transform: newTrs);
				SKAnimatorToolsProxy.IncrementProgress();
			}
		}


	}
}
