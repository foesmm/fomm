namespace Fomm {
    partial class SetupForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
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
			this.cbFomod = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.tbFomod = new System.Windows.Forms.TextBox();
			this.bBrowseFomod = new System.Windows.Forms.Button();
			this.bBrowseFallout = new System.Windows.Forms.Button();
			this.tbFallout = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.cbFallout = new System.Windows.Forms.CheckBox();
			this.tbLaunch = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.cbLaunch = new System.Windows.Forms.CheckBox();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.cbAssociateFomod = new System.Windows.Forms.CheckBox();
			this.cbAssociateBsa = new System.Windows.Forms.CheckBox();
			this.cbAssociateSdp = new System.Windows.Forms.CheckBox();
			this.cbShellExtensions = new System.Windows.Forms.CheckBox();
			this.cbEsmShow = new System.Windows.Forms.CheckBox();
			this.cbDisableIPC = new System.Windows.Forms.CheckBox();
			this.cbDisableUAC = new System.Windows.Forms.CheckBox();
			this.tbLaunchArgs = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.cbUseDocs = new System.Windows.Forms.CheckBox();
			this.ckbCheckFomodVersions = new System.Windows.Forms.CheckBox();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.cbxPFPFormat = new System.Windows.Forms.ComboBox();
			this.cbxPFPCompression = new System.Windows.Forms.ComboBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.cbxFomodFormat = new System.Windows.Forms.ComboBox();
			this.cbxFomodCompression = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// cbFomod
			// 
			this.cbFomod.AutoSize = true;
			this.cbFomod.Location = new System.Drawing.Point(200, 4);
			this.cbFomod.Name = "cbFomod";
			this.cbFomod.Size = new System.Drawing.Size(101, 17);
			this.cbFomod.TabIndex = 1;
			this.cbFomod.Text = "Override default";
			this.cbFomod.UseVisualStyleBackColor = true;
			this.cbFomod.CheckedChanged += new System.EventHandler(this.cbFomod_CheckedChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(7, 5);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(81, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "fomod Directory";
			// 
			// tbFomod
			// 
			this.tbFomod.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbFomod.Location = new System.Drawing.Point(7, 27);
			this.tbFomod.Name = "tbFomod";
			this.tbFomod.ReadOnly = true;
			this.tbFomod.Size = new System.Drawing.Size(301, 20);
			this.tbFomod.TabIndex = 2;
			// 
			// bBrowseFomod
			// 
			this.bBrowseFomod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.bBrowseFomod.Enabled = false;
			this.bBrowseFomod.Location = new System.Drawing.Point(314, 25);
			this.bBrowseFomod.Name = "bBrowseFomod";
			this.bBrowseFomod.Size = new System.Drawing.Size(75, 23);
			this.bBrowseFomod.TabIndex = 3;
			this.bBrowseFomod.Text = "Browse";
			this.bBrowseFomod.UseVisualStyleBackColor = true;
			this.bBrowseFomod.Click += new System.EventHandler(this.bBrowseFomod_Click);
			// 
			// bBrowseFallout
			// 
			this.bBrowseFallout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.bBrowseFallout.Enabled = false;
			this.bBrowseFallout.Location = new System.Drawing.Point(314, 80);
			this.bBrowseFallout.Name = "bBrowseFallout";
			this.bBrowseFallout.Size = new System.Drawing.Size(75, 23);
			this.bBrowseFallout.TabIndex = 7;
			this.bBrowseFallout.Text = "Browse";
			this.bBrowseFallout.UseVisualStyleBackColor = true;
			this.bBrowseFallout.Click += new System.EventHandler(this.bBrowseFallout_Click);
			// 
			// tbFallout
			// 
			this.tbFallout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbFallout.Location = new System.Drawing.Point(7, 82);
			this.tbFallout.Name = "tbFallout";
			this.tbFallout.ReadOnly = true;
			this.tbFallout.Size = new System.Drawing.Size(301, 20);
			this.tbFallout.TabIndex = 6;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(7, 60);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(83, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Fallout Directory";
			// 
			// cbFallout
			// 
			this.cbFallout.AutoSize = true;
			this.cbFallout.Location = new System.Drawing.Point(200, 59);
			this.cbFallout.Name = "cbFallout";
			this.cbFallout.Size = new System.Drawing.Size(101, 17);
			this.cbFallout.TabIndex = 5;
			this.cbFallout.Text = "Override default";
			this.cbFallout.UseVisualStyleBackColor = true;
			this.cbFallout.CheckedChanged += new System.EventHandler(this.cbFallout_CheckedChanged);
			// 
			// tbLaunch
			// 
			this.tbLaunch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbLaunch.Location = new System.Drawing.Point(7, 137);
			this.tbLaunch.Name = "tbLaunch";
			this.tbLaunch.ReadOnly = true;
			this.tbLaunch.Size = new System.Drawing.Size(382, 20);
			this.tbLaunch.TabIndex = 10;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(7, 115);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(111, 13);
			this.label3.TabIndex = 8;
			this.label3.Text = "Launch command line";
			// 
			// cbLaunch
			// 
			this.cbLaunch.AutoSize = true;
			this.cbLaunch.Location = new System.Drawing.Point(200, 114);
			this.cbLaunch.Name = "cbLaunch";
			this.cbLaunch.Size = new System.Drawing.Size(101, 17);
			this.cbLaunch.TabIndex = 9;
			this.cbLaunch.Text = "Override default";
			this.cbLaunch.UseVisualStyleBackColor = true;
			this.cbLaunch.CheckedChanged += new System.EventHandler(this.cbLaunch_CheckedChanged);
			// 
			// cbAssociateFomod
			// 
			this.cbAssociateFomod.AutoSize = true;
			this.cbAssociateFomod.Location = new System.Drawing.Point(10, 202);
			this.cbAssociateFomod.Name = "cbAssociateFomod";
			this.cbAssociateFomod.Size = new System.Drawing.Size(131, 17);
			this.cbAssociateFomod.TabIndex = 13;
			this.cbAssociateFomod.Text = "Associate with fomods";
			this.cbAssociateFomod.UseVisualStyleBackColor = true;
			this.cbAssociateFomod.CheckedChanged += new System.EventHandler(this.cbAssociateFomod_CheckedChanged);
			// 
			// cbAssociateBsa
			// 
			this.cbAssociateBsa.AutoSize = true;
			this.cbAssociateBsa.Location = new System.Drawing.Point(10, 225);
			this.cbAssociateBsa.Name = "cbAssociateBsa";
			this.cbAssociateBsa.Size = new System.Drawing.Size(123, 17);
			this.cbAssociateBsa.TabIndex = 15;
			this.cbAssociateBsa.Text = "Associate with BSAs";
			this.cbAssociateBsa.UseVisualStyleBackColor = true;
			this.cbAssociateBsa.CheckedChanged += new System.EventHandler(this.cbAssociateBsa_CheckedChanged);
			// 
			// cbAssociateSdp
			// 
			this.cbAssociateSdp.AutoSize = true;
			this.cbAssociateSdp.Location = new System.Drawing.Point(159, 202);
			this.cbAssociateSdp.Name = "cbAssociateSdp";
			this.cbAssociateSdp.Size = new System.Drawing.Size(124, 17);
			this.cbAssociateSdp.TabIndex = 14;
			this.cbAssociateSdp.Text = "Associate with SDPs";
			this.cbAssociateSdp.UseVisualStyleBackColor = true;
			this.cbAssociateSdp.CheckedChanged += new System.EventHandler(this.cbAssociateSdp_CheckedChanged);
			// 
			// cbShellExtensions
			// 
			this.cbShellExtensions.AutoSize = true;
			this.cbShellExtensions.Location = new System.Drawing.Point(159, 225);
			this.cbShellExtensions.Name = "cbShellExtensions";
			this.cbShellExtensions.Size = new System.Drawing.Size(231, 17);
			this.cbShellExtensions.TabIndex = 16;
			this.cbShellExtensions.Text = "Add shell extensions for supported file types";
			this.cbShellExtensions.UseVisualStyleBackColor = true;
			this.cbShellExtensions.CheckedChanged += new System.EventHandler(this.cbShellExtensions_CheckedChanged);
			// 
			// cbEsmShow
			// 
			this.cbEsmShow.AutoSize = true;
			this.cbEsmShow.Location = new System.Drawing.Point(10, 259);
			this.cbEsmShow.Name = "cbEsmShow";
			this.cbEsmShow.Size = new System.Drawing.Size(118, 17);
			this.cbEsmShow.TabIndex = 17;
			this.cbEsmShow.Text = "Show ESMs in bold";
			this.cbEsmShow.UseVisualStyleBackColor = true;
			this.cbEsmShow.CheckedChanged += new System.EventHandler(this.bEsmShow_CheckedChanged);
			// 
			// cbDisableIPC
			// 
			this.cbDisableIPC.AutoSize = true;
			this.cbDisableIPC.Location = new System.Drawing.Point(159, 259);
			this.cbDisableIPC.Name = "cbDisableIPC";
			this.cbDisableIPC.Size = new System.Drawing.Size(135, 17);
			this.cbDisableIPC.TabIndex = 18;
			this.cbDisableIPC.Text = "Disable IPC Messaging";
			this.cbDisableIPC.UseVisualStyleBackColor = true;
			this.cbDisableIPC.CheckedChanged += new System.EventHandler(this.cbDisableIPC_CheckedChanged);
			// 
			// cbDisableUAC
			// 
			this.cbDisableUAC.AutoSize = true;
			this.cbDisableUAC.Location = new System.Drawing.Point(10, 282);
			this.cbDisableUAC.Name = "cbDisableUAC";
			this.cbDisableUAC.Size = new System.Drawing.Size(124, 17);
			this.cbDisableUAC.TabIndex = 19;
			this.cbDisableUAC.Text = "Disable UAC checks";
			this.cbDisableUAC.UseVisualStyleBackColor = true;
			this.cbDisableUAC.CheckedChanged += new System.EventHandler(this.cbDisableUAC_CheckedChanged);
			// 
			// tbLaunchArgs
			// 
			this.tbLaunchArgs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbLaunchArgs.Location = new System.Drawing.Point(7, 176);
			this.tbLaunchArgs.Name = "tbLaunchArgs";
			this.tbLaunchArgs.ReadOnly = true;
			this.tbLaunchArgs.Size = new System.Drawing.Size(382, 20);
			this.tbLaunchArgs.TabIndex = 12;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(7, 160);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(144, 13);
			this.label4.TabIndex = 11;
			this.label4.Text = "Launch command arguments";
			// 
			// cbUseDocs
			// 
			this.cbUseDocs.AutoSize = true;
			this.cbUseDocs.Location = new System.Drawing.Point(159, 282);
			this.cbUseDocs.Name = "cbUseDocs";
			this.cbUseDocs.Size = new System.Drawing.Size(175, 17);
			this.cbUseDocs.TabIndex = 20;
			this.cbUseDocs.Text = "Use docs folder for new fomods";
			this.cbUseDocs.UseVisualStyleBackColor = true;
			this.cbUseDocs.CheckedChanged += new System.EventHandler(this.cbUseDocs_CheckedChanged);
			// 
			// ckbCheckFomodVersions
			// 
			this.ckbCheckFomodVersions.AutoSize = true;
			this.ckbCheckFomodVersions.Location = new System.Drawing.Point(10, 316);
			this.ckbCheckFomodVersions.Name = "ckbCheckFomodVersions";
			this.ckbCheckFomodVersions.Size = new System.Drawing.Size(182, 17);
			this.ckbCheckFomodVersions.TabIndex = 21;
			this.ckbCheckFomodVersions.Text = "Check for new FOMOD versions.";
			this.ckbCheckFomodVersions.UseVisualStyleBackColor = true;
			this.ckbCheckFomodVersions.CheckedChanged += new System.EventHandler(this.ckbCheckFomodVersions_CheckedChanged);
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(406, 367);
			this.tabControl1.TabIndex = 22;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.label1);
			this.tabPage1.Controls.Add(this.ckbCheckFomodVersions);
			this.tabPage1.Controls.Add(this.cbFomod);
			this.tabPage1.Controls.Add(this.cbUseDocs);
			this.tabPage1.Controls.Add(this.tbFomod);
			this.tabPage1.Controls.Add(this.label4);
			this.tabPage1.Controls.Add(this.bBrowseFomod);
			this.tabPage1.Controls.Add(this.tbLaunchArgs);
			this.tabPage1.Controls.Add(this.cbFallout);
			this.tabPage1.Controls.Add(this.cbDisableUAC);
			this.tabPage1.Controls.Add(this.label2);
			this.tabPage1.Controls.Add(this.cbDisableIPC);
			this.tabPage1.Controls.Add(this.tbFallout);
			this.tabPage1.Controls.Add(this.cbEsmShow);
			this.tabPage1.Controls.Add(this.bBrowseFallout);
			this.tabPage1.Controls.Add(this.cbShellExtensions);
			this.tabPage1.Controls.Add(this.cbLaunch);
			this.tabPage1.Controls.Add(this.cbAssociateSdp);
			this.tabPage1.Controls.Add(this.label3);
			this.tabPage1.Controls.Add(this.cbAssociateBsa);
			this.tabPage1.Controls.Add(this.tbLaunch);
			this.tabPage1.Controls.Add(this.cbAssociateFomod);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(398, 341);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "General";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.groupBox2);
			this.tabPage2.Controls.Add(this.groupBox1);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(398, 341);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "FOMOD Options";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.cbxPFPFormat);
			this.groupBox2.Controls.Add(this.cbxPFPCompression);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Controls.Add(this.label8);
			this.groupBox2.Location = new System.Drawing.Point(8, 124);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(382, 80);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Premade FOMOD Pack Compression";
			// 
			// cbxPFPFormat
			// 
			this.cbxPFPFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbxPFPFormat.FormattingEnabled = true;
			this.cbxPFPFormat.Location = new System.Drawing.Point(111, 19);
			this.cbxPFPFormat.Name = "cbxPFPFormat";
			this.cbxPFPFormat.Size = new System.Drawing.Size(186, 21);
			this.cbxPFPFormat.TabIndex = 0;
			// 
			// cbxPFPCompression
			// 
			this.cbxPFPCompression.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbxPFPCompression.FormattingEnabled = true;
			this.cbxPFPCompression.Location = new System.Drawing.Point(111, 46);
			this.cbxPFPCompression.Name = "cbxPFPCompression";
			this.cbxPFPCompression.Size = new System.Drawing.Size(186, 21);
			this.cbxPFPCompression.TabIndex = 1;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(63, 22);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(42, 13);
			this.label7.TabIndex = 4;
			this.label7.Text = "Format:";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(6, 49);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(99, 13);
			this.label8.TabIndex = 6;
			this.label8.Text = "Compression Level:";
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.label9);
			this.groupBox1.Controls.Add(this.cbxFomodFormat);
			this.groupBox1.Controls.Add(this.cbxFomodCompression);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Location = new System.Drawing.Point(8, 6);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(382, 112);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "FOMOD Compression";
			// 
			// cbxFomodFormat
			// 
			this.cbxFomodFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbxFomodFormat.FormattingEnabled = true;
			this.cbxFomodFormat.Location = new System.Drawing.Point(111, 50);
			this.cbxFomodFormat.Name = "cbxFomodFormat";
			this.cbxFomodFormat.Size = new System.Drawing.Size(186, 21);
			this.cbxFomodFormat.TabIndex = 0;
			// 
			// cbxFomodCompression
			// 
			this.cbxFomodCompression.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbxFomodCompression.FormattingEnabled = true;
			this.cbxFomodCompression.Location = new System.Drawing.Point(111, 77);
			this.cbxFomodCompression.Name = "cbxFomodCompression";
			this.cbxFomodCompression.Size = new System.Drawing.Size(186, 21);
			this.cbxFomodCompression.TabIndex = 1;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(63, 53);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(42, 13);
			this.label5.TabIndex = 0;
			this.label5.Text = "Format:";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(6, 80);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(99, 13);
			this.label6.TabIndex = 2;
			this.label6.Text = "Compression Level:";
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(22, 16);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(354, 31);
			this.label9.TabIndex = 3;
			this.label9.Text = "NOTE: Using a format other than Zip can make the Package Manager respond slowly.";
			// 
			// SetupForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(406, 367);
			this.Controls.Add(this.tabControl1);
			this.Name = "SetupForm";
			this.Text = "SetupForm";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SetupForm_FormClosing);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			this.tabPage2.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox cbFomod;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbFomod;
        private System.Windows.Forms.Button bBrowseFomod;
        private System.Windows.Forms.Button bBrowseFallout;
        private System.Windows.Forms.TextBox tbFallout;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cbFallout;
        private System.Windows.Forms.TextBox tbLaunch;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox cbLaunch;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.CheckBox cbAssociateFomod;
        private System.Windows.Forms.CheckBox cbAssociateBsa;
        private System.Windows.Forms.CheckBox cbAssociateSdp;
        private System.Windows.Forms.CheckBox cbShellExtensions;
        private System.Windows.Forms.CheckBox cbEsmShow;
        private System.Windows.Forms.CheckBox cbDisableIPC;
        private System.Windows.Forms.CheckBox cbDisableUAC;
        private System.Windows.Forms.TextBox tbLaunchArgs;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox cbUseDocs;
		private System.Windows.Forms.CheckBox ckbCheckFomodVersions;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox cbxFomodCompression;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ComboBox cbxFomodFormat;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.ComboBox cbxPFPFormat;
		private System.Windows.Forms.ComboBox cbxPFPCompression;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
    }
}