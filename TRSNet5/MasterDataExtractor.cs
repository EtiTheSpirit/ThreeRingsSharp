using OOOReader.Clyde;
using OOOReader.Reader;
using SKAnimatorTools.PrimaryInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.ConfigHandlers.ModelConfigs;
using ThreeRingsSharp.ConfigHandlers.TudeyScenes;
using ThreeRingsSharp.Utilities;
using ThreeRingsSharp.XansData;
using XDataTree;
using XDataTree.Data;
using XDataTree.TreeElements;

namespace ThreeRingsSharp {

	/// <summary>
	/// This class manages the extraction and conversion of data from a <see cref="ClydeFile"/>. The sort of "main hub" for all read operations.
	/// </summary>
	public static class MasterDataExtractor {

		/*
		 * TO OTHER FUTURE PROGRAMMERS:
		 * 
		 *		If you're coming into this program some time in the distant future because I dropped TRS in favor of some other project, I
		 * apologize ahead of time for the hellscape you are about to enter. ShadowClass and its sister-classes will take a while to get used to, since
		 * they rely on a fundamental understanding of OOO's engine. The only reason I was able to make them so reliably was because I've been working
		 * with this for so long. SC is an intuitive data type, and relies on you having a very strong understanding of its workings. If you've dealt with
		 * a language like Lua before, they are basically glorified Tables when it comes to reading/writing their fields (and are indexed (roughly) the same
		 * way, with ["key"]).
		 * 
		 *		I don't really have any advice as far as "how to work with it" goes, other than just getting used to it. ShadowClass has some helpers on it
		 * that allow it to be viewed in the debugger with ease, so you can watch what the data looks like. The way it reads the data is (roughly) identical
		 * to OOO's BinaryImporter class, minus the fact that it relies on a cache of OOO's existing types to optimize how it stores data (see the text file
		 * dump of all OOO classes and fields).
		 * 
		 *		The only issues you may face will likely stem from "What data do I use?", and to answer that question, the fact that OOO published its 
		 * source code is your #1 lead. It's how I did it long ago, and how you should do it too. Like I said - it requires a fundamental understanding
		 * of Clyde to really work with efficiently.
		 * 
		 *		I'll try to fill my code with comments, but I can't guarantee it everywhere.
		 * 
		 */

		static MasterDataExtractor() {
			ShadowClass.Initialize();
		}

		private static readonly Dictionary<FileInfo, ShadowClass> Cache = new Dictionary<FileInfo, ShadowClass>();
		private static readonly Dictionary<FileInfo, ShadowClass[]> ArrayCache = new Dictionary<FileInfo, ShadowClass[]>();

		/// <summary>
		/// "CLF" is a lazy abbreviation for "Clyde File". This binds a <see cref="FileInfo"/> to a set of information about the <see cref="ClydeFile"/> that represents
		/// its main attributes for display in the GUI.
		/// </summary>
		private static readonly Dictionary<FileInfo, (string, string, string, string)> CLFBindings = new Dictionary<FileInfo, (string, string, string, string)>();

		/// <summary>
		/// Intended for <see cref="ClydeFile"/>s that return a single instance of, or an array of, <see cref="ShadowClass"/>.
		/// This functions as a caching layer by caching the original return from <see cref="ClydeFile.ReadObject"/>. If it is referenced again,
		/// then the cached value is cloned and returned rather than opening a <see cref="ClydeFile"/> again.
		/// </summary>
		/// <param name="file">The file to read data from.</param>
		/// <param name="updateOpenedFileDisplay">This action accepts arguments in the order of <c>fileName</c>, <c>clydeVersion</c>, <c>isCompressed</c>, <c>baseManagedConfigClass</c></param>
		public static object Open(FileInfo file, Action<string, string, string, string>? updateOpenedFileDisplay) {
			if (updateOpenedFileDisplay != null && CLFBindings.TryGetValue(file, out ValueTuple<string, string, string, string> value)) {
				updateOpenedFileDisplay.Invoke(value.Item1, value.Item2, value.Item3, value.Item4);
			}
			if (Cache.TryGetValue(file, out ShadowClass? sc)) {
				ShadowClass copy = sc.Clone();
				copy.SetField("__SOURCELOCALPATH", SKEnvironment.GetRSRCRelativePath(file));
				copy.SetField("__FILE", file);
				return copy;
			}
			if (ArrayCache.TryGetValue(file, out ShadowClass[]? scs)) {
				ShadowClass[] dest = new ShadowClass[scs.Length];
				for (int idx = 0; idx < scs.Length; idx++) {
					ShadowClass copy = scs[idx].Clone();
					copy.SetField("__SOURCELOCALPATH", SKEnvironment.GetRSRCRelativePath(file));
					copy.SetField("__FILE", file);
					dest[idx] = copy;
				}
				return dest;
			}
			using ClydeFile clf = new ClydeFile(file);
			object? data = clf.ReadObject();
			if (data is ShadowClass shadow) {
				Cache[file] = shadow;
				(string fName, string vName, string comp, string baseType) = (clf.OriginalFile!.Name, clf.Version.ToString(), clf.Compressed.ToString(), shadow.Signature[(shadow.Signature.LastIndexOf('.') + 1)..]);
				CLFBindings[file] = (fName, vName, comp, baseType);
				updateOpenedFileDisplay?.Invoke(fName, vName, comp, baseType);
				ShadowClass copy = shadow.Clone();
				copy.SetField("__SOURCELOCALPATH", SKEnvironment.GetRSRCRelativePath(file));
				copy.SetField("__FILE", file);
				return copy;
			} else if (data is ShadowClass[] shadowArray) {
				ArrayCache[file] = shadowArray;
				(string fName, string vName, string comp, string baseType) = (clf.OriginalFile!.Name, clf.Version.ToString(), clf.Compressed.ToString(), shadowArray[0].Signature[(shadowArray[0].Signature.LastIndexOf('.') + 1)..] + $"[{shadowArray.Length}]");
				CLFBindings[file] = (fName, vName, comp, baseType);
				updateOpenedFileDisplay?.Invoke(fName, vName, comp, baseType);
				ShadowClass[] dest = new ShadowClass[shadowArray.Length];
				for (int idx = 0; idx < shadowArray.Length; idx++) {
					ShadowClass copy = shadowArray[idx].Clone();
					copy.SetField("__SOURCELOCALPATH", SKEnvironment.GetRSRCRelativePath(file));
					copy.SetField("__FILE", file);
					dest[idx] = copy;
				}
				return dest;
			} else {
				throw new InvalidOperationException("Unsupported data type: " + data?.GetType().ToString() ?? "null");
			}
		}

		/// <summary>
		/// Begin extracting data from the given context's file, and populate the context with said data.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="updateOpenedFileDisplay"></param>
		public static void ExtractFrom(ReadFileContext context, Action<string, string, string, string> updateOpenedFileDisplay) {
			object scType = Open(context.File, updateOpenedFileDisplay);
			if (scType is ShadowClass sc) {
				ExtractFrom(context, sc);
			} else if (scType is ShadowClass[] scs) {
				for (int idx = 0; idx < scs.Length; idx++) {
					ShadowClass shd = scs[idx];
					ExtractFrom(context, shd, idx);
				}
			}
		}

		/// <summary>
		/// Intended to be called by handlers rather than externally, this continues a chain of models, and is only called when a ConfigReference is resolved.
		/// </summary>
		/// <param name="currentContext"></param>
		/// <param name="subShadow"></param>
		/// <param name="arrayIndex">If this is part of a loop extracting from an array of <see cref="ShadowClass"/> instances, then this is the array index.</param>
		public static void ExtractFrom(ReadFileContext currentContext, ShadowClass subShadow, int? arrayIndex = null) {
			if (subShadow.IsA("com.threerings.opengl.model.config.ModelConfig")) {
				ShadowClass impl = subShadow["implementation"]!;
				string implName = impl.Signature[(impl.Signature.LastIndexOf('.') + 1)..];

				XanLogger.WriteLine($"Attempting to translate {implName} instance...", XanLogger.TRACE);
				if (impl.IsA("com.threerings.opengl.model.config.ArticulatedConfig")) {
					ArticulatedConfig.ReadData(currentContext, subShadow);
				} else if (impl.IsA("com.threerings.opengl.model.config.StaticConfig")) {
					StaticConfig.ReadData(currentContext, subShadow);
				} else if (impl.IsA("com.threerings.opengl.model.config.StaticSetConfig")) {
					StaticSetConfig.ReadData(currentContext, subShadow);
				} else if (impl.IsA("com.threerings.opengl.model.config.MergedStaticConfig")) {
					MergedStaticConfig.ReadData(currentContext, subShadow);
				} else if (impl.IsA("com.threerings.opengl.model.config.CompoundConfig")) {
					CompoundConfig.ReadData(currentContext, subShadow);
				} else if (impl.IsA("com.threerings.opengl.model.config.ModelConfig$Derived")) {
					DerivedModelConfig.ReadData(currentContext, subShadow);
				} else if (impl.IsA("com.threerings.opengl.model.config.ModelConfig$Schemed")) {
					SchemedModelConfig.ReadData(currentContext, subShadow);
				} else if (impl.IsA("com.threerings.opengl.scene.config.ViewerAffecterConfig")) { 
					ViewerAffecterConfig.ReadData(currentContext, subShadow);
				} else {
					SetupBaseInformation(subShadow, currentContext.Push(currentContext.File.Name, SilkImage.Missing), true);
					currentContext.Pop();
				}
			} else if (subShadow.IsA("com.threerings.tudey.data.TudeySceneModel")) {
				XanLogger.WriteLine("A scene was loaded. Fair warning that these tend to take a much longer time to open! Additionally, they can take a long time to export (so the program will appear unresponsive). Give it a couple minutes when importing and exporting scenes.");
				TudeySceneModelReader.ReadData(currentContext, subShadow);
				//SetupBaseInformation(subShadow, currentContext.Push(currentContext.File.Name, SilkImage.Missing), true);
				//currentContext.Pop();
			//} else if (subShadow.IsA("com.threerings.opengl.model.config.AnimationConfig")) {
				//SetupBaseInformation(subShadow, currentContext.Push(currentContext.File.Name, SilkImage.Missing), true);
				//currentContext.Pop();
			} else if (subShadow.IsA("com.threerings.tudey.config.TileConfig")) {
				if (arrayIndex == null) {
					//SetupBaseInformation(subShadow, currentContext.Push(currentContext.File.Name, SilkImage.Tile), true);
					ShadowClass impl = subShadow["implementation"]!;
					string implName = impl.Signature[(impl.Signature.LastIndexOf('.') + 1)..];
					XanLogger.WriteLine($"Attempting to translate {implName} instance...", XanLogger.TRACE);

					if (impl.TryGetField("model", out ShadowClass? mdlRef)) {
						if (mdlRef!.IsA("com.threerings.config.ConfigReference")) {

						}
					}
				} else {
					// We've actually loaded a config object
					SetupBaseInformation(subShadow, currentContext.Push(currentContext.File.Name + $"[{arrayIndex.Value}]", SilkImage.Tile), true);
				}
				currentContext.Pop();
			} else {
				// AsyncMessageBox.ShowAsync("Unfortunately, the .dat file you opened contains an unsupported base type. The type in question: " + subShadow.Signature, "Unsupported Subformat", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
				XanLogger.WriteLine("Unsupported type " + subShadow.Signature, 0, System.Drawing.Color.FromArgb(127, 0, 0));
			}
		}

		/// <summary>
		/// Extracts directly from a <see cref="ConfigReference"/>.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="reference"></param>
		/// <param name="extraTag">If defined, then the root element for this model will have the tag appended in brackets: model.dat [extraTag]</param>
		public static void ExtractFrom(ReadFileContext context, ConfigReference reference, string? extraTag = null) {
			ShadowClass? scRef = reference.Resolve();
			FileInfo? orgFile = reference.FileReference;
			if (scRef != null && orgFile != null) {
				if (!string.IsNullOrWhiteSpace(extraTag)) {
					scRef.SetField("__TAG", extraTag, true);
				}
				context.File = orgFile;
				ExtractFrom(context, scRef);
			}
		}

		internal static GenericElement SetupBaseInformation(ShadowClass managedConfig, GenericElement objectTreeElement, bool isMissing = false) {
			if (objectTreeElement.Properties is RootSubstituteElement rse) {
				if (managedConfig.IsA("com.threerings.opengl.model.config.ModelConfig")) {
					string impl = managedConfig["implementation"]!.Signature;
					if (!isMissing) {
						rse.Add(new KeyValueElement("Implementation", impl, false, SilkImage.Config));
					} else {
						KeyValueElement kve = new KeyValueElement("Implementation", impl, false, SilkImage.MissingConfig) {
							Tooltip = "This implementation wasn't recognized, so I can't read from it!"
						};
						rse.Add(kve);
					}
				} else if (managedConfig.IsA("com.threerings.tudey.data.TudeySceneModel")) {
					rse.Add(new KeyValueElement("Implementation", managedConfig.Signature, false, SilkImage.Config));
				}

				if (managedConfig.TryGetField("__SOURCELOCALPATH", out string? srcFile)) {
					rse.Add(new KeyValueElement("Source", srcFile!));
				}
				if (managedConfig.TryGetField("__TAG", out string? tag)) {
					objectTreeElement.SetText(objectTreeElement.Text + $" [{tag}]");
				}
			}
			return objectTreeElement;
		}

		public static bool CanPurgeCache() => !(Cache.Count == 0 && ArrayCache.Count == 0);

		/// <summary>
		/// Purges the cache.
		/// </summary>
		public static void PurgeCache() {
			if (!CanPurgeCache()) return;
			XanLogger.WriteLine("Purging cache. The program might freeze for a moment!");
			TRSLog.WaitForNextMessagesToWrite();

			long mem = GC.GetTotalMemory(false);
			Cache.Clear();
			ArrayCache.Clear();
			GC.Collect();

			double disposed = mem - GC.GetTotalMemory(true);
			string disp;
			if (disposed > 1_000_000_000) {
				disp = (disposed / 1000000000).ToString("#.#") + "GB";
			} else if (disposed > 1_000_000) {
				disp = (disposed / 1000000).ToString("#.#") + "MB";
			} else if (disposed > 1_000) {
				disp = (disposed / 1000).ToString("#.#") + "KB";
			} else {
				disp = disposed + "B";
			}

			XanLogger.WriteLine($"Cleaned up {disp} of memory.");
		}

	}
}
