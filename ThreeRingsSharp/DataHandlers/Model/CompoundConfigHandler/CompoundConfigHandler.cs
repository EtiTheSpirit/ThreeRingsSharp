using com.threerings.math;
using com.threerings.opengl.model.config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.DataHandlers.Model.ConfigReferenceHandler;
using ThreeRingsSharp.DataHandlers.Model.ModelConfigHandlers;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.Utility.Interface;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.XansData.Exceptions;
using static com.threerings.opengl.model.config.CompoundConfig;

namespace ThreeRingsSharp.DataHandlers.Model.CompoundConfigHandler {
	public class CompoundConfigHandler : Singleton<CompoundConfigHandler>, IModelDataHandler, IDataTreeInterface<CompoundConfig> {

		public void SetupCosmeticInformation(CompoundConfig model, DataTreeObject dataTreeParent) {
			ComponentModel[] componentModels = model.models;
			List<object> refs = new List<object>();
			foreach (ComponentModel mdl in componentModels) {
				refs.Add(mdl.model.getName());
			}
			dataTreeParent.AddSimpleProperty(componentModels.Length + " model references", refs.ToArray(), SilkImage.Reference, SilkImage.Reference, false);
		}

		public void HandleModelConfig(FileInfo sourceFile, ModelConfig baseModel, List<Model3D> modelCollection, DataTreeObject dataTreeParent = null, Transform3D globalTransform = null) {
			// ModelConfigHandler.SetupCosmeticInformation(baseModel, dataTreeParent);
			CompoundConfig compound = (CompoundConfig)baseModel.implementation;
			SetupCosmeticInformation(compound, dataTreeParent);

			ComponentModel[] componentModels = compound.models;
			foreach (ComponentModel model in componentModels) {
				ConfigReferenceUtil.HandleComponentModel(sourceFile, model, modelCollection, dataTreeParent, globalTransform);
			}
		}
	}
}
