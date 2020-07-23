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
		public string ConfigReferenceContainer { get; }

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
		/// Given a full path from a <see cref="Parameter.Direct"/>, this will traverse it and acquire the data at the end.<para/>
		/// This will stop if it runs into another direct and instantiate a new <see cref="WrappedDirect"/>. This will occur if there is a reference chain, for instance, in many textures it references material["Texture"] (a direct) followed by a second direct ["File"]. Since each may have multiple paths, it's best to reference a new <see cref="WrappedDirect"/>.
		/// </summary>
		/// <param name="path"></param>
		private void TraverseDirectPath(string path) {
			// implementation.material_mappings[0].material["Texture"]["File"]
			// StringExtensions.SnakeToCamel();

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

						// First things first: Resolve the config reference.
						string configRefPath = latestAsCfg.getName();
						ParameterizedConfig referencedConfig = ConfigReferenceBootstrapper.ConfigReferences.TryGetReferenceFromName(configRefPath) as ParameterizedConfig;

						ArgumentMap args = latestAsCfg.getArguments();
						if (Arguments != null) {
							object[] keys = Arguments.keySet().toArray();
							foreach (object key in keys) {
								args.put(key, Arguments.get(key)); // Inherit the args.
							}
						}
						
						// So there's our reference. Now we need to get a parameter from it.
						Parameter referencedParam = referencedConfig.getParameter(parameterName);
						if (referencedParam is Parameter.Direct referencedDirect) {
							_EndReferences.Add(new DirectEndReference(new WrappedDirect(referencedConfig, referencedDirect, null, args)));
						} else if (referencedParam is Parameter.Choice referencedChoice) {
							_EndReferences.Add(new DirectEndReference(new WrappedChoice(referencedConfig, referencedChoice)));
						}
						return;
					}
				} else {
					// This is referencing a property.
					latestObject = ReflectionHelper.Get(latestObject, currentIndex);
				}
			}

			// Now here's something important: Does an argument override this?
			if (Arguments.containsKey(Name)) {
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
