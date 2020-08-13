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

		public const string DEFAULT_DIRECTORY = @"C:\Program Files (x86)\Steam\steamapps\common\Spiral Knights\rsrc";

		public static string CurrentVersion { get; set; } = "0.0.0";

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
			LabelCurrentVersion.Text = "ThreeRingsSharp v" + CurrentVersion;
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
			CheckBox_PreferSpeed.Checked = UserConfiguration.PreferSpeed;
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
			UserConfiguration.PreferSpeed = CheckBox_PreferSpeed.Checked;
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
			if (Option_LogLevel.SelectedIndex > XanLogger.STANDARD) {
				if (!CheckBox_PreferSpeed.Checked) {
					PicBox_VerboseLogging.Image = Warning;
					MainTooltip.SetToolTip(PicBox_VerboseLogging, "Enabling debug or trace logging can slow down the program\n(it has to wait while the text is written to the console). This does not need to be set\nto change how latest.log is written, and is only useful for debugging during runtime.");
				} else {
					PicBox_VerboseLogging.Image = Information;
					MainTooltip.SetToolTip(PicBox_VerboseLogging, "Generally, setting this above standard is not advised, however since\nPrefer Speed Over Feedback is enabled, the effects of this option do not apply.");
				}
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

		private void OnFolderSelMouseEnter(object sender, EventArgs e) {
			((Control)sender).BackColor = ((SolidBrush)SystemBrushes.ControlLight).Color;
		}

		private void OnFolderSelMouseLeave(object sender, EventArgs e) {
			((Control)sender).BackColor = ((SolidBrush)SystemBrushes.Control).Color;
		}

		private void CheckBox_PreferSpeed_CheckedChanged(object sender, EventArgs e) {
			VerboseLoggingChanged(sender, e);
		}
	}
}
