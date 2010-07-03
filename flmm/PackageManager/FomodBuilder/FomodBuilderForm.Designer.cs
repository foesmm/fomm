namespace Fomm.PackageManager.FomodBuilder
{
	partial class FomodBuilderForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FomodBuilderForm));
			this.pnlHeader = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.pnlButtons = new System.Windows.Forms.Panel();
			this.butOK = new System.Windows.Forms.Button();
			this.butCancel = new System.Windows.Forms.Button();
			this.vtcFomodData = new Fomm.Controls.VerticalTabControl();
			this.vtpOutput = new Fomm.Controls.VerticalTabPage();
			this.label10 = new System.Windows.Forms.Label();
			this.butSelectPFPFolder = new System.Windows.Forms.Button();
			this.tbxPFPPath = new System.Windows.Forms.TextBox();
			this.cbxPFP = new System.Windows.Forms.CheckBox();
			this.cbxFomod = new System.Windows.Forms.CheckBox();
			this.vtpSources = new Fomm.Controls.VerticalTabPage();
			this.ffsFileStructure = new Fomm.PackageManager.FomodBuilder.FomodFileSelector();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.tbxFomodFileName = new System.Windows.Forms.TextBox();
			this.vtpInfo = new Fomm.Controls.VerticalTabPage();
			this.finInfo = new Fomm.PackageManager.FomodInfo();
			this.vtpScript = new Fomm.Controls.VerticalTabPage();
			this.fseScriptEditor = new Fomm.PackageManager.Controls.FomodScriptEditor();
			this.panel3 = new System.Windows.Forms.Panel();
			this.cbxUseScript = new System.Windows.Forms.CheckBox();
			this.label8 = new System.Windows.Forms.Label();
			this.vtpReadme = new Fomm.Controls.VerticalTabPage();
			this.redReadmeEditor = new Fomm.PackageManager.Controls.ReadmeEditor();
			this.panel2 = new System.Windows.Forms.Panel();
			this.butGenerateReadme = new System.Windows.Forms.Button();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.vtpDownloadLocations = new Fomm.Controls.VerticalTabPage();
			this.sdsDownloadLocations = new Fomm.PackageManager.FomodBuilder.SourceDownloadSelector();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.sspError = new Fomm.Controls.SiteStatusProvider();
			this.sspWarning = new Fomm.Controls.SiteStatusProvider();
			this.fbdPFPPath = new System.Windows.Forms.FolderBrowserDialog();
			this.pnlHeader.SuspendLayout();
			this.pnlButtons.SuspendLayout();
			this.vtcFomodData.SuspendLayout();
			this.vtpOutput.SuspendLayout();
			this.vtpSources.SuspendLayout();
			this.panel1.SuspendLayout();
			this.vtpInfo.SuspendLayout();
			this.vtpScript.SuspendLayout();
			this.panel3.SuspendLayout();
			this.vtpReadme.SuspendLayout();
			this.panel2.SuspendLayout();
			this.vtpDownloadLocations.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.sspError)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sspWarning)).BeginInit();
			this.SuspendLayout();
			// 
			// pnlHeader
			// 
			this.pnlHeader.Controls.Add(this.label1);
			this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlHeader.Location = new System.Drawing.Point(0, 0);
			this.pnlHeader.Name = "pnlHeader";
			this.pnlHeader.Size = new System.Drawing.Size(595, 36);
			this.pnlHeader.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(135, 19);
			this.label1.TabIndex = 0;
			this.label1.Text = "Create a FOMod";
			// 
			// pnlButtons
			// 
			this.pnlButtons.Controls.Add(this.butOK);
			this.pnlButtons.Controls.Add(this.butCancel);
			this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlButtons.Location = new System.Drawing.Point(0, 403);
			this.pnlButtons.Name = "pnlButtons";
			this.pnlButtons.Size = new System.Drawing.Size(595, 39);
			this.pnlButtons.TabIndex = 1;
			// 
			// butOK
			// 
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Location = new System.Drawing.Point(427, 6);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(75, 23);
			this.butOK.TabIndex = 0;
			this.butOK.Text = "OK";
			this.butOK.UseVisualStyleBackColor = true;
			this.butOK.Click += new System.EventHandler(this.butOK_Click);
			// 
			// butCancel
			// 
			this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.butCancel.Location = new System.Drawing.Point(508, 6);
			this.butCancel.Name = "butCancel";
			this.butCancel.Size = new System.Drawing.Size(75, 23);
			this.butCancel.TabIndex = 1;
			this.butCancel.Text = "Cancel";
			this.butCancel.UseVisualStyleBackColor = true;
			// 
			// vtcFomodData
			// 
			this.vtcFomodData.BackColor = System.Drawing.SystemColors.Window;
			this.vtcFomodData.Controls.Add(this.vtpSources);
			this.vtcFomodData.Controls.Add(this.vtpOutput);
			this.vtcFomodData.Controls.Add(this.vtpInfo);
			this.vtcFomodData.Controls.Add(this.vtpScript);
			this.vtcFomodData.Controls.Add(this.vtpReadme);
			this.vtcFomodData.Controls.Add(this.vtpDownloadLocations);
			this.vtcFomodData.Dock = System.Windows.Forms.DockStyle.Fill;
			this.vtcFomodData.Location = new System.Drawing.Point(0, 36);
			this.vtcFomodData.Name = "vtcFomodData";
			this.vtcFomodData.SelectedIndex = 1;
			this.vtcFomodData.SelectedTabPage = this.vtpSources;
			this.vtcFomodData.Size = new System.Drawing.Size(595, 367);
			this.vtcFomodData.TabIndex = 2;
			this.vtcFomodData.Text = "verticalTabControl1";
			this.vtcFomodData.SelectedTabPageChanged += new System.EventHandler<Fomm.Controls.VerticalTabControl.TabPageEventArgs>(this.vtcFomodData_SelectedTabPageChanged);
			// 
			// vtpOutput
			// 
			this.vtpOutput.BackColor = System.Drawing.SystemColors.Control;
			this.vtpOutput.Controls.Add(this.label10);
			this.vtpOutput.Controls.Add(this.butSelectPFPFolder);
			this.vtpOutput.Controls.Add(this.tbxPFPPath);
			this.vtpOutput.Controls.Add(this.cbxPFP);
			this.vtpOutput.Controls.Add(this.cbxFomod);
			this.vtpOutput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.vtpOutput.Location = new System.Drawing.Point(150, 0);
			this.vtpOutput.Name = "vtpOutput";
			this.vtpOutput.PageIndex = 5;
			this.vtpOutput.Size = new System.Drawing.Size(445, 367);
			this.vtpOutput.TabIndex = 5;
			this.vtpOutput.Text = "Save Locations";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(3, 3);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(191, 13);
			this.label10.TabIndex = 4;
			this.label10.Text = "Select the out you would like to create.";
			// 
			// butSelectPFPFolder
			// 
			this.butSelectPFPFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butSelectPFPFolder.AutoSize = true;
			this.butSelectPFPFolder.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.butSelectPFPFolder.Location = new System.Drawing.Point(407, 84);
			this.butSelectPFPFolder.Name = "butSelectPFPFolder";
			this.butSelectPFPFolder.Size = new System.Drawing.Size(26, 23);
			this.butSelectPFPFolder.TabIndex = 3;
			this.butSelectPFPFolder.Text = "...";
			this.butSelectPFPFolder.UseVisualStyleBackColor = true;
			this.butSelectPFPFolder.Click += new System.EventHandler(this.butSelectPFPFolder_Click);
			// 
			// tbxPFPPath
			// 
			this.tbxPFPPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbxPFPPath.Location = new System.Drawing.Point(34, 86);
			this.tbxPFPPath.Name = "tbxPFPPath";
			this.tbxPFPPath.Size = new System.Drawing.Size(367, 20);
			this.tbxPFPPath.TabIndex = 2;
			this.tbxPFPPath.Validating += new System.ComponentModel.CancelEventHandler(this.tbxPFPPath_Validating);
			// 
			// cbxPFP
			// 
			this.cbxPFP.AutoSize = true;
			this.cbxPFP.Location = new System.Drawing.Point(16, 63);
			this.cbxPFP.Name = "cbxPFP";
			this.cbxPFP.Size = new System.Drawing.Size(172, 17);
			this.cbxPFP.TabIndex = 1;
			this.cbxPFP.Text = "Create Premade FOMod Pack";
			this.cbxPFP.UseVisualStyleBackColor = true;
			// 
			// cbxFomod
			// 
			this.cbxFomod.AutoSize = true;
			this.cbxFomod.Location = new System.Drawing.Point(16, 40);
			this.cbxFomod.Name = "cbxFomod";
			this.cbxFomod.Size = new System.Drawing.Size(99, 17);
			this.cbxFomod.TabIndex = 0;
			this.cbxFomod.Text = "Create FOMod";
			this.cbxFomod.UseVisualStyleBackColor = true;
			// 
			// vtpSources
			// 
			this.vtpSources.BackColor = System.Drawing.SystemColors.Control;
			this.vtpSources.Controls.Add(this.ffsFileStructure);
			this.vtpSources.Controls.Add(this.panel1);
			this.vtpSources.Dock = System.Windows.Forms.DockStyle.Fill;
			this.vtpSources.Location = new System.Drawing.Point(150, 0);
			this.vtpSources.Name = "vtpSources";
			this.vtpSources.PageIndex = 0;
			this.vtpSources.Size = new System.Drawing.Size(445, 367);
			this.vtpSources.TabIndex = 1;
			this.vtpSources.Text = "Sources";
			// 
			// ffsFileStructure
			// 
			this.ffsFileStructure.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ffsFileStructure.Location = new System.Drawing.Point(0, 78);
			this.ffsFileStructure.Name = "ffsFileStructure";
			this.ffsFileStructure.Size = new System.Drawing.Size(445, 289);
			this.ffsFileStructure.TabIndex = 1;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.label3);
			this.panel1.Controls.Add(this.label2);
			this.panel1.Controls.Add(this.tbxFomodFileName);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(445, 78);
			this.panel1.TabIndex = 0;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(3, 3);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(383, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "Enter the name of the FOMod file, and select the files you would like to include." +
				"";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 33);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(190, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "FOMod File Name (without extension):";
			// 
			// tbxFomodFileName
			// 
			this.tbxFomodFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbxFomodFileName.Location = new System.Drawing.Point(6, 49);
			this.tbxFomodFileName.Name = "tbxFomodFileName";
			this.tbxFomodFileName.Size = new System.Drawing.Size(416, 20);
			this.tbxFomodFileName.TabIndex = 0;
			this.tbxFomodFileName.Validating += new System.ComponentModel.CancelEventHandler(this.tbxFomodFileName_Validating);
			// 
			// vtpInfo
			// 
			this.vtpInfo.BackColor = System.Drawing.SystemColors.Control;
			this.vtpInfo.Controls.Add(this.finInfo);
			this.vtpInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.vtpInfo.Location = new System.Drawing.Point(150, 0);
			this.vtpInfo.Name = "vtpInfo";
			this.vtpInfo.PageIndex = 3;
			this.vtpInfo.Size = new System.Drawing.Size(445, 367);
			this.vtpInfo.TabIndex = 4;
			this.vtpInfo.Text = "FOMod Info";
			// 
			// finInfo
			// 
			this.finInfo.Author = "";
			this.finInfo.AutoScroll = true;
			this.finInfo.Description = "";
			this.finInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.finInfo.Email = "";
			this.finInfo.Groups = new string[0];
			this.finInfo.HumanReadableVersion = "";
			this.finInfo.Location = new System.Drawing.Point(0, 0);
			this.finInfo.MachineVersion = ((System.Version)(resources.GetObject("finInfo.MachineVersion")));
			this.finInfo.MinFommVersion = ((System.Version)(resources.GetObject("finInfo.MinFommVersion")));
			this.finInfo.ModName = "";
			this.finInfo.Name = "finInfo";
			this.finInfo.Screenshot = null;
			this.finInfo.Size = new System.Drawing.Size(445, 367);
			this.finInfo.TabIndex = 0;
			this.finInfo.Website = "";
			// 
			// vtpScript
			// 
			this.vtpScript.BackColor = System.Drawing.SystemColors.Control;
			this.vtpScript.Controls.Add(this.fseScriptEditor);
			this.vtpScript.Controls.Add(this.panel3);
			this.vtpScript.Dock = System.Windows.Forms.DockStyle.Fill;
			this.vtpScript.Location = new System.Drawing.Point(0, 0);
			this.vtpScript.Name = "vtpScript";
			this.vtpScript.PageIndex = 4;
			this.vtpScript.Size = new System.Drawing.Size(595, 367);
			this.vtpScript.TabIndex = 6;
			this.vtpScript.Text = "Script";
			// 
			// fseScriptEditor
			// 
			this.fseScriptEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.fseScriptEditor.Location = new System.Drawing.Point(0, 40);
			this.fseScriptEditor.Name = "fseScriptEditor";
			this.fseScriptEditor.Script = null;
			this.fseScriptEditor.Size = new System.Drawing.Size(595, 327);
			this.fseScriptEditor.TabIndex = 1;
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.cbxUseScript);
			this.panel3.Controls.Add(this.label8);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel3.Location = new System.Drawing.Point(0, 0);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(595, 40);
			this.panel3.TabIndex = 0;
			// 
			// cbxUseScript
			// 
			this.cbxUseScript.AutoSize = true;
			this.cbxUseScript.Location = new System.Drawing.Point(16, 20);
			this.cbxUseScript.Name = "cbxUseScript";
			this.cbxUseScript.Size = new System.Drawing.Size(91, 17);
			this.cbxUseScript.TabIndex = 1;
			this.cbxUseScript.Text = "Include Script";
			this.cbxUseScript.UseVisualStyleBackColor = true;
			this.cbxUseScript.CheckedChanged += new System.EventHandler(this.cbxUseScript_CheckedChanged);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(3, 3);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(215, 13);
			this.label8.TabIndex = 0;
			this.label8.Text = "Provide the script for the FOMod, if desired.";
			// 
			// vtpReadme
			// 
			this.vtpReadme.BackColor = System.Drawing.SystemColors.Control;
			this.vtpReadme.Controls.Add(this.redReadmeEditor);
			this.vtpReadme.Controls.Add(this.panel2);
			this.vtpReadme.Dock = System.Windows.Forms.DockStyle.Fill;
			this.vtpReadme.Location = new System.Drawing.Point(0, 0);
			this.vtpReadme.Name = "vtpReadme";
			this.vtpReadme.PageIndex = 2;
			this.vtpReadme.Size = new System.Drawing.Size(595, 367);
			this.vtpReadme.TabIndex = 3;
			this.vtpReadme.Text = "Readme";
			// 
			// redReadmeEditor
			// 
			this.redReadmeEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.redReadmeEditor.Location = new System.Drawing.Point(0, 75);
			this.redReadmeEditor.Name = "redReadmeEditor";
			this.redReadmeEditor.Readme = null;
			this.redReadmeEditor.Size = new System.Drawing.Size(595, 292);
			this.redReadmeEditor.TabIndex = 1;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.butGenerateReadme);
			this.panel2.Controls.Add(this.label7);
			this.panel2.Controls.Add(this.label6);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel2.Location = new System.Drawing.Point(0, 0);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(595, 75);
			this.panel2.TabIndex = 0;
			// 
			// butGenerateReadme
			// 
			this.butGenerateReadme.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butGenerateReadme.Location = new System.Drawing.Point(358, 46);
			this.butGenerateReadme.Name = "butGenerateReadme";
			this.butGenerateReadme.Size = new System.Drawing.Size(75, 23);
			this.butGenerateReadme.TabIndex = 2;
			this.butGenerateReadme.Text = "Generate...";
			this.butGenerateReadme.UseVisualStyleBackColor = true;
			this.butGenerateReadme.Click += new System.EventHandler(this.butGenerateReadme_Click);
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(3, 20);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(430, 33);
			this.label7.TabIndex = 1;
			this.label7.Text = "You can either enter the text manually, or click the Generate button to create a " +
				"readme from existing files.";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(3, 3);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(171, 13);
			this.label6.TabIndex = 0;
			this.label6.Text = "Provide the text for the readme file.";
			// 
			// vtpDownloadLocations
			// 
			this.vtpDownloadLocations.BackColor = System.Drawing.SystemColors.Control;
			this.vtpDownloadLocations.Controls.Add(this.sdsDownloadLocations);
			this.vtpDownloadLocations.Controls.Add(this.label5);
			this.vtpDownloadLocations.Controls.Add(this.label4);
			this.vtpDownloadLocations.Dock = System.Windows.Forms.DockStyle.Fill;
			this.vtpDownloadLocations.Location = new System.Drawing.Point(0, 0);
			this.vtpDownloadLocations.Name = "vtpDownloadLocations";
			this.vtpDownloadLocations.PageIndex = 1;
			this.vtpDownloadLocations.Size = new System.Drawing.Size(595, 367);
			this.vtpDownloadLocations.TabIndex = 2;
			this.vtpDownloadLocations.Text = "Download Locations";
			// 
			// sdsDownloadLocations
			// 
			this.sdsDownloadLocations.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.sdsDownloadLocations.Location = new System.Drawing.Point(6, 75);
			this.sdsDownloadLocations.Name = "sdsDownloadLocations";
			this.sdsDownloadLocations.Size = new System.Drawing.Size(577, 286);
			this.sdsDownloadLocations.TabIndex = 2;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(3, 20);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(430, 58);
			this.label5.TabIndex = 1;
			this.label5.Text = resources.GetString("label5.Text");
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(3, 3);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(247, 13);
			this.label4.TabIndex = 0;
			this.label4.Text = "Provide the download locations for the source files.";
			// 
			// sspError
			// 
			this.sspError.ContainerControl = this;
			// 
			// sspWarning
			// 
			this.sspWarning.ContainerControl = this;
			this.sspWarning.Icon = ((System.Drawing.Icon)(resources.GetObject("sspWarning.Icon")));
			// 
			// FomodBuilderForm
			// 
			this.AcceptButton = this.butOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.butCancel;
			this.ClientSize = new System.Drawing.Size(595, 442);
			this.Controls.Add(this.vtcFomodData);
			this.Controls.Add(this.pnlButtons);
			this.Controls.Add(this.pnlHeader);
			this.Name = "FomodBuilderForm";
			this.Text = "FomodBuilderForm";
			this.pnlHeader.ResumeLayout(false);
			this.pnlHeader.PerformLayout();
			this.pnlButtons.ResumeLayout(false);
			this.vtcFomodData.ResumeLayout(false);
			this.vtpOutput.ResumeLayout(false);
			this.vtpOutput.PerformLayout();
			this.vtpSources.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.vtpInfo.ResumeLayout(false);
			this.vtpScript.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.vtpReadme.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.vtpDownloadLocations.ResumeLayout(false);
			this.vtpDownloadLocations.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.sspError)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sspWarning)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel pnlHeader;
		private System.Windows.Forms.Panel pnlButtons;
		private System.Windows.Forms.Button butOK;
		private System.Windows.Forms.Button butCancel;
		private Fomm.Controls.VerticalTabControl vtcFomodData;
		private Fomm.Controls.VerticalTabPage vtpSources;
		private Fomm.Controls.VerticalTabPage vtpDownloadLocations;
		private System.Windows.Forms.Label label1;
		private FomodFileSelector ffsFileStructure;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox tbxFomodFileName;
		private SourceDownloadSelector sdsDownloadLocations;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label4;
		private Fomm.Controls.VerticalTabPage vtpReadme;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button butGenerateReadme;
		private Fomm.Controls.VerticalTabPage vtpInfo;
		private FomodInfo finInfo;
		private Fomm.Controls.SiteStatusProvider sspError;
		private Fomm.Controls.SiteStatusProvider sspWarning;
		private Fomm.Controls.VerticalTabPage vtpOutput;
		private Fomm.Controls.VerticalTabPage vtpScript;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.CheckBox cbxUseScript;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Button butSelectPFPFolder;
		private System.Windows.Forms.TextBox tbxPFPPath;
		private System.Windows.Forms.CheckBox cbxPFP;
		private System.Windows.Forms.CheckBox cbxFomod;
		private Fomm.PackageManager.Controls.ReadmeEditor redReadmeEditor;
		private Fomm.PackageManager.Controls.FomodScriptEditor fseScriptEditor;
		private System.Windows.Forms.FolderBrowserDialog fbdPFPPath;
	}
}