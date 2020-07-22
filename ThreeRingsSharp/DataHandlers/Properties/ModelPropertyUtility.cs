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

namespace ThreeRingsSharp.DataHandlers.Properties {

	/// <summary>
	/// Specialized methods to pull specific data out of models via their directs.
	/// </summary>
	public class ModelPropertyUtility {

		public static List<string> FindTexturesFromDirects(ModelConfig cfg) {
			List<string> retn = new List<string>();

			foreach (Parameter param in cfg.parameters) {
				if (param is Parameter.Choice choice) {
					foreach (Parameter.Direct direct in choice.directs) {
						// This will get the object pointed to by the first direct.
						(object, object) directData = TraverseDirectPath(cfg, direct.paths[0]);
						//object directPtr = directData.Item1;
						object directPtrParent = directData.Item2;

						if (directPtrParent is TextureConfig.Original2D.ImageFile) {
							// In this very specific context, we now have 100% confirmation that this direct is undeniably a texture.
							// This is required because if, for whatever reason, someone goes ham with config names and gives some completely
							// random config reference an unfitting name (e.g. they name a tile config container "Texture"), and if I were
							// to directly read the string, it would cause a read error.

							// The basic gist is that this is guaranteed to support any stock clyde behavior. It's extra, but if someone sandboxes
							// SK or something and uses their own naming convention, this is a MUST.
							// Even something as simple as a different name would wreak havoc on the converter if I hardcoded the reference.

							foreach (Parameter.Choice.Option option in choice.options) {
								// n.b. _arguments isn't exposed normally, I edited the IL to expose it as a public member.
								// this code won't work in another setup. if it's java, you need to use reflection
								// if it's C#, you're screwed unless you modify the IL
								// - because it's marked as protected internal which removes it from reflection.
								ArgumentMap args = option._arguments;
								string texture = (string)args.get(direct.name); // Get the argument for the given direct.

								// Now one important thing here is that this is relative to rsrc so...
								retn.Add(ResourceDirectoryGrabber.ResourceDirectoryPath + texture);
								// Add the rest of the path.
							}
						}
					}
				}
			}

			return retn;
		}

		#region Old Stuff

		/// <summary>
		/// Navigates through <paramref name="cfg"/> with the given <paramref name="path"/> as a direct.<para/>
		/// This returns the end value at the path, as well as the value one element up from the bottom of the path (since this can often be used for context)
		/// </summary>
		/// <param name="cfg"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public static (object, object) TraverseDirectPath(object cfg, string path) {
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
						last = ReflectionHelper.Index(last, int.Parse(step));
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
