using EtiLogger.Logging;
using OOOReader.Clyde;
using OOOReader.Reader;
using SKAnimatorTools.Component;
using SKAnimatorTools.Configuration;
using SKAnimatorTools.PrimaryInterface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ThreeRingsSharp;
using ThreeRingsSharp.ConfigHandlers.Presets;
using ThreeRingsSharp.Utilities;
using ThreeRingsSharp.Utilities.Parameters.Implementation;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.XansData.Exceptions;
using ThreeRingsSharp.XansData.Extensions;
using ThreeRingsSharp.XansData.IO.GLTF;
using XDataTree;
using XDataTree.TreeElements;

namespace SKAnimatorTools {
	public partial class MainWindow : Form {

		#region Externs for console

		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();

		public IntPtr ConsolePtr;

		#endregion

		#region Properties

		/// <summary>
		/// The synchronization context used to allow various asynchronous operations to communicate with the GUI.
		/// </summary>
		public static SynchronizationContext? UISyncContext { get; private set; }

		#region GUI Information

		/// <summary>
		/// The root <see cref="DataTreeObject"/> that represents the loaded model in the object hierarchy.<para/>
		/// Rather than creating a new instance for every time a model is loaded, call <see cref="DataTreeObject.ClearAllChildren"/> instead.
		/// </summary>
		public TreeElement RootDataTreeObject { get; } = new GenericElement("implementation");

		/// <summary>
		/// The current <see cref="ReadFileContext"/> of the latest opened file, which contains all applicable information.
		/// </summary>
		public ReadFileContext? CurrentContext { get; set; }

		/// <summary>
		/// The current configuration form, if it exists.
		/// </summary>
		private ConfigurationForm? ConfigForm { get; set; }

		#endregion

		#region Program State

		/// <summary>
		/// This will be true of we've opened a model before configs were loaded. This is used to prevent two delays at once.
		/// </summary>
		private bool IsYieldingForModel { get; set; } = false;

		/// <summary>
		/// If true, any other models shouldn't load because one is loading right now.
		/// </summary>
		private bool Busy { get; set; } = false;

		#endregion

		#endregion

		#region Version Information

		/// <summary>
		/// The version of this release of the program.
		/// </summary>
		public readonly int[] THIS_VERSION = { 3, 0, 0 };

		/// <summary>
		/// Attempts to access the github to acquire the latest version.
		/// </summary>
		/// <param name="version"></param>
		/// <returns></returns>
		public static bool TryGetVersion(out (int, int, int)? version) {
			try {
				using (WebClient cli = new WebClient()) {
					string v = cli.DownloadString("https://raw.githubusercontent.com/EtiTheSpirit/ThreeRingsSharp/master/version.txt");
					string[] revs = v.Split('.');
					int major = 0;
					int minor = 0;
					int patch = 0;
					if (revs.Length == 3) {
						major = int.Parse(revs[0]);
						minor = int.Parse(revs[1]);
						patch = int.Parse(revs[2]);
						version = (major, minor, patch);
						return true;
					}
					version = null;
				}
				return true;
			} catch {
				version = null;
				return false;
			}
		}

		private void AskForUpdate(int major, int minor, int patch) {
			string verStr = major + "." + minor + "." + patch;
			Updater updWindow = new Updater(verStr);
			updWindow.ShowDialog(); // Because I want it to yield.
		}

		/// <summary>
		/// Checks the version of the program, and shows the update window if this instance of the program is out of date.
		/// </summary>
		/// <returns></returns>
		public void ShowUpdateDialogIfNeeded(bool force = false) {
			if (TryGetVersion(out (int, int, int)? newVersion) && newVersion.HasValue) {
				(int major, int minor, int patch) = newVersion.Value;
				bool newMajor = major > THIS_VERSION[0];
				bool newMinor = minor > THIS_VERSION[1];
				bool newPatch = patch > THIS_VERSION[2];

				if (!force) {
					if (THIS_VERSION[0] > major) return; // This major version is newer than the latest. This is a beta build or dev build.
					if (newMajor) {
						AskForUpdate(major, minor, patch);
						return;
					}

					if (THIS_VERSION[1] > minor) return;
					if (newMinor) {
						AskForUpdate(major, minor, patch);
						return;
					}

					if (THIS_VERSION[2] > patch) return;
					if (newPatch) {
						AskForUpdate(major, minor, patch);
						return;
					}
				}
			} else {
				XanLogger.WriteLine("Failed to download new version information. TRS may have an update, but there's no way to know automatically. If you care enough, go check the github page.", XanLogger.INFO, Color.Red);
			}
		}

		#endregion

		#region Initialization Code

		/// <summary>
		/// Sets up the Windows console by enabling VT (if possible), setting it's title, and preparing other necessary information.
		/// </summary>
		public void SetupConsole() {
			ConsolePtr = GetConsoleWindow();
			// Console.Title = "Spiral Knights Animator Tools";
			VTConsole.EnableVTSupport();

		}

		/// <summary>
		/// Populates proxy references which allows the DLL library if TRS to communicate with this portion of the program.
		/// </summary>
		public void PopulateProxyReferences() {

			#region Actions
			SKAnimatorToolsProxy.UpdateGUIAction = (string fileName, string isCompressed, string formatVersion, string type) => {
				LabelFileName.Text = fileName ?? LabelFileName.Text;
				LabelModelCompressed.Text = isCompressed ?? LabelModelCompressed.Text;
				LabelFormatVersion.Text = formatVersion ?? LabelFormatVersion.Text;
				LabelType.Text = type ?? LabelType.Text;
				Update(); // Update all of the display data.
			};

			SKAnimatorToolsProxy.ConfigsLoadingAction = (int? progress, int? max, object? stateO) => {
				ProgressBarState? state = (ProgressBarState?)stateO;
				if (state.HasValue) {
					ModelLoadProgress.SetColorFromState(state.Value);
				}
				if (max.HasValue) {
					ModelLoadProgress.Maximum = max.Value;
				}
				if (progress.HasValue) {
					ModelLoadProgress.Value = progress.Value;
				}
			};
			#endregion

			#region Elements
			SKAnimatorToolsProxy.ModelLoaderWorker = ModelLoaderBGWorker;
			SKAnimatorToolsProxy.Progress = ModelLoadProgress;
			SKAnimatorToolsProxy.UISyncContext = UISyncContext;
			#endregion

			AsyncMessageBox.IsInGUIContext = true;
		}

		/// <summary>
		/// Loads user configurations.
		/// </summary>
		public void LoadConfigs() {
			Model3D.MultiplyScaleByHundred = UserConfiguration.ScaleBy100;
			Model3D.ProtectAgainstZeroScale = UserConfiguration.ProtectAgainstZeroScale;
			GLTFExporter.EmbedTextures = UserConfiguration.EmbedTextures;

			if (XanLogger.IsDebugMode) {
				if (UserConfiguration.LoggingLevel == XanLogger.INFO) {
					UserConfiguration.LoggingLevel = XanLogger.DEBUG;
				}
			}

			XanLogger.LoggingLevel = UserConfiguration.LoggingLevel;

			#region Prepare Information
			if (Directory.Exists(UserConfiguration.DefaultLoadDirectory)) {
				OpenModel.InitialDirectory = UserConfiguration.DefaultLoadDirectory;
				OpenModel.Tag = UserConfiguration.DefaultLoadDirectory; // Store it in the tag which makes it easy to keep track of.
			}
			if (Directory.Exists(UserConfiguration.LastSaveDirectory)) {
				SaveModel.InitialDirectory = UserConfiguration.LastSaveDirectory;
			}
			if (Directory.Exists(UserConfiguration.RsrcDirectory)) {
				SKEnvironment.RSRC_DIR = new DirectoryInfo(UserConfiguration.RsrcDirectory);
			}
			OpenModel.RestoreDirectory = UserConfiguration.RememberDirectoryAfterOpen;
			#endregion

			ConfigurationInterface.OnConfigurationChanged += OnConfigChanged;
			if (UserConfiguration.IsFirstTimeOpening) {
				MessageBox.Show("Welcome to ThreeRingsSharp! Before you can use the program, you need to set up your configuration so that the program knows where to look for game data.", "ThreeRingsSharp Setup", MessageBoxButtons.OK);
				OnConfigClicked(null, null);
			}
		}

		#endregion

		public MainWindow(string[] args) {
			UISyncContext = SynchronizationContext.Current;
			ConfigurationForm.CurrentVersion = THIS_VERSION[0] + "." + THIS_VERSION[1] + "." + THIS_VERSION[2];

			// Logger.setFactory(new DummyLoggerFactory());
			// Under the current modified version of OOOLibAndDeps, this MUST BE SET.
			// The new DLL makes the static ctor of Logger that creates this empty.

			// Why was this change made?
			// Strangely, some users get a TypeInitializationException in ClydeLog. Who gets this error is seemingly random and it
			// makes absolutely no sense. Ideally, removing the faulty field and replacing it with something that does nothing
			// should resolve the issue and make the program usable for these people.

			ShowUpdateDialogIfNeeded(args.Contains("forceupdate"));

			InitializeComponent();
			XanLogger.InitializeWith(ProgramLog);
			ModelLoadProgress = new ColoredProgressBar();
			ProgramTooltip = new FastToolTip(components!);

			SetupConsole();
			PopulateProxyReferences();
			LoadConfigs();
			
			if (XanLogger.LoggingLevel > XanLogger.INFO) {
				ShowWindow(ConsolePtr, 5);
			} else {
				ShowWindow(ConsolePtr, 0);
			}

			if (!UserConfiguration.IsFirstTimeOpening) MasterSKConfigs.Initialize(); // For first time, the dir might be wrong. Prevent init.
		}

		private void OnConfigChanged(string configKey, dynamic oldValue, dynamic newValue) {
			if (configKey == "RememberDirectoryAfterOpen") {
				OpenModel.RestoreDirectory = newValue;
				OpenModel.InitialDirectory = null;
			} else if (configKey == "RsrcDirectory") {
				if (Directory.Exists(newValue)) {
					SKEnvironment.RSRC_DIR = new DirectoryInfo(newValue);
				}
			} else if (configKey == "DefaultLoadDirectory") {
				OpenModel.InitialDirectory = newValue;
			} else if (configKey == "LastSaveDirectory") {
				SaveModel.InitialDirectory = newValue;
			} else if (configKey == "ScaleBy100") {
				Model3D.MultiplyScaleByHundred = newValue;
			} else if (configKey == "LoggingLevel") {
				XanLogger.LoggingLevel = newValue;
				if (newValue > XanLogger.INFO) {
					ShowWindow(ConsolePtr, 5);
				} else {
					ShowWindow(ConsolePtr, 0);
				}
			} else if (configKey == "EmbedTextures") {
				GLTFExporter.EmbedTextures = newValue;
			}
		}

		#region Opening Files

		private void OpenClicked(object sender, EventArgs e) {
			if (IsYieldingForModel) return;
			OpenModel.ShowDialog();
		}

		private void OnFileSelectedOpenModel(object sender, CancelEventArgs e) {
			// FileInfo file = new FileInfo(OpenModel.FileName);
			/*
			if (!VersionInfoScraper.IsValidClydeFile(file).Item1) {
				XanLogger.WriteLine("Can't open this file! It is not a valid Clyde file.", color: Color.DarkGoldenrod);
				XanLogger.ForceUpdateLog();
				e.Cancel = true;
				return;
			}

			// Check if the program has loaded up all of the external data, if it hasn't, wait.
			if (!ConfigReferenceBootstrapper.HasPopulatedConfigs) {
				XanLogger.WriteLine("Just a second! I'm still loading up the config references. The model will load once that's done.");
				XanLogger.ForceUpdateLog();
				IsYieldingForModel = true;
				BtnOpenModel.Enabled = false;
				SKAnimatorToolsProxy.ConfigsPopulatedAction = new Action(OnConfigsPopulated);
				return;
			}

			SKAnimatorToolsProxy.ResetProgress();
			*/
			ModelLoaderBGWorker.RunWorkerAsync(UISyncContext);
		}

		private void OnConfigsPopulated() {
			XanLogger.WriteLine("Alright. Let's load it up now.");
			//XanLogger.ForceUpdateLog();
			SKAnimatorToolsProxy.ResetProgress();
			ModelLoaderBGWorker.RunWorkerAsync(UISyncContext);
			IsYieldingForModel = false;
			BtnOpenModel.Enabled = true;
			//SKAnimatorToolsProxy.ConfigsPopulatedAction = null;
		}

		#endregion

		#region Saving Model

		private void SaveClicked(object sender, EventArgs e) {
			DialogResult result = SaveModel.ShowDialog();
			if (result == DialogResult.OK) {
				BtnSaveModel.Enabled = false;
				XanLogger.WriteLine("Exporting model...");

				FileInfo saveTo = new FileInfo(SaveModel.FileName);
				ConfigurationInterface.SetConfigurationValue("LastSaveDirectory", saveTo.DirectoryName);
				ModelFormat targetFmt = ModelFormatUtil.ExtensionToFormatBindings[saveTo.Extension];

				try {
					Model3D.ExportIntoOne(saveTo, targetFmt, CurrentContext!.AllModels.ToArray());
					XanLogger.WriteLine($"Done! Exported to [{saveTo.FullName}]");
				} catch (Exception ex) {
					XanLogger.WriteLine($"Failed to save to [{saveTo.FullName}] -- Reason: {ex.GetType().Name} thrown!\n{ex.Message}\n\n{ex.StackTrace}", color: Color.IndianRed);
				}
				BtnSaveModel.Enabled = true;
			}
		}

		#endregion

		/// <summary>
		/// NOTE: This runs in an external context from a BackgroundWorker!
		/// </summary>
		private void LoadOpenedModel() {
			if (Busy) return;
			SKAnimatorToolsProxy.UISyncContext?.Send(delegate {
				LabelFileName.Text = "Working...";
				LabelFormatVersion.Text = "N/A";
				LabelModelCompressed.Text = "N/A";
				LabelType.Text = "N/A";
			}, null);
			//RootDataTreeObject.ClearAllChildren();

			SKAnimatorToolsProxy.UISyncContext?.Send(delegate {
				ModelStructureTree.Nodes.Clear();
				SelectedObjectProperties.Nodes.Clear();
				BtnOpenModel.Enabled = false;
				BtnSaveModel.Enabled = false;
				BtnConfig.Enabled = false;
			}, null);

			FileInfo targetFile = new FileInfo(OpenModel.FileName);
			if (CurrentContext != null) CurrentContext.Dispose();
			CurrentContext = new ReadFileContext(targetFile);
			//ctx.Push(RootDataTreeObject);
			try {
				XanLogger.WriteLine("Working. This could take a bit...");
				// ClydeFileHandler.HandleClydeFile(clydeFile, AllModels, true, ModelStructureTree);
				MasterDataExtractor.PurgeCache();
				MasterDataExtractor.ExtractFrom(CurrentContext, (fName, vName, comp, baseType) => {
					SKAnimatorToolsProxy.UISyncContext?.Send(data => {
						(string file, string version, string compressed, string type) = (ValueTuple<string, string, string, string>)data!;
						LabelFileName.Text = file;
						LabelFormatVersion.Text = version;
						LabelModelCompressed.Text = compressed;
						LabelType.Text = type;
					}, (fName, vName, comp, baseType));
				});
				
			} catch (ClydeDataReadException exc) {
				XanLogger.WriteLine("Clyde Data Read Exception Thrown!\n" + exc.Message, color: Color.IndianRed);
				AsyncMessageBox.Show(exc.Message + "\n\n\nIt is safe to click CONTINUE after this error occurs, if you see another pop up.", exc.ErrorWindowTitle ?? "Oh no!", MessageBoxButtons.OK, exc.ErrorWindowIcon);
				SKAnimatorToolsProxy.UISyncContext?.Send(callback => {
					BtnOpenModel.Enabled = true;
					BtnSaveModel.Enabled = false;
					BtnConfig.Enabled = true;
				}, null);
				Busy = false;
				throw;
			} catch (TypeInitializationException tExc) {
				Exception err = tExc.InnerException!;
				if (err is ClydeDataReadException exc) {
					XanLogger.WriteLine("Clyde Data Read Exception Thrown!\n" + exc.Message, color: Color.IndianRed);
					AsyncMessageBox.Show(exc.Message + "\n\n\nIt is safe to click CONTINUE after this error occurs, if you see another pop up.", exc.ErrorWindowTitle ?? "Oh no!", MessageBoxButtons.OK, exc.ErrorWindowIcon);
				} else {
					XanLogger.WriteLine($"A critical error has occurred when processing: [{err.GetType().Name} Thrown]\n{err.Message}", color: Color.IndianRed);
					XanLogger.LogException(err);
					AsyncMessageBox.Show($"A critical error has occurred when attempting to process this file:\n{err.GetType().Name} -- {err.Message}\n\n\nIt is safe to click CONTINUE after this error occurs.", "Oh no!", icon: MessageBoxIcon.Error);
				}
				SKAnimatorToolsProxy.UISyncContext?.Send(callback => {
					BtnOpenModel.Enabled = true;
					BtnSaveModel.Enabled = false;
					BtnConfig.Enabled = true;
				}, null);
				Busy = false;
				throw;
			} catch (Exception err) {
				XanLogger.WriteLine($"A critical error has occurred when processing: [{err.GetType().Name} Thrown]\n{err.Message}", color: Color.IndianRed);
				XanLogger.LogException(err);
				AsyncMessageBox.Show($"A critical error has occurred when attempting to process this file:\n{err.GetType().Name} -- {err.Message}\n\n\nIt is safe to click CONTINUE after this error occurs.", "Oh no!", icon: MessageBoxIcon.Error);
				SKAnimatorToolsProxy.UISyncContext?.Send(callback => {
					BtnOpenModel.Enabled = true;
					BtnSaveModel.Enabled = false;
					BtnConfig.Enabled = true;
				}, null);
				Busy = false;
				throw;
			}

			SKAnimatorToolsProxy.UISyncContext?.Send(callbackParam => {
				if (callbackParam is TreeElement element) {
					SetModelStructure(element);
				}
			}, CurrentContext.Root);

			XanLogger.WriteLine($"Number of models loaded: {CurrentContext.AllModels.Where(model => model.IsEmptyObject == false).Count()} ({CurrentContext.AllModels.Where(model => model.ExtraData.ContainsKey("StaticSetConfig")).Count()} as variants in one or more StaticSetConfigs, which may not be exported depending on your preferences.)");

			// TODO: Something more efficient.
			int meshCount = 0;
			List<MeshData> alreadyCountedMeshes = new List<MeshData>(CurrentContext.AllModels.Count);
			for (int idx = 0; idx < CurrentContext.AllModels.Count; idx++) {
				if (!alreadyCountedMeshes.Contains(CurrentContext.AllModels[idx].Mesh!)) {
					meshCount++;
					alreadyCountedMeshes.Add(CurrentContext.AllModels[idx].Mesh!);
				}
			}
			alreadyCountedMeshes.Clear();

			XanLogger.WriteLine("Number of unique meshes instantiated: " + meshCount);
			XanLogger.WriteLine("Done loading this model!");
			SKAnimatorToolsProxy.UISyncContext?.Send(callback => {
				BtnOpenModel.Enabled = true;
				BtnSaveModel.Enabled = true;
				BtnConfig.Enabled = true;
				ProgramLog.Invalidate();
			}, null);
		}

		#region Properties Visualizer

		private TreeNode? LastNode { get; set; } = null;
		private void ChangeSelection(TreeNode original, int clicks = 1) {
			TreeElement element = (TreeElement)original.Tag;
			Debug.WriteLine(clicks);
			if (clicks > 1) {
				// Allow activated to fire again
				element.OnActivated(UISyncContext, this);
			} else {
				if (LastNode == original) return;
				// Only allow select to fire if it's the first time
				element.OnSelected(UISyncContext, this);
			}
			if (element.Properties != null) {
				SetPropertiesMenu(element.Properties);
			} else {
				SelectedObjectProperties.Nodes.Clear();
			}
		}

		private void OnMainNodeClicked(object sender, TreeNodeMouseClickEventArgs evt) {
			if (LastNode != null && LastNode.Tag is TreeElement previousElement) {
				if (LastNode == evt.Node) return; // Don't fire deselect if we click on the same thing.
				previousElement.OnDeselected(UISyncContext, this);
			}
			if (evt.Node.Tag is TreeElement element) {
				if (evt.Button == MouseButtons.Left) {
					ChangeSelection(evt.Node, evt.Clicks);
				} else {
					if (LastNode == evt.Node) return;
					// Only allow select to fire if it's the first time
					element.OnSelected(UISyncContext, this);
				}
			}

			LastNode = evt.Node;
		}

		private void OnNodeSelected(object sender, TreeViewEventArgs e) {
			ChangeSelection(e.Node);
			LastNode = e.Node;
		}

		private void SetModelStructure(TreeElement root) {
			ModelStructureTree.Nodes.Clear();
			if (root is RootSubstituteElement rse) {
				rse.AddToTreeView(ModelStructureTree);
			} else {
				ModelStructureTree.Nodes.Add(root.ConvertToNode());
			}
		}

		private void SetPropertiesMenu(TreeElement propsContainer) {
			if (propsContainer == null) return;

			SelectedObjectProperties.Nodes.Clear();
			if (propsContainer is RootSubstituteElement rootSub) {
				rootSub.AddToTreeView(SelectedObjectProperties);
			} else {
				SelectedObjectProperties.Nodes.Add(propsContainer.ConvertToNode());
			}
			// Need to translate properties manually rather than using hierarchy.
			/*
			foreach (KeyValuePair<DataTreeObjectProperty, List<DataTreeObject>> prop in propsContainer.Properties) {
				DataTreeObjectProperty propName = prop.Key;
				List<DataTreeObject> propValues = prop.Value;

				TreeNode nodeObj = propName.ToTreeNode();
				if (propName.DisplaySingleChildInline && propValues.Count == 1) {
					//nodeObj.Text += ": " + propValues[0].Text;
					if (!string.IsNullOrEmpty(propValues[0].Text)) {
						// Add the colon and value only if there's actually a value.
						// To create nested containers, it's easier to just create another data tree object without text, populate that new object with the sub-properties, and then add the new object as a property to something else.
						if (propName.ExtraData.ContainsKey("StaticSetConfig")) {
							// This is a StaticSetConfig so it needs to be able to choose between submodels.
							nodeObj.Tag = propName.ExtraData["StaticSetConfig"];
							nodeObj.ImageIndex = (int)SilkImage.Wrench;
							nodeObj.ForeColor = Color.Blue;
							nodeObj.ToolTipText = "Click on this to change the target model.";
							nodeObj.NodeFont = new Font(FontFamily.GenericSansSerif, 8.25f, FontStyle.Underline);
						}
						nodeObj.Text += ": " + propValues[0].Text;

					}
					if (!propValues[0].CreatedFromProperty) {
						// TreeNode propZeroNode = propValues[0].ToTreeNode();
						// nodeObj.Nodes.Add(propZeroNode);
						PopulateTreeNodeForProperties(nodeObj, propValues[0]);
					}
				} else {
					foreach (DataTreeObject property in propValues) {
						if (property.CreatedFromProperty) {
							nodeObj.Nodes.Add(property.ToTreeNode());
						} else {
							TreeNode containerNode = property.ToTreeNode();
							if (property.ExtraData.ContainsKey("RawOOOChoice")) {
								containerNode.Tag = (property.ExtraData["ModelConfig"], property.ExtraData["RawOOOChoice"]);
								containerNode.ImageIndex = (int)SilkImage.Value;
								containerNode.ForeColor = Color.Blue;
								containerNode.ToolTipText = "Click on this to change the Choice Option.";
								containerNode.NodeFont = new Font(FontFamily.GenericSansSerif, 8.25f, FontStyle.Underline);
							}
							nodeObj.Nodes.Add(containerNode);
							PopulateTreeNodeForProperties(containerNode, property);
						}
					}
				}
				SelectedObjectProperties.Nodes.Add(nodeObj);
			}
			*/
		}

		/*
		/// <summary>
		/// Identical to <see cref="SetPropertiesMenu(DataTreeObject)"/> but for nested properties.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="propsContainer"></param>
		private void PopulateTreeNodeForProperties(TreeNode node, DataTreeObject propsContainer) {
			if (propsContainer == null) return;
			// Need to translate properties manually rather than using hierarchy.
			foreach (KeyValuePair<DataTreeObjectProperty, List<DataTreeObject>> prop in propsContainer.Properties) {
				DataTreeObjectProperty propName = prop.Key;
				List<DataTreeObject> propValues = prop.Value;

				TreeNode nodeObj = propName.ToTreeNode();
				if (propName.DisplaySingleChildInline && propValues.Count == 1) {
					
					nodeObj.Text += ": " + propValues[0].Text;
					if (!propValues[0].CreatedFromProperty) {
						// TreeNode propZeroNode = propValues[0].ToTreeNode();
						// nodeObj.Nodes.Add(propZeroNode);
						PopulateTreeNodeForProperties(nodeObj, propValues[0]);
					}
				} else {
					foreach (DataTreeObject property in propValues) {
						if (property.CreatedFromProperty) {
							nodeObj.Nodes.Add(property.ToTreeNode());
						} else {
							TreeNode containerNode = property.ToTreeNode();
							nodeObj.Nodes.Add(containerNode);
							PopulateTreeNodeForProperties(containerNode, property);
						}
					}
				}
				node.Nodes.Add(nodeObj);
			}
		}
		*/
		#endregion

		#region Config GUI Handling

		private void OnConfigClicked(object? sender, EventArgs? e) {
			ConfigForm = new ConfigurationForm();
			ConfigForm.Show();
			ConfigForm.Activate();
			ConfigForm.FormClosed += OnConfigFormClosed;
		}

		private void OnConfigFormClosed(object? sender, FormClosedEventArgs? e) {
			ConfigForm = null;
			MasterSKConfigs.Initialize(); // Does nothing if it is already init
		}

		/// <summary>
		/// Force the config window to be the top window if it's open.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnMainWindowFocused(object? sender, EventArgs? e) {
			if (ConfigForm == null) return;
			ConfigForm.Activate();
		}

		#endregion

		#region StaticSetConfig & ConditionalConfig

		private void SaveModel_PromptFileExport(object? sender, CancelEventArgs? e) {
			bool hasStaticSetConfig = CurrentContext!.HasStaticSetConfig;
			bool hasConditionalConfig = false;//CurrentContext.AllModels.Where(model => model.ExtraData.ContainsKey("ConditionalConfigFlag")).Any();
			if (hasStaticSetConfig) {
				// NEW: If their model includes a StaticSetConfig, we need to give them the choice to export all or one model.
				DialogResult saveAllStaticSetModels = DialogResult.Cancel;
				if (UserConfiguration.StaticSetExportMode == 0) {
					// "Ask Me" mode
					saveAllStaticSetModels = MessageBox.Show(
						"The model you're saving contains one or more StaticSetConfigs! " +
						"Think of this type of model like a restaurant menu - it provides " +
						"a set of options, and it's often intended that only one of the " +
						"models is actually used.\n\n" +
						"In some cases, you might want all variants (for instance, if you " +
						"want to get every subtype of a gun such as the Antigua, you would " +
						"export all of the models in the set. Likewise, there are cases in which " +
						"you may want only one variant, for instance, in a StaticSetConfig " +
						"for a scene where it's using the set to select a specific tile or " +
						"prop.\n\n" +
						"With that in mind...\n" +
						"Would you like to export ALL of the models within the StaticSetConfigs?",
						"Export All StaticSetConfig components?",
						MessageBoxButtons.YesNoCancel,
						MessageBoxIcon.Information
					);
					if (saveAllStaticSetModels == DialogResult.Cancel) {
						e!.Cancel = true;
						return;
					}
				} else {
					if (UserConfiguration.StaticSetExportMode == 1) saveAllStaticSetModels = DialogResult.Yes;
					if (UserConfiguration.StaticSetExportMode == 2) saveAllStaticSetModels = DialogResult.No;
				}

				bool onlyExportActive = saveAllStaticSetModels == DialogResult.No;
				CurrentContext.UpdateExportabilityOfStaticSets(onlyExportActive);
			}
			if (hasConditionalConfig) {
				int expType = 5;
				if (UserConfiguration.ConditionalConfigExportMode == 0) {
					ConditionalConfigOptions cfgOpts = new ConditionalConfigOptions();
					expType = (int)cfgOpts.ShowDialog();
				} else {
					expType = UserConfiguration.ConditionalConfigExportMode;
				}
				if (expType == 5) {
					// 5 = cancel
					e!.Cancel = true;
					return;
				} else {
					// 1 = all
					// 2 = enabled
					// 3 = disabled
					// 4 = default
					foreach (Model3D model in CurrentContext.AllModels) {
						if (model.ExtraData.GetOrDefault("ConditionalConfigFlag", null) != null) {
							if (expType != 1) {
								bool isDefault = (bool)model.ExtraData.GetOrDefault("ConditionalConfigDefault", false)! == true;
								bool isEnabled = (bool)model.ExtraData.GetOrDefault("ConditionalConfigValue", false)! == true;
								bool skipExport = (expType == 2 && !isEnabled) || (expType == 3 && isEnabled) || (expType == 4 && !isDefault);
								// Skip conditions:
								// Export type is default only and it's not default
								// Export type is enabled and it's not enabled
								// Export type is disabled and it's enabled
								model.ExtraData["SkipExport"] = skipExport;
							} else {
								model.ExtraData["SkipExport"] = false;
							}
						}
					}
				}
			}
		}

		private void OnStaticSetOrChoiceSelectionClosed(object? sender, FormClosedEventArgs? e) {
			if (sender is ChangeTargetPrompt prompt) {
				string start = prompt.Node!.Text;
				if (start.Contains(":")) {
					start = start.Substring(0, start.IndexOf(":"));
				}
				if (prompt.Model != null) {
					prompt.Node.Text = $"{start}: {prompt.Model["model"]!}";
				} else if (prompt.Choice != null) {
					// Text = "Choice: " + choice.name + " [Current: " + choice.choice + "]"
					prompt.Node.Text = $"{start}: {prompt.Choice?.CurrentName ?? "ERR_NULL_CHOICE"}";
					//prompt.ChoiceAffects.ApplyArguments(prompt.Choice.Options.First(opt => opt.name == prompt.Choice.choice).arguments, prompt.Choice.name);
					prompt.Choice?.Current?.Apply();
				}
				prompt.FormClosed -= OnStaticSetOrChoiceSelectionClosed;
			}
		}

		private TreeElement? LastSelectedProperty = null;
		private TreeNode? LastActivatedPropertyNode = null;
		private void OnPropertyNodeClicked(object? sender, TreeNodeMouseClickEventArgs? evt) {
			TreeElement element = (TreeElement)evt!.Node.Tag;
			if (evt.Clicks >= 2) {
				LastActivatedPropertyNode = evt!.Node;
				element.OnActivated(UISyncContext, this);
			} else {
				if (LastSelectedProperty == element) return;
				if (LastSelectedProperty != null) {
					LastSelectedProperty.OnDeselected(UISyncContext, this);
				}
				LastSelectedProperty = element;
				element.OnSelected(UISyncContext, this);
			}
			/*
			if (e.Node.Tag is StaticSetConfig staticSetConfig) {
				ChangeTargetPrompt prompt = new ChangeTargetPrompt();
				prompt.SetPossibleOptionsFrom(staticSetConfig);
				prompt.Node = e.Node;
				prompt.FormClosed += OnStaticSetOrChoiceSelectionClosed;
				prompt.Show();
			} else if (e.Node.Tag is ValueTuple<object, object> directTag) {
				ParameterizedConfig cfg = directTag.Item1 as ParameterizedConfig;
				Parameter.Choice choice = directTag.Item2 as Parameter.Choice;

				ChangeTargetPrompt prompt = new ChangeTargetPrompt();
				prompt.SetPossibleOptionsFrom(cfg, choice);
				prompt.Node = e.Node;
				prompt.FormClosed += OnStaticSetOrChoiceSelectionClosed;
				prompt.Show();
			}
			*/
		}

		#endregion

		#region Utilities for external stuffs

		public void ShowChangeTargetPrompt(ShadowClass staticSetConfig) {
			ChangeTargetPrompt ctp = new ChangeTargetPrompt();
			ctp.SetPossibleOptionsFrom(staticSetConfig);
			ctp.FormClosed += OnStaticSetOrChoiceSelectionClosed;
			ctp.Node = LastActivatedPropertyNode!;
			ctp.Show(this);
		}

		public void ShowChangeTargetPrompt(ShadowClass parameterizedConfig, Choice choice) {
			ChangeTargetPrompt ctp = new ChangeTargetPrompt();
			ctp.SetPossibleOptionsFrom(parameterizedConfig, choice);
			ctp.FormClosed += OnStaticSetOrChoiceSelectionClosed;
			ctp.Node = LastActivatedPropertyNode!;
			ctp.Show(this);
		}

		#endregion

		#region Background Worker

		private void ModelLoaderBGWorker_DoWork(object? sender, DoWorkEventArgs? e) {
			if (SKAnimatorToolsProxy.UISyncContext == null) SKAnimatorToolsProxy.UISyncContext = e!.Argument as SynchronizationContext;
			LoadOpenedModel();
		}

		private void ModelLoaderBGWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			if (e.UserState is int maxProgress) {
				ModelLoadProgress.Maximum = maxProgress;
			} else if (e.UserState is ProgressBarState state) {
				ModelLoadProgress.SetColorFromState(state);
				return;
			}
			if (e.ProgressPercentage > 0) {
				ModelLoadProgress.Value = e.ProgressPercentage;
			}
		}

		private void ModelLoaderBGWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs? e) {
			Update();
			Refresh();
		}

		#endregion
	}
}
