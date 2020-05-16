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
using java.awt;
using ThreeRingsSharp.DataHandlers;
using ThreeRingsSharp.Utility.Interface;
using ThreeRingsSharp.XansData;
using SKAnimatorTools.Configuration;

namespace SKAnimatorTools {
	public partial class MainWindow : Form {
		public MainWindow() {
			InitializeComponent();
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
			ClydeFileHandler.MultiplyScaleByHundred = ConfigurationInterface.GetConfigurationValue("ScaleBy100", true, true);
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

			ConfigurationInterface.OnConfigurationChanged += OnConfigChanged;
			if (isFreshLoad) {
				MessageBox.Show("Welcome to ThreeRingsSharp! Before you can use the program, you need to set up your configuration so that the program knows where to look for game data.", "ThreeRingsSharp Setup", MessageBoxButtons.OK);
				OnConfigClicked(null, null);
			}
		}

		/// <summary>
		/// The current configuration form, if it exists.
		/// </summary>
		private ConfigurationForm ConfigForm { get; set; }

		private void OnConfigChanged(string configKey, dynamic oldValue, dynamic newValue) {
			if (configKey == "RememberDirectoryAfterOpen") {
				OpenModel.RestoreDirectory = newValue;
			} else if (configKey == "RsrcDirectory") {
				if (Directory.Exists(newValue)) {
					ResourceDirectoryGrabber.ResourceDirectory = new DirectoryInfo(newValue);
				}
			} else if (configKey == "ScaleBy100") {
				ClydeFileHandler.MultiplyScaleByHundred = newValue;
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
		private static List<Model3D> AllModels;

		private void OpenClicked(object sender, EventArgs e) {
			DialogResult result = OpenModel.ShowDialog();
			if (result == DialogResult.OK) {
				DataTreeObjectEventMarshaller.ClearAllNodeBindings();
				RootDataTreeObject.ClearAllChildren();
				RootDataTreeObject.Properties.Clear();
				ModelStructureTree.Nodes.Clear();


				FileInfo clydeFile = new FileInfo(OpenModel.FileName);
				AllModels = new List<Model3D>();
				bool isOK = true;
				try {
					ClydeFileHandler.HandleClydeFile(clydeFile, AllModels, true, ModelStructureTree);
				} catch (System.Exception err) {
					XanLogger.WriteLine($"A critical error has occurred when processing: [{err.GetType().Name} Thrown]\n{err.Message}");
					AsyncMessageBox.Show($"A critical error has occurred when attempting to process this file:\n{err.GetType().Name} -- {err.Message}", "Oh no!", icon: MessageBoxIcon.Error);
					isOK = false;
				}

				if (ModelStructureTree.Nodes.Count != 0 && ModelStructureTree.Nodes[0] != null) SetPropertiesMenu(DataTreeObjectEventMarshaller.GetDataObjectOf(ModelStructureTree.Nodes[0]));
				//BtnSaveModel.Enabled = CurrentBrancher.OK;
				BtnSaveModel.Enabled = isOK;
				XanLogger.WriteLine("Number of models: " + AllModels.Count);
			}
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
				Debug.WriteLine("Warning: Props is null!");
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
					if (propValues[0].CreatedFromProperty) {
						nodeObj.Text += ": " + propValues[0].Text;
					} else {
						TreeNode propZeroNode = propValues[0].ToTreeNode();
						nodeObj.Nodes.Add(propZeroNode);
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
					if (propValues[0].CreatedFromProperty) {
						nodeObj.Text += ": " + propValues[0].Text;
					} else {
						TreeNode propZeroNode = propValues[0].ToTreeNode();
						nodeObj.Nodes.Add(propZeroNode);
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

		private void OnConfigClicked(object sender, EventArgs e) {
			ConfigForm = new ConfigurationForm();
			ConfigForm.SetDataFromConfig(OpenModel.InitialDirectory, SaveModel.InitialDirectory, ResourceDirectoryGrabber.ResourceDirectory?.FullName ?? @"C:\", OpenModel.RestoreDirectory);
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
	}
}
