using com.threerings.math;
using com.threerings.opengl.model.config;
using System.Collections.Generic;
using System.IO;
using ThreeRingsSharp.Logging.Interface;
using ThreeRingsSharp.XansData;
using static com.threerings.opengl.model.config.CompoundConfig;

namespace ThreeRingsSharp.DataHandlers.Model {
	public class CompoundConfigHandler : Singleton<CompoundConfigHandler>, IModelDataHandler, IDataTreeInterface<CompoundConfig> {

		public void SetupCosmeticInformation(CompoundConfig model, DataTreeObject dataTreeParent) {
			ComponentModel[] componentModels = model.models;
			List<object> refs = new List<object>();
			foreach (ComponentModel mdl in componentModels) {
				// Yes, there are cases where this is null.
				if (mdl.model?.getName() != null) refs.Add(mdl.model.getName());
			}
			dataTreeParent.AddSimpleProperty(componentModels.Length + " model references", refs.ToArray(), SilkImage.Reference, SilkImage.Reference, false);
		}

		public void HandleModelConfig(FileInfo sourceFile, ModelConfig baseModel, List<Model3D> modelCollection, DataTreeObject dataTreeParent = null, Transform3D globalTransform = null, Dictionary<string, dynamic> extraData = null) {
			// ModelConfigHandler.SetupCosmeticInformation(baseModel, dataTreeParent);
			CompoundConfig compound = (CompoundConfig)baseModel.implementation;
			SetupCosmeticInformation(compound, dataTreeParent);

			// NEW: Some stuff uses this trick to pick a specific model out of a compound
			// Parameters contain three key bits of data:
			// 1: Directs (short for Directives) -- These store the values that each option edits.
			// 2: Options -- These are the actual options you can pick, and contain a value for each direct that's been defined for this param
			// 3: Choice -- The currently selected option (or default option)

			ComponentModel[] componentModels = compound.models;
			SKAnimatorToolsProxy.IncrementEnd(componentModels.Length);
			foreach (ComponentModel model in componentModels) {
				ConfigReferenceUtil.HandleComponentModel(sourceFile, model, modelCollection, dataTreeParent, globalTransform, true, extraData);
				SKAnimatorToolsProxy.IncrementProgress();
			}
		}
	}
}
