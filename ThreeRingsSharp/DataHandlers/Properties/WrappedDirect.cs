using com.threerings.config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.XansData.Extensions;
using ThreeRingsSharp.Utility;
using com.threerings.opengl.material.config;
using static com.threerings.opengl.model.config.ModelConfig.Imported;
using ThreeRingsSharp.XansData.XML.ConfigReferences;
using System.Diagnostics;
using com.threerings.opengl.renderer.config;

namespace ThreeRingsSharp.DataHandlers.Properties {

	/// <summary>
	/// Represents a <see cref="Parameter.Direct"/> in a different manner that allows directly
	/// accessing the object or data it points to.<para/>
	/// <para/>
	/// WARNING: Object not functional entirely.
	/// </summary>
	public class WrappedDirect {

		/// <summary>
		/// The reference to the Clyde object associated with this direct.
		/// </summary>
		public ParameterizedConfig Config { get; }

		/// <summary>
		/// A reference to <see cref="BaseDirect"/>'s <c>name</c> property (see <see cref="Parameter.name"/>).
		/// </summary>
		public string Name => BaseDirect.name;

		/// <summary>
		/// The raw text-based paths for this direct.
		/// </summary>
		public string[] Paths { get; set; }

		/// <summary>
		/// If this direct references a ConfigReference, and if that ConfigReference points to one of the packed configs, this is the name of the packed config (e.g. <c>"material"</c>)
		/// </summary>
		public string ConfigReferenceContainerName { get; private set; }

		/// <summary>
		/// If this is non-null, this is a reference to the parent <see cref="Parameter.Choice"/> that contains this <see cref="WrappedDirect"/>.
		/// </summary>
		public WrappedChoice ParentChoice { get; }

		/// <summary>
		/// A reference to the <see cref="Parameter.Direct"/> that this <see cref="WrappedDirect"/> was created from.
		/// </summary>
		public Parameter.Direct BaseDirect { get; }

		public ArgumentMap Arguments { get; } = null;

		/// <summary>
		/// The object at the end of each path in this <see cref="WrappedDirect"/>.<para/>
		/// Some of these objects may be other <see cref="WrappedDirect"/>s or <see cref="WrappedChoice"/>s. See <see cref="DirectEndReference"/> for more information on how to handle this.
		/// </summary>
		public IReadOnlyList<DirectEndReference> EndReferences => _EndReferences;
		private List<DirectEndReference> _EndReferences = new List<DirectEndReference>();

		public WrappedDirect(ParameterizedConfig cfg, Parameter.Direct direct, WrappedChoice parentChoice = null, ArgumentMap args = null) {
			Config = cfg;
			BaseDirect = direct;
			ParentChoice = parentChoice;
			Arguments = args;

			foreach (string path in direct.paths) {
				TraverseDirectPath(path);
			}
		}

		/// <summary>
		/// Since Directs always point to a specific value, it is possible to get this value.<para/>
		/// Some directs may branch into more than one sub-direct, but since it is not possible to select which branch is traversed, all values are the same.
		/// </summary>
		public object GetValue() {
			if (EndReferences.Count == 0) return null;
			DirectEndReference endRef = EndReferences[0];
			while (endRef.Object is WrappedDirect subDir) {
				if (subDir.EndReferences.Count == 0) return null;
				endRef = subDir.EndReferences[0];
			}
			return endRef.Object;
		}

		/// <summary>
		/// Assuming this <see cref="WrappedDirect"/> has arguments, this will apply the arguments to <see cref="Config"/>.
		/// </summary>
		public void ApplyArgs() {
			SetDataOn(Config, BaseDirect.paths, Arguments?.getOrDefault(BaseDirect.name, null));
		}

		/// <summary>
		/// Given a full path from a <see cref="Parameter.Direct"/>, this will traverse it and acquire the data at the end.<para/>
		/// This will stop if it runs into another direct and instantiate a new <see cref="WrappedDirect"/>. This will occur if there is a reference chain, for instance, in many textures it references material["Texture"] (a direct) followed by a second direct ["File"]. Since each may have multiple paths, it's best to reference a new <see cref="WrappedDirect"/>.
		/// </summary>
		/// <param name="path"></param>
		private void TraverseDirectPath(string path) {
			// implementation.material_mappings[0].material["Texture"]["File"]

			// A bit of a hack to make splitting this path easier:
			path = path.Replace("[", ".[");

			// The latest object stored when traversing this direct's path.
			object latestObject = Config;

			// Split it by the segments of this path, and get rid of the implementation word at the start if needed.
			string[] pathSegments = path.Split('.');
			if (pathSegments[0] == "implementation") {
				latestObject = ReflectionHelper.Get(latestObject, "implementation");
				pathSegments = pathSegments.Skip(1).ToArray();
			}

			for (int idx = 0; idx < pathSegments.Length; idx++) {
				string currentIndex = pathSegments[idx].SnakeToCamel();
				string betweenBrackets = currentIndex.BetweenBrackets();
				if (betweenBrackets != null) {
					// This is either an array index, or a reference to a config.
					// The simple way to test this is that if it's a numeric index, it's an array index.
					if (int.TryParse(betweenBrackets, out int arrayIndex)) {
						// Access this array index. It is a number in brackets like [0]
						latestObject = ReflectionHelper.Index(latestObject, arrayIndex);
					} else {
						// Access the config reference. This is branching from a config reference and accesses a parameter ["Parameter Name"]
						ConfigReference latestAsCfg = (ConfigReference)latestObject;
						string parameterName = betweenBrackets.Substring(1, betweenBrackets.Length - 2);

						// First things first: Resolve the config reference (AND CLONE IT. Don't edit the template object!)
						string configRefPath = latestAsCfg.getName();
						ParameterizedConfig referencedConfig = (ParameterizedConfig)ConfigReferenceBootstrapper.ConfigReferences.TryGetReferenceFromName(configRefPath).clone();

						ArgumentMap args = latestAsCfg.getArguments();

						// So there's our reference. Now we need to get a parameter from it.
						ConfigReferenceContainerName = ConfigReferenceBootstrapper.ConfigReferences.GetCategoryFromEntryName(configRefPath);
						Parameter referencedParam = referencedConfig.getParameter(parameterName);
						if (referencedParam is Parameter.Direct referencedDirect) {
							_EndReferences.Add(new DirectEndReference(new WrappedDirect(referencedConfig, referencedDirect, null, args)));
						} else if (referencedParam is Parameter.Choice referencedChoice) {
							_EndReferences.Add(new DirectEndReference(new WrappedChoice(referencedConfig, referencedChoice, args)));
						}
						return;
					}
				} else {
					// This is referencing a property.
					latestObject = ReflectionHelper.Get(latestObject, currentIndex);
				}
			}

			// Now here's something important: Does an argument override this?
			if (Arguments != null && Arguments.containsKey(Name)) {
				// This direct is included as an argument...
				// And if we're down here, then we're not referencing another direct, we're referencing a property.
				// But as a final sanity check:
				if (latestObject.GetType() == Arguments.get(Name).GetType()) {
					// Yep! Our argument is the same type as the latestObject.
					// Let's set latestObject to that argument.
					latestObject = Arguments.get(Name);
				}
			}
			_EndReferences.Add(new DirectEndReference(latestObject));
		}

		/// <summary>
		/// Given a <see cref="ParameterizedConfig"/> and a set of paths from a direct as well as a target value, this will modify the <see cref="ParameterizedConfig"/> so that its fields reflect the given value.
		/// </summary>
		/// <param name="config"></param>
		/// <param name="paths"></param>
		/// <param name="argValue"></param>
		/// <param name="setToNull">If true, and if argValue is null, the property will actually be set to null (if this is false, it will skip applying it)</param>
		public static void SetDataOn(ParameterizedConfig config, string[] paths, object argValue, bool setToNull = false) {
			if (argValue == null && !setToNull) return;
			foreach (string path in paths) {
				SetDataOn(config, path, argValue, setToNull);
			}
		}

		/// <summary>
		/// Given a <see cref="ParameterizedConfig"/> and a path from a direct as well as a target value, this will modify the <see cref="ParameterizedConfig"/> so that its fields reflect the given value.
		/// </summary>
		/// <param name="config"></param>
		/// <param name="path"></param>
		/// <param name="argValue"></param>
		/// <param name="setToNull">If true, and if argValue is null, the property will actually be set to null (if this is false, it will skip applying it)</param>
		public static void SetDataOn(ParameterizedConfig config, string path, object argValue, bool setToNull = false) {
			if (argValue == null && !setToNull) return;
			// implementation.material_mappings[0].material["Texture"]["File"]

			// A bit of a hack to make splitting this path easier:
			path = path.Replace("[", ".[");

			// The latest object stored when traversing this direct's path.
			object latestObject = config;
			string previousIndex = null;
			object previousObject = null;

			// Split it by the segments of this path, and get rid of the implementation word at the start if needed.
			string[] pathSegments = path.Split('.');
			if (pathSegments[0] == "implementation") {
				latestObject = ReflectionHelper.Get(latestObject, "implementation");
				pathSegments = pathSegments.Skip(1).ToArray();
			}

			for (int idx = 0; idx < pathSegments.Length; idx++) {
				string currentIndex = pathSegments[idx].SnakeToCamel();
				string betweenBrackets = currentIndex.BetweenBrackets();
				if (betweenBrackets != null) {
					// This is either an array index, or a reference to a config.
					// The simple way to test this is that if it's a numeric index, it's an array index.
					if (int.TryParse(betweenBrackets, out int arrayIndex)) {
						// Access this array index. It is a number in brackets like [0]
						latestObject = ReflectionHelper.Index(latestObject, arrayIndex);
						previousObject = latestObject;
					} else {
						// Access the config reference. This is branching from a config reference and accesses a parameter ["Parameter Name"]
						ConfigReference latestAsCfg = (ConfigReference)latestObject;
						string parameterName = betweenBrackets.Substring(1, betweenBrackets.Length - 2);

						// First things first: Resolve the config reference.
						string configRefPath = latestAsCfg.getName();

						// cloning this is super important as the tryget method will return a template object.
						// Do not edit the template!
						ParameterizedConfig referencedConfig = (ParameterizedConfig)ConfigReferenceBootstrapper.ConfigReferences.TryGetReferenceFromName(configRefPath).clone();

						// So there's our reference. Now we need to get a parameter from it.
						Parameter referencedParam = referencedConfig.getParameter(parameterName);
						if (referencedParam is Parameter.Direct referencedDirect) {
							SetDataOn(referencedConfig, referencedDirect.paths, argValue);

							// This is by far one of the most hacky methods I've ever done in OOO stuff.
							// So basically, a model has a property for something like say, materials.
							// This property is a ConfigReference to a material object, and that ConfigReference has arguments in it
							//    that tell the referenced material what it should be.
							// Rather than trying to traverse that ConfigReference and set the data on the remote object (PAINFUL), I 
							//    instead decided to write a system that can wrap any ParameterizedConfig into a ConfigReference and just
							//    call it a day.
							ReflectionHelper.Set(previousObject, previousIndex, ConfigReferenceConstructor.MakeConfigReferenceTo(referencedConfig));
						} else {
							throw new NotImplementedException("Cannot set data on referenced parameters that are not Directs (yet).");
						}
						return;
					}
				} else {
					// This is referencing a property.
					// But wait: If this is the second to last object, then we gotta modify it.
					if (idx == pathSegments.Length - 1) {
						// Second to last object. latestObject will contain the property that we want to set.
						// Let's manually find that field and set it
						if (currentIndex.BetweenBrackets() == null) {
							// We're good here.
							object ptr = ReflectionHelper.Get(previousObject, currentIndex);
							if (ptr is ConfigReference cfgRef) {
								// Special handling. argValue goes to a property on the config reference
								ManagedConfig mgCfg = ConfigReferenceBootstrapper.ConfigReferences.TryGetReferenceFromName(cfgRef.getName());
									
								if (mgCfg is TextureConfig texCfg) {
									// Special handling for textures. System wants to set the ConfigReference to a string of the file path.
									// Instead of this, I will create the new object (via the bootstrapper), set its data, and then set the texture to that.
									if (texCfg.implementation is TextureConfig.Original2D image2d && image2d.contents is TextureConfig.Original2D.ImageFile) {
										cfgRef.getArguments().put("File", argValue);
										return;
									}
								} else {
									throw new NotImplementedException("Not sure how to handle this data yet. Sorry!");
								}
								
							}
							ReflectionHelper.Set(previousObject, currentIndex, argValue);
							return;
						}
					}
					if (previousIndex != null) {
						if (int.TryParse(previousIndex.BetweenBrackets(), out int _) && idx == pathSegments.Length - 1) {
							if (currentIndex.BetweenBrackets() == null) {
								// We're good here.
								ReflectionHelper.Set(previousObject, previousIndex, argValue);
								return;
							}
						}
					}
					previousObject = latestObject;
					latestObject = ReflectionHelper.Get(latestObject, currentIndex);
				}
				previousIndex = currentIndex;
			}
		}

		/// <summary>
		/// An object at the end of a direct path. This is used because in some cases, the end of a direct chain might be early because it points to another direct (which needs to be resolved separately)<para/>
		/// Rather than storing <see cref="object"/>s in <see cref="WrappedDirect.EndReferences"/>, this is used so that developers can easily discern between cases like mentioned above and know whether to continue traversing or that they truly have their end object.
		/// </summary>
		public class DirectEndReference {

			/// <summary>
			/// Discerns what type of object <see cref="Object"/> is, which will either be a generic object (your end goal), a <see cref="WrappedDirect"/>, or a <see cref="WrappedChoice"/>.
			/// </summary>
			public ReferenceType Reference { get; } = ReferenceType.Object;

			/// <summary>
			/// The object that the <see cref="WrappedDirect"/> containing this <see cref="DirectEndReference"/> is pointing to. This may need to be cast to a <see cref="WrappedDirect"/> or <see cref="WrappedChoice"/> depending on <see cref="Reference"/>'s value.
			/// </summary>
			public object Object { get; }

			public DirectEndReference(object endObject) {
				Object = endObject;
				if (endObject is WrappedDirect) {
					Reference = ReferenceType.Direct;
				} else if (endObject is WrappedChoice) {
					Reference = ReferenceType.Choice;
				}
			}


			/// <summary>
			/// Used to discern what a <see cref="DirectEndReference"/> is referencing.
			/// </summary>
			public enum ReferenceType {
				/// <summary>
				/// The <see cref="DirectEndReference"/> associated with this <see cref="ReferenceType"/> is a generic object.
				/// </summary>
				Object,

				/// <summary>
				/// The <see cref="DirectEndReference"/> associated with this <see cref="ReferenceType"/> is a <see cref="WrappedDirect"/>.
				/// </summary>
				Direct,

				/// <summary>
				/// The <see cref="DirectEndReference"/> associated with this <see cref="ReferenceType"/> is a <see cref="WrappedChoice"/>.
				/// </summary>
				Choice
			}

		}

	}
}
