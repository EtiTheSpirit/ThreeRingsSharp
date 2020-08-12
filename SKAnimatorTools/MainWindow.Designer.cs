namespace SKAnimatorTools {
	partial class MainWindow {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
			this.SaveModel = new System.Windows.Forms.SaveFileDialog();
			this.OpenModel = new System.Windows.Forms.OpenFileDialog();
			this.ModelStructureTree = new System.Windows.Forms.TreeView();
			this.SilkImages = new System.Windows.Forms.ImageList(this.components);
			this.GroupBoxModelInfo = new System.Windows.Forms.GroupBox();
			this.GroupBoxProperties = new System.Windows.Forms.GroupBox();
			this.SelectedObjectProperties = new System.Windows.Forms.TreeView();
			this.GroupBoxModelTree = new System.Windows.Forms.GroupBox();
			this.GroupBoxCoreModelInfo = new System.Windows.Forms.GroupBox();
			this.ModelCoreDataTable = new System.Windows.Forms.TableLayoutPanel();
			this.LabelFileName_Left = new System.Windows.Forms.Label();
			this.LabelFormatVersion_Left = new System.Windows.Forms.Label();
			this.LabelModelCompressed_Left = new System.Windows.Forms.Label();
			this.LabelModelCompressed = new System.Windows.Forms.Label();
			this.LabelType_Left = new System.Windows.Forms.Label();
			this.LabelType = new System.Windows.Forms.Label();
			this.LabelFormatVersion = new System.Windows.Forms.Label();
			this.LabelFileName = new System.Windows.Forms.Label();
			this.BtnSaveModel = new System.Windows.Forms.Button();
			this.BtnOpenModel = new System.Windows.Forms.Button();
			this.GroupBoxProgramInfo = new System.Windows.Forms.GroupBox();
			this.ProgramLog = new System.Windows.Forms.RichTextBox();
			this.BtnConfig = new System.Windows.Forms.Button();
			this.ProgramTooltip = new System.Windows.Forms.ToolTip(this.components);
			this.ModelLoaderBGWorker = new System.ComponentModel.BackgroundWorker();
			this.ModelLoadProgress = new SKAnimatorTools.Component.ColoredProgressBar();
			this.GroupBoxModelInfo.SuspendLayout();
			this.GroupBoxProperties.SuspendLayout();
			this.GroupBoxModelTree.SuspendLayout();
			this.GroupBoxCoreModelInfo.SuspendLayout();
			this.ModelCoreDataTable.SuspendLayout();
			this.GroupBoxProgramInfo.SuspendLayout();
			this.SuspendLayout();
			// 
			// SaveModel
			// 
			this.SaveModel.Filter = "gLTF 2.0 Binary|*.glb";
			this.SaveModel.RestoreDirectory = true;
			this.SaveModel.SupportMultiDottedExtensions = true;
			this.SaveModel.Title = "Save your Model";
			this.SaveModel.FileOk += new System.ComponentModel.CancelEventHandler(this.SaveModel_PromptFileExport);
			// 
			// OpenModel
			// 
			this.OpenModel.FileName = "model.dat";
			this.OpenModel.Filter = "Clyde Files|*.dat;*.xml|All Files|*";
			this.OpenModel.Title = "Open a Model";
			this.OpenModel.FileOk += new System.ComponentModel.CancelEventHandler(this.OnFileSelectedOpenModel);
			// 
			// ModelStructureTree
			// 
			this.ModelStructureTree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ModelStructureTree.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ModelStructureTree.HideSelection = false;
			this.ModelStructureTree.ImageIndex = 1;
			this.ModelStructureTree.ImageList = this.SilkImages;
			this.ModelStructureTree.Location = new System.Drawing.Point(3, 16);
			this.ModelStructureTree.Name = "ModelStructureTree";
			this.ModelStructureTree.SelectedImageKey = "Object";
			this.ModelStructureTree.Size = new System.Drawing.Size(395, 198);
			this.ModelStructureTree.TabIndex = 4;
			this.ModelStructureTree.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.OnNodeClicked);
			// 
			// SilkImages
			// 
			this.SilkImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("SilkImages.ImageStream")));
			this.SilkImages.TransparentColor = System.Drawing.Color.Transparent;
			this.SilkImages.Images.SetKeyName(0, "Generic");
			this.SilkImages.Images.SetKeyName(1, "Object");
			this.SilkImages.Images.SetKeyName(2, "Scene");
			this.SilkImages.Images.SetKeyName(3, "Sky");
			this.SilkImages.Images.SetKeyName(4, "Model");
			this.SilkImages.Images.SetKeyName(5, "ModelSet");
			this.SilkImages.Images.SetKeyName(6, "Articulated");
			this.SilkImages.Images.SetKeyName(7, "Billboard");
			this.SilkImages.Images.SetKeyName(8, "Static");
			this.SilkImages.Images.SetKeyName(9, "MergedStatic");
			this.SilkImages.Images.SetKeyName(10, "Sound");
			this.SilkImages.Images.SetKeyName(11, "Attachment");
			this.SilkImages.Images.SetKeyName(12, "Derived");
			this.SilkImages.Images.SetKeyName(13, "Conditional");
			this.SilkImages.Images.SetKeyName(14, "CameraShake");
			this.SilkImages.Images.SetKeyName(15, "Generated");
			this.SilkImages.Images.SetKeyName(16, "Schemed");
			this.SilkImages.Images.SetKeyName(17, "SchemedModel");
			this.SilkImages.Images.SetKeyName(18, "Scheme");
			this.SilkImages.Images.SetKeyName(19, "Animation");
			this.SilkImages.Images.SetKeyName(20, "Scripted");
			this.SilkImages.Images.SetKeyName(21, "TimedAction");
			this.SilkImages.Images.SetKeyName(22, "Config");
			this.SilkImages.Images.SetKeyName(23, "Shading");
			this.SilkImages.Images.SetKeyName(24, "Reference");
			this.SilkImages.Images.SetKeyName(25, "Array");
			this.SilkImages.Images.SetKeyName(26, "Texture");
			this.SilkImages.Images.SetKeyName(27, "Variant");
			this.SilkImages.Images.SetKeyName(28, "Light");
			this.SilkImages.Images.SetKeyName(29, "Value");
			this.SilkImages.Images.SetKeyName(30, "Triangle");
			this.SilkImages.Images.SetKeyName(31, "Tile");
			this.SilkImages.Images.SetKeyName(32, "Matrix");
			this.SilkImages.Images.SetKeyName(33, "Missing");
			this.SilkImages.Images.SetKeyName(34, "Tag");
			this.SilkImages.Images.SetKeyName(35, "None");
			// 
			// GroupBoxModelInfo
			// 
			this.GroupBoxModelInfo.Controls.Add(this.GroupBoxProperties);
			this.GroupBoxModelInfo.Controls.Add(this.GroupBoxModelTree);
			this.GroupBoxModelInfo.Controls.Add(this.GroupBoxCoreModelInfo);
			this.GroupBoxModelInfo.ForeColor = System.Drawing.SystemColors.ControlText;
			this.GroupBoxModelInfo.Location = new System.Drawing.Point(363, 12);
			this.GroupBoxModelInfo.Name = "GroupBoxModelInfo";
			this.GroupBoxModelInfo.Size = new System.Drawing.Size(413, 545);
			this.GroupBoxModelInfo.TabIndex = 1;
			this.GroupBoxModelInfo.TabStop = false;
			this.GroupBoxModelInfo.Text = "Model Information";
			// 
			// GroupBoxProperties
			// 
			this.GroupBoxProperties.Controls.Add(this.SelectedObjectProperties);
			this.GroupBoxProperties.ForeColor = System.Drawing.SystemColors.ControlText;
			this.GroupBoxProperties.Location = new System.Drawing.Point(6, 341);
			this.GroupBoxProperties.Name = "GroupBoxProperties";
			this.GroupBoxProperties.Size = new System.Drawing.Size(401, 198);
			this.GroupBoxProperties.TabIndex = 7;
			this.GroupBoxProperties.TabStop = false;
			this.GroupBoxProperties.Text = "Selected Object\'s Properties [Click Blue Entries To Edit]";
			// 
			// SelectedObjectProperties
			// 
			this.SelectedObjectProperties.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SelectedObjectProperties.ForeColor = System.Drawing.SystemColors.ControlText;
			this.SelectedObjectProperties.HideSelection = false;
			this.SelectedObjectProperties.ImageIndex = 0;
			this.SelectedObjectProperties.ImageList = this.SilkImages;
			this.SelectedObjectProperties.Location = new System.Drawing.Point(3, 16);
			this.SelectedObjectProperties.Name = "SelectedObjectProperties";
			this.SelectedObjectProperties.SelectedImageIndex = 0;
			this.SelectedObjectProperties.Size = new System.Drawing.Size(395, 179);
			this.SelectedObjectProperties.TabIndex = 4;
			this.SelectedObjectProperties.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.SelectedObjectProperties_NodeMouseClick);
			// 
			// GroupBoxModelTree
			// 
			this.GroupBoxModelTree.Controls.Add(this.ModelStructureTree);
			this.GroupBoxModelTree.ForeColor = System.Drawing.SystemColors.ControlText;
			this.GroupBoxModelTree.Location = new System.Drawing.Point(6, 118);
			this.GroupBoxModelTree.Name = "GroupBoxModelTree";
			this.GroupBoxModelTree.Size = new System.Drawing.Size(401, 217);
			this.GroupBoxModelTree.TabIndex = 6;
			this.GroupBoxModelTree.TabStop = false;
			this.GroupBoxModelTree.Text = "Hierarchy";
			// 
			// GroupBoxCoreModelInfo
			// 
			this.GroupBoxCoreModelInfo.Controls.Add(this.ModelCoreDataTable);
			this.GroupBoxCoreModelInfo.ForeColor = System.Drawing.SystemColors.ControlText;
			this.GroupBoxCoreModelInfo.Location = new System.Drawing.Point(6, 20);
			this.GroupBoxCoreModelInfo.Name = "GroupBoxCoreModelInfo";
			this.GroupBoxCoreModelInfo.Size = new System.Drawing.Size(401, 98);
			this.GroupBoxCoreModelInfo.TabIndex = 5;
			this.GroupBoxCoreModelInfo.TabStop = false;
			this.GroupBoxCoreModelInfo.Text = "Base Import Core Data";
			// 
			// ModelCoreDataTable
			// 
			this.ModelCoreDataTable.ColumnCount = 2;
			this.ModelCoreDataTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.ModelCoreDataTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.ModelCoreDataTable.Controls.Add(this.LabelFileName_Left, 0, 0);
			this.ModelCoreDataTable.Controls.Add(this.LabelFormatVersion_Left, 0, 1);
			this.ModelCoreDataTable.Controls.Add(this.LabelModelCompressed_Left, 0, 2);
			this.ModelCoreDataTable.Controls.Add(this.LabelModelCompressed, 1, 2);
			this.ModelCoreDataTable.Controls.Add(this.LabelType_Left, 0, 3);
			this.ModelCoreDataTable.Controls.Add(this.LabelType, 1, 3);
			this.ModelCoreDataTable.Controls.Add(this.LabelFormatVersion, 1, 1);
			this.ModelCoreDataTable.Controls.Add(this.LabelFileName, 1, 0);
			this.ModelCoreDataTable.Location = new System.Drawing.Point(6, 17);
			this.ModelCoreDataTable.Name = "ModelCoreDataTable";
			this.ModelCoreDataTable.RowCount = 4;
			this.ModelCoreDataTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.ModelCoreDataTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.ModelCoreDataTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.ModelCoreDataTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.ModelCoreDataTable.Size = new System.Drawing.Size(389, 75);
			this.ModelCoreDataTable.TabIndex = 1;
			// 
			// LabelFileName_Left
			// 
			this.LabelFileName_Left.AutoSize = true;
			this.LabelFileName_Left.Location = new System.Drawing.Point(3, 0);
			this.LabelFileName_Left.Name = "LabelFileName_Left";
			this.LabelFileName_Left.Size = new System.Drawing.Size(57, 13);
			this.LabelFileName_Left.TabIndex = 0;
			this.LabelFileName_Left.Text = "File Name:";
			this.LabelFileName_Left.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// LabelFormatVersion_Left
			// 
			this.LabelFormatVersion_Left.AutoSize = true;
			this.LabelFormatVersion_Left.Location = new System.Drawing.Point(3, 20);
			this.LabelFormatVersion_Left.Name = "LabelFormatVersion_Left";
			this.LabelFormatVersion_Left.Size = new System.Drawing.Size(80, 13);
			this.LabelFormatVersion_Left.TabIndex = 2;
			this.LabelFormatVersion_Left.Text = "Format Version:";
			this.LabelFormatVersion_Left.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// LabelModelCompressed_Left
			// 
			this.LabelModelCompressed_Left.AutoSize = true;
			this.LabelModelCompressed_Left.Location = new System.Drawing.Point(3, 40);
			this.LabelModelCompressed_Left.Name = "LabelModelCompressed_Left";
			this.LabelModelCompressed_Left.Size = new System.Drawing.Size(68, 13);
			this.LabelModelCompressed_Left.TabIndex = 4;
			this.LabelModelCompressed_Left.Text = "Compressed:";
			this.LabelModelCompressed_Left.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// LabelModelCompressed
			// 
			this.LabelModelCompressed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.LabelModelCompressed.AutoSize = true;
			this.LabelModelCompressed.Location = new System.Drawing.Point(359, 40);
			this.LabelModelCompressed.Name = "LabelModelCompressed";
			this.LabelModelCompressed.Size = new System.Drawing.Size(27, 13);
			this.LabelModelCompressed.TabIndex = 5;
			this.LabelModelCompressed.Text = "N/A";
			this.LabelModelCompressed.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// LabelType_Left
			// 
			this.LabelType_Left.AutoSize = true;
			this.LabelType_Left.Location = new System.Drawing.Point(3, 60);
			this.LabelType_Left.Name = "LabelType_Left";
			this.LabelType_Left.Size = new System.Drawing.Size(59, 13);
			this.LabelType_Left.TabIndex = 6;
			this.LabelType_Left.Text = "Core Type:";
			this.LabelType_Left.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// LabelType
			// 
			this.LabelType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.LabelType.AutoSize = true;
			this.LabelType.Location = new System.Drawing.Point(359, 60);
			this.LabelType.Name = "LabelType";
			this.LabelType.Size = new System.Drawing.Size(27, 13);
			this.LabelType.TabIndex = 7;
			this.LabelType.Text = "N/A";
			this.LabelType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// LabelFormatVersion
			// 
			this.LabelFormatVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.LabelFormatVersion.AutoSize = true;
			this.LabelFormatVersion.Location = new System.Drawing.Point(359, 20);
			this.LabelFormatVersion.Name = "LabelFormatVersion";
			this.LabelFormatVersion.Size = new System.Drawing.Size(27, 13);
			this.LabelFormatVersion.TabIndex = 3;
			this.LabelFormatVersion.Text = "N/A";
			this.LabelFormatVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// LabelFileName
			// 
			this.LabelFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.LabelFileName.AutoSize = true;
			this.LabelFileName.Location = new System.Drawing.Point(300, 0);
			this.LabelFileName.Name = "LabelFileName";
			this.LabelFileName.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.LabelFileName.Size = new System.Drawing.Size(86, 13);
			this.LabelFileName.TabIndex = 1;
			this.LabelFileName.Text = "Nothing Loaded!";
			this.LabelFileName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// BtnSaveModel
			// 
			this.BtnSaveModel.Enabled = false;
			this.BtnSaveModel.Location = new System.Drawing.Point(149, 534);
			this.BtnSaveModel.Name = "BtnSaveModel";
			this.BtnSaveModel.Size = new System.Drawing.Size(129, 23);
			this.BtnSaveModel.TabIndex = 3;
			this.BtnSaveModel.Text = "&Export";
			this.BtnSaveModel.UseVisualStyleBackColor = true;
			this.BtnSaveModel.Click += new System.EventHandler(this.SaveClicked);
			// 
			// BtnOpenModel
			// 
			this.BtnOpenModel.Location = new System.Drawing.Point(12, 534);
			this.BtnOpenModel.Name = "BtnOpenModel";
			this.BtnOpenModel.Size = new System.Drawing.Size(129, 23);
			this.BtnOpenModel.TabIndex = 2;
			this.BtnOpenModel.Text = "&Open .DAT Model...";
			this.BtnOpenModel.UseVisualStyleBackColor = true;
			this.BtnOpenModel.Click += new System.EventHandler(this.OpenClicked);
			// 
			// GroupBoxProgramInfo
			// 
			this.GroupBoxProgramInfo.Controls.Add(this.ProgramLog);
			this.GroupBoxProgramInfo.ForeColor = System.Drawing.SystemColors.ControlText;
			this.GroupBoxProgramInfo.Location = new System.Drawing.Point(12, 12);
			this.GroupBoxProgramInfo.Name = "GroupBoxProgramInfo";
			this.GroupBoxProgramInfo.Size = new System.Drawing.Size(345, 500);
			this.GroupBoxProgramInfo.TabIndex = 0;
			this.GroupBoxProgramInfo.TabStop = false;
			this.GroupBoxProgramInfo.Text = "Program Information Feed";
			// 
			// ProgramLog
			// 
			this.ProgramLog.BackColor = System.Drawing.SystemColors.Control;
			this.ProgramLog.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProgramLog.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ProgramLog.HideSelection = false;
			this.ProgramLog.Location = new System.Drawing.Point(3, 16);
			this.ProgramLog.Name = "ProgramLog";
			this.ProgramLog.ReadOnly = true;
			this.ProgramLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
			this.ProgramLog.Size = new System.Drawing.Size(339, 481);
			this.ProgramLog.TabIndex = 0;
			this.ProgramLog.Text = "Welcome to ThreeRingsSharp!\nPlease wait while I load up the config references...\n" +
    "";
			// 
			// BtnConfig
			// 
			this.BtnConfig.Location = new System.Drawing.Point(284, 534);
			this.BtnConfig.Name = "BtnConfig";
			this.BtnConfig.Size = new System.Drawing.Size(75, 23);
			this.BtnConfig.TabIndex = 4;
			this.BtnConfig.Text = "&Configure";
			this.BtnConfig.UseVisualStyleBackColor = true;
			this.BtnConfig.Click += new System.EventHandler(this.OnConfigClicked);
			// 
			// ModelLoaderBGWorker
			// 
			this.ModelLoaderBGWorker.WorkerReportsProgress = true;
			this.ModelLoaderBGWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.ModelLoaderBGWorker_DoWork);
			this.ModelLoaderBGWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.ModelLoaderBGWorker_ProgressChanged);
			this.ModelLoaderBGWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.ModelLoaderBGWorker_RunWorkerCompleted);
			// 
			// ModelLoadProgress
			// 
			this.ModelLoadProgress.BackColor = System.Drawing.SystemColors.ControlLight;
			this.ModelLoadProgress.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(192)))), ((int)(((byte)(31)))));
			this.ModelLoadProgress.Location = new System.Drawing.Point(15, 515);
			this.ModelLoadProgress.Maximum = 1;
			this.ModelLoadProgress.Name = "ModelLoadProgress";
			this.ModelLoadProgress.RightToLeftLayout = true;
			this.ModelLoadProgress.Size = new System.Drawing.Size(339, 16);
			this.ModelLoadProgress.Step = 100;
			this.ModelLoadProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.ModelLoadProgress.TabIndex = 5;
			this.ModelLoadProgress.UseSystemBar = false;
			// 
			// MainWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(788, 569);
			this.Controls.Add(this.ModelLoadProgress);
			this.Controls.Add(this.BtnConfig);
			this.Controls.Add(this.GroupBoxProgramInfo);
			this.Controls.Add(this.BtnOpenModel);
			this.Controls.Add(this.BtnSaveModel);
			this.Controls.Add(this.GroupBoxModelInfo);
			this.DoubleBuffered = true;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(804, 608);
			this.MinimumSize = new System.Drawing.Size(804, 608);
			this.Name = "MainWindow";
			this.Text = "Spiral Knights Animator Tools";
			this.Activated += new System.EventHandler(this.OnMainWindowFocused);
			this.GroupBoxModelInfo.ResumeLayout(false);
			this.GroupBoxProperties.ResumeLayout(false);
			this.GroupBoxModelTree.ResumeLayout(false);
			this.GroupBoxCoreModelInfo.ResumeLayout(false);
			this.ModelCoreDataTable.ResumeLayout(false);
			this.ModelCoreDataTable.PerformLayout();
			this.GroupBoxProgramInfo.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		public System.Windows.Forms.SaveFileDialog SaveModel;
		public System.Windows.Forms.OpenFileDialog OpenModel;
		public System.Windows.Forms.TreeView ModelStructureTree;
		public System.Windows.Forms.GroupBox GroupBoxModelInfo;
		public System.Windows.Forms.Button BtnSaveModel;
		public System.Windows.Forms.Button BtnOpenModel;
		public System.Windows.Forms.GroupBox GroupBoxProgramInfo;
		public System.Windows.Forms.GroupBox GroupBoxModelTree;
		public System.Windows.Forms.GroupBox GroupBoxCoreModelInfo;
		public System.Windows.Forms.TableLayoutPanel ModelCoreDataTable;
		public System.Windows.Forms.Label LabelFileName_Left;
		public System.Windows.Forms.Label LabelFileName;
		public System.Windows.Forms.Label LabelFormatVersion_Left;
		public System.Windows.Forms.Label LabelFormatVersion;
		public System.Windows.Forms.Label LabelModelCompressed_Left;
		public System.Windows.Forms.Label LabelModelCompressed;
		public System.Windows.Forms.Label LabelType_Left;
		public System.Windows.Forms.Label LabelType;
		public System.Windows.Forms.ImageList SilkImages;
		public System.Windows.Forms.GroupBox GroupBoxProperties;
		public System.Windows.Forms.TreeView SelectedObjectProperties;
		public System.Windows.Forms.Button BtnConfig;
		public System.Windows.Forms.RichTextBox ProgramLog;
		public System.Windows.Forms.ToolTip ProgramTooltip;
		private System.ComponentModel.BackgroundWorker ModelLoaderBGWorker;
		private SKAnimatorTools.Component.ColoredProgressBar ModelLoadProgress;
	}
}

