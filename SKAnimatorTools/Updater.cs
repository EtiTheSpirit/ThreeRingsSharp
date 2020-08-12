using System;
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
			string dlLink = "https://github.com/XanTheDragon/ThreeRingsSharp/releases/download/{0}/ThreeRingsSharp.zip";
			System.Diagnostics.Process.Start(string.Format(dlLink, LatestVersion));
			Close();
			Environment.Exit(0);
		}

		private void BtnOpenPage_Click(object sender, EventArgs e) {
			string releaseLink = "https://github.com/XanTheDragon/ThreeRingsSharp/releases/{0}";
			System.Diagnostics.Process.Start(string.Format(releaseLink, LatestVersion));
		}
	}
}
