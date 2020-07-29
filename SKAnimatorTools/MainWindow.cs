using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ThreeRingsSharp.Utility;

using java.lang;
using java.io;
using com.threerings.export;
using System.Diagnostics;
using com.threerings.opengl.model.config;
using ThreeRingsSharp.DataHandlers;
using ThreeRingsSharp.Utility.Interface;
using ThreeRingsSharp.XansData;
using SKAnimatorTools.Configuration;
using ThreeRingsSharp.XansData.Exceptions;
using ThreeRingsSharp.XansData.IO.GLTF;
using ThreeRingsSharp.XansData.Extensions;
using ThreeRingsSharp.XansData.XML.ConfigReferences;
using System.Threading;
using ThreeRingsSharp.DataHandlers.Properties;
using ThreeRingsSharp.DataHandlers.Model;
using System.Runtime.InteropServices;

namespace SKAnimatorTools {
	public partial class MainWindow : Form {

		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();

		public readonly IntPtr ConsolePtr;

		public MainWindow() {
			VTConsole.EnableVTSupport();
			System.Console.Title = "Spiral Knights Animator Tools";
			ConsolePtr = GetConsoleWindow();
			InitializeComponent();
			ConfigReferenceBootstrapper.UISyncContext = SynchronizationContext.Current;

			XanLogger.BoxReference = ProgramLog;
			// ClydeFileHandler.GetRootDataTreeObject = () => RootDataTreeObject;
			ClydeFileHandler.UpdateGUIAction = (string fileName, string isCompressed, string formatVersion, string type) => {
				LabelFileName.Text = fileName ?? LabelFileName.Text;
				LabelModelCompressed.Text = isCompressed ?? LabelModelCompressed.Text;
				LabelFormatVersion.Text = formatVersion ?? LabelFormatVersion.Text;
				LabelType.Text = type ?? LabelType.Text;
				Update(); // Update all of the display data.
			};
			AsyncMessageBox.IsInGUIContext = true;

			bool isFreshLoad = ConfigurationInterface.GetConfigurationValue("IsFirstTimeOpening", true, true);
			string loadDir = ConfigurationInterface.GetConfigurationValue("DefaultLoadDirectory", @"C:\", true);
			string saveDir = ConfigurationInterface.GetConfigurationValue("LastSaveDirectory", @"C:\", true);
			string rsrcDir = ConfigurationInterface.GetConfigurationValue("RsrcDirectory", @"C:\", true);
			bool restoreDirectoryWhenLoading = ConfigurationInterface.GetConfigurationValue("RememberDirectoryAfterOpen", false, true);
			Model3D.MultiplyScaleByHundred = ConfigurationInterface.GetConfigurationValue("ScaleBy100", true, true);
			Model3D.ProtectAgainstZeroScale = ConfigurationInterface.GetConfigurationValue("ProtectAgainstZeroScale", true, true);
			Model3D.TargetUpAxis = ConfigurationForm.AxisIntMap[(int)ConfigurationInterface.GetConfigurationValue("UpAxisIndex", 1, true)];
			GLTFExporter.EmbedTextures = ConfigurationInterface.GetConfigurationValue("EmbedTextures", false, true);
			XanLogger.VerboseLogging = ConfigurationInterface.GetConfigurationValue("VerboseLogging", false, true);
			StaticSetExportMode = (int)ConfigurationInterface.GetConfigurationValue("StaticSetExportMode", 0L, true);
			ModelPropertyUtility.TryGettingAllTextures = ConfigurationInterface.GetConfigurationValue("GetAllTextures", true, true);
			if (Directory.Exists(loadDir)) {
				OpenModel.InitialDirectory = loadDir;
			}
			if (Directory.Exists(saveDir)) {
				SaveModel.InitialDirectory = saveDir;
			}
			if (Directory.Exists(rsrcDir)) {
				ResourceDirectoryGrabber.ResourceDirectory = new DirectoryInfo(rsrcDir);
			}
			OpenModel.RestoreDirectory = restoreDirectoryWhenLoading;

			//BtnOpenModel.Tag = BtnOpenModel.Text;
			//BtnOpenModel.Enabled = false;
			//BtnOpenModel.Text = "Loading Configs. Hold up...";

			// ConfigReferenceBootstrapper.OnConfigsPopulated += OnConfigReferencesPopulated;

			_ = ConfigReferenceBootstrapper.PopulateConfigRefsAsync();

			ConfigurationInterface.OnConfigurationChanged += OnConfigChanged;
			if (isFreshLoad) {
				MessageBox.Show("Welcome to ThreeRingsSharp! Before you can use the program, you need to set up your configuration so that the program knows where to look for game data.", "ThreeRingsSharp Setup", MessageBoxButtons.OK);
				OnConfigClicked(null, null);
			}

			XanLogger.UpdateAutomatically = false;
			if (XanLogger.VerboseLogging) {
				ShowWindow(ConsolePtr, 5);
			} else {
				ShowWindow(ConsolePtr, 0);
			}
		}

		/*
		private void OnConfigReferencesPopulated() {
			BtnOpenModel.Enabled = true;
			ConfigReferenceBootstrapper.OnConfigsPopulated -= OnConfigReferencesPopulated;
		}*/

		/// <summary>
		/// The mode to use when dealing with <see cref="StaticSetConfig"/> instances in the export.<para/>
		/// <c>0 = Prompt Me</c><para/>
		/// <c>1 = All</c><para/>
		/// <c>2 = One</c><para/>
		/// </summary>
		public int StaticSetExportMode { get; set; } = 0;

		/// <summary>
		/// The mode to use when dealing with <see cref="ConditionalConfig"/> instances in the export.<para/>
		/// <c>0 = Prompt Me</c><para/>
		/// <c>1 = All models</c><para/>
		/// <c>2 = Enabled models</c><para/>
		/// <c>3 = Disabled models</c><para/>
		/// <c>4 = Default model</c><para/>
		/// </summary>
		public int ConditionalExportMode { get; set; } = 0;

		/// <summary>
		/// The current configuration form, if it exists.
		/// </summary>
		private ConfigurationForm ConfigForm { get; set; }

		private void OnConfigChanged(string configKey, dynamic oldValue, dynamic newValue) {
			if (configKey == "RememberDirectoryAfterOpen") {
				OpenModel.RestoreDirectory = newValue;
				if (newValue == false) {
					OpenModel.InitialDirectory = ConfigurationInterface.GetConfigurationValue("DefaultLoadDirectory", @"C:\", true);
				} else {
					OpenModel.InitialDirectory = string.Empty;
				}
			} else if (configKey == "RsrcDirectory") {
				if (Directory.Exists(newValue)) {
					ResourceDirectoryGrabber.ResourceDirectory = new DirectoryInfo(newValue);
				}
			} else if (configKey == "ScaleBy100") {
				Model3D.MultiplyScaleByHundred = newValue;
			} else if (configKey == "VerboseLogging") {
				XanLogger.VerboseLogging = newValue;
				if (newValue == true) {
					ShowWindow(ConsolePtr, 5);
				} else {
					ShowWindow(ConsolePtr, 0);
				}
			} else if (configKey == "EmbedTextures") {
				GLTFExporter.EmbedTextures = newValue;
			} else if (configKey == "StaticSetExportMode") {
				StaticSetExportMode = newValue;
			} else if (configKey == "GetAllTextures") {
				ModelPropertyUtility.TryGettingAllTextures = newValue;
			} else if (configKey == "ConditionalConfigExportMode") {
				ConditionalExportMode = newValue;
			}
		}

		/// <summary>
		/// The root <see cref="DataTreeObject"/> that represents the loaded model in the object hierarchy.<para/>
		/// Rather than creating a new instance for every time a model is loaded, call <see cref="DataTreeObject.ClearAllChildren"/> instead.
		/// </summary>
		public static DataTreeObject RootDataTreeObject { get; } = new DataTreeObject();

		/// <summary>
		/// All models from the latest opened .DAT file.
		/// </summary>
		private static List<Model3D> AllModels { get; set; } = new List<Model3D>();

		/// <summary>
		/// This will be true of we've opened a model before configs were loaded. This is used to prevent two delays at once.
		/// </summary>
		private static bool IsYieldingForModel = false;

		private void OpenClicked(object sender, EventArgs e) {
			if (IsYieldingForModel) return;
			DialogResult result = OpenModel.ShowDialog();
			if (result == DialogResult.OK) {

				if (!ConfigReferenceBootstrapper.HasPopulatedConfigs) {
					XanLogger.WriteLine("Just a second! I'm still loading up the config references. The model will load once that's done.");
					XanLogger.UpdateLog();
					IsYieldingForModel = true;
					BtnOpenModel.Enabled = false;
					ConfigReferenceBootstrapper.OnConfigsPopulatedAction = new Action(OnConfigsPopulated);
					return;
				}

				LoadOpenedModel();
			}
		}

		private void LoadOpenedModel() {
			DataTreeObjectEventMarshaller.ClearAllNodeBindings();
			RootDataTreeObject.ClearAllChildren();
			RootDataTreeObject.Properties.Clear();
			ModelStructureTree.Nodes.Clear();

			FileInfo clydeFile = new FileInfo(OpenModel.FileName);
			AllModels.Clear();
			bool isOK = true;
			XanLogger.UpdateAutomatically = false;
			try {
				XanLogger.WriteLine("Working. This might take a bit...");
				XanLogger.UpdateLog();
				ClydeFileHandler.HandleClydeFile(clydeFile, AllModels, true, ModelStructureTree);
			} catch (ClydeDataReadException exc) {
				XanLogger.WriteLine("Clyde Data Read Exception Thrown!\n" + exc.Message, false, Color.IndianRed);
				AsyncMessageBox.Show(exc.Message + "\n\n\nIt is safe to click CONTINUE after this error occurs.", exc.ErrorWindowTitle ?? "Oh no!", MessageBoxButtons.OK, exc.ErrorWindowIcon);
				isOK = false;
				throw;
			} catch (TypeInitializationException tExc) {
				System.Exception err = tExc.InnerException;
				if (err is ClydeDataReadException exc) {
					XanLogger.WriteLine("Clyde Data Read Exception Thrown!\n" + exc.Message, false, Color.IndianRed);
					AsyncMessageBox.Show(exc.Message + "\n\n\nIt is safe to click CONTINUE after this error occurs.", exc.ErrorWindowTitle ?? "Oh no!", MessageBoxButtons.OK, exc.ErrorWindowIcon);
					isOK = false;
				} else {
					XanLogger.WriteLine($"A critical error has occurred when processing: [{err.GetType().Name} Thrown]\n{err.Message}", false, Color.IndianRed);
					AsyncMessageBox.Show($"A critical error has occurred when attempting to process this file:\n{err.GetType().Name} -- {err.Message}\n\n\nIt is safe to click CONTINUE after this error occurs.", "Oh no!", icon: MessageBoxIcon.Error);
					isOK = false;
				}
				throw;
			} catch (System.Exception err) {
				XanLogger.WriteLine($"A critical error has occurred when processing: [{err.GetType().Name} Thrown]\n{err.Message}", false, Color.IndianRed);
				AsyncMessageBox.Show($"A critical error has occurred when attempting to process this file:\n{err.GetType().Name} -- {err.Message}\n\n\nIt is safe to click CONTINUE after this error occurs.", "Oh no!", icon: MessageBoxIcon.Error);
				isOK = false;
				throw;
			}

			if (ModelStructureTree.Nodes.Count != 0 && ModelStructureTree.Nodes[0] != null) SetPropertiesMenu(DataTreeObjectEventMarshaller.GetDataObjectOf(ModelStructureTree.Nodes[0]));
			//BtnSaveModel.Enabled = CurrentBrancher.OK;
			BtnSaveModel.Enabled = isOK;
			XanLogger.UpdateAutomatically = true;
			XanLogger.WriteLine($"Number of models loaded: {AllModels.Count} ({AllModels.Where(model => model.ExtraData.ContainsKey("UnselectedStaticSetModel")).Count()} as variants in one or more StaticSetConfigs, which may not be exported depending on your preferences.)");

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
		}

		private void OnConfigsPopulated() {
			LoadOpenedModel();
			IsYieldingForModel = false;
			BtnOpenModel.Enabled = true;
			ConfigReferenceBootstrapper.OnConfigsPopulatedAction = null;
		}

		private void SaveClicked(object sender, EventArgs e) {
			DialogResult result = SaveModel.ShowDialog();
			if (result == DialogResult.OK) {
				FileInfo saveTo = new FileInfo(SaveModel.FileName);
				ConfigurationInterface.SetConfigurationValue("LastSaveDirectory", saveTo.DirectoryName);
				ModelFormat targetFmt = ModelFormatUtil.ExtensionToFormatBindings[saveTo.Extension];

				try {
					Model3D.ExportIntoOne(saveTo, targetFmt, AllModels.ToArray());
					XanLogger.WriteLine($"Done! Exported to [{saveTo.FullName}]");
				} catch (System.Exception ex) {
					XanLogger.WriteLine($"Failed to save to [{saveTo.FullName}] -- Reason: {ex.GetType().Name} thrown!\n{ex.Message}");
				}
			}
		}

		private TreeNode LastNode { get; set; } = null;
		private void OnNodeClicked(object sender, TreeNodeMouseClickEventArgs evt) {
			if (LastNode == evt.Node) return;
			LastNode = evt.Node;
			DataTreeObject associatedDataTreeObject = DataTreeObjectEventMarshaller.GetDataObjectOf(evt.Node);
			if (associatedDataTreeObject != null) {
				SetPropertiesMenu(associatedDataTreeObject);
			} else {
				Debug.WriteLine("Warning: Props is null!", false, Color.DarkGoldenrod);
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
							nodeObj.ForeColor = Color.Blue;
							nodeObj.ToolTipText = "Click on this to change the target model.";
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

		private void OnConfigClicked(object sender, EventArgs e) {
			ConfigForm = new ConfigurationForm();
			// todo: better method of doing this
			ConfigForm.SetDataFromConfig(
				OpenModel.InitialDirectory, 
				SaveModel.InitialDirectory, 
				ResourceDirectoryGrabber.ResourceDirectory?.FullName ?? @"C:\", 
				OpenModel.RestoreDirectory, 
				Model3D.MultiplyScaleByHundred, 
				Model3D.ProtectAgainstZeroScale, 
				ConditionalExportMode, 
				GLTFExporter.EmbedTextures, 
				XanLogger.VerboseLogging, 
				StaticSetExportMode,
				ModelPropertyUtility.TryGettingAllTextures
			);
			ConfigForm.Show();
			ConfigForm.Activate();
			ConfigForm.FormClosed += OnConfigFormClosed;
		}

		private void OnConfigFormClosed(object sender, FormClosedEventArgs e) {
			ConfigForm = null;
		}

		private void OnMainWindowFocused(object sender, EventArgs e) {
			if (ConfigForm == null) return;
			ConfigForm.Activate();
		}

		private void SaveModel_PromptFileExport(object sender, CancelEventArgs e) {
			bool hasStaticSetConfig = AllModels.Where(model => model.ExtraData.ContainsKey("UnselectedStaticSetModel")).Count() > 0;
			bool hasConditionalConfig = AllModels.Where(model => model.ExtraData.ContainsKey("ConditionalConfigFlag")).Count() > 0;
			if (hasStaticSetConfig) {
				// NEW: If their model includes a StaticSetConfig, we need to give them the choice to export all or one model.
				DialogResult saveAllStaticSetModels = DialogResult.Cancel;
				if (StaticSetExportMode == 0) {
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
					if (StaticSetExportMode == 1) saveAllStaticSetModels = DialogResult.Yes;
					if (StaticSetExportMode == 2) saveAllStaticSetModels = DialogResult.No;
				}
				foreach (Model3D model in AllModels) {
					if ((bool)model.ExtraData.GetOrDefault("UnselectedStaticSetModel", false) == true) {
						model.ExtraData["SkipExport"] = saveAllStaticSetModels == DialogResult.No;
						// If we select no, we want to skip models that aren't the selected ones.
					}
				}
			}
			if (hasConditionalConfig) {
				ConditionalConfigOptions cfgOpts = new ConditionalConfigOptions();
				int expType = (int)cfgOpts.ShowDialog();
				cfgOpts = null;
				if (expType == 5) {
					// 5 = cancel
					e.Cancel = true;
					return;
				} else {
					// 1 = all
					// 2 = default
					// 3 = enabled
					// 4 = disabled
					foreach (Model3D model in AllModels) {
						if (model.ExtraData.GetOrDefault("ConditionalConfigFlag", null) != null) {
							if (expType != 1) {
								bool isDefault = (bool)model.ExtraData.GetOrDefault("ConditionalConfigDefault", false) == true;
								bool isEnabled = (bool)model.ExtraData.GetOrDefault("ConditionalConfigValue", false) == true;
								bool skipExport = (expType == 2 && !isDefault) || (expType == 3 && !isEnabled) || (expType == 4 && isEnabled);
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
	}
}
