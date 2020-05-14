namespace SKAnimatorTools {
	partial class ConfigurationForm {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigurationForm));
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.LabelDefaultPath = new System.Windows.Forms.Label();
			this.LabelSaveLocation = new System.Windows.Forms.Label();
			this.TextBox_DefaultLoadLoc = new System.Windows.Forms.TextBox();
			this.TextBox_DefaultSaveLoc = new System.Windows.Forms.TextBox();
			this.LabelRsrcDir = new System.Windows.Forms.Label();
			this.CheckBox_RememberLastLoad = new System.Windows.Forms.CheckBox();
			this.TextBox_RsrcDirectory = new System.Windows.Forms.TextBox();
			this.MainTooltip = new System.Windows.Forms.ToolTip(this.components);
			this.BtnCancel = new System.Windows.Forms.Button();
			this.BtnSave = new System.Windows.Forms.Button();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26.93157F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 73.06843F));
			this.tableLayoutPanel1.Controls.Add(this.LabelDefaultPath, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.LabelSaveLocation, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.TextBox_DefaultLoadLoc, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.TextBox_DefaultSaveLoc, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.LabelRsrcDir, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.CheckBox_RememberLastLoad, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.TextBox_RsrcDirectory, 1, 2);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(13, 13);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 4;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(453, 118);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// LabelDefaultPath
			// 
			this.LabelDefaultPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.LabelDefaultPath.AutoSize = true;
			this.LabelDefaultPath.Location = new System.Drawing.Point(3, 0);
			this.LabelDefaultPath.Name = "LabelDefaultPath";
			this.LabelDefaultPath.Size = new System.Drawing.Size(112, 29);
			this.LabelDefaultPath.TabIndex = 0;
			this.LabelDefaultPath.Text = "Default Load Location";
			this.LabelDefaultPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.MainTooltip.SetToolTip(this.LabelDefaultPath, "When loading .DAT files, the program will start in this directory.");
			// 
			// LabelSaveLocation
			// 
			this.LabelSaveLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.LabelSaveLocation.AutoSize = true;
			this.LabelSaveLocation.Location = new System.Drawing.Point(3, 29);
			this.LabelSaveLocation.Name = "LabelSaveLocation";
			this.LabelSaveLocation.Size = new System.Drawing.Size(113, 29);
			this.LabelSaveLocation.TabIndex = 1;
			this.LabelSaveLocation.Text = "Default Save Location";
			this.LabelSaveLocation.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.MainTooltip.SetToolTip(this.LabelSaveLocation, "When exporting files, the program will go here by default.");
			// 
			// TextBox_DefaultLoadLoc
			// 
			this.TextBox_DefaultLoadLoc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.TextBox_DefaultLoadLoc.Location = new System.Drawing.Point(125, 3);
			this.TextBox_DefaultLoadLoc.Name = "TextBox_DefaultLoadLoc";
			this.TextBox_DefaultLoadLoc.Size = new System.Drawing.Size(325, 20);
			this.TextBox_DefaultLoadLoc.TabIndex = 1;
			this.MainTooltip.SetToolTip(this.TextBox_DefaultLoadLoc, "When loading .DAT files, the program will start in this directory.");
			this.TextBox_DefaultLoadLoc.WordWrap = false;
			this.TextBox_DefaultLoadLoc.Validating += new System.ComponentModel.CancelEventHandler(this.VerifyLoadLocationIntegrity);
			// 
			// TextBox_DefaultSaveLoc
			// 
			this.TextBox_DefaultSaveLoc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.TextBox_DefaultSaveLoc.Location = new System.Drawing.Point(125, 32);
			this.TextBox_DefaultSaveLoc.Name = "TextBox_DefaultSaveLoc";
			this.TextBox_DefaultSaveLoc.Size = new System.Drawing.Size(325, 20);
			this.TextBox_DefaultSaveLoc.TabIndex = 2;
			this.MainTooltip.SetToolTip(this.TextBox_DefaultSaveLoc, "When exporting files, the program will go here by default.");
			this.TextBox_DefaultSaveLoc.WordWrap = false;
			this.TextBox_DefaultSaveLoc.Validating += new System.ComponentModel.CancelEventHandler(this.VerifySaveLocationIntegrity);
			// 
			// LabelRsrcDir
			// 
			this.LabelRsrcDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.LabelRsrcDir.AutoSize = true;
			this.LabelRsrcDir.Location = new System.Drawing.Point(3, 58);
			this.LabelRsrcDir.Name = "LabelRsrcDir";
			this.LabelRsrcDir.Size = new System.Drawing.Size(69, 29);
			this.LabelRsrcDir.TabIndex = 4;
			this.LabelRsrcDir.Text = "rsrc Directory";
			this.LabelRsrcDir.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.MainTooltip.SetToolTip(this.LabelRsrcDir, "This should point to the rsrc directory in the Spiral Knights folder. If set inco" +
        "rrectly, CompoundConfigs and other reference-based assets will fail to convert.");
			// 
			// CheckBox_RememberLastLoad
			// 
			this.CheckBox_RememberLastLoad.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CheckBox_RememberLastLoad.AutoSize = true;
			this.CheckBox_RememberLastLoad.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.CheckBox_RememberLastLoad.Location = new System.Drawing.Point(220, 90);
			this.CheckBox_RememberLastLoad.Name = "CheckBox_RememberLastLoad";
			this.CheckBox_RememberLastLoad.Size = new System.Drawing.Size(230, 25);
			this.CheckBox_RememberLastLoad.TabIndex = 3;
			this.CheckBox_RememberLastLoad.Text = "Remember Last Load Directory (In Session)";
			this.MainTooltip.SetToolTip(this.CheckBox_RememberLastLoad, "If enabled, opening multiple .DAT files separately will not cause you go to back " +
        "to /rsrc/ every time.");
			this.CheckBox_RememberLastLoad.UseVisualStyleBackColor = true;
			// 
			// TextBox_RsrcDirectory
			// 
			this.TextBox_RsrcDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.TextBox_RsrcDirectory.Location = new System.Drawing.Point(125, 61);
			this.TextBox_RsrcDirectory.Name = "TextBox_RsrcDirectory";
			this.TextBox_RsrcDirectory.Size = new System.Drawing.Size(325, 20);
			this.TextBox_RsrcDirectory.TabIndex = 5;
			this.MainTooltip.SetToolTip(this.TextBox_RsrcDirectory, "This should point to the rsrc directory in the Spiral Knights folder. If set inco" +
        "rrectly, CompoundConfigs and other reference-based assets will fail to convert.");
			this.TextBox_RsrcDirectory.WordWrap = false;
			this.TextBox_RsrcDirectory.Validating += new System.ComponentModel.CancelEventHandler(this.VerifyRsrcDirectoryIntegrity);
			// 
			// BtnCancel
			// 
			this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.BtnCancel.Location = new System.Drawing.Point(12, 137);
			this.BtnCancel.Name = "BtnCancel";
			this.BtnCancel.Size = new System.Drawing.Size(75, 23);
			this.BtnCancel.TabIndex = 5;
			this.BtnCancel.Text = "Cancel";
			this.MainTooltip.SetToolTip(this.BtnCancel, "Undo all changes and close this form.");
			this.BtnCancel.UseVisualStyleBackColor = true;
			this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// BtnSave
			// 
			this.BtnSave.Enabled = false;
			this.BtnSave.Location = new System.Drawing.Point(363, 137);
			this.BtnSave.Name = "BtnSave";
			this.BtnSave.Size = new System.Drawing.Size(103, 23);
			this.BtnSave.TabIndex = 6;
			this.BtnSave.Text = "Save and Close";
			this.MainTooltip.SetToolTip(this.BtnSave, "Save your current values and close this form.");
			this.BtnSave.UseVisualStyleBackColor = true;
			this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
			// 
			// ConfigurationForm
			// 
			this.AcceptButton = this.BtnSave;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.BtnCancel;
			this.ClientSize = new System.Drawing.Size(478, 172);
			this.ControlBox = false;
			this.Controls.Add(this.BtnSave);
			this.Controls.Add(this.BtnCancel);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(494, 173);
			this.Name = "ConfigurationForm";
			this.Text = "ThreeRingsSharp Configuration";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label LabelDefaultPath;
		private System.Windows.Forms.Label LabelSaveLocation;
		private System.Windows.Forms.ToolTip MainTooltip;
		private System.Windows.Forms.CheckBox CheckBox_RememberLastLoad;
		private System.Windows.Forms.TextBox TextBox_DefaultLoadLoc;
		private System.Windows.Forms.TextBox TextBox_DefaultSaveLoc;
		private System.Windows.Forms.Button BtnCancel;
		private System.Windows.Forms.Button BtnSave;
		private System.Windows.Forms.Label LabelRsrcDir;
		private System.Windows.Forms.TextBox TextBox_RsrcDirectory;
	}
}