using com.threerings.export;
using com.threerings.math;
using com.threerings.opengl.model.config;
using com.threerings.tudey.data;
using com.threerings.util;
using java.io;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.Utility.Interface;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.XansData.Exceptions;
using ThreeRingsSharp.XansData.Extensions;

namespace ThreeRingsSharp.DataHandlers {

	/// <summary>
	/// A class designed to handle files exported by the Clyde library.
	/// </summary>
	public class ClydeFileHandler {

		/// <summary>
		/// An object cache of files that have already been read. Should drastically speed up models with a lot of references.
		/// </summary>
		private static readonly Dictionary<string, object> ClydeObjectCache = new Dictionary<string, object>();
		private static readonly Dictionary<string, (string, string, string)> ModelInfoCache = new Dictionary<string, (string, string, string)>();

		/// <summary>
		/// Depending on the value of the input <see cref="ClydeFormat"/>, this will return the appropriate <see cref="Importer"/> to read the given <paramref name="clydeFile"/>. Remember to close the <see cref="Importer"/>!<para/>
		/// <list type="table">
		/// <item>
		/// <term><c>ClydeFormat.Binary</c></term>
		/// <description><see cref="BinaryImporter"/></description>
		/// </item>
		/// <item>
		/// <term><c>ClydeFormat.XML</c></term>
		/// <description><see cref="XMLImporter"/></description>
		/// </item>
		/// <item>
		/// <term><c>ClydeFormat.None</c></term>
		/// <description><see langword="null"/></description>
		/// </item>
		/// </list>
		/// </summary>
		/// <param name="clydeFile"></param>
		/// <param name="format"></param>
		/// <returns></returns>
		private static Importer GetAppropriateImporter(FileInfo clydeFile, ClydeFormat format) {
			DataInputStream dataInput = new DataInputStream(new FileInputStream(clydeFile.FullName));
			if (format == ClydeFormat.Binary) return new BinaryImporter(dataInput);
			if (format == ClydeFormat.XML) return new XMLImporter(dataInput);
			return null;
		}

		/// <summary>
		/// Takes in a <see cref="FileInfo"/> representing a file that was created with the Clyde library.<para/>
		/// This will throw a <see cref="ClydeDataReadException"/> if anything goes wrong during reading.
		/// </summary>
		/// <param name="clydeFile">The file to load and decode.</param>
		/// <param name="allGrabbedModels">A list containing every processed model from the entire hierarchy. This list should be defined by you and then passed in.</param>
		/// <param name="isBaseFile">If <see langword="true"/>, then it means that <paramref name="clydeFile"/> is the literal file the user opened. Otherwise, if it is <see langword="false"/>, it was loaded by a different model (e.g. a CompoundConfig)</param>
		/// <param name="lastNodeParent">Intended for use if <paramref name="isBaseFile"/> is <see langword="false"/>, this is the parent Data Tree element to add this model into (so that the hierarchy can be constructed). This should be a <see cref="TreeNode"/>, a <see cref="TreeView"/>, or a <see cref="DataTreeObject"/>. This is intended for use in a GUI context, and if TRS is not being used with a GUI, this should be null.</param>
		/// <param name="useFileName">If <see langword="true"/>, the name of the loaded file will be displayed in the tree hierarchy's root node, e.g. model.dat. This is intended for use in a GUI context, and if TRS is not being used with a GUI, this should be null.</param>
		/// <param name="transform">Intended to be used by reference loaders, this specifies an offset for referenced models. All models loaded by this method in the given chain / hierarchy will have this transform applied to them. If null, it will be created as an identity transform.</param>
		/// <param name="extraData">Any extra data that should be included. This is mainly used by references (e.g. a reference is a <see cref="StaticSetConfig"/>, the target model in the set may be included as extra data)</param>
		public static void HandleClydeFile(FileInfo clydeFile, List<Model3D> allGrabbedModels, bool isBaseFile = false, dynamic lastNodeParent = null, bool useFileName = false, Transform3D transform = null, Dictionary<string, dynamic> extraData = null) {
			object obj;
			string modelClass = null;
			//string modelSubclass;
			DataTreeObject rootDataTreeObject = new DataTreeObject();
			// Other case was put at the bottom
			if (lastNodeParent is DataTreeObject datObj) {
				rootDataTreeObject.Parent = datObj;
			}
			// Since I want to tie up some UI stuff before throwing the error, I'll store it for later.
			ClydeDataReadException errToThrow = null;

			// Cache models we've already read. 
			// This isn't really important for single models, but for loading stuff like scenes, this speeds up load speed incredibly.

			// New task.
			SKAnimatorToolsTransfer.IncrementEnd();

			if (!ClydeObjectCache.ContainsKey(clydeFile.FullName)) {
				XanLogger.WriteLine($"Loading [{clydeFile.FullName}] because it hasn't been initialized before...", XanLogger.DEBUG);
				SKAnimatorToolsTransfer.IncrementEnd(2);
				SKAnimatorToolsTransfer.SetProgressState(ProgressBarState.ExtraWork);

				(bool isValidClydeFile, ClydeFormat format) = VersionInfoScraper.IsValidClydeFile(clydeFile);

				if (!isValidClydeFile) {
					XanLogger.WriteLine("Invalid file. Sending error.", XanLogger.DEBUG);
					throw new ClydeDataReadException("This file isn't a valid Clyde file! (Reason: Incorrect header)");
				}
				(string isCompressed, string formatVersion, string modelFullClass) = VersionInfoScraper.GetCosmeticInformation(clydeFile, format);
				XanLogger.WriteLine($"Read file to grab the raw info.", XanLogger.TRACE);
				string[] modelClassInfo = JavaClassNameStripper.GetSplitClassName(modelFullClass);

				if (modelClassInfo != null) {
					modelClass = modelClassInfo[0];
					//if (modelClassInfo.Length == 2) modelSubclass = modelClassInfo[1];
				}
				SKAnimatorToolsTransfer.IncrementProgress();

				// Just abort early here. We can't load these.
				if (modelClass == "ProjectXModelConfig") {
					SKAnimatorToolsTransfer.SetProgressState(ProgressBarState.Error);

					XanLogger.WriteLine("User imported a Player Knight model. These are unsupported. Sending warning.", XanLogger.DEBUG);
					if (isBaseFile) {
						SKAnimatorToolsTransfer.UpdateGUIThroughSync(modelType: "ModelConfig");
					}
					if (rootDataTreeObject != null) {
						rootDataTreeObject.Text = "ProjectXModelConfig";
						rootDataTreeObject.ImageKey = SilkImage.Articulated;
					}
					errToThrow = new ClydeDataReadException("Player Knights do not use the standard ArticulatedConfig type (used for all animated character models) and instead use a unique type called ProjectXModelConfig. Unfortunately, this type cannot be read by the program (I had to use some hacky data skimming to even get this error message to work)!\n\nConsider using /character/npc/crew/model.dat instead.", "Knights are not supported!", MessageBoxIcon.Error);
					goto FINALIZE_NODES;
				}

				// So if this is the file we actually opened (not a referenced file) and we've defined the action necessary to update the GUI...
				if (isBaseFile) {
					// Then update the display text in the top right to reflect on this.
					SKAnimatorToolsTransfer.UpdateGUIThroughSync(clydeFile.Name, isCompressed, formatVersion, "Processing...");
				}

				Importer targetImporter = GetAppropriateImporter(clydeFile, format);
				try {
					obj = targetImporter.readObject();
					ClydeObjectCache[clydeFile.FullName] = obj;
					ModelInfoCache[clydeFile.FullName] = (isCompressed, formatVersion, modelFullClass);
					SKAnimatorToolsTransfer.IncrementProgress();
				} catch {
					targetImporter.close();
					SKAnimatorToolsTransfer.SetProgressState(ProgressBarState.Error);
					throw;
				}
			} else {
				XanLogger.WriteLine("Loading Clyde object from cache because this .dat file has already been loaded.", XanLogger.TRACE);
				obj = ((DeepObject)ClydeObjectCache[clydeFile.FullName]).clone();
				(string, string, string) cosmeticInfo = ModelInfoCache[clydeFile.FullName];
				string modelFullClass = cosmeticInfo.Item3;
				string[] modelClassInfo = JavaClassNameStripper.GetSplitClassName(modelFullClass);

				if (modelClassInfo != null) {
					modelClass = modelClassInfo[0];
					//if (modelClassInfo.Length == 2) modelSubclass = modelClassInfo[1];
				}
			}

			SKAnimatorToolsTransfer.SetProgressState(ProgressBarState.OK);
			SKAnimatorToolsTransfer.IncrementProgress();

			// This is kind of hacky behavior but it (ab)uses the fact that this will only run on the first call for any given chain of .DAT files.
			// That is, all external referenced files by this .dat have this value passed into the method we're in right now instead of it being null.
			if (transform == null) {
				transform = new Transform3D(Vector3f.ZERO, Quaternion.IDENTITY, Model3D.MultiplyScaleByHundred ? 100f : 1f);
				XanLogger.WriteLine("Instantiated global transform as identity transform with scale=" + transform.getScale(), XanLogger.TRACE);
			} else {
				// If we ever load a new file, give it a fresh transform based on whatever was passed in so that it can be manipulated by this model.
				transform = transform.Clone();
				XanLogger.WriteLine("Instantiated global transform by cloning the existing passed in transform of " + transform.toString(), XanLogger.TRACE);
			}

			if (obj is null) {
				XanLogger.WriteLine("Clyde object is null. Unknown object type.", XanLogger.TRACE);
				if (isBaseFile) {
					SKAnimatorToolsTransfer.UpdateGUIThroughSync(modelType: "Unknown");
				}
				if (rootDataTreeObject != null) {
					rootDataTreeObject.Text = "Unknown Base";
					rootDataTreeObject.ImageKey = SilkImage.Generic;
				}
				errToThrow = new ClydeDataReadException("The root type of this data is null!\nThis usually happens if the implementation is from an outside source that uses Clyde (e.g. Spiral Knights itself) which has its own custom classes that are not part of Clyde.\n\nAs a result of this issue, the program is unfortunately unable to extract any meaningful information from this file.", "Unsupported Implementation");
				SKAnimatorToolsTransfer.SetProgressState(ProgressBarState.Error);
			}

			if (obj is ModelConfig model) {
				XanLogger.WriteLine("Clyde object is ModelConfig.", XanLogger.TRACE);
				if (isBaseFile) {
					SKAnimatorToolsTransfer.UpdateGUIThroughSync(modelType: "ModelConfig");
				}

				try {
					ModelConfigBrancher.HandleDataFrom(clydeFile, model, allGrabbedModels, rootDataTreeObject, useFileName, transform, extraData);
				} catch (ClydeDataReadException exc) {
					errToThrow = exc;
				}

			} else if (obj is AnimationConfig) {
				XanLogger.WriteLine("Clyde object is AnimationConfig.", XanLogger.TRACE);
				if (isBaseFile) {
					SKAnimatorToolsTransfer.UpdateGUIThroughSync(modelType: "AnimationConfig");
				}

				if (rootDataTreeObject != null) {
					rootDataTreeObject.Text = modelClass;
					rootDataTreeObject.ImageKey = SilkImage.Animation;
				}
				errToThrow = new ClydeDataReadException("You can't open animations directly! Please instead load an ArticulatedConfig that references this animation.", "Unsupported Implementation", MessageBoxIcon.Warning);
				SKAnimatorToolsTransfer.SetProgressState(ProgressBarState.Error);

			} else if (obj is TudeySceneModel scene) {
				XanLogger.WriteLine("Clyde object is TudeySceneModel.", XanLogger.TRACE);
				if (isBaseFile) {
					SKAnimatorToolsTransfer.UpdateGUIThroughSync(modelType: "TudeySceneModel");
				}

				if (rootDataTreeObject != null) {
					rootDataTreeObject.Text = "TudeySceneModel";
					rootDataTreeObject.ImageKey = SilkImage.Scene;
				}
				//errToThrow = new ClydeDataReadException("Scenes are unsupported! Come back later c:", "Unsupported Implementation", MessageBoxIcon.Warning);
				XanLogger.WriteLine("WARNING: TudeySceneModel is not fully supported right now! A lot of stuff will export misaligned or in the wrong location.", color: System.Drawing.Color.DarkGoldenrod);
				TudeySceneConfigBrancher.HandleDataFrom(clydeFile, scene, allGrabbedModels, rootDataTreeObject, useFileName, transform);

			} else {
				string mdlClass = modelClass ?? obj.GetType().Name;
				XanLogger.WriteLine("Clyde object is an unknown class [" + mdlClass + "]", XanLogger.TRACE);
				if (isBaseFile) {
					SKAnimatorToolsTransfer.UpdateGUIThroughSync(modelType: mdlClass);
				}
				if (rootDataTreeObject != null) {
					rootDataTreeObject.Text = mdlClass;
					rootDataTreeObject.ImageKey = SilkImage.Generic;
				}
				errToThrow = new ClydeDataReadException($"This implementation type ({mdlClass}) is unsupported by the program.", "Unsupported Implementation", MessageBoxIcon.Warning);
				SKAnimatorToolsTransfer.SetProgressState(ProgressBarState.Error);

			}

		FINALIZE_NODES:
			if (lastNodeParent is TreeNode || lastNodeParent is TreeView) {
				//lastNodeParent.Nodes.Add(rootDataTreeObject.ConvertHierarchyToTreeNodes());
				SKAnimatorToolsTransfer.RegisterNodes(lastNodeParent, rootDataTreeObject);
			}

			if (errToThrow != null) throw errToThrow;
		}

		/// <summary>
		/// Similar to <see cref="HandleClydeFile(FileInfo, List{Model3D}, bool, dynamic, bool, Transform3D, Dictionary{string, dynamic})"/>, except it just returns the raw object returned by clyde. This will return <see langword="null"/> if the model fails to laod.
		/// </summary>
		/// <param name="clydeFile"></param>
		/// <returns></returns>
		public static object GetRaw(FileInfo clydeFile) {
			object obj;

			if (!ClydeObjectCache.ContainsKey(clydeFile.FullName)) {
				XanLogger.WriteLine($"Loading [{clydeFile.FullName}] because it hasn't been initialized before...", XanLogger.DEBUG);
				(bool isValidClydeFile, ClydeFormat format) = VersionInfoScraper.IsValidClydeFile(clydeFile);

				if (!isValidClydeFile) {
					XanLogger.WriteLine("Invalid file. Sending error.", XanLogger.DEBUG);
					throw new ClydeDataReadException("This file isn't a valid Clyde file! (Reason: Incorrect header)");
				}
				(string, string, string) cosmeticInfo = VersionInfoScraper.GetCosmeticInformation(clydeFile, format);
				XanLogger.WriteLine($"Read file to grab the raw info.", XanLogger.TRACE);
				string modelClass = null;

				// Just abort early here. We can't laod these.
				if (modelClass == "ProjectXModelConfig") {
					XanLogger.WriteLine("User imported a Player Knight model. These are unsupported.", XanLogger.DEBUG);
					return null;
				}

				Importer targetImporter = GetAppropriateImporter(clydeFile, format);
				try {
					obj = targetImporter.readObject();
					ClydeObjectCache[clydeFile.FullName] = obj;
					ModelInfoCache[clydeFile.FullName] = cosmeticInfo;
				} catch {
					targetImporter.close();
					return null;
				}
			} else {
				XanLogger.WriteLine("Loading Clyde object from cache because this .dat file has already been loaded.", XanLogger.TRACE);
				obj = ((DeepObject)ClydeObjectCache[clydeFile.FullName]).clone();
			}

			return obj;
		}
	}
}
