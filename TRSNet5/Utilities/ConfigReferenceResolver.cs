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

		public static Dictionary<string, object?> GetArgumentMap(object? unknown) {
			if (unknown is Dictionary<string, object?> dict) return dict;
			if (unknown is Dictionary<object, object?> dict2) {
				Dictionary<string, object?> arguments = new Dictionary<string, object?>();
				foreach (KeyValuePair<object, object?> kvp in dict2) {
					arguments[kvp.Key.ToString()!] = kvp.Value;
				}
				return arguments;
			}
			if (unknown is ShadowClass argMap) {
				if (argMap.IsA("com.threerings.config.ArgumentMap")) {
					object sortableArrayList = argMap.GetField<object>("_entries")!;
					if (sortableArrayList is ShadowClass scSortable) {
						object entries = scSortable.GetField<object>("_elements")!;
						// This may be a shadow of java.lang.Object due to java type erasure.
						// Usually it will be so I'll just quietly hope this works.

						if (entries is ShadowClass[] entriesArray) {
							Dictionary<string, object?> arguments = new Dictionary<string, object?>();

							foreach (ShadowClass entry in entriesArray) {
								if (entry.IsA("java.util.AbstractMap$SimpleEntry")) {
									arguments[entry["key"]!.ToString()] = entry["value"];
								} else {
									throw new InvalidOperationException("Unknown entry type! " + entry.Signature);
								}
							}

							return arguments;
						}
					} else if (sortableArrayList is List<object> list) {
						Dictionary<string, object?> arguments = new Dictionary<string, object?>();
						foreach (object element in list) {
							if (element is ShadowClass entry) {
								arguments[entry["key"]!.ToString()] = entry["value"];
							}
						}
						return arguments;
					}
				}
			}
			return new Dictionary<string, object?>();
		}

		/// <summary>
		/// Returns the object pointed to by this <see cref="ShadowClass"/> representing a ConfigReference. Additionally, this
		/// adds a field named <c>__REFERENCE</c> onto the input <see cref="ShadowClass"/> for caching.
		/// </summary>
		/// <param name="shadow">The <see cref="ShadowClass"/> representing the ConfigReference.</param>
		/// <exception cref="ShadowTypeMismatchException">If the given <see cref="ShadowClass"/> is not an instance of <c>com.threerings.config.ConfigReference</c></exception>
		public static (ShadowClass?, FileInfo?) ResolveConfigReference(ShadowClass shadow) {
			shadow.AssertIsInstanceOf("com.threerings.config.ConfigReference");

			if (shadow.TryGetField("__REFERENCE", out ShadowClass? reference)) {
				return (reference, reference?.GetFieldOrDefault<FileInfo>("__FILE"));
			}

			string config = shadow["_name"] ?? "";
			FileInfo resolvedFile = SKEnvironment.ResolveSKFile(config);
			if (!resolvedFile.Exists) {
				ShadowClass? retn = MasterSKConfigs.GetConfig(config);
				if (retn != null) {
					shadow["__REFERENCE"] = retn;

					Dictionary<string, object?> args = GetArgumentMap(shadow["_arguments"]!);
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

					return (retn, retn.GetFieldOrDefault<FileInfo>("__FILE"));
				}
				return (null, null);
			} else {
				//using ClydeFile clf = new ClydeFile(resolvedFile.OpenRead());
				//object? retn = clf.ReadObject();
				ShadowClass retn = (ShadowClass)MasterDataExtractor.Open(resolvedFile, null);

				Dictionary<string, object?> args = GetArgumentMap(shadow["_arguments"]!);
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

				shadow["__REFERENCE"] = retn;
				return (retn, resolvedFile);
			}
		}

	}
}
