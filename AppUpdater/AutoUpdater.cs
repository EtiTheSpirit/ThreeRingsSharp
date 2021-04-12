using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;

namespace AppUpdater {
	public partial class AutoUpdater : Form {

		/// <summary>
		/// Attempts to access the github to acquire the latest version.
		/// </summary>
		/// <param name="version"></param>
		/// <returns></returns>
		public bool TryGetVersion(out (int, int, int, string)? version) {
			try {
				using (WebClient cli = new WebClient()) {
					string v = cli.DownloadString("https://raw.githubusercontent.com/EtiTheSpirit/ThreeRingsSharp/master/version.txt");
					string[] revs = v.Split('.');
					int major = 0;
					int minor = 0;
					int patch = 0;
					if (revs.Length == 3) {
						major = int.Parse(revs[0]);
						minor = int.Parse(revs[1]);
						patch = int.Parse(revs[2]);
						version = (major, minor, patch, v);
						return true;
					}
					version = null;
				}
				return true;
			} catch {
				version = null;
				return false;
			}
		}

		private readonly FileInfo Destination = new FileInfo(@".\ThreeRingsSharp-Update.zip");

		public AutoUpdater() {
			InitializeComponent();

			if (Destination.Exists) Destination.Delete();
			bool gotVersion = TryGetVersion(out (int major, int minor, int patch, string actualVersionText)? versionInfo);
			if (gotVersion && versionInfo.HasValue) {
				string dlLink = $"https://github.com/EtiTheSpirit/ThreeRingsSharp/releases/download/{versionInfo.Value.actualVersionText}/ThreeRingsSharp.zip";
				bool exists = false;

				try {
					WebRequest verifyExistenceRequest = WebRequest.Create(dlLink);
					verifyExistenceRequest.Method = "HEAD";
					using (HttpWebResponse response = (HttpWebResponse)verifyExistenceRequest.GetResponse()) {
						if (response.StatusCode == HttpStatusCode.OK) {
							exists = true;
						}
					}
				} catch { }

				if (exists) {
					using (WebClient client = new WebClient()) {
						client.DownloadFileCompleted += OnDownloadCompleted;
						client.DownloadProgressChanged += OnProgressChanged;
						client.DownloadFileAsync(new Uri(dlLink), Destination.FullName);
					}
				} else {
					MessageBox.Show($"A new version ({versionInfo.Value.actualVersionText}) has been released, however its download could not be found. Try again in a few minutes (chances are, the new version was pushed to GitHub, but the download file hasn't been uploaded yet). If it still fails, consider checking manually.", "New version not available right now", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			} else {
				MessageBox.Show("Failed to download version information! You will need to go to\nhttps://github.com/XanTheDragon/ThreeRingsSharp/releases yourself and download the top-most release.", "Failed to get version information", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void OnProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
			DownloadBar.Value = e.ProgressPercentage;
		}

		private void OnDownloadCompleted(object sender, AsyncCompletedEventArgs e) {
			LabelDownloading.Text = "Extracting Zip...";
			Update();
			try {
				// Silently drop any errors here.
				ZipFile.ExtractToDirectory(Destination.FullName, Destination.Directory.CreateSubdirectory("NewVersion").FullName);
			} catch { } 
			Destination.Delete();
			MessageBox.Show("The new version has been put into a folder called \"NewVersion\". You may overwrite the files in this directory if you wish.", "Download Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
			Close();
		}
	}
}
