using com.sun.tools.javac.main;
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
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.XansData.Structs;

namespace SKAnimatorTools {
	public partial class ConfigurationForm : Form {


		/// <summary>
		/// A map from <see cref="int"/> to <see cref="Axis"/> values based on their index in the dropdown menu.
		/// </summary>
		public static IReadOnlyDictionary<int, Axis> AxisIntMap = new Dictionary<int, Axis>() {
			[0] = Axis.PositiveX,
			[1] = Axis.PositiveY,
			[2] = Axis.PositiveZ,
			[3] = Axis.NegativeX,
			[4] = Axis.NegativeY,
			[5] = Axis.NegativeZ
		};

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

		/// <summary>
		/// An image of a green circle with a white checkmark.
		/// </summary>
		private static readonly Bitmap Accepted = Properties.Resources.accept;

		/// <summary>
		/// An image of a yellow triangle with an exclamation point.
		/// </summary>
		private static readonly Bitmap Warning = Properties.Resources.error;

		/// <summary>
		/// An image of a red circle with an exclamation point.
		/// </summary>
		private static readonly Bitmap Error = Properties.Resources.exclamation;

		public ConfigurationForm() {
			InitializeComponent();
			MainTooltip.ShowAlways = true;
			MainTooltip.AutoPopDelay = short.MaxValue;
			MainTooltip.ShowAlways = true;
			MainTooltip.InitialDelay = 200;
			MainTooltip.ReshowDelay = 200;
			IsOK = true;
		}

		public void SetDataFromConfig(string defaultLoad, string defaultSave, string defaultRsrc, bool rememberLoad, bool scale100, bool protectZeroScale, int cndExportModeIndex, bool embedTextures, bool verboseLogging, int exportStaticSetMode, bool getAllTextures) {
			TextBox_DefaultLoadLoc.Text = defaultLoad;
			TextBox_DefaultSaveLoc.Text = defaultSave;
			TextBox_RsrcDirectory.Text = defaultRsrc;
			CheckBox_RememberLastLoad.Checked = rememberLoad;
			CheckBox_MultiplyScaleByHundred.Checked = scale100;
			CheckBox_ProtectAgainstZeroScale.Checked = protectZeroScale;
			Option_ConditionalExportMode.SelectedIndex = cndExportModeIndex;
			CheckBox_EmbedTextures.Checked = embedTextures;
			CheckBox_VerboseLogging.Checked = verboseLogging;
			Option_StaticSetExportMode.SelectedIndex = exportStaticSetMode;
			CheckBox_TryGettingAllTextures.Checked = getAllTextures;
			VerifyAllPathIntegrity();
		}

		private void BtnCancel_Click(object sender, EventArgs e) {
			Close();
		}

		private void BtnSave_Click(object sender, EventArgs e) {
			ConfigurationInterface.SetConfigurationValue("DefaultLoadDirectory", TextBox_DefaultLoadLoc.Text);
			ConfigurationInterface.SetConfigurationValue("LastSaveDirectory", TextBox_DefaultSaveLoc.Text);
			ConfigurationInterface.SetConfigurationValue("RsrcDirectory", TextBox_RsrcDirectory.Text);
			ConfigurationInterface.SetConfigurationValue("RememberDirectoryAfterOpen", CheckBox_RememberLastLoad.Checked);
			ConfigurationInterface.SetConfigurationValue("ScaleBy100", CheckBox_MultiplyScaleByHundred.Checked);
			ConfigurationInterface.SetConfigurationValue("ProtectAgainstZeroScale", CheckBox_ProtectAgainstZeroScale.Checked);
			ConfigurationInterface.SetConfigurationValue("ConditionalConfigExportMode", Option_ConditionalExportMode.SelectedIndex);
			ConfigurationInterface.SetConfigurationValue("EmbedTextures", CheckBox_EmbedTextures.Checked);
			ConfigurationInterface.SetConfigurationValue("VerboseLogging", CheckBox_VerboseLogging.Checked);
			ConfigurationInterface.SetConfigurationValue("StaticSetExportMode", Option_StaticSetExportMode.SelectedIndex);
			ConfigurationInterface.SetConfigurationValue("GetAllTextures", CheckBox_TryGettingAllTextures.Checked);
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

			/*
			TextBox_DefaultLoadLoc.BackColor = loadOK ? SystemColors.Window : RedColor;
			TextBox_DefaultSaveLoc.BackColor = saveOK ? SystemColors.Window : RedColor;
			TextBox_RsrcDirectory.BackColor = rsrcOK ? (new DirectoryInfo(rsrcPath).Name == "rsrc" ? SystemColors.Window : YellowColor) : RedColor;

			if (TextBox_RsrcDirectory.BackColor == YellowColor) {
				MainTooltip.SetToolTip(TextBox_RsrcDirectory, "WARNING: rsrc directory isn't named rsrc! Please consider verifying that this is the correct directory. Example:\nC:\\Program Files (x86)\\Steam\\steamapps\\common\\Spiral Knights\\rsrc");
			} else {
				MainTooltip.SetToolTip(TextBox_RsrcDirectory, MainTooltip.GetToolTip(LabelRsrcDir));
			}
			*/
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
			if (CheckBox_VerboseLogging.Checked) {
				PicBox_VerboseLogging.Image = Warning;
				MainTooltip.SetToolTip(PicBox_VerboseLogging, "Enabling verbose logging can slow down the program (it has to wait while the text is written to the console).");
			} else {
				PicBox_VerboseLogging.Image = Accepted;
				MainTooltip.SetToolTip(PicBox_VerboseLogging, string.Empty);
			}
		}
	}
}
