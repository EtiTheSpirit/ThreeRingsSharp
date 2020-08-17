using com.threerings.opengl.model.config;
using SKAnimatorTools.Configuration;
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
using System.Windows.Forms;
using ThreeRingsSharp;
using ThreeRingsSharp.DataHandlers;
using ThreeRingsSharp.DataHandlers.Properties;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.Utility.Interface;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.XansData.Exceptions;
using ThreeRingsSharp.XansData.Extensions;
using ThreeRingsSharp.XansData.IO.GLTF;
using ThreeRingsSharp.XansData.XML.ConfigReferences;

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
		public static SynchronizationContext UISyncContext { get; private set; }

		#region GUI Information

		/// <summary>
		/// The root <see cref="DataTreeObject"/> that represents the loaded model in the object hierarchy.<para/>
		/// Rather than creating a new instance for every time a model is loaded, call <see cref="DataTreeObject.ClearAllChildren"/> instead.
		/// </summary>
		public DataTreeObject RootDataTreeObject { get; } = new DataTreeObject();

		/// <summary>
		/// All models from the latest opened .DAT file.
		/// </summary>
		private List<Model3D> AllModels { get; set; } = new List<Model3D>();

		/// <summary>
		/// The current configuration form, if it exists.
		/// </summary>
		private ConfigurationForm ConfigForm { get; set; }

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
		public readonly int[] THIS_VERSION = { 2, 0, 0 };

		/// <summary>
		/// Attempts to access the github to acquire the latest version.
		/// </summary>
		/// <param name="version"></param>
		/// <returns></returns>
		public bool TryGetVersion(out (int, int, int)? version) {
			try {
				using (WebClient cli = new WebClient()) {
					string v = cli.DownloadString("https://raw.githubusercontent.com/XanTheDragon/ThreeRingsSharp/master/version.txt");
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

		/// <summary>
		/// Checks the version of the program, and shows the update window if this instance of the program is out of date.
		/// </summary>
		/// <returns></returns>
		public void ShowUpdateDialogIfNeeded() {
			if (TryGetVersion(out (int, int, int)? newVersion) && newVersion.HasValue) {
				(int major, int minor, int patch) = newVersion.Value;
				bool newMajor = major > THIS_VERSION[0];
				bool newMinor = minor > THIS_VERSION[1];
				bool newPatch = patch > THIS_VERSION[2];
				bool newUpdate = newMajor || newMinor || newPatch;
				// Benefit of semver: ALL increments = new update.

				if (newUpdate) {
					string verStr = major + "." + minor + "." + patch;
					Updater updWindow = new Updater(verStr);
					updWindow.ShowDialog(); // Because I want it to yield.
				}
			}
		}

		#endregion

		#region Initialization Code

		/// <summary>
		/// Sets up the Windows console by enabling VT (if possible), setting it's title, and preparing other necessary information.
		/// </summary>
		public void SetupConsole() {
			Console.Title = "Spiral Knights Animator Tools";
			VTConsole.EnableVTSupport();
			ConsolePtr = GetConsoleWindow();
		}

		/// <summary>
		/// Populates proxy references which allows the DLL library if TRS to communicate with this portion of the program.
		/// </summary>
		public void PopulateProxyReferences() {
			XanLogger.BoxReference = ProgramLog;

			#region Actions
			SKAnimatorToolsProxy.UpdateGUIAction = (string fileName, string isCompressed, string formatVersion, string type) => {
				LabelFileName.Text = fileName ?? LabelFileName.Text;
				LabelModelCompressed.Text = isCompressed ?? LabelModelCompressed.Text;
				LabelFormatVersion.Text = formatVersion ?? LabelFormatVersion.Text;
				LabelType.Text = type ?? LabelType.Text;
				Update(); // Update all of the display data.
			};

			SKAnimatorToolsProxy.ConfigsErroredAction = error => {
				MessageBox.Show(error.Message + "\n\nThe program cannot continue when this error occurs and must exit, as this data is 100% required for it to function properly.", "Configuration Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Environment.Exit(1);
			};

			SKAnimatorToolsProxy.ConfigsLoadingAction = (int? progress, int? max, ProgressBarState? state) => {
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

			SKAnimatorToolsProxy.PreferSpeedOverFeedback = UserConfiguration.PreferSpeed;
			XanLogger.LoggingLevel = UserConfiguration.LoggingLevel;

			#region Prepare Information
			if (SKAnimatorToolsProxy.PreferSpeedOverFeedback) {
				ModelLoadProgress.SpecialDisabled = true;
				ProgramTooltip.SetToolTip(ModelLoadProgress, "The loading bar will not render if Prefer Speed Over Feedback is enabled.");
			}
			if (Directory.Exists(UserConfiguration.DefaultLoadDirectory)) {
				OpenModel.InitialDirectory = UserConfiguration.DefaultLoadDirectory;
				OpenModel.Tag = UserConfiguration.DefaultLoadDirectory; // Store it in the tag which makes it easy to keep track of.
			}
			if (Directory.Exists(UserConfiguration.LastSaveDirectory)) {
				SaveModel.InitialDirectory = UserConfiguration.LastSaveDirectory;
			}
			if (Directory.Exists(UserConfiguration.RsrcDirectory)) {
				ResourceDirectoryGrabber.ResourceDirectory = new DirectoryInfo(UserConfiguration.RsrcDirectory);
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

		public MainWindow() {
			UISyncContext = SynchronizationContext.Current;
			InitializeComponent();

			SetupConsole();
			PopulateProxyReferences();
			LoadConfigs();

			ConfigurationForm.CurrentVersion = THIS_VERSION[0] + "." + THIS_VERSION[1] + "." + THIS_VERSION[2];
			ConfigReferenceBootstrapper.PopulateConfigRefsAsync();

			XanLogger.UpdateAutomatically = false;
			if (XanLogger.LoggingLevel > XanLogger.STANDARD) {
				ShowWindow(ConsolePtr, 5);
			} else {
				ShowWindow(ConsolePtr, 0);
			}
		}

		private void OnConfigChanged(string configKey, dynamic oldValue, dynamic newValue) {
			if (configKey == "RememberDirectoryAfterOpen") {
				OpenModel.RestoreDirectory = newValue;
			} else if (configKey == "RsrcDirectory") {
				if (Directory.Exists(newValue)) {
					ResourceDirectoryGrabber.ResourceDirectory = new DirectoryInfo(newValue);
				}
			} else if (configKey == "DefaultLoadDirectory") {
				OpenModel.InitialDirectory = newValue;
			} else if (configKey == "LastSaveDirectory") {
				SaveModel.InitialDirectory = newValue;
			} else if (configKey == "ScaleBy100") {
				Model3D.MultiplyScaleByHundred = newValue;
			} else if (configKey == "LoggingLevel") {
				XanLogger.LoggingLevel = newValue;
				if (newValue > XanLogger.STANDARD) {
					ShowWindow(ConsolePtr, 5);
				} else {
					ShowWindow(ConsolePtr, 0);
				}
			} else if (configKey == "EmbedTextures") {
				GLTFExporter.EmbedTextures = newValue;
			} else if (configKey == "PreferSpeed") {
				SKAnimatorToolsProxy.PreferSpeedOverFeedback = newValue;
				if (newValue == true) {
					// Render the progress bar a different color. All progress bar changes will be deferred.
					ModelLoadProgress.SpecialDisabled = true;
					ProgramTooltip.SetToolTip(ModelLoadProgress, "The loading bar will not render if Prefer Speed Over Feedback is enabled.");
				} else {
					ModelLoadProgress.SpecialDisabled = false;
					ProgramTooltip.SetToolTip(ModelLoadProgress, string.Empty);
				}
			}
		}

		#region Opening Files

		private void OpenClicked(object sender, EventArgs e) {
			if (IsYieldingForModel) return;
			OpenModel.ShowDialog();
		}

		private void OnFileSelectedOpenModel(object sender, CancelEventArgs e) {
			FileInfo file = new FileInfo(OpenModel.FileName);
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
			ModelLoaderBGWorker.RunWorkerAsync(UISyncContext);
		}

		private void OnConfigsPopulated() {
			XanLogger.WriteLine("Alright. Let's load it up now.");
			XanLogger.ForceUpdateLog();
			SKAnimatorToolsProxy.ResetProgress();
			ModelLoaderBGWorker.RunWorkerAsync(UISyncContext);
			IsYieldingForModel = false;
			BtnOpenModel.Enabled = true;
			SKAnimatorToolsProxy.ConfigsPopulatedAction = null;
		}

		#endregion

		#region Saving Model

		private void SaveClicked(object sender, EventArgs e) {
			DialogResult result = SaveModel.ShowDialog();
			if (result == DialogResult.OK) {
				FileInfo saveTo = new FileInfo(SaveModel.FileName);
				ConfigurationInterface.SetConfigurationValue("LastSaveDirectory", saveTo.DirectoryName);
				ModelFormat targetFmt = ModelFormatUtil.ExtensionToFormatBindings[saveTo.Extension];

				try {
					Model3D.ExportIntoOne(saveTo, targetFmt, AllModels.ToArray());
					XanLogger.WriteLine($"Done! Exported to [{saveTo.FullName}]");
				} catch (Exception ex) {
					XanLogger.WriteLine($"Failed to save to [{saveTo.FullName}] -- Reason: {ex.GetType().Name} thrown!\n{ex.Message}", color: Color.IndianRed);
				}
			}
		}

		#endregion

		/// <summary>
		/// NOTE: This runs in an external context!
		/// </summary>
		private void LoadOpenedModel() {
			if (Busy) return;
			SKAnimatorToolsProxy.UISyncContext?.Send(callback => {
				BtnOpenModel.Enabled = false;
			}, null);
			DataTreeObjectEventMarshaller.ClearAllNodeBindings();
			RootDataTreeObject.ClearAllChildren();
			RootDataTreeObject.Properties.Clear();

			SKAnimatorToolsProxy.UISyncContext?.Send(callback => {
				ModelStructureTree.Nodes.Clear();
			}, null);

			FileInfo clydeFile = new FileInfo(OpenModel.FileName);
			AllModels.Clear();
			bool isOK = true;
			XanLogger.UpdateAutomatically = false;
			try {
				XanLogger.WriteLine("Working. This might take a bit...");
				XanLogger.UpdateLog();
				ClydeFileHandler.HandleClydeFile(clydeFile, AllModels, true, ModelStructureTree);
			} catch (ClydeDataReadException exc) {
				XanLogger.WriteLine("Clyde Data Read Exception Thrown!\n" + exc.Message, color: Color.IndianRed);
				AsyncMessageBox.Show(exc.Message + "\n\n\nIt is safe to click CONTINUE after this error occurs.", exc.ErrorWindowTitle ?? "Oh no!", MessageBoxButtons.OK, exc.ErrorWindowIcon);
				isOK = false;
				SKAnimatorToolsProxy.UISyncContext?.Send(callback => {
					BtnOpenModel.Enabled = true;
				}, null);
				Busy = false;
				throw;
			} catch (TypeInitializationException tExc) {
				Exception err = tExc.InnerException;
				if (err is ClydeDataReadException exc) {
					XanLogger.WriteLine("Clyde Data Read Exception Thrown!\n" + exc.Message, color: Color.IndianRed);
					AsyncMessageBox.Show(exc.Message + "\n\n\nIt is safe to click CONTINUE after this error occurs.", exc.ErrorWindowTitle ?? "Oh no!", MessageBoxButtons.OK, exc.ErrorWindowIcon);
					isOK = false;
				} else {
					XanLogger.WriteLine($"A critical error has occurred when processing: [{err.GetType().Name} Thrown]\n{err.Message}", color: Color.IndianRed);
					XanLogger.LogException(err);
					AsyncMessageBox.Show($"A critical error has occurred when attempting to process this file:\n{err.GetType().Name} -- {err.Message}\n\n\nIt is safe to click CONTINUE after this error occurs.", "Oh no!", icon: MessageBoxIcon.Error);
					isOK = false;
				}
				SKAnimatorToolsProxy.UISyncContext?.Send(callback => {
					BtnOpenModel.Enabled = true;
				}, null);
				Busy = false;
				throw;
			} catch (Exception err) {
				XanLogger.WriteLine($"A critical error has occurred when processing: [{err.GetType().Name} Thrown]\n{err.Message}", color: Color.IndianRed);
				XanLogger.LogException(err);
				AsyncMessageBox.Show($"A critical error has occurred when attempting to process this file:\n{err.GetType().Name} -- {err.Message}\n\n\nIt is safe to click CONTINUE after this error occurs.", "Oh no!", icon: MessageBoxIcon.Error);
				isOK = false;
				SKAnimatorToolsProxy.UISyncContext?.Send(callback => {
					BtnOpenModel.Enabled = true;
				}, null);
				Busy = false;
				throw;
			}

			SKAnimatorToolsProxy.UISyncContext?.Send(callbackParam => {
				TreeView modelStructureTree = callbackParam as TreeView;
				if (modelStructureTree.Nodes.Count != 0 && modelStructureTree.Nodes[0] != null) {
					SetPropertiesMenu(DataTreeObjectEventMarshaller.GetDataObjectOf(modelStructureTree.Nodes[0]));
				}
			}, ModelStructureTree);


			Debug.WriteLine(SKAnimatorToolsProxy.UISyncContext);
			SKAnimatorToolsProxy.UISyncContext?.Send(callbackParam => {
				Debug.WriteLine(BtnSaveModel.Enabled);
				Debug.WriteLine(callbackParam);
				BtnSaveModel.Enabled = (bool)callbackParam;
			}, isOK);
			XanLogger.UpdateAutomatically = true;
			XanLogger.WriteLine($"Number of models loaded: {AllModels.Where(model => model.IsEmptyObject == false).Count()} ({AllModels.Where(model => model.ExtraData.ContainsKey("StaticSetConfig")).Count()} as variants in one or more StaticSetConfigs, which may not be exported depending on your preferences.)");

			// TODO: Something more efficient.
			int meshCount = 0;
			List<MeshData> alreadyCountedMeshes = new List<MeshData>(AllModels.Count);
			for (int idx = 0; idx < AllModels.Count; idx++) {
				if (!alreadyCountedMeshes.Contains(AllModels[idx].Mesh)) {
					meshCount++;
					alreadyCountedMeshes.Add(AllModels[idx].Mesh);
				}
			}
			alreadyCountedMeshes.Clear();

			XanLogger.WriteLine("Number of unique meshes instantiated: " + meshCount);
			SKAnimatorToolsProxy.UISyncContext?.Send(callback => {
				BtnOpenModel.Enabled = true;
			}, null);
		}

		#region Properties Visualizer

		private TreeNode LastNode { get; set; } = null;
		private void OnNodeClicked(object sender, TreeNodeMouseClickEventArgs evt) {
			if (LastNode == evt.Node) return;
			LastNode = evt.Node;
			DataTreeObject associatedDataTreeObject = DataTreeObjectEventMarshaller.GetDataObjectOf(evt.Node);
			if (associatedDataTreeObject != null) {
				SetPropertiesMenu(associatedDataTreeObject);
			} else {
				XanLogger.WriteLine("Warning: Props is null!", XanLogger.DEBUG, Color.DarkGoldenrod);
			}
		}

		private void SetPropertiesMenu(DataTreeObject propsContainer) {
			if (propsContainer == null) return;

			SelectedObjectProperties.Nodes.Clear();
			// Need to translate properties manually rather than using hierarchy.
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
							nodeObj.Nodes.Add(containerNode);
							PopulateTreeNodeForProperties(containerNode, property);
						}
					}
				}
				SelectedObjectProperties.Nodes.Add(nodeObj);
			}
		}

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
						if (!property.CreatedFromProperty) {
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
		#endregion

		#region Config GUI Handling

		private void OnConfigClicked(object sender, EventArgs e) {
			ConfigForm = new ConfigurationForm();
			ConfigForm.Show();
			ConfigForm.Activate();
			ConfigForm.FormClosed += OnConfigFormClosed;
		}

		private void OnConfigFormClosed(object sender, FormClosedEventArgs e) {
			ConfigForm = null;
		}

		/// <summary>
		/// Force the config window to be the top window if it's open.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnMainWindowFocused(object sender, EventArgs e) {
			if (ConfigForm == null) return;
			ConfigForm.Activate();
		}

		#endregion

		#region StaticSetConfig & ConditionalConfig

		private void SaveModel_PromptFileExport(object sender, CancelEventArgs e) {
			bool hasStaticSetConfig = AllModels.Where(model => model.ExtraData.ContainsKey("StaticSetConfig")).Count() > 0;
			bool hasConditionalConfig = AllModels.Where(model => model.ExtraData.ContainsKey("ConditionalConfigFlag")).Count() > 0;
			if (hasStaticSetConfig) {
				// NEW: If their model includes a StaticSetConfig, we need to give them the choice to export all or one model.
				DialogResult saveAllStaticSetModels = DialogResult.Cancel;
				if (UserConfiguration.StaticSetExportMode == 0) {
					saveAllStaticSetModels = MessageBox.Show(
						"The model you're saving contains one or more StaticSetConfigs! " +
						"This type of model is used like a variant selection (it provides " +
						"a set of variants, and it's intended that only one of the models " +
						"is actually used by the system.\n\n" +
						"In some cases, you might want all variants (for instance, if you " +
						"want to get a subtype of a gun such as the Antigua, you would export " +
						"all of the models in the set and then pick the one you want in your " +
						"3D editor). Likewise, there are cases in which " +
						"you may want only one variant, for instance, in a CompoundConfig " +
						"for a scene where it's using the set to select a specific tile or " +
						"prop.\n\n" +
						"Would you like to export ALL of the models within the StaticSetConfigs?",
						"Export All StaticSetConfig components?",
						MessageBoxButtons.YesNoCancel,
						MessageBoxIcon.Information
					);
					if (saveAllStaticSetModels == DialogResult.Cancel) {
						e.Cancel = true;
						return;
					}
				} else {
					if (UserConfiguration.StaticSetExportMode == 1) saveAllStaticSetModels = DialogResult.Yes;
					if (UserConfiguration.StaticSetExportMode == 2) saveAllStaticSetModels = DialogResult.No;
				}

				bool onlyExportActive = saveAllStaticSetModels == DialogResult.No;
				foreach (Model3D model in AllModels) {
					model.ExtraData["SkipExport"] = false; // This is important because if we change any data, we want to clear this out and restart from scratch.
					StaticSetConfig associatedStaticSet = (StaticSetConfig)model.ExtraData.GetOrDefault("StaticSetConfig", null);
					if (associatedStaticSet != null) {
						string targetModel = (string)model.ExtraData.GetOrDefault("StaticSetEntryName", null);
						bool isTargetModel = associatedStaticSet.model == targetModel;

						bool shouldExport = (isTargetModel && onlyExportActive) || !onlyExportActive;
						// Should export if:
						// - This is the target model and only export active is true, or
						// - Only export active is false
						model.ExtraData["SkipExport"] = !shouldExport;
					}
				}
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
					e.Cancel = true;
					return;
				} else {
					// 1 = all
					// 2 = enabled
					// 3 = disabled
					// 4 = default
					foreach (Model3D model in AllModels) {
						if (model.ExtraData.GetOrDefault("ConditionalConfigFlag", null) != null) {
							if (expType != 1) {
								bool isDefault = (bool)model.ExtraData.GetOrDefault("ConditionalConfigDefault", false) == true;
								bool isEnabled = (bool)model.ExtraData.GetOrDefault("ConditionalConfigValue", false) == true;
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

		private void OnStaticSetSelectionClosed(object sender, FormClosedEventArgs e) {
			if (sender is ChangeTargetPrompt prompt) {
				string start = prompt.Node.Text;
				if (start.Contains(":")) {
					start = start.Substring(0, start.IndexOf(":"));
				}
				prompt.Node.Text = start + ": " + prompt.Model.model;
				prompt.FormClosed -= OnStaticSetSelectionClosed;
			}
		}

		private void SelectedObjectProperties_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) {
			if (e.Node.Tag is StaticSetConfig staticSetConfig) {
				ChangeTargetPrompt prompt = new ChangeTargetPrompt();
				prompt.SetPossibleOptionsFrom(staticSetConfig);
				prompt.Node = e.Node;
				prompt.FormClosed += OnStaticSetSelectionClosed;
				prompt.Show();
			}
		}

		#endregion

		private void ModelLoaderBGWorker_DoWork(object sender, DoWorkEventArgs e) {
			if (SKAnimatorToolsProxy.UISyncContext == null) SKAnimatorToolsProxy.UISyncContext = e.Argument as SynchronizationContext;
			LoadOpenedModel();
		}

		private void ModelLoaderBGWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			if (SKAnimatorToolsProxy.PreferSpeedOverFeedback) return;
			

			if (e.UserState is int maxProgress) {
				ModelLoadProgress.Maximum = maxProgress;
			} else if (e.UserState is ProgressBarState state) {
				ModelLoadProgress.SetColorFromState(state);
				return;
			}
			if (e.ProgressPercentage > 0) {
				ModelLoadProgress.Value = e.ProgressPercentage;
			}
			XanLogger.UpdateLog();
			ModelLoadProgress.Update();
		}

		private void ModelLoaderBGWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			XanLogger.ForceUpdateLog();
			Update();
		}
	}
}
