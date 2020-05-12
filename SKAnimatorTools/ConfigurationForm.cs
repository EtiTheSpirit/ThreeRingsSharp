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

		public ConfigurationForm() {
			InitializeComponent();
			MainTooltip.ShowAlways = true;
			MainTooltip.AutoPopDelay = short.MaxValue;
			MainTooltip.ShowAlways = true;
			MainTooltip.InitialDelay = 200;
			MainTooltip.ReshowDelay = 200;
			IsOK = true;
		}

		public void SetDataFromConfig(string defaultLoad, string defaultSave, bool rememberLoad) {
			TextBox_DefaultLoadLoc.Text = defaultLoad;
			TextBox_DefaultSaveLoc.Text = defaultSave;
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
			Close();
		}

		private void VerifyLoadLocationIntegrity(object sender, CancelEventArgs e) => VerifyAllPathIntegrity();

		private void VerifySaveLocationIntegrity(object sender, CancelEventArgs e) => VerifyAllPathIntegrity();

		/// <summary>
		/// Verifies the integrity of both the load and save path.
		/// </summary>
		private void VerifyAllPathIntegrity() {
			string loadPath = TextBox_DefaultLoadLoc.Text;
			string savePath = TextBox_DefaultSaveLoc.Text;
			bool loadOK = IsPathOK(loadPath);
			bool saveOK = IsPathOK(savePath);
			IsOK = loadOK && saveOK;

			TextBox_DefaultLoadLoc.BackColor = loadOK ? SystemColors.Window : RedColor;
			TextBox_DefaultSaveLoc.BackColor = saveOK ? SystemColors.Window : RedColor;

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
