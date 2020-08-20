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

namespace ThreeRingsSharp.DataHandlers.Properties {

	/// <summary>
	/// Specialized methods to pull specific data out of models via their directs.
	/// </summary>
	public class ModelPropertyUtility {

		/// <summary>
		/// Assuming this is a <see cref="ModelConfig"/> storing textures, this will try to find the textures from its parameters.
		/// </summary>
		/// <param name="cfg"></param>
		/// <returns></returns>
		public static List<string> FindTexturesFromDirects(ModelConfig cfg) {
			SKAnimatorToolsProxy.SetProgressState(ProgressBarState.ExtraWork);

			List<string> retn = new List<string>();
			if (cfg.implementation is Imported imported) retn.AddRange(GetDefaultTextures(imported).ToList());

			SKAnimatorToolsProxy.IncrementEnd(cfg.parameters.Length);
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
				SKAnimatorToolsProxy.IncrementProgress();
			}

			SKAnimatorToolsProxy.SetProgressState(ProgressBarState.OK);
			return retn;
		}

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
					WrappedChoice wChoice = new WrappedChoice(cfg, choice);
					ParameterizedConfig[] variants = wChoice.CreateVariantsFromOptions();

					foreach (ParameterizedConfig variant in variants) {
						// Each variant is now a dupe of cfg with the given variant information applied to it.
						if (variant is ModelConfig mdlVariant) {
							// This should always be true but it's just here for sanity checking.
							if (mdlVariant.implementation is Imported subImported) {
								(string[], string) info = GetDefaultTexturesAndActive(subImported, defaultTextureFromVisibleMesh);
								// retn.AddRange(GetDefaultTextures(subImported));
								retn.AddRange(info.Item1);
								defaultTextureFromVisibleMesh = info.Item2; // If it couldn't be found, the method returns what is input.
																			// As such, this will remain unchanged.
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

			/*
			for (int index = 0; index < model.materialMappings.Length; index++) {
				MaterialMapping mapping = model.materialMappings[index];
				MaterialConfig material = mapping.material.ResolveAuto<MaterialConfig>();
				if (material != null) {
					while (material.implementation is MaterialConfig.Derived derivedMtl) {
						material = derivedMtl.material.ResolveAuto<MaterialConfig>();
					}

					if (material.implementation is MaterialConfig.Original originalMtl) {
						foreach (TechniqueConfig technique in originalMtl.techniques) {
							TechniqueConfig.Enqueuer enqueuer = technique.enqueuer;
							if (enqueuer is TechniqueConfig.NormalEnqueuer normalEnq) {
								foreach (PassConfig pass in normalEnq.passes) {
									// Just find the first pass with a texture. It's not possible to have multiple textures right now in gltf
									// that is, on one model
									if (pass.textureState != null) {
										TextureStateConfig texState = pass.textureState;
										if (texState.units.Length > 0) {
											TextureConfig texture = texState.units[0].texture.ResolveAuto<TextureConfig>();
											while (texture.implementation is TextureConfig.Derived derivedTexture) {
												texture = derivedTexture.texture.ResolveAuto<TextureConfig>();
											}

											if (texture.implementation is TextureConfig.Original2D tex2d) {
												TextureConfig.Original2D.Contents contents = tex2d.contents;
												if (contents is TextureConfig.Original2D.ImageFile imageFile) {
													textures[index] = imageFile.file;
												} else {
													// Blank.
													textures[index] = null;
												}
											} else {
												XanLogger.WriteLine("Unsupported texture type " + texture.GetType().Name);
											}
										}
									}
								}
							}
						}
					}
				}
				
				SKAnimatorToolsProxy.IncrementProgress();
			}
			*/
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
