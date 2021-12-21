using OOOReader.Clyde;
using OOOReader.Reader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.ConfigHandlers.Presets {
	public static class MasterSKConfigs {

		private static readonly Dictionary<string, ShadowClass[]> _configs = new Dictionary<string, ShadowClass[]>();
		private static readonly List<ShadowClass> _everything = new List<ShadowClass>();
		private static readonly List<string> _everything_name = new List<string>();
		private static bool _initialized = false;

		public static void Initialize() {
			if (_initialized) return;
			_initialized = true;
			try {
				bool originalStrictTypes = ShadowClass.StrictTypes;
				ShadowClass.StrictTypes = false; // Very hacky but it gets rid of some confusion in the engine.

				DirectoryInfo dir = new DirectoryInfo(Path.Combine(Utilities.SKEnvironment.RSRC_DIR.FullName, "config"));
				FileInfo[] allFiles = dir.EnumerateFiles().ToArray();
				foreach (FileInfo file in allFiles) {
					if (file.Extension == ".dat") {
						try {
							using ClydeFile clyde = new ClydeFile(file);
							ShadowClass[] stuff = (ShadowClass[])clyde.ReadObject()!;
							_configs[file.Name.Replace(file.Extension, "")] = stuff;
							_everything.AddRange(stuff);
							foreach (ShadowClass cls in stuff) {
								_everything_name.Add(cls["_name"]);
								if (cls.TryGetField("implementation", out ShadowClass? impl)) {
									if (impl == null) Debug.WriteLine($"Config {file.Name}>{cls["_name"]} has a null implementation.");
								}
								cls.SetField("__FILE", file, true); // For organizational purposes, this associates a config with its file.
							}
						} catch (Exception exc) {
							string msg = $"Failed to convert {file.Name} - {exc.Message}";
							Debug.WriteLine(msg);
						}
					}
				}

				_everything.Sort((left, right) => {
					string leftName = left["_name"]!;
					string rightName = right["_name"]!;
					return leftName.CompareTo(rightName);
				});
				_everything_name.Sort();

				ShadowClass.StrictTypes = originalStrictTypes;
#pragma warning disable CS0168 // Variable is declared but never used
			} catch (Exception exc) {
#pragma warning restore CS0168 // Variable is declared but never used
				// Keep for debugging.
				Debugger.Break();
			}
		}

		/// <summary>
		/// Locates and returns a clone of the ManagedConfig instance representing a default config with the given name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static ShadowClass? GetConfig(string name) {
			if (!_initialized) Initialize();
			int index = _everything_name.BinarySearch(name);
			if (index >= 0) {
				return _everything[index].NewInstance();
			}
			return null;
		}

	}
}
