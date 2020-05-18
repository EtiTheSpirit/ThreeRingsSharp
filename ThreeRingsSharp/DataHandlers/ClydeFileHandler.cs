using com.threerings.export;
using com.threerings.math;
using com.threerings.opengl.model.config;
using com.threerings.tudey.data;
using java.io;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.Utility.Interface;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.XansData.Exceptions;

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
		/// A delegate action that is called when the GUI needs to update. This can safely be <see langword="null"/> for contexts that do not have a GUI, as it is used for the SK Animator Tools UI.<para/>
		/// Pass in <see langword="null"/> for arguments to make their data remain unchanged.<para/>
		/// This is designed to work with the UI offered by SK Animator Tools V2. The parameters are as follows:<para/>
		/// <c>string fileName (the name of the file that was opened)<para/>
		/// string isCompressed (a string of true/false, yes/no, etc.)<para/>
		/// string formatVersion (represents clyde file version. Classic, Intermediate, or VarInt for example)<para/>
		/// string type (the base class, e.g. ModelConfig, AnimationConfig, ScriptedConfig)</c>
		/// </summary>
		public static Action<string, string, string, string> UpdateGUIAction { get; set; } = null;

		/// <summary>
		/// Takes in a <see cref="FileInfo"/> representing a file that was created with the Clyde library.<para/>
		/// This will throw a <see cref="ClydeDataReadException"/> if anything goes wrong during reading.
		/// </summary>
		/// <param name="clydeFile">The file to load and decode.</param>
		/// <param name="allGrabbedModels">A list containing every processed model from the entire hierarchy. This list should be defined by you and then passed in.</param>
		/// <param name="isBaseFile">If <see langword="true"/>, this will update the main GUI display data for the base loaded model. If the GUI is not defined (e.g. this is being used in a library) this will do nothing.</param>
		/// <param name="lastNodeParent">Intended for use if <paramref name="isBaseFile"/> is <see langword="false"/>, this is the parent Data Tree element to add this model into (so that the hierarchy can be constructed). This should be a <see cref="TreeNode"/>, a <see cref="TreeView"/>, or a <see cref="DataTreeObject"/>.</param>
		/// <param name="useFileName">If <see langword="true"/>, the name of the loaded file will be displayed in the tree hierarchy's root node, e.g. model.dat</param>
		/// <param name="transform">Intended to be used by reference loaders, this specifies an offset for referenced models. All models loaded by this method in the given chain / hierarchy will have this transform applied to them. If it doesn't exist, it will be created.</param>
		/// <param name="extraData">Any extra data that should be included. This is mainly used by references (e.g. a reference is a <see cref="StaticSetConfig"/>, the target model in the set may be included as extra data)</param>
		public static void HandleClydeFile(FileInfo clydeFile, List<Model3D> allGrabbedModels, bool isBaseFile = false, dynamic lastNodeParent = null, bool useFileName = false, Transform3D transform = null, Dictionary<string, dynamic> extraData = null) {
			// TODO: Do NOT update the main GUI if this is a referenced file (e.g. a CompoundConfig wants to load other data, don't change the table view in the top right)

			object obj = null;
			string modelClass = null;
			string modelSubclass = null;
			DataTreeObject rootDataTreeObject = new DataTreeObject();
			// Other case was put at the bottom
			if (lastNodeParent is DataTreeObject datObj) {
				rootDataTreeObject.Parent = datObj;
			}
			// Since I want to tie up some UI stuff before throwing the error, I'll store it for later.
			ClydeDataReadException errToThrow = null;

			if (!ClydeObjectCache.ContainsKey(clydeFile.FullName)) {
				XanLogger.WriteLine($"Loading [{clydeFile.FullName}]...");
				// ProgramLog.Update();
				if (!VersionInfoScraper.IsValidClydeFile(clydeFile)) {
					XanLogger.WriteLine("Invalid file. Sending error.");
					// if (isBaseFile) AsyncMessageBox.ShowAsync("This file isn't a valid Clyde file! (Reason: Incorrect header)", "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Error);
					throw new ClydeDataReadException("This file isn't a valid Clyde file! (Reason: Incorrect header)");
				}
				(string, string, string) cosmeticInfo = VersionInfoScraper.GetCosmeticInformation(clydeFile);
				XanLogger.WriteLine($"Read file to grab the raw info.");
				string modelFullClass = cosmeticInfo.Item3;
				string[] modelClassInfo = ClassNameStripper.GetSplitClassName(modelFullClass);

				if (modelClassInfo != null) {
					modelClass = modelClassInfo[0];
					if (modelClassInfo.Length == 2) modelSubclass = modelClassInfo[1];
				}

				// Just abort early here.
				if (modelClass == "ProjectXModelConfig") {
					XanLogger.WriteLine("User imported a Player Knight model. These are unsupported. Sending warning.");
					if (isBaseFile && UpdateGUIAction != null) {
						UpdateGUIAction(null, null, null, "ModelConfig");
					}
					if (rootDataTreeObject != null) {
						rootDataTreeObject.Text = "ProjectXModelConfig";
						rootDataTreeObject.ImageKey = SilkImage.Articulated;
					}
					errToThrow = new ClydeDataReadException("Player Knights do not use the standard ArticulatedConfig type (used for all animated character models) and instead use a unique type called ProjectXModelConfig. Unfortunately, this type cannot be read by the program (I had to use some hacky data skimming to even get this error message to work)!", "Knights are not supported!", MessageBoxIcon.Error);
					goto FINALIZE_NODES;
				}

				// So if this is the file we actually opened (not a referenced file) and we've defined the action necessary to update the GUI...
				if (isBaseFile && UpdateGUIAction != null) {
					// Then update the display text in the top right to reflect on this.
					UpdateGUIAction(clydeFile.Name, cosmeticInfo.Item1, cosmeticInfo.Item2, "Processing...");
				}

				DataInputStream dataInput = new DataInputStream(new FileInputStream(clydeFile.FullName));
				BinaryImporter importer = new BinaryImporter(dataInput);
				obj = importer.readObject();
				ClydeObjectCache[clydeFile.FullName] = obj;
				ModelInfoCache[clydeFile.FullName] = cosmeticInfo;
			} else {
				obj = ClydeObjectCache[clydeFile.FullName];
				(string, string, string) cosmeticInfo = ModelInfoCache[clydeFile.FullName];
				string modelFullClass = cosmeticInfo.Item3;
				string[] modelClassInfo = ClassNameStripper.GetSplitClassName(modelFullClass);

				if (modelClassInfo != null) {
					modelClass = modelClassInfo[0];
					if (modelClassInfo.Length == 2) modelSubclass = modelClassInfo[1];
				}
			}

			// This is kind of hacky behavior but it (ab)uses the fact that this will only run on the first call for any given chain of .DAT files.
			// That is, all external referenced files have this value passed in instead of it being null.
			if (transform == null) transform = new Transform3D(Vector3f.ZERO, Quaternion.IDENTITY, Model3D.MultiplyScaleByHundred ? 100f : 1f);
			// transform.setScale(transform.getScale() * (MultiplyScaleByHundred ? 100f : 1f));

			if (obj is null) {
				if (isBaseFile && UpdateGUIAction != null) {
					UpdateGUIAction(null, null, null, "Unknown");
				}
				if (rootDataTreeObject != null) {
					rootDataTreeObject.Text = "Unknown Implementation";
					rootDataTreeObject.ImageKey = SilkImage.Object;
				}
				errToThrow = new ClydeDataReadException("This implementation is null!\nThis usually happens if the implementation is from an outside source that uses Clyde (e.g. Spiral Knights itself) has its own custom classes that are not part of Clyde.\n\nAs a result of this issue, the program is unfortunately unable to extract its data type name.", "Unsupported Implementation");
			}

			if (obj is ModelConfig model) {
				if (isBaseFile && UpdateGUIAction != null) {
					UpdateGUIAction(null, null, null, "ModelConfig");
				}

				try {
					ModelConfigBrancher.HandleDataFrom(clydeFile, model, allGrabbedModels, rootDataTreeObject, useFileName, transform, extraData);
				} catch (ClydeDataReadException exc) {
					errToThrow = exc;
				}

			} else if (obj is AnimationConfig animation) {
				if (isBaseFile && UpdateGUIAction != null) {
					UpdateGUIAction(null, null, null, "AnimationConfig");
				}

				if (rootDataTreeObject != null) {
					rootDataTreeObject.Text = modelClass;
					rootDataTreeObject.ImageKey = SilkImage.Animation;
				}
				errToThrow = new ClydeDataReadException("Animations are unsupported! Come back later c:", "Unsupported Implementation", MessageBoxIcon.Warning);

			} else if (obj is TudeySceneModel scene) {
				if (isBaseFile && UpdateGUIAction != null) {
					UpdateGUIAction(null, null, null, "TudeySceneModel");
				}

				if (rootDataTreeObject != null) {
					rootDataTreeObject.Text = "TudeySceneModel";
					rootDataTreeObject.ImageKey = SilkImage.Scene;
				}
				//errToThrow = new ClydeDataReadException("Scenes are unsupported! Come back later c:", "Unsupported Implementation", MessageBoxIcon.Warning);
				TudeySceneConfigBrancher.HandleDataFrom(clydeFile, scene, allGrabbedModels, rootDataTreeObject, useFileName, transform);

			} else {
				string mdlClass = modelClass ?? obj.GetType().Name;
				if (isBaseFile && UpdateGUIAction != null) {
					UpdateGUIAction(null, null, null, mdlClass);
				}
				if (rootDataTreeObject != null) {
					rootDataTreeObject.Text = mdlClass;
					rootDataTreeObject.ImageKey = SilkImage.Generic;
				}
				errToThrow = new ClydeDataReadException($"This implementation type ({mdlClass}) is unsupported by the program.", "Unsupported Implementation", MessageBoxIcon.Warning);
			}

			FINALIZE_NODES:
			if (lastNodeParent is TreeNode || lastNodeParent is TreeView) {
				lastNodeParent.Nodes.Add(rootDataTreeObject.ConvertHierarchyToTreeNodes());
			}

			if (errToThrow != null) throw errToThrow;
		}


		protected internal static bool UseHardcodedConfigRefsPath = true;

		/// <summary>
		/// Directly handles a <see cref="FileInfo"/> that is expected to be a config reference (that is, stored in <c>rsrc/config/</c>).<para/>
		/// This method is unchecked. It does not look to see if the file is an actual proper file. Please be careful.
		/// </summary>
		/// <param name="clydeFile"></param>
		public static void HandleConfigReferenceLookup(FileInfo clydeFile) {
			DataInputStream dataInput = new DataInputStream(new FileInputStream(clydeFile.FullName));
			XMLImporter importer = new XMLImporter(dataInput);
			var obj = importer.readObject();
			// 
			XanLogger.WriteLine(obj.ToString());
		}
	}
}
