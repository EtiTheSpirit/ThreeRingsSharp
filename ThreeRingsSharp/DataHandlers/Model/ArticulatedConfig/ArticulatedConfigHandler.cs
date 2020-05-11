using com.threerings.opengl.model.config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.Utility.Interface;
using static com.threerings.opengl.model.config.ArticulatedConfig;
using static com.threerings.opengl.model.config.ModelConfig.Imported;

namespace ThreeRingsSharp.DataHandlers.Model.ArticulatedConfigHandler {
	class ArticulatedConfigHandler {

		/// <summary>
		/// A reference to the <see cref="RootDataTreeObject"/> property of the GUI, which contains the model hierarchy.
		/// </summary>
		public static DataTreeObject RootDataTreeObject => ModelConfigBrancher.RootDataTreeObject;

		/// <summary>
		/// Sets up the cosmetic data for this model, or, what's displayed in the GUI for the program.
		/// </summary>
		/// <param name="model"></param>
		public static void SetupCosmeticInformation(ArticulatedConfig model) {
			//RootDataTreeObject.AddSimpleProperty("Scale", model.scale);
			List<object> influences = new List<object>(3);
			if (model.influences.fog) influences.Add(new DataTreeObjectProperty("Fog", SilkImage.Shading));
			if (model.influences.lights) influences.Add(new DataTreeObjectProperty("Lights", SilkImage.Light));
			if (model.influences.projections) influences.Add(new DataTreeObjectProperty("Projections", SilkImage.Texture));
			if (influences.Count > 0) RootDataTreeObject.AddSimpleProperty("Influenced By...", influences.ToArray());

			MaterialMapping[] matMaps = model.materialMappings;
			AnimationMapping[] animMaps = model.animationMappings;
			object[] materials = new object[matMaps.Length];
			object[] animations = new object[animMaps.Length];
			for (int idx = 0; idx < materials.Length; idx++) {
				materials[idx] = matMaps[idx].texture;
			}
			for (int idx = 0; idx < animations.Length; idx++) {
				animations[idx] = animMaps[idx].name;
			}
			RootDataTreeObject.AddSimpleProperty("Textures", materials, SilkImage.Value, SilkImage.Texture);
			RootDataTreeObject.AddSimpleProperty("Animations", animations, SilkImage.Value, SilkImage.Animation);
			XanLogger.WriteLine("Populated property data for root node.");
		}

		/// <summary>
		/// Handles an ArticulatedConfig
		/// </summary>
		/// <param name="model"></param>
		public static void HandleArticulatedConfig(ArticulatedConfig model) {
			SetupCosmeticInformation(model);

			
		}

	}
}
