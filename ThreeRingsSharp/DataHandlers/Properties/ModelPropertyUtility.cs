using com.threerings.config;
using com.threerings.opengl.model.config;
using System.Collections.Generic;
using System.Linq;
using static com.threerings.opengl.model.config.ModelConfig;
using static com.threerings.opengl.model.config.ModelConfig.Imported;

namespace ThreeRingsSharp.DataHandlers.Properties {

	/// <summary>
	/// Specialized methods to pull specific data out of models via their directs.
	/// </summary>
	public class ModelPropertyUtility {

		/// <summary>
		/// Set by the main code. If this is <see langword="true"/>, the directs on a given model will be traversed to try to find its textures. If false, only the active materials will be used.
		/// </summary>
		public static bool TryGettingAllTextures { get; set; } = true;

		/// <summary>
		/// Assuming this is a <see cref="ModelConfig"/> storing textures, this will try to find the textures from its parameters.
		/// </summary>
		/// <param name="cfg"></param>
		/// <returns></returns>
		public static List<string> FindTexturesFromDirects(ModelConfig cfg) {
			if (!TryGettingAllTextures) {
				if (cfg.implementation is Imported importedModel) {
					return GetDefaultTextures(importedModel).ToList();
				} else {
					return new List<string>(); // Can't grab anything from this.
				}
			}
			SKAnimatorToolsTransfer.SetProgressState(ProgressBarState.ExtraWork);

			List<string> retn = new List<string>();
			if (cfg.implementation is Imported imported) retn.AddRange(GetDefaultTextures(imported).ToList());

			SKAnimatorToolsTransfer.IncrementEnd(cfg.parameters.Length);
			foreach (Parameter param in cfg.parameters) {
				if (param is Parameter.Choice choice) {
					WrappedChoice wChoice = new WrappedChoice(cfg, choice);
					ParameterizedConfig[] variants = wChoice.CreateVariantsFromOptions();
					foreach (ParameterizedConfig variant in variants) {
						// Each variant is now a dupe of cfg with the given variant information applied to it.
						if (variant is ModelConfig mdlVariant) {
							// This should always be true but it's just here for sanity checking.
							if (mdlVariant.implementation is Imported subImported) {
								retn.AddRange(GetDefaultTextures(subImported));
							}
						}
					}
				}
				SKAnimatorToolsTransfer.IncrementProgress();
			}

			SKAnimatorToolsTransfer.SetProgressState(ProgressBarState.OK);
			return retn;
		}

		/// <summary>
		/// Returns the currently active textures for each of the <see cref="MaterialMapping"/>s within this <see cref="ModelConfig"/>.<para/>
		/// This will only pull the textures actively in use by the model. Any other variants will not be acquired.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		public static string[] GetDefaultTextures(Imported model) {

			string[] textures = new string[model.materialMappings.Length];
			SKAnimatorToolsTransfer.IncrementEnd(textures.Length);

			for (int index = 0; index < model.materialMappings.Length; index++) {
				MaterialMapping mapping = model.materialMappings[index];
				ConfigReference texRef = (ConfigReference)mapping.material.getArguments().getOrDefault("Texture", null);
				if (texRef != null) {
					textures[index] = (string)texRef.getArguments().getOrDefault("File", null);
				}
				SKAnimatorToolsTransfer.IncrementProgress();
			}
			return textures;
		}
	}
}
