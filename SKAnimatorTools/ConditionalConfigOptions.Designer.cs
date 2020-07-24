namespace SKAnimatorTools {
	partial class ConditionalConfigOptions {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConditionalConfigOptions));
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.BtnExportAll = new System.Windows.Forms.Button();
			this.BtnOnlyEnabled = new System.Windows.Forms.Button();
			this.BtnOnlyDisabled = new System.Windows.Forms.Button();
			this.BtnOnlyDefault = new System.Windows.Forms.Button();
			this.BtnCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// richTextBox1
			// 
			this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox1.Location = new System.Drawing.Point(13, 13);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.ReadOnly = true;
			this.richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
			this.richTextBox1.Size = new System.Drawing.Size(294, 127);
			this.richTextBox1.TabIndex = 0;
			this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
			// 
			// BtnExportAll
			// 
			this.BtnExportAll.Location = new System.Drawing.Point(314, 13);
			this.BtnExportAll.Name = "BtnExportAll";
			this.BtnExportAll.Size = new System.Drawing.Size(155, 23);
			this.BtnExportAll.TabIndex = 1;
			this.BtnExportAll.Text = "Export Everything";
			this.BtnExportAll.UseVisualStyleBackColor = true;
			this.BtnExportAll.Click += new System.EventHandler(this.BtnExportAll_Click);
			// 
			// BtnOnlyEnabled
			// 
			this.BtnOnlyEnabled.Location = new System.Drawing.Point(314, 71);
			this.BtnOnlyEnabled.Name = "BtnOnlyEnabled";
			this.BtnOnlyEnabled.Size = new System.Drawing.Size(155, 23);
			this.BtnOnlyEnabled.TabIndex = 3;
			this.BtnOnlyEnabled.Text = "Only Export Enabled Models";
			this.BtnOnlyEnabled.UseVisualStyleBackColor = true;
			this.BtnOnlyEnabled.Click += new System.EventHandler(this.BtnOnlyEnabled_Click);
			// 
			// BtnOnlyDisabled
			// 
			this.BtnOnlyDisabled.Location = new System.Drawing.Point(314, 100);
			this.BtnOnlyDisabled.Name = "BtnOnlyDisabled";
			this.BtnOnlyDisabled.Size = new System.Drawing.Size(155, 23);
			this.BtnOnlyDisabled.TabIndex = 4;
			this.BtnOnlyDisabled.Text = "Only Export Disabled Models";
			this.BtnOnlyDisabled.UseVisualStyleBackColor = true;
			this.BtnOnlyDisabled.Click += new System.EventHandler(this.BtnOnlyDisabled_Click);
			// 
			// BtnOnlyDefault
			// 
			this.BtnOnlyDefault.Location = new System.Drawing.Point(314, 42);
			this.BtnOnlyDefault.Name = "BtnOnlyDefault";
			this.BtnOnlyDefault.Size = new System.Drawing.Size(155, 23);
			this.BtnOnlyDefault.TabIndex = 2;
			this.BtnOnlyDefault.Text = "Only Export Default Model";
			this.BtnOnlyDefault.UseVisualStyleBackColor = true;
			this.BtnOnlyDefault.Click += new System.EventHandler(this.BtnOnlyDefault_Click);
			// 
			// BtnCancel
			// 
			this.BtnCancel.Location = new System.Drawing.Point(314, 129);
			this.BtnCancel.Name = "BtnCancel";
			this.BtnCancel.Size = new System.Drawing.Size(155, 23);
			this.BtnCancel.TabIndex = 5;
			this.BtnCancel.Text = "Cancel";
			this.BtnCancel.UseVisualStyleBackColor = true;
			this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// ConditionalConfigOptions
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(481, 161);
			this.ControlBox = false;
			this.Controls.Add(this.BtnCancel);
			this.Controls.Add(this.BtnOnlyDefault);
			this.Controls.Add(this.BtnOnlyDisabled);
			this.Controls.Add(this.BtnOnlyEnabled);
			this.Controls.Add(this.BtnExportAll);
			this.Controls.Add(this.richTextBox1);
			this.Name = "ConditionalConfigOptions";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "How should we handle conditional models?";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RichTextBox richTextBox1;
		private System.Windows.Forms.Button BtnExportAll;
		private System.Windows.Forms.Button BtnOnlyEnabled;
		private System.Windows.Forms.Button BtnOnlyDisabled;
		private System.Windows.Forms.Button BtnOnlyDefault;
		private System.Windows.Forms.Button BtnCancel;
	}
}