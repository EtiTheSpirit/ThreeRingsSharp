﻿using com.google.inject;
using com.threerings.config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.DataHandlers;
using ThreeRingsSharp.DataHandlers.Model;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.XansData.XML.ConfigReferences;

namespace ThreeRingsSharp.XansData.Extensions {
	public static class ConfigReferenceExtension {

		/// <summary>
		/// Returns <see langword="true"/> if this <see cref="ConfigReference"/> points to an actual config, and <see langword="false"/> if it does not (for instance, it points to a model file instead).
		/// </summary>
		/// <param name="cfgRef"></param>
		/// <returns>False if this ConfigReference points to a literal file (points to a .dat file in the rsrc directory), and true if it points to an actual configuration object (e.g. Texture/2D/Default for textures)</returns>
		public static bool IsRealReference(this ConfigReference cfgRef) {
			return !File.Exists(ResourceDirectoryGrabber.ResourceDirectoryPath + cfgRef.getName());
		}

		/// <summary>
		/// Returns <see langword="true"/> if this <see cref="ConfigReference"/> points to a .dat file, and <see langword="false"/> if it does not (for instance, it's being used as an actual reference to a config object).
		/// </summary>
		/// <param name="cfgRef"></param>
		/// <returns>True if this ConfigReference points to a literal file (points to a .dat file in the rsrc directory), and false if it points to an actual configuration object (e.g. Texture/2D/Default for textures)</returns>
		public static bool IsFileReference(this ConfigReference cfgRef) => !IsRealReference(cfgRef);

		/// <summary>
		/// Sets the arguments of the given <see cref="ConfigReference"/>.
		/// </summary>
		/// <param name="cfgRef">The ConfigReference to alter.</param>
		/// <param name="newArgMap">The new ArgumentMap that this ConfigReference will use.</param>
		/// <exception cref="ArgumentNullException">If args is null</exception>
		public static void SetArguments(this ConfigReference cfgRef, ArgumentMap newArgMap) {
			if (newArgMap == null) throw new ArgumentNullException("newArgMap");
			ArgumentMap currentArgs = cfgRef.getArguments();
			currentArgs.clear();
			object[] keys = newArgMap.keySet().toArray();
			foreach (object key in keys) {
				currentArgs.put(key, newArgMap.get(key));
			}
		}

		/// <summary>
		/// Takes the <see cref="ArgumentMap"/> within this <see cref="ConfigReference"/> and translates it into the dictionary format used by TRS's extra data containers.
		/// </summary>
		/// <param name="cfgRef">The ConfigReference to get the data from.</param>
		/// <param name="createSubDict">If true, the returned dictionary has a single key called "DirectArgs" which contains the arguments. If false, the arguments are returned as the dictionary itself.</param>
		/// <returns>A dictionary containing the arguments for the directs on the given destination object.</returns>
		public static Dictionary<string, dynamic> ArgumentsToExtraData(this ConfigReference cfgRef, bool createSubDict = true) {
			Dictionary<string, dynamic> dict = new Dictionary<string, dynamic>();
			ArgumentMap args = cfgRef.getArguments();
			object[] keys = args.keySet().toArray();
			foreach (object key in keys) {
				dict[key.ToString()] = args.get(key);
			}
			if (createSubDict) {
				return new Dictionary<string, dynamic> {
					["DirectArgs"] = dict
				};
			} else {
				return dict;
			}
		}

		#region Resolution

		/// <summary>
		/// Using the information in this <see cref="ConfigReference"/>, the object this reference points to will be resolved and returned. This <see cref="ConfigReference"/> must point to an actual configuration. If it points to a file, an <see cref="InvalidOperationException"/> will be thrown.<para/>
		/// Consider using <see cref="ResolveAs{T}(ConfigReference)"/> if the type of the result is known.<para/>
		/// This will automatically populate the arguments in the referenced config if applicable, making usage of the returned object relatively straightforward.
		/// </summary>
		/// <param name="cfgRef">The ConfigReference to resolve the reference to.</param>
		/// <returns>The ManagedConfig the given ConfigReference is pointing to.</returns>
		/// <exception cref="InvalidOperationException">If IsFileReference() returns true on this ConfigReference.</exception>
		public static ManagedConfig Resolve(this ConfigReference cfgRef) {
			if (cfgRef.IsFileReference()) throw new InvalidOperationException("Cannot resolve the path to a non-config reference (this ConfigReference points to a file!)");
			ManagedConfig mgCfg = ConfigReferenceBootstrapper.ConfigReferences.TryGetReferenceFromName(cfgRef.getName())?.Clone();

			// Populate the values of the referenced object if possible.
			if (mgCfg is ParameterizedConfig paramCfg) {
				// This can store arguments.
				paramCfg.ApplyArguments(cfgRef.getArguments());
				return paramCfg;
			}
			// It's gotta stay as a ManagedConfig
			// Can't make use of this to apply arguments to. How do I handle this?
			// For now: Return the object without any changes.
			return mgCfg;
		}

		/// <summary>
		/// Using the information in this <see cref="ConfigReference"/>, the object this reference points to will be resolved and returned. This <see cref="ConfigReference"/> must point to an actual configuration. If it points to a file, an <see cref="InvalidOperationException"/> will be thrown.<para/>
		/// This will automatically populate the arguments in the referenced config if applicable, making usage of the returned object relatively straightforward.
		/// </summary>
		/// <param name="cfgRef">The ConfigReference to resolve the reference to.</param>
		/// <typeparam name="T">The destination type for the new ManagedConfig</typeparam>
		/// <returns>The ManagedConfig the given ConfigReference is pointing to.</returns>
		/// <exception cref="InvalidOperationException">If IsFileReference() returns true on this ConfigReference.</exception>
		public static T ResolveAs<T>(this ConfigReference cfgRef) where T : ManagedConfig {
			if (cfgRef.IsFileReference()) throw new InvalidOperationException("Cannot resolve the path to a non-config reference (this ConfigReference points to a file!)");
			T mgCfg = ConfigReferenceBootstrapper.ConfigReferences.TryGetReferenceFromName(cfgRef.getName())?.CloneAs<T>();

			// Populate the values of the referenced object if possible.
			if (mgCfg is ParameterizedConfig paramCfg) {
				// This can store arguments.
				paramCfg.ApplyArguments(cfgRef.getArguments());
				return paramCfg as T;
			}
			// It's gotta stay as a ManagedConfig
			// Can't make use of this to apply arguments to. How do I handle this?
			// For now: Return the object without any changes.
			return mgCfg;
		}

		#endregion

		#region Resolution (Files)

		/// <summary>
		/// Attempts to resolve this <see cref="ConfigReference"/>, which is expected to point to a file, and returns the Clyde object that it points to.<para/>
		/// This is intended for use in cases where data must absolutely be loaded in-line. Most cases are better suited for <see cref="ConfigReferenceUtil"/> and its methods.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cfgRef"></param>
		/// <returns></returns>
		public static T ResolveFile<T>(this ConfigReference cfgRef) where T : class {
			if (!cfgRef.IsFileReference()) throw new InvalidOperationException("Cannot resolve this ConfigReference as a file because the file it points to does not exist (or it references an actual config object)!");
			object clydeObject = ClydeFileHandler.GetRaw(new FileInfo(ResourceDirectoryGrabber.ResourceDirectoryPath + cfgRef.getName()));
			if (clydeObject == null) throw new NullReferenceException("Failed to load Clyde file!");
			
			// Apply any arguments.
			if (clydeObject is ParameterizedConfig paramCfg) paramCfg.ApplyArguments(cfgRef.getArguments() ?? new ArgumentMap());
			return clydeObject as T;
		}

		#endregion

	}
}
