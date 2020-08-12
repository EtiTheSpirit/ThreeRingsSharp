using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			//string releaseLink = "https://github.com/XanTheDragon/ThreeRingsSharp/releases/{0}";
			//System.Diagnostics.Process.Start(string.Format(releaseLink, LatestVersion));
			System.Diagnostics.Process.Start(string.Format(dlLink, LatestVersion));
			Close();
			Environment.Exit(0);
		}
	}
}
