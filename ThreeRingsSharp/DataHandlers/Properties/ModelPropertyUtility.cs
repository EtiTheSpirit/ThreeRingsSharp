using com.threerings.config;
using com.threerings.opengl.material.config;
using com.threerings.opengl.model.config;
using com.threerings.opengl.renderer.config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.XansData.Extensions;
using ThreeRingsSharp.XansData.XML.ConfigReferences;
using com.threerings.editor;
using static com.threerings.opengl.model.config.ModelConfig;
using static com.threerings.opengl.model.config.ModelConfig.Imported;
using System.Reflection;
using ThreeRingsSharp.Utility;
using com.threerings.tudey.config;
using java.io;
using static ThreeRingsSharp.DataHandlers.Properties.WrappedDirect;

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

			List<string> retn = new List<string>();

			foreach (Parameter param in cfg.parameters) {
				if (param is Parameter.Choice choice) {
					WrappedChoice wChoice = new WrappedChoice(cfg, choice);
					ParameterizedConfig[] variants = wChoice.CreateVariantsFromOptions();
					foreach (ParameterizedConfig variant in variants) {
						// Each variant is now a dupe of cfg with the given variant information applied to it.
						if (variant is ModelConfig mdlVariant) {
							// This should always be true but it's just here for sanity checking.
							if (mdlVariant.implementation is Imported imported) {
								retn.AddRange(GetDefaultTextures(imported));
							}
						}


						/*
						if (variant is TextureConfig texture && texture.implementation is TextureConfig.Original2D texture2D && texture2D.contents is TextureConfig.Original2D.ImageFile imageFile) {
							retn.Add(imageFile.file);
						}*/
					}
				}
			}

			/*
			foreach (Parameter param in cfg.parameters) {
				if (param is Parameter.Choice choice) {
					WrappedChoice wChoice = new WrappedChoice(cfg, choice);
					Parameter.Direct[] rawDirects = wChoice.BaseChoice.directs;

					// Filter out the directs that deal with materials.
					// We only want the ones that actually do stuff with materials.
					bool isMaterialChoice = false;
					foreach (Parameter.Direct direct in rawDirects) {
						WrappedDirect wrapped = new WrappedDirect(cfg, direct, wChoice);
						foreach (DirectEndReference endRef in wrapped.EndReferences) {
							if (endRef.Object is WrappedDirect subDir) {
								if (subDir.Config is MaterialConfig mtl) {
									isMaterialChoice = true;
									break;
								}
							}
						}
						if (isMaterialChoice) break;
					}

					if (isMaterialChoice) {
						// This choice corresponds to a material!
						// This is stupidly lazy but it works.
						foreach (Parameter.Choice.Option option in wChoice.BaseChoice.options) {
							if (option._arguments.containsKey("Texture")) {
								if (option._arguments.get("Texture") is string tex) {
									if (tex.StartsWith("/")) {
										tex = tex.Substring(1);
									}
									retn.Add(tex);
								}
							}
						}
					}

				}
			}*/

			return retn;
		}

		/*
		public static void RecurseWrappedDirects(WrappedDirect direct, List<string> retn) {
			foreach (DirectEndReference endRef in direct.EndReferences) {
				if (endRef.Object is WrappedDirect wDir) {
					RecurseWrappedDirects(wDir, retn);
				} else if (endRef.Object is ConfigReference cfgRef) {
					PopulateIntoTexture(cfgRef, retn);
				}
			}
		}

		public static void PopulateIntoTexture(ConfigReference textureRef, List<string> retn) {
			// Gotta resolve it.
			ManagedConfig mgTexture = ConfigReferenceBootstrapper.ConfigReferences.TryGetReferenceFromName(textureRef.getName());
			if (mgTexture is TextureConfig texCfg) {
				// I expect a parameter named "File"
				// texCfg is a ParameterizedConfig which means I can use my extension.
				texCfg.ApplyArguments(textureRef.getArguments());
				if (texCfg.implementation is TextureConfig.Original2D original2D) {
					if (original2D.contents is TextureConfig.Original2D.ImageFile imageFile) {
						retn.Add(imageFile.file);
					}
				}
			}
		}
		*/

		/// <summary>
		/// Returns the currently active textures for each of the <see cref="MaterialMapping"/>s within this <see cref="ModelConfig"/>.<para/>
		/// This will only pull the textures actively in use by the model. Any other variants will not be acquired.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		public static string[] GetDefaultTextures(Imported model) {
			string[] textures = new string[model.materialMappings.Length];
			for (int index = 0; index < model.materialMappings.Length; index++) {
				MaterialMapping mapping = model.materialMappings[index];
				ConfigReference texRef = (ConfigReference)mapping.material.getArguments().getOrDefault("Texture", null);
				if (texRef != null) {
					textures[index] = (string)texRef.getArguments().getOrDefault("File", null);
				}
			}
			return textures;
		}

		#region Old Stuff

		/// <summary>
		/// Navigates through <paramref name="cfg"/> with the given <paramref name="path"/> as a direct.<para/>
		/// This returns the end value at the path, as well as the value one element up from the bottom of the path (since this can often be used for context)
		/// </summary>
		/// <param name="cfg"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		[Obsolete("Use WrappedDirect instead.")] public static (object, object) TraverseDirectPath(object cfg, string path) {
			// TODO: Make the second return value a chain, like an ordered list of everywhere that we've traversed.
			// The second to last object is very specific and likely only works in certain limited contexts.

			// This is a super hacky method of doing things.
			path = path.Replace("[", ".[");
			// This will now split things in brackets.

			string[] steps = path.Split('.');
			object beforeLast = null;
			object last = cfg;
			string lastName = null;
			object directOnLastStep = null;
			int currentStep = 0;

			foreach (string s in steps) {
				string step = s.SnakeToCamel();

				// Brackets either denote an index to an array or a reference to a parameter, so they get special handling.
				if (s.StartsWith("[") && s.EndsWith("]")) {
					step = step.BetweenBrackets(); // Get the text between the brackets.
					if (step.StartsWith("\"") && step.EndsWith("\"")) {
						// If it has quotes around it, then this references a direct on whatever the last step was.
						// This implies that the last step was likely a reference to a config of its own, so we need to actually get that
						if (directOnLastStep == null) {
							// TODO: DO NOT USE LASTNAME!!!
							// This is a very strange method of getting it, and relies on naming convention
							// For instance, implementation.material_mappings[0].texture
							// It looks at "texture" to realize it should look at the texture xml.
							// The proper method would be to create a map from every xml to its root type, and then actually look at the property
							// - of the object and grab the config reference by type.

							if (last == null) {
								XanLogger.WriteLine("Alert: Unable to locate the data for " + path, false, System.Drawing.Color.DarkGoldenrod);
								return (null, null);
							}

							(object, object) data;
							if (ConfigReferenceBootstrapper.ConfigReferences.ValidNames.Contains(lastName ?? "")) {
								// Explicit name grab.
								data = ReflectionHelper.GetOOOConfigAndParameter(last, step.Substring(1, step.Length - 2), lastName);
							} else {
								// Search.
								data = ReflectionHelper.GetOOOConfigAndParameter(last, step.Substring(1, step.Length - 2));
							}

							object referencedConfigInstance = data.Item1; // This is the instance of whatever OOO Config was selected.
																		  // ^ In the context of the direct pointing to something, e.g. texture.xml, this is the specific texture config
																		  // (since the xml contains dozens of them)

							object parameter = data.Item2; // This is the parameter with the given name (lastName) in referencedConfigInstance
														   // This specific data is acquired via calling referencedConfigInstance.getParameter(lastName);

							last = referencedConfigInstance;
							directOnLastStep = parameter;
						} else {
							// So now we're inside of another parameter (that's what last is)
							string[] paths = ReflectionHelper.GetPaths(directOnLastStep);
							string firstPath = paths.FirstOrDefault();
							(object, object) nextPathData = TraverseDirectPath(last, firstPath);
							object endOfNextPath = nextPathData.Item1;
							if (endOfNextPath == null) {
								// Something wasn't able to be located.
								XanLogger.WriteLine("Alert: Unable to locate the data for " + path, false, System.Drawing.Color.DarkGoldenrod);
								return (null, null);
							}
							// Now in this context, we've just traversed a path in a chain of references.
							// In the case of materials, this looks a bit like ["Texture"]["File"]
							// Now do note, this case is a bit odd since when we make it here, we're actually on "File", but this references Texture
							// The reason this references Texture is because we spent the last cycle running the if statement above.
							// So this makes this part of the code sort of "lag behind" if you will.
							// That is, instead of traversing through "Texture" itself, we just replaced it with the actual real path defined in its directs.
							// So now, we have that path, and now we want to try to find a direct within this called "File".
							// To put it in other terms, this changes it from something like:
							// ["Texture"]["File"]
							// To something like:
							// implementation.techniques[0].enqueuer.passes[0].texture_state.units[0].texture["File"]

							// As such, endOfNextPath is where "Texture" points.
							// So now, access the direct in this object with the current name, which is "File" in this context.

							// We have to use a hacky method of getting the config name from the path.
							string firstPathAlt = firstPath.Replace("[", ".[");
							string configName = firstPathAlt.Split('.').Last();
							(object, object) data;
							// We have to be super careful here. Avoid calling the overall grabber that instantiates everything.
							if (ConfigReferenceBootstrapper.ConfigReferences.ValidNames.Contains(lastName ?? "")) {
								// Explicit name grab.
								data = ReflectionHelper.GetOOOConfigAndParameter(endOfNextPath, step.Substring(1, step.Length - 2), lastName);
							} else {
								// Search.
								data = ReflectionHelper.GetOOOConfigAndParameter(endOfNextPath, step.Substring(1, step.Length - 2));
							}


							object referencedConfigInstance = data.Item1;
							// ^ This is the instance of whatever OOO Config was selected.
							// In the context of the direct pointing to something, e.g. texture.xml, this is the specific texture config
							// (since the xml contains dozens of them)

							// So now, we have to use File.
							// In this case, File is going to be a parameter.
							if (referencedConfigInstance != null) {
								Parameter[] parms = (Parameter[])((dynamic)referencedConfigInstance).parameters;
								Parameter param = parms.Where(p => p.name == step.Substring(1, step.Length - 2)).FirstOrDefault();
								if (param is Parameter.Direct direct) {
									string allNewPath = direct.paths.FirstOrDefault();
									(object, object) next = TraverseDirectPath(referencedConfigInstance, allNewPath);
									beforeLast = next.Item2;
									last = next.Item1;
								} else {
									beforeLast = last;
									last = referencedConfigInstance;
								}
							} else {
								beforeLast = last;
								last = referencedConfigInstance;
							}
						}
					} else {
						// This is an index.
						beforeLast = last;
						last = ReflectionHelper.GetArray(last, int.Parse(step));
						directOnLastStep = null;
					}

				} else {
					// This is a field.
					beforeLast = last;
					if (last == null) {
						// Catch case: The object ceased to exist. Can't get the data.
						return (null, null);
					}
					last = ReflectionHelper.Get(last, step);
					directOnLastStep = null;
				}
				lastName = step;
				currentStep++;
			}

			return (last, beforeLast);
		}

		#endregion

	}
}
