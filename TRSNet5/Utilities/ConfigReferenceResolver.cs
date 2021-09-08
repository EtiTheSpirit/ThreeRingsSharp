using OOOReader.Clyde;
using OOOReader.Exceptions;
using OOOReader.Reader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.ConfigHandlers.Presets;
using ThreeRingsSharp.Utilities.Parameters;
using ThreeRingsSharp.Utilities.Parameters.Implementation;

namespace ThreeRingsSharp.Utilities {

	/// <summary>
	/// ConfigReference is a type used in Clyde to politely reference another file without actually loading it. This class provides a method that will actually load it and return the appropriate data.
	/// </summary>
	public static class ConfigReferenceResolver {

		/// <summary>
		/// Returns the object pointed to by this <see cref="ShadowClass"/> representing a ConfigReference. Additionally, this
		/// adds a field named <c>__reference</c> onto the input <see cref="ShadowClass"/> for caching.
		/// </summary>
		/// <param name="shadow">The <see cref="ShadowClass"/> representing the ConfigReference.</param>
		/// <exception cref="ShadowTypeMismatchException">If the given <see cref="ShadowClass"/> is not an instance of <c>com.threerings.config.ConfigReference</c></exception>
		public static ShadowClass? ResolveConfigReference(ShadowClass shadow) {
			shadow.AssertIsInstanceOf("com.threerings.config.ConfigReference");

			if (shadow.TryGetField("__reference", out ShadowClass? reference)) {
				return reference;
			}

			string config = shadow["_name"] ?? "";
			FileInfo resolvedFile = SKEnvironment.ResolveSKFile(config);
			if (!resolvedFile.Exists) {
				ShadowClass? retn = MasterSKConfigs.GetConfig(config);
				if (retn != null) {
					shadow["__reference"] = retn;

					Dictionary<object, object?> args = shadow["_arguments"]!;
					if (retn.IsA("com.threerings.config.ParameterizedConfig")) {
						Parameter[] parameters = retn.GetParameters();
						foreach (Parameter param in parameters) {
							if (args.TryGetValue(param.Name, out object? value)) {
								if (param is Direct direct) {
									direct.SetAllValuesTo(value);
								} else if (param is Choice choice) {
									foreach (Direct direct1 in choice.Directs) {
										direct1.SetAllValuesTo(value);
									}
								}
							}
						}
					}

					return retn;
				}
				return null;
			} else {
				//using ClydeFile clf = new ClydeFile(resolvedFile.OpenRead());
				//object? retn = clf.ReadObject();
				ShadowClass retn = (ShadowClass)MasterDataExtractor.Open(resolvedFile, null);

				Dictionary<object, object?> args = shadow["_arguments"]!;
				if (retn.IsA("com.threerings.config.ParameterizedConfig")) {
					Parameter[] parameters = retn.GetParameters();
					foreach (Parameter param in parameters) {
						if (args.TryGetValue(param.Name, out object? value)) {
							if (param is Direct direct) {
								direct.SetAllValuesTo(value);
							} else if (param is Choice choice) {
								foreach (Direct directValue in choice.Directs) {
									directValue.SetAllValuesTo(value);
								}
							}
						}
					}
				}

				shadow["__reference"] = retn;
				return retn;
			}
		}

	}
}
