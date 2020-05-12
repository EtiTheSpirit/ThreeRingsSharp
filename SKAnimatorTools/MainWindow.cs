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
			ModelConfigBrancher.GetRootDataTreeObject = () => RootDataTreeObject;
			string loadDir = ConfigurationInterface.GetConfigurationValue("DefaultLoadDirectory", @"C:\", true);
			string saveDir = ConfigurationInterface.GetConfigurationValue("LastSaveDirectory", @"C:\", true);
			bool restoreDirectoryWhenLoading = ConfigurationInterface.GetConfigurationValue("RememberDirectoryAfterOpen", false, true);
			if (Directory.Exists(loadDir)) {
				OpenModel.InitialDirectory = loadDir;
			}
			if (Directory.Exists(saveDir)) {
				SaveModel.InitialDirectory = saveDir;
			}
			OpenModel.RestoreDirectory = restoreDirectoryWhenLoading;

			ConfigurationInterface.OnConfigurationChanged += OnConfigChanged;
		}

		private void OnConfigChanged(string configKey, dynamic oldValue, dynamic newValue) {
			if (configKey == "RememberDirectoryAfterOpen") {
				OpenModel.RestoreDirectory = newValue;
			}
		}

		/// <summary>
		/// The root <see cref="DataTreeObject"/> that represents the loaded model in the object hierarchy.<para/>
		/// Rather than creating a new instance for every time a model is loaded, call <see cref="DataTreeObject.ClearAllChildren"/> instead.
		/// </summary>
		public static DataTreeObject RootDataTreeObject { get; } = new DataTreeObject();

		/// <summary>
		/// A reference to the current active <see cref="ModelConfigBrancher"/>
		/// </summary>
		private static ModelConfigBrancher CurrentBrancher { get; set; }

		private void OpenClicked(object sender, EventArgs e) {
			DialogResult result = OpenModel.ShowDialog();
			if (result == DialogResult.OK) {
				DataTreeObjectEventMarshaller.ClearAllNodeBindings();
				RootDataTreeObject.ClearAllChildren();
				RootDataTreeObject.Properties.Clear();
				ModelStructureTree.Nodes.Clear();


				FileInfo fInfo = new FileInfo(OpenModel.FileName);
				XanLogger.WriteLine($"Loading [{fInfo.FullName}]...");
				ProgramLog.Update();
				if (!VersionInfoScraper.IsValidClydeFile(fInfo)) {
					XanLogger.WriteLine("Invalid file. Sending error.");
					AsyncMessageBox.Show("This file isn't a valid Clyde file! (Reason: Incorrect header)", "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				(string, string, string) cosmeticInfo = VersionInfoScraper.GetCosmeticInformation(fInfo);
				XanLogger.WriteLine($"Read file to grab the raw info.");
				string modelFullClass = cosmeticInfo.Item3;
				string[] modelClassInfo = ClassNameStripper.GetSplitClassName(modelFullClass);

				string modelClass = null;
				string modelSubclass = null;
				if (modelClassInfo != null) {
					modelClass = modelClassInfo[0];
					if (modelClassInfo.Length == 2) modelSubclass = modelClassInfo[1];
				}

				if (modelClass == "ProjectXModelConfig") {
					XanLogger.WriteLine("User imported a Player Knight model. These are unsupported. Sending warning.");
					MessageBox.Show("Player Knights do not use the standard ArticulatedConfig type (used for all animated character models) and instead use a unique type called ProjectXModelConfig. Unless the Spiral Knights jar is directly referenced, this type cannot be loaded.\n\nThankfully, an automatic fix will be employed for you! I'm going to load /rsrc/character/npc/crew/model.dat instead, which is a Knight model that uses ArticulatedConfig since it's an NPC.", "Knights are not supported!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					string characterFolder = fInfo.Directory.Parent.FullName;
					if (!characterFolder.EndsWith("/")) characterFolder += "/";
					fInfo = new FileInfo(characterFolder + "npc/crew/model.dat");
					if (!fInfo.Exists) {
						XanLogger.WriteLine("Failed to locate substitute crew NPC model in target directory.");
						AsyncMessageBox.Show("Oh no! The file at " + fInfo.FullName + " doesn't exist :(\nThis path is intended to work only for cases where you loaded /rsrc/character/pc/model.dat directly, so if this was a custom saved .DAT file, this error was bound to happen.", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						LabelFileName.Text = "Nothing Loaded!";
						LabelModelCompressed.Text = "N/A";
						LabelFormatVersion.Text = "N/A";
						LabelType.Text = "N/A";
						return;
					}
				}

				LabelFileName.Text = fInfo.Name;
				LabelModelCompressed.Text = cosmeticInfo.Item1;
				LabelFormatVersion.Text = cosmeticInfo.Item2;
				LabelType.Text = "Processing...";

				DataInputStream dataInput = new DataInputStream(new FileInputStream(fInfo.FullName));
				BinaryImporter importer = new BinaryImporter(dataInput);
				CurrentBrancher = new ModelConfigBrancher();


				var obj = (java.lang.Object)importer.readObject();
				if (obj is ModelConfig model) {
					LabelType.Text = "ModelConfig";
					CurrentBrancher.HandleDataFrom(fInfo, model);
				} else {
					LabelType.Text = "Unknown! :(";
					AsyncMessageBox.Show("Oh Fiddlesticks! While this *is* a valid .DAT file, I'm afraid I can't actually do anything with this data!\n\nData Class:\n" + obj.getClass().getTypeName(), "Invalid Type", MessageBoxButtons.OK, MessageBoxIcon.Error);
					RootDataTreeObject.Text = modelClass;
					RootDataTreeObject.ImageKey = SilkImage.Generic;
				}

				ModelStructureTree.Nodes.Add(RootDataTreeObject.ConvertHierarchyToTreeNodes());
				SetPropertiesMenu(DataTreeObjectEventMarshaller.GetDataObjectOf(ModelStructureTree.Nodes[0]));

				BtnSaveModel.Enabled = CurrentBrancher.OK;
			}
		}

		private void SaveClicked(object sender, EventArgs e) {
			DialogResult result = SaveModel.ShowDialog();
			if (result == DialogResult.OK) {
				FileInfo saveTo = new FileInfo(SaveModel.FileName);
				ConfigurationInterface.SetConfigurationValue("LastSaveDirectory", saveTo.DirectoryName);
				ModelFormat targetFmt = ModelFormatUtil.ExtensionToFormatBindings[saveTo.Extension];

				if (CurrentBrancher.OK) {
					//Model3D.ExportIntoOne(saveTo, targetFmt, CurrentBrancher.Models.ToArray());
					try {
						CurrentBrancher.SaveAllToFile(saveTo, targetFmt);
						XanLogger.WriteLine($"Done! Exported to [{saveTo.FullName}]");
					} catch (System.Exception ex) {
						XanLogger.WriteLine($"Failed to save to [{saveTo.FullName}] -- Reason: {ex.GetType().Name} thrown!\n{ex.Message}");
					}
				}
			}
		}

		private void OnNodeClicked(object sender, TreeNodeMouseClickEventArgs evt) {
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
			ConfigurationForm cfgForm = new ConfigurationForm();
			cfgForm.SetDataFromConfig(OpenModel.InitialDirectory, SaveModel.InitialDirectory, OpenModel.RestoreDirectory);
			cfgForm.Show();
		}
	}
}
