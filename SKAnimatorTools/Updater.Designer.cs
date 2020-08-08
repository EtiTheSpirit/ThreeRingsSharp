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
			this.BtnCancel = new System.Windows.Forms.Button();
			this.BtnDownload = new System.Windows.Forms.Button();
			this.LabelNewDL = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// BtnCancel
			// 
			this.BtnCancel.Location = new System.Drawing.Point(12, 72);
			this.BtnCancel.Name = "BtnCancel";
			this.BtnCancel.Size = new System.Drawing.Size(96, 23);
			this.BtnCancel.TabIndex = 0;
			this.BtnCancel.Text = "Remind Me Later";
			this.BtnCancel.UseVisualStyleBackColor = true;
			this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// BtnDownload
			// 
			this.BtnDownload.Location = new System.Drawing.Point(156, 72);
			this.BtnDownload.Name = "BtnDownload";
			this.BtnDownload.Size = new System.Drawing.Size(96, 23);
			this.BtnDownload.TabIndex = 1;
			this.BtnDownload.Text = "Download";
			this.BtnDownload.UseVisualStyleBackColor = true;
			this.BtnDownload.Click += new System.EventHandler(this.BtnDownload_Click);
			// 
			// LabelNewDL
			// 
			this.LabelNewDL.AutoSize = true;
			this.LabelNewDL.Location = new System.Drawing.Point(12, 13);
			this.LabelNewDL.Name = "LabelNewDL";
			this.LabelNewDL.Size = new System.Drawing.Size(230, 39);
			this.LabelNewDL.TabIndex = 2;
			this.LabelNewDL.Text = "A new version of ThreeRingsSharp is available!\r\nWould you like me to close the pr" +
    "ogram\r\nand open the download link for you?\r\n";
			this.LabelNewDL.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// Updater
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(264, 106);
			this.Controls.Add(this.LabelNewDL);
			this.Controls.Add(this.BtnDownload);
			this.Controls.Add(this.BtnCancel);
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
	}
}