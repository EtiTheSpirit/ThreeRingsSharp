namespace AppUpdater {
	partial class AutoUpdater {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoUpdater));
			this.DownloadBar = new System.Windows.Forms.ProgressBar();
			this.LabelDownloading = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// DownloadBar
			// 
			this.DownloadBar.Location = new System.Drawing.Point(12, 29);
			this.DownloadBar.Name = "DownloadBar";
			this.DownloadBar.Size = new System.Drawing.Size(181, 12);
			this.DownloadBar.TabIndex = 0;
			// 
			// LabelDownloading
			// 
			this.LabelDownloading.AutoSize = true;
			this.LabelDownloading.Location = new System.Drawing.Point(13, 13);
			this.LabelDownloading.Name = "LabelDownloading";
			this.LabelDownloading.Size = new System.Drawing.Size(180, 13);
			this.LabelDownloading.TabIndex = 2;
			this.LabelDownloading.Text = "Downloading ThreeRingsSharp.zip...";
			// 
			// AutoUpdater
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(207, 57);
			this.Controls.Add(this.LabelDownloading);
			this.Controls.Add(this.DownloadBar);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "AutoUpdater";
			this.Text = "ThreeRingsSharp Updater";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ProgressBar DownloadBar;
		private System.Windows.Forms.Label LabelDownloading;
	}
}

