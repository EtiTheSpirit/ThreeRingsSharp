namespace SKAnimatorTools {
	partial class Updater {
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
			this.components = new System.ComponentModel.Container();
			this.BtnCancel = new System.Windows.Forms.Button();
			this.BtnDownload = new System.Windows.Forms.Button();
			this.LabelNewDL = new System.Windows.Forms.Label();
			this.BtnOpenPage = new System.Windows.Forms.Button();
			this.UpdaterTooltip = new SKAnimatorTools.Component.FastToolTip(this.components);
			this.SuspendLayout();
			// 
			// BtnCancel
			// 
			this.BtnCancel.Location = new System.Drawing.Point(12, 72);
			this.BtnCancel.Name = "BtnCancel";
			this.BtnCancel.Size = new System.Drawing.Size(96, 23);
			this.BtnCancel.TabIndex = 0;
			this.BtnCancel.Text = "Remind Me Later";
			this.UpdaterTooltip.SetToolTip(this.BtnCancel, "Close this dialog and use the program as it is.");
			this.BtnCancel.UseVisualStyleBackColor = true;
			this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// BtnDownload
			// 
			this.BtnDownload.Location = new System.Drawing.Point(219, 72);
			this.BtnDownload.Name = "BtnDownload";
			this.BtnDownload.Size = new System.Drawing.Size(96, 23);
			this.BtnDownload.TabIndex = 1;
			this.BtnDownload.Text = "Download";
			this.UpdaterTooltip.SetToolTip(this.BtnDownload, "Close the program and automatically open the webpage\r\nto the zip file download fo" +
        "r the new version of TRS.");
			this.BtnDownload.UseVisualStyleBackColor = true;
			this.BtnDownload.Click += new System.EventHandler(this.BtnDownload_Click);
			// 
			// LabelNewDL
			// 
			this.LabelNewDL.AutoSize = true;
			this.LabelNewDL.Location = new System.Drawing.Point(12, 13);
			this.LabelNewDL.Name = "LabelNewDL";
			this.LabelNewDL.Size = new System.Drawing.Size(303, 26);
			this.LabelNewDL.TabIndex = 2;
			this.LabelNewDL.Text = "A new version of ThreeRingsSharp is available! Would you like\r\nme to close the pr" +
    "ogram and open the download link for you?\r\n";
			this.LabelNewDL.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// BtnOpenPage
			// 
			this.BtnOpenPage.Location = new System.Drawing.Point(114, 72);
			this.BtnOpenPage.Name = "BtnOpenPage";
			this.BtnOpenPage.Size = new System.Drawing.Size(99, 23);
			this.BtnOpenPage.TabIndex = 3;
			this.BtnOpenPage.Text = "Show Me";
			this.UpdaterTooltip.SetToolTip(this.BtnOpenPage, "Open the page where the release information is instead of automatically\r\ndownload" +
        "ing the new version of the program. This is so you can see the\r\nnew features.");
			this.BtnOpenPage.UseVisualStyleBackColor = true;
			this.BtnOpenPage.Click += new System.EventHandler(this.BtnOpenPage_Click);
			// 
			// Updater
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(325, 106);
			this.Controls.Add(this.BtnOpenPage);
			this.Controls.Add(this.LabelNewDL);
			this.Controls.Add(this.BtnDownload);
			this.Controls.Add(this.BtnCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Updater";
			this.ShowIcon = false;
			this.Text = "Update Available";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button BtnCancel;
		private System.Windows.Forms.Button BtnDownload;
		private System.Windows.Forms.Label LabelNewDL;
		private SKAnimatorTools.Component.FastToolTip UpdaterTooltip;
		private System.Windows.Forms.Button BtnOpenPage;
	}
}