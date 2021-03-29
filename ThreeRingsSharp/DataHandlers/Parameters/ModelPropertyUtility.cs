using com.threerings.config;
using com.threerings.opengl.material.config;
using com.threerings.opengl.model.config;
using com.threerings.opengl.renderer.config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.XansData.Extensions;
using static com.threerings.opengl.model.config.ModelConfig;
using static com.threerings.opengl.model.config.ModelConfig.Imported;

namespace ThreeRingsSharp.DataHandlers.Parameters {

	/// <summary>
	/// Specialized methods to pull specific data out of models via their directs.
	/// </summary>
	public class ModelPropertyUtility {

		/// <summary>
		/// Assuming this is a <see cref="ModelConfig"/> storing textures, this will try to find the textures from its parameters. Additionally, it will find the default texture of the model.
		/// </summary>
		/// <param name="cfg"></param>
		/// <param name="defaultTextureFromVisibleMesh"></param>
		/// <returns></returns>
		public static (List<string>, string) FindTexturesAndActiveFromDirects(ModelConfig cfg, string defaultTextureFromVisibleMesh) {
			SKAnimatorToolsProxy.SetProgressState(ProgressBarState.ExtraWork);

			List<string> retn = new List<string>();
			if (cfg.implementation is Imported imported) {
				(string[], string) info = GetDefaultTexturesAndActive(imported, defaultTextureFromVisibleMesh);
				retn.AddRange(info.Item1);
				defaultTextureFromVisibleMesh = info.Item2;
			}
			

			SKAnimatorToolsProxy.IncrementEnd(cfg.parameters.Length);
			foreach (Parameter param in cfg.parameters) {
				if (param is Parameter.Choice choice) {
					XChoice cho = new XChoice(cfg, choice);
					// Find a direct that looks like it's got textures.
					foreach (XDirect direct in cho.Directs) {
						XDirect.DirectPointer ptr = direct.GetValuePointer();
						if (ptr.DereferencedPath.Contains("material_mappings") && ptr.DereferencedPath.EndsWith("file")) {
							// bingo!
							foreach (KeyValuePair<string, XChoice.XOption> data in cho.Options) {
								XChoice.XOption option = data.Value;
								object value = option.Original.arguments.getOrDefault(direct.Name, null);
								if (value is string str) {
									retn.Add(str);
								}
							}
						}
					}
					
				}

				SKAnimatorToolsProxy.IncrementProgress();
			}

			SKAnimatorToolsProxy.SetProgressState(ProgressBarState.OK);
			return (retn, defaultTextureFromVisibleMesh);
		}

		/// <summary>
		/// Returns the currently active textures for each of the <see cref="MaterialMapping"/>s within this <see cref="ModelConfig"/>.<para/>
		/// This will only pull the textures actively in use by the model. Any other variants will not be acquired.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		public static string[] GetDefaultTextures(Imported model) {

			string[] textures = new string[model.materialMappings.Length];
			SKAnimatorToolsProxy.IncrementEnd(textures.Length);

			for (int index = 0; index < model.materialMappings.Length; index++) {
				MaterialMapping mapping = model.materialMappings[index];
				ConfigReference texRef = (ConfigReference)mapping.material.getArguments().getOrDefault("Texture", null);
				if (texRef != null) {
					string file = (string)texRef.getArguments().getOrDefault("File", null);
					if (file != null) {
						textures[index] = file;
					}
				}
				SKAnimatorToolsProxy.IncrementProgress();
			}
			return textures;
		}

		public static (string[], string) GetDefaultTexturesAndActive(Imported model, string defFromVisibleMesh) {

			string[] textures = new string[model.materialMappings.Length];
			SKAnimatorToolsProxy.IncrementEnd(textures.Length);

			for (int index = 0; index < model.materialMappings.Length; index++) {
				MaterialMapping mapping = model.materialMappings[index];
				ConfigReference texRef = (ConfigReference)mapping.material.getArguments().getOrDefault("Texture", null);
				if (texRef != null) {
					string file = (string)texRef.getArguments().getOrDefault("File", null);
					if (file != null) {
						textures[index] = file;
						if (mapping.texture == defFromVisibleMesh) {
							defFromVisibleMesh = new FileInfo(file).Name;
						}
					}
				}
				SKAnimatorToolsProxy.IncrementProgress();
			}

			return (textures, defFromVisibleMesh);
		}
	}
}
