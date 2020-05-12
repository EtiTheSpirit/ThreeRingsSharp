using com.threerings.opengl.model.config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.DataHandlers.Model.ModelConfigHandlers;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.Utility.Interface;
using ThreeRingsSharp.XansData;
using static com.threerings.opengl.model.config.ArticulatedConfig;
using static com.threerings.opengl.model.config.ModelConfig.Imported;

namespace ThreeRingsSharp.DataHandlers.Model.ArticulatedConfigHandlers {
	class ArticulatedConfigHandler : IModelDataHandler, IDataTreeInterface<ArticulatedConfig> {

		/// <summary>
		/// A reference to the singleton instance of <see cref="ArticulatedConfigHandler"/>.
		/// </summary>
		public static ArticulatedConfigHandler Instance { get; } = new ArticulatedConfigHandler();

		public void SetupCosmeticInformation(ArticulatedConfig model, DataTreeObject dataTreeParent) {
			if (dataTreeParent == null) return;

			AnimationMapping[] animMaps = model.animationMappings;
			object[] animations = new object[animMaps.Length];
			for (int idx = 0; idx < animations.Length; idx++) {
				animations[idx] = animMaps[idx].name;
			}
			dataTreeParent.AddSimpleProperty("Animations", animations, SilkImage.Value, SilkImage.Animation, false);
		}

		public void HandleModelConfig(FileInfo sourceFile, ModelConfig baseModel, ref List<Model3D> modelCollection, DataTreeObject dataTreeParent = null) {
			ModelConfigHandler.SetupCosmeticInformation(baseModel, dataTreeParent);
			ArticulatedConfig model = (ArticulatedConfig)baseModel.implementation;
			SetupCosmeticInformation(model, dataTreeParent);

		}
	}
}
