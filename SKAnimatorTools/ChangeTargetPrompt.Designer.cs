namespace SKAnimatorTools {
	partial class ChangeTargetPrompt {
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
			this.BtnSave = new System.Windows.Forms.Button();
			this.Option_NewTarget = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// BtnCancel
			// 
			this.BtnCancel.Location = new System.Drawing.Point(13, 61);
			this.BtnCancel.Name = "BtnCancel";
			this.BtnCancel.Size = new System.Drawing.Size(75, 23);
			this.BtnCancel.TabIndex = 0;
			this.BtnCancel.Text = "Cancel";
			this.BtnCancel.UseVisualStyleBackColor = true;
			this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// BtnSave
			// 
			this.BtnSave.Enabled = false;
			this.BtnSave.Location = new System.Drawing.Point(178, 61);
			this.BtnSave.Name = "BtnSave";
			this.BtnSave.Size = new System.Drawing.Size(75, 23);
			this.BtnSave.TabIndex = 1;
			this.BtnSave.Text = "Apply";
			this.BtnSave.UseVisualStyleBackColor = true;
			this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
			// 
			// Option_NewTarget
			// 
			this.Option_NewTarget.ForeColor = System.Drawing.Color.Black;
			this.Option_NewTarget.FormattingEnabled = true;
			this.Option_NewTarget.Location = new System.Drawing.Point(13, 12);
			this.Option_NewTarget.Name = "Option_NewTarget";
			this.Option_NewTarget.Size = new System.Drawing.Size(240, 21);
			this.Option_NewTarget.TabIndex = 2;
			this.Option_NewTarget.Text = "StaticSetConfig Entry";
			this.Option_NewTarget.SelectedIndexChanged += new System.EventHandler(this.Option_NewTarget_SelectedIndexChanged);
			// 
			// ChangeTargetPrompt
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(265, 96);
			this.Controls.Add(this.Option_NewTarget);
			this.Controls.Add(this.BtnSave);
			this.Controls.Add(this.BtnCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximumSize = new System.Drawing.Size(281, 135);
			this.MinimumSize = new System.Drawing.Size(281, 135);
			this.Name = "ChangeTargetPrompt";
			this.Text = "Pick New Target";
			this.TopMost = true;
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button BtnCancel;
		private System.Windows.Forms.Button BtnSave;
		private System.Windows.Forms.ComboBox Option_NewTarget;
	}
}