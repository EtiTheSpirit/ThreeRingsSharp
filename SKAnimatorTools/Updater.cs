using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace SKAnimatorTools {
	public partial class Updater : Form {

		public string LatestVersion { get; set; }

		public Updater(string ver) {
			LatestVersion = ver;
			InitializeComponent();
		}

		private void BtnCancel_Click(object sender, EventArgs e) {
			Close();
		}

		private void BtnDownload_Click(object sender, EventArgs e) {
			if (File.Exists(@".\TRSUpdaterV2.exe")) {
				Process.Start(@".\TRSUpdaterV2.exe");
			} else {
				string dlLink = "https://github.com/EtiTheSpirit/ThreeRingsSharp/releases/download/{0}/ThreeRingsSharp.zip";
				Process.Start(string.Format(dlLink, LatestVersion));
			}
			Close();
			Environment.Exit(0);
		}

		private void BtnOpenPage_Click(object sender, EventArgs e) {
			string releaseLink = "https://github.com/EtiTheSpirit/ThreeRingsSharp/releases/{0}";
			Process.Start(string.Format(releaseLink, LatestVersion));
		}
	}
}
