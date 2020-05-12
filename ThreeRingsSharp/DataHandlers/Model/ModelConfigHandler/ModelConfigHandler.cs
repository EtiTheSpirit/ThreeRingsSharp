using com.threerings.opengl.model.config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility.Interface;
using static com.threerings.opengl.model.config.ModelConfig;
using static com.threerings.opengl.model.config.ModelConfig.Imported;
using static com.threerings.opengl.model.config.ModelConfig.Implementation;

namespace ThreeRingsSharp.DataHandlers.Model.ModelConfigHandlers {
	public class ModelConfigHandler {

		/// <summary>
		/// Sets up the cosmetic data for this model, or, what's displayed in the GUI for the program.<para/>
		/// This specific method populates data that is common across all imported models.
		/// </summary>
		/// <param name="model">The model containing other data, such as the <see cref="Implementation"/>.</param>
		/// <param name="dataTreeParent">This is the instance in the data tree that represents this object in the hierarchy. If null, this method call is skipped.</param>
		public static void SetupCosmeticInformation(ModelConfig model, DataTreeObject dataTreeParent) {
			if (dataTreeParent == null) return;
			Implementation imp = model.implementation;
			Imported imported = (Imported)imp;

			//RootDataTreeObject.AddSimpleProperty("Scale", model.scale);
			List<object> influences = new List<object>(3);
			if (imported.influences.fog) influences.Add(new DataTreeObjectProperty("Fog", SilkImage.Shading));
			if (imported.influences.lights) influences.Add(new DataTreeObjectProperty("Lights", SilkImage.Light));
			if (imported.influences.projections) influences.Add(new DataTreeObjectProperty("Projections", SilkImage.Texture));
			if (influences.Count > 0) dataTreeParent.AddSimpleProperty("Influenced By...", influences.ToArray(), displaySinglePropertiesInline: false);

			MaterialMapping[] matMaps = imported.materialMappings;
			object[] materials = new object[matMaps.Length];
			for (int idx = 0; idx < materials.Length; idx++) {
				materials[idx] = matMaps[idx].texture;
			}
			dataTreeParent.AddSimpleProperty("Textures", materials, SilkImage.Value, SilkImage.Texture, false);
		}

	}
}
