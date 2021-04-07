using Microsoft.WindowsAPICodePack.Dialogs;
using SKAnimatorTools.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.XansData.Structs;

namespace SKAnimatorTools {
	public partial class ConfigurationForm : Form {

		/// <summary>
		/// A general default directory for where Spiral Knights might be located on most users' PCs.
		/// </summary>
		public const string DEFAULT_DIRECTORY = @"C:\Program Files (x86)\Steam\steamapps\common\Spiral Knights\rsrc";

		/// <summary>
		/// The current version to display at the top of the config form. Set by the main window's update checker routine.
		/// </summary>
		public static string CurrentVersion { get; set; } = "0.0.0";

		/// <summary>
		/// Whether or not the program is out of date. Set by the main window's update checker routine.
		/// </summary>
		public static bool IsOutOfDate { get; set; } = false;

		/// <summary>
		/// The text to use in the version display label.
		/// </summary>
		private static string VersionLabel => $"ThreeRingsSharp v{CurrentVersion}{(IsOutOfDate ? " (New Version Available)" : "")}";

		/// <summary>
		/// True if the values in the UI are all okay and it's safe to save the configuration values.
		/// </summary>
		private bool IsOK { get; set; } = true;

		/// <summary>
		/// An image of a green circle with a white checkmark.
		/// </summary>
		private static readonly Bitmap Accepted = Properties.Resources.Accept;

		/// <summary>
		/// An image of a yellow triangle with an exclamation point.
		/// </summary>
		private static readonly Bitmap Warning = Properties.Resources.Warning;

		/// <summary>
		/// An image of a red circle with an exclamation point.
		/// </summary>
		private static readonly Bitmap Error = Properties.Resources.Exclamation;

		/// <summary>
		/// A blue circle with a lowercase letter <c>i</c> within it.
		/// </summary>
		private static readonly Bitmap Information = Properties.Resources.Info;

		public ConfigurationForm() {
			InitializeComponent();
			LabelCurrentVersion.Text = VersionLabel;
			IsOK = true;

			TextBox_DefaultLoadLoc.Text = UserConfiguration.DefaultLoadDirectory;
			TextBox_DefaultSaveLoc.Text = UserConfiguration.LastSaveDirectory;
			TextBox_RsrcDirectory.Text = UserConfiguration.RsrcDirectory;
			CheckBox_RememberLastLoad.Checked = UserConfiguration.RememberDirectoryAfterOpen;
			CheckBox_MultiplyScaleByHundred.Checked = UserConfiguration.ScaleBy100;
			CheckBox_ProtectAgainstZeroScale.Checked = UserConfiguration.ProtectAgainstZeroScale;
			Option_ConditionalExportMode.SelectedIndex = UserConfiguration.ConditionalConfigExportMode;
			CheckBox_EmbedTextures.Checked = UserConfiguration.EmbedTextures;
			Option_LogLevel.SelectedIndex = UserConfiguration.LoggingLevel;
			Option_StaticSetExportMode.SelectedIndex = UserConfiguration.StaticSetExportMode;
			VerifyAllPathIntegrity();
		}

		private void BtnCancel_Click(object sender, EventArgs e) {
			Close();
		}

		private void BtnSave_Click(object sender, EventArgs e) {
			UserConfiguration.DefaultLoadDirectory = TextBox_DefaultLoadLoc.Text;
			UserConfiguration.LastSaveDirectory = TextBox_DefaultSaveLoc.Text;
			UserConfiguration.RsrcDirectory = TextBox_RsrcDirectory.Text;
			UserConfiguration.RememberDirectoryAfterOpen = CheckBox_RememberLastLoad.Checked;
			UserConfiguration.ScaleBy100 = CheckBox_MultiplyScaleByHundred.Checked;
			UserConfiguration.ProtectAgainstZeroScale = CheckBox_ProtectAgainstZeroScale.Checked;
			UserConfiguration.ConditionalConfigExportMode = Option_ConditionalExportMode.SelectedIndex;
			UserConfiguration.StaticSetExportMode = Option_StaticSetExportMode.SelectedIndex;
			UserConfiguration.EmbedTextures = CheckBox_EmbedTextures.Checked;
			UserConfiguration.LoggingLevel = Option_LogLevel.SelectedIndex;
			UserConfiguration.IsFirstTimeOpening = false;

			UserConfiguration.SaveConfiguration();
			Close();
		}

		private void VerifyLoadLocationIntegrity(object sender, CancelEventArgs e) => VerifyAllPathIntegrity();

		private void VerifySaveLocationIntegrity(object sender, CancelEventArgs e) => VerifyAllPathIntegrity();

		private void VerifyRsrcDirectoryIntegrity(object sender, CancelEventArgs e) => VerifyAllPathIntegrity();

		/// <summary>
		/// Verifies the integrity of both the load and save path.
		/// </summary>
		private void VerifyAllPathIntegrity() {
			string loadPath = TextBox_DefaultLoadLoc.Text;
			string savePath = TextBox_DefaultSaveLoc.Text;
			string rsrcPath = TextBox_RsrcDirectory.Text;
			bool loadOK = IsPathOK(loadPath);
			bool saveOK = IsPathOK(savePath);
			bool rsrcOK = IsPathOK(rsrcPath);
			IsOK = loadOK && saveOK & rsrcOK;

			PicBox_DefLoadLoc.Image = loadOK ? Accepted : Error;
			MainTooltip.SetToolTip(PicBox_DefLoadLoc,
				loadOK ? string.Empty : "The given directory is invalid (you didn't input a path) or it doesn't exist!"
			);

			PicBox_DefSaveLoc.Image = saveOK ? Accepted : Error;
			MainTooltip.SetToolTip(PicBox_DefSaveLoc,
				saveOK ? string.Empty : "The given directory is invalid (you didn't input a path) or it doesn't exist!"
			);

			if (rsrcOK) {
				bool namedRsrc = new DirectoryInfo(rsrcPath).Name == "rsrc";
				if (namedRsrc) {
					PicBox_RsrcDir.Image = Accepted;
					MainTooltip.SetToolTip(PicBox_RsrcDir, string.Empty);
				} else {
					PicBox_RsrcDir.Image = Warning;
					MainTooltip.SetToolTip(PicBox_RsrcDir, "This path is valid, but the folder is not named rsrc. Only continue if you're sure this is the right folder (e.g. you made it)!");
				}
			} else {
				PicBox_RsrcDir.Image = Error;
				MainTooltip.SetToolTip(PicBox_RsrcDir, "The given directory is invalid (you didn't input a path) or it doesn't exist!");
			}

			Option_StaticSetExportMode_SelectedIndexChanged(null, null);

			BtnSave.Enabled = IsOK;
		}

		/// <summary>
		/// Returns <see langword="true"/> if the given path is valid and exists, <see langword="false"/> if it does not.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private bool IsPathOK(string path) {
			try {
				DirectoryInfo dir = new DirectoryInfo(path);
				if (!dir.Exists) {
					return false;
				}
				return true;
			} catch { }
			return false;
		}

		private void ProtectAgainstZeroScaleChanged(object sender, EventArgs e) {
			if (CheckBox_ProtectAgainstZeroScale.Checked == false) {
				PicBox_ZeroScale.Image = Warning;
				MainTooltip.SetToolTip(PicBox_ZeroScale, "Some models may be unusable in your editor if they have a zero scale!");
			} else {
				PicBox_ZeroScale.Image = Accepted;
				MainTooltip.SetToolTip(PicBox_ZeroScale, string.Empty);
			}
		}

		private void VerboseLoggingChanged(object sender, EventArgs e) {
			// SelectedIndex translates to the level nicely.
			if (Option_LogLevel.SelectedIndex > XanLogger.INFO) {
				PicBox_VerboseLogging.Image = Information;
				MainTooltip.SetToolTip(PicBox_VerboseLogging, "Enabling debug or trace logging can make tracking progress hard for standard users, and it is only advised to do this for actual program debugging.");
			} else {
				PicBox_VerboseLogging.Image = Accepted;
				MainTooltip.SetToolTip(PicBox_VerboseLogging, string.Empty);
			}
		}

		private void BtnSelectDefLoadLoc_Click(object sender, EventArgs e) {
			CommonOpenFileDialog dialog = new CommonOpenFileDialog {
				InitialDirectory = Directory.Exists(TextBox_DefaultLoadLoc.Text) ? TextBox_DefaultLoadLoc.Text : DEFAULT_DIRECTORY,
				IsFolderPicker = true
			};
			if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
				TextBox_DefaultLoadLoc.Text = dialog.FileName;
				VerifyAllPathIntegrity();
			}
		}

		private void BtnSelectDefSaveLoc_Click(object sender, EventArgs e) {
			CommonOpenFileDialog dialog = new CommonOpenFileDialog {
				InitialDirectory = Directory.Exists(TextBox_DefaultSaveLoc.Text) ? TextBox_DefaultSaveLoc.Text : "C:\\",
				IsFolderPicker = true
			};
			if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
				TextBox_DefaultSaveLoc.Text = dialog.FileName;
				VerifyAllPathIntegrity();
			}
		}

		private void BtnSelectDefRsrcLoc_Click(object sender, EventArgs e) {
			CommonOpenFileDialog dialog = new CommonOpenFileDialog {
				InitialDirectory = Directory.Exists(TextBox_RsrcDirectory.Text) ? TextBox_RsrcDirectory.Text : DEFAULT_DIRECTORY,
				IsFolderPicker = true
			};
			if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
				TextBox_RsrcDirectory.Text = dialog.FileName;
				VerifyAllPathIntegrity();
			}
		}


		private void BtnHelpConditionalConfig_Click(object sender, EventArgs e) {
			MessageBox.Show(
				"ConditionalConfigs are a special type of model that rely on game " +
				"code to determine whether or not they show up. Generally speaking, " +
				"these are only used in scenes (for example, they might be used " +
				"when a model appears or disappears after you step on a button or " +
				"flip a switch).\n\nFor most cases, the default option is fine, but " +
				"if you want an automatic selection, models whose conditions = true " +
				"is a good default.", "What Are ConditionalConfigs?", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void BtnHelpStaticSetConfig_Click(object sender, EventArgs e) {
			MessageBox.Show(
				"StaticSetConfigs are, hence the name, a set of meshes. " +
				"Generally speaking, these are used for models that contain several " +
				"variations. These are used extensively for things like tiles in scenes " +
				"as they can pack hundreds of models into a single file. Every StaticSet " +
				"has a value that says which model out of the set is being used, which is " +
				"referred to as the \"Target Model\".\n\n" +
				"For most cases, only exporting the target model in the " +
				"set is the best way to go.", "What Are StaticSetConfigs?", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void OnPicButtonMouseEnter(object sender, EventArgs e) {
			((Control)sender).BackColor = ((SolidBrush)SystemBrushes.ControlLight).Color;
		}

		private void OnPicButtonMouseLeave(object sender, EventArgs e) {
			((Control)sender).BackColor = ((SolidBrush)SystemBrushes.Control).Color;
		}

		private void CheckBox_PreferSpeed_CheckedChanged(object sender, EventArgs e) {
			VerboseLoggingChanged(sender, e);
		}

		private void Option_StaticSetExportMode_SelectedIndexChanged(object sender, EventArgs e) {
			if (Option_StaticSetExportMode.SelectedIndex == 1) {
				PicBox_StaticSetExpMode.Image = Warning;
				MainTooltip.SetToolTip(PicBox_StaticSetExpMode, "This option can result in a exporting lot of extra/junk models that you don't want!");
			} else {
				PicBox_StaticSetExpMode.Image = Accepted;
				MainTooltip.SetToolTip(PicBox_StaticSetExpMode, string.Empty);
			}
		}
	}
}
