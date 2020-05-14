using SKAnimatorTools.Configuration;
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

namespace SKAnimatorTools {
	public partial class ConfigurationForm : Form {

		/// <summary>
		/// True if the values in the UI are all okay and it's safe to save the configuration values.
		/// </summary>
		private bool IsOK { get; set; } = true;

		/// <summary>
		/// A reference to a red color that represents an invalid control.
		/// </summary>
		private static readonly Color RedColor = Color.FromArgb(255, 127, 127);

		/// <summary>
		/// A reference to a yellow color that represents a control *could* be invalid (e.g. rsrc directory isn't named rsrc).
		/// </summary>
		private static readonly Color YellowColor = Color.FromArgb(255, 255, 127);

		public ConfigurationForm() {
			InitializeComponent();
			MainTooltip.ShowAlways = true;
			MainTooltip.AutoPopDelay = short.MaxValue;
			MainTooltip.ShowAlways = true;
			MainTooltip.InitialDelay = 200;
			MainTooltip.ReshowDelay = 200;
			IsOK = true;
		}

		public void SetDataFromConfig(string defaultLoad, string defaultSave, string defaultRsrc, bool rememberLoad) {
			TextBox_DefaultLoadLoc.Text = defaultLoad;
			TextBox_DefaultSaveLoc.Text = defaultSave;
			TextBox_RsrcDirectory.Text = defaultRsrc;
			CheckBox_RememberLastLoad.Checked = rememberLoad;
			VerifyAllPathIntegrity();
		}

		private void BtnCancel_Click(object sender, EventArgs e) {
			Close();
		}

		private void BtnSave_Click(object sender, EventArgs e) {
			ConfigurationInterface.SetConfigurationValue("DefaultLoadDirectory", TextBox_DefaultLoadLoc.Text);
			ConfigurationInterface.SetConfigurationValue("LastSaveDirectory", TextBox_DefaultSaveLoc.Text);
			ConfigurationInterface.SetConfigurationValue("RememberDirectoryAfterOpen", CheckBox_RememberLastLoad.Checked);
			ConfigurationInterface.SetConfigurationValue("RsrcDirectory", TextBox_RsrcDirectory.Text);
			ConfigurationInterface.SetConfigurationValue("IsFirstTimeOpening", false);
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

			TextBox_DefaultLoadLoc.BackColor = loadOK ? SystemColors.Window : RedColor;
			TextBox_DefaultSaveLoc.BackColor = saveOK ? SystemColors.Window : RedColor;
			TextBox_RsrcDirectory.BackColor = rsrcOK ? (new DirectoryInfo(rsrcPath).Name == "rsrc" ? SystemColors.Window : YellowColor) : RedColor;

			if (TextBox_RsrcDirectory.BackColor == YellowColor) {
				MainTooltip.SetToolTip(TextBox_RsrcDirectory, "WARNING: rsrc directory isn't named rsrc! Please consider verifying that this is the correct directory. Example:\nC:\\Program Files (x86)\\Steam\\steamapps\\common\\Spiral Knights\\rsrc");
			} else {
				MainTooltip.SetToolTip(TextBox_RsrcDirectory, MainTooltip.GetToolTip(LabelRsrcDir));
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
					throw new IOException();
				}
				return true;
			} catch { }
			return false;
		}
	}
}
