using com.threerings.config;
using com.threerings.opengl.material.config;
using System;
using System.Collections.Generic;
using System.Linq;
using ThreeRingsSharp.Logging;

namespace ThreeRingsSharp.XansData.XML.ConfigReferences {

	/// <summary>
	/// A container for config references. This behaves like a <see cref="Dictionary{TKey, TValue}"/> where keys are the name of the config file, and values are their corresponding container object.<para/>
	/// For a crude example, indexing this class <c>myConfigReferenceContainer["material"]</c> will return the Clyde data within material.xml
	/// </summary>
	public class ConfigReferenceContainer {

		/// <summary>
		/// All of the existing references.
		/// </summary>
		internal protected readonly Dictionary<string, ManagedConfig[]> References = new Dictionary<string, ManagedConfig[]>();

		/// <summary>
		/// All of the existing references, but indexed by their type.
		/// </summary>
		internal protected readonly Dictionary<Type, ManagedConfig[]> ReferencesByType = new Dictionary<Type, ManagedConfig[]>();

		/// <summary>
		/// A cache from the literal config entry name to the corresponding object. The returned value could be any reference type.
		/// </summary>
		internal protected readonly Dictionary<string, ManagedConfig> ReferencesByEntryName = new Dictionary<string, ManagedConfig>();

		/// <summary>
		/// An array of all of the names that are valid config containers.
		/// </summary>
		public IReadOnlyList<string> ValidNames {
			get {
				// Why this? 
				// When the system is being instantiated, this needs to be cached.
				// Before, this was an init field (ValidNames = method()) but what happened was the singleton container
				// - would be created before loading the rest of the data.
				// If this occurs, and if the data hasn't been created by a previous instance of the program, the method will return null.
				// By repeating this call if it's null, we can fix it.
				if (_ValidNames == null) {
					_ValidNames = ConfigReferenceBootstrapper.GetConfigReferenceNames();
				}
				return _ValidNames;
			}
		}
		private string[] _ValidNames = null;

		/// <summary>
		/// A lookup from entry name (e.g. "Texture/2D/Default") to its container config reference's name ("texture"),
		/// </summary>
		protected internal Dictionary<string, string> ConfigEntryToContainerName = new Dictionary<string, string>();

		/// <summary>
		/// Get the configuration data for the given name, which should be identical to the config dat file name without its extension (e.g. "tile").<para/>
		/// Throws an <see cref="ArgumentException"/> if the given name isn't valid, and an <see cref="ArgumentNullException"/> if the given name is <see langword="null"/>.<para/>
		/// In many cases this will be an <see cref="Array"/> type, but this is not guaranteed.
		/// </summary>
		/// <param name="cfgName">The name of the configuration, identical to its dat name without the extension (e.g. "tile")</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">If the name isn't correct.</exception>
		/// <exception cref="ArgumentNullException">If the name is null.</exception>
		public ManagedConfig[] this[string cfgName] {
			get {
				if (cfgName == null) throw new ArgumentNullException("cfgName");
				cfgName = cfgName.ToLower();
				if (!ValidNames.Contains(cfgName)) {
					throw new ArgumentException("The given index is not valid! (" + cfgName + " is not a valid OOO Configuration name)");
				}
				if (References.ContainsKey(cfgName)) return References[cfgName];

				XanLogger.WriteLine("One sec! I'm loading up the config data for [" + cfgName + "]...");
				XanLogger.UpdateLog(); // UpdateLog call is here since auto-logging might be off here, and this is a status update message
									   // people get antsy when things take time, so it's good to tell them.

				ManagedConfig[] reference = ConfigReferenceBootstrapper.GetConfigReferenceFromName(cfgName);
				// ^ calls the setter down here.
				return reference;
			}
		}

		/// <summary>
		/// Given a <see cref="Type"/> representing a specific config (e.g. <see cref="MaterialConfig"/>), the container for this config and all of its instances will be returned.<para/>
		/// In many cases this will be an <see cref="Array"/> type, but this is not guaranteed.
		/// </summary>
		/// <param name="cfgType"></param>
		/// <returns></returns>
		public ManagedConfig[] this[Type cfgType] {
			get {
				if (ReferencesByType.ContainsKey(cfgType)) return ReferencesByType[cfgType];

				XanLogger.WriteLine("One sec! I'm loading up the config data for [" + cfgType.Name + "]...");
				XanLogger.UpdateLog(); // UpdateLog call is here since auto-logging might be off here, and this is a status update message
									   // people get antsy when things take time, so it's good to tell them.

				ManagedConfig[] reference = ConfigReferenceBootstrapper.GetConfigReferenceFromType(cfgType);
				// ^ calls the setter on the name variant (which sets the type too)
				return reference;
			}
		}

		protected internal void Put(string cfgName, (ManagedConfig[], Type) objAndTypeName) {
			References[cfgName] = objAndTypeName.Item1;
			ReferencesByType[objAndTypeName.Item2] = objAndTypeName.Item1;
		}

		/// <summary>
		/// When given the name of a specific entry (acquired via <see cref="ConfigReference.getName"/>), this will iterate through ALL configs and try to find it.<para/>
		/// Warning: This will be slow if references were not prepopulated. It will cache what results it gets, however, so that should assist for repeated calls.<para/>
		/// Returns <see langword="null"/> if the reference could not be found.<para/>
		/// Note: This will not have any arguments applied and will be populated with default data! You need to make sure you apply the arguments to this, and most importantly, MAKE SURE YOU CLONE THE RESULT OF THIS. Editing the template object may have unwanted side effects!
		/// </summary>
		/// <param name="targetName"></param>
		/// <returns></returns>
		public ManagedConfig TryGetReferenceFromName(string targetName) {
			if (ReferencesByEntryName.ContainsKey(targetName)) return ReferencesByEntryName[targetName];
			foreach (string cfgName in ValidNames) {
				Array configContainer = (Array)this[cfgName];
				ManagedConfig[] container = configContainer.OfType<ManagedConfig>().ToArray();
				foreach (ManagedConfig cfg in container) {
					if (cfg.getName() == targetName) {
						ReferencesByEntryName.Add(targetName, cfg);
						XanLogger.WriteLine("Resolved target " + targetName + " as a " + cfg.GetType().Name, XanLogger.INFO, System.Drawing.Color.Gray);
						return cfg;
					}
				}
			}
			return null;
		}

		public string GetCategoryFromEntryName(string entryName) {
			if (ConfigEntryToContainerName.ContainsKey(entryName)) {
				return ConfigEntryToContainerName[entryName];
			}
			return null;
		}

		internal ConfigReferenceContainer() { }
	}
}
