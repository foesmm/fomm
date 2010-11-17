namespace Fomm {
    partial class SettingsForm {
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
			this.components = new System.ComponentModel.Container();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.cbAssociateFomod = new System.Windows.Forms.CheckBox();
			this.cbAssociateBsa = new System.Windows.Forms.CheckBox();
			this.cbAssociateSdp = new System.Windows.Forms.CheckBox();
			this.cbShellExtensions = new System.Windows.Forms.CheckBox();
			this.cbDisableIPC = new System.Windows.Forms.CheckBox();
			this.cbDisableUAC = new System.Windows.Forms.CheckBox();
			this.ckbCheckFomodVersions = new System.Windows.Forms.CheckBox();
			this.tbcTabs = new System.Windows.Forms.TabControl();
			this.tpgGeneral = new System.Windows.Forms.TabPage();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.gbxAssociations = new System.Windows.Forms.GroupBox();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.cbUseDocs = new System.Windows.Forms.CheckBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.cbxPFPFormat = new System.Windows.Forms.ComboBox();
			this.cbxPFPCompression = new System.Windows.Forms.ComboBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label9 = new System.Windows.Forms.Label();
			this.cbxFomodFormat = new System.Windows.Forms.ComboBox();
			this.cbxFomodCompression = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.comboBox2 = new System.Windows.Forms.ComboBox();
			this.label11 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.butCancel = new System.Windows.Forms.Button();
			this.butOK = new System.Windows.Forms.Button();
			this.ttpTip = new System.Windows.Forms.ToolTip(this.components);
			this.ckbAddMissingInfo = new System.Windows.Forms.CheckBox();
			this.tbcTabs.SuspendLayout();
			this.tpgGeneral.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.gbxAssociations.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// cbAssociateFomod
			// 
			this.cbAssociateFomod.AutoSize = true;
			this.cbAssociateFomod.Location = new System.Drawing.Point(16, 19);
			this.cbAssociateFomod.Name = "cbAssociateFomod";
			this.cbAssociateFomod.Size = new System.Drawing.Size(131, 17);
			this.cbAssociateFomod.TabIndex = 0;
			this.cbAssociateFomod.Text = "Associate with fomods";
			this.cbAssociateFomod.UseVisualStyleBackColor = true;
			// 
			// cbAssociateBsa
			// 
			this.cbAssociateBsa.AutoSize = true;
			this.cbAssociateBsa.Location = new System.Drawing.Point(16, 42);
			this.cbAssociateBsa.Name = "cbAssociateBsa";
			this.cbAssociateBsa.Size = new System.Drawing.Size(123, 17);
			this.cbAssociateBsa.TabIndex = 1;
			this.cbAssociateBsa.Text = "Associate with BSAs";
			this.cbAssociateBsa.UseVisualStyleBackColor = true;
			// 
			// cbAssociateSdp
			// 
			this.cbAssociateSdp.AutoSize = true;
			this.cbAssociateSdp.Location = new System.Drawing.Point(16, 65);
			this.cbAssociateSdp.Name = "cbAssociateSdp";
			this.cbAssociateSdp.Size = new System.Drawing.Size(124, 17);
			this.cbAssociateSdp.TabIndex = 2;
			this.cbAssociateSdp.Text = "Associate with SDPs";
			this.cbAssociateSdp.UseVisualStyleBackColor = true;
			// 
			// cbShellExtensions
			// 
			this.cbShellExtensions.AutoSize = true;
			this.cbShellExtensions.Location = new System.Drawing.Point(16, 88);
			this.cbShellExtensions.Name = "cbShellExtensions";
			this.cbShellExtensions.Size = new System.Drawing.Size(231, 17);
			this.cbShellExtensions.TabIndex = 3;
			this.cbShellExtensions.Text = "Add shell extensions for supported file types";
			this.cbShellExtensions.UseVisualStyleBackColor = true;
			// 
			// cbDisableIPC
			// 
			this.cbDisableIPC.AutoSize = true;
			this.cbDisableIPC.Location = new System.Drawing.Point(16, 19);
			this.cbDisableIPC.Name = "cbDisableIPC";
			this.cbDisableIPC.Size = new System.Drawing.Size(135, 17);
			this.cbDisableIPC.TabIndex = 4;
			this.cbDisableIPC.Text = "Disable IPC Messaging";
			this.cbDisableIPC.UseVisualStyleBackColor = true;
			// 
			// cbDisableUAC
			// 
			this.cbDisableUAC.AutoSize = true;
			this.cbDisableUAC.Location = new System.Drawing.Point(16, 42);
			this.cbDisableUAC.Name = "cbDisableUAC";
			this.cbDisableUAC.Size = new System.Drawing.Size(124, 17);
			this.cbDisableUAC.TabIndex = 5;
			this.cbDisableUAC.Text = "Disable UAC checks";
			this.cbDisableUAC.UseVisualStyleBackColor = true;
			// 
			// ckbCheckFomodVersions
			// 
			this.ckbCheckFomodVersions.AutoSize = true;
			this.ckbCheckFomodVersions.Location = new System.Drawing.Point(16, 65);
			this.ckbCheckFomodVersions.Name = "ckbCheckFomodVersions";
			this.ckbCheckFomodVersions.Size = new System.Drawing.Size(175, 17);
			this.ckbCheckFomodVersions.TabIndex = 6;
			this.ckbCheckFomodVersions.Text = "Check for new FOMod versions";
			this.ckbCheckFomodVersions.UseVisualStyleBackColor = true;
			// 
			// tbcTabs
			// 
			this.tbcTabs.Controls.Add(this.tpgGeneral);
			this.tbcTabs.Controls.Add(this.tabPage2);
			this.tbcTabs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tbcTabs.Location = new System.Drawing.Point(0, 0);
			this.tbcTabs.Name = "tbcTabs";
			this.tbcTabs.SelectedIndex = 0;
			this.tbcTabs.Size = new System.Drawing.Size(406, 320);
			this.tbcTabs.TabIndex = 22;
			// 
			// tpgGeneral
			// 
			this.tpgGeneral.Controls.Add(this.groupBox5);
			this.tpgGeneral.Controls.Add(this.gbxAssociations);
			this.tpgGeneral.Location = new System.Drawing.Point(4, 22);
			this.tpgGeneral.Name = "tpgGeneral";
			this.tpgGeneral.Padding = new System.Windows.Forms.Padding(3);
			this.tpgGeneral.Size = new System.Drawing.Size(398, 294);
			this.tpgGeneral.TabIndex = 0;
			this.tpgGeneral.Text = "General";
			this.tpgGeneral.UseVisualStyleBackColor = true;
			this.tpgGeneral.MouseMove += new System.Windows.Forms.MouseEventHandler(this.tpgGeneral_MouseMove);
			this.tpgGeneral.MouseHover += new System.EventHandler(this.tpgGeneral_MouseHover);
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.ckbAddMissingInfo);
			this.groupBox5.Controls.Add(this.cbDisableIPC);
			this.groupBox5.Controls.Add(this.cbDisableUAC);
			this.groupBox5.Controls.Add(this.ckbCheckFomodVersions);
			this.groupBox5.Location = new System.Drawing.Point(6, 126);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(384, 114);
			this.groupBox5.TabIndex = 23;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Options";
			// 
			// gbxAssociations
			// 
			this.gbxAssociations.Controls.Add(this.cbAssociateFomod);
			this.gbxAssociations.Controls.Add(this.cbAssociateBsa);
			this.gbxAssociations.Controls.Add(this.cbAssociateSdp);
			this.gbxAssociations.Controls.Add(this.cbShellExtensions);
			this.gbxAssociations.Location = new System.Drawing.Point(6, 6);
			this.gbxAssociations.Name = "gbxAssociations";
			this.gbxAssociations.Size = new System.Drawing.Size(384, 114);
			this.gbxAssociations.TabIndex = 22;
			this.gbxAssociations.TabStop = false;
			this.gbxAssociations.Text = "File Type Associations";
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.cbUseDocs);
			this.tabPage2.Controls.Add(this.groupBox2);
			this.tabPage2.Controls.Add(this.groupBox1);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(398, 294);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "FOMod Options";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// cbUseDocs
			// 
			this.cbUseDocs.AutoSize = true;
			this.cbUseDocs.Location = new System.Drawing.Point(17, 210);
			this.cbUseDocs.Name = "cbUseDocs";
			this.cbUseDocs.Size = new System.Drawing.Size(175, 17);
			this.cbUseDocs.TabIndex = 2;
			this.cbUseDocs.Text = "Use docs folder for new fomods";
			this.cbUseDocs.UseVisualStyleBackColor = true;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.cbxPFPFormat);
			this.groupBox2.Controls.Add(this.cbxPFPCompression);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Controls.Add(this.label8);
			this.groupBox2.Location = new System.Drawing.Point(6, 124);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(384, 80);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Premade FOMod Pack Compression";
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
			this.groupBox1.Location = new System.Drawing.Point(6, 6);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(384, 112);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "FOMod Compression";
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(22, 16);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(354, 31);
			this.label9.TabIndex = 3;
			this.label9.Text = "NOTE: Using a format other than Zip can make the Package Manager respond slowly.";
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
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(22, 16);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(354, 31);
			this.label10.TabIndex = 3;
			this.label10.Text = "NOTE: Using a format other than Zip can make the Package Manager respond slowly.";
			// 
			// comboBox1
			// 
			this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Location = new System.Drawing.Point(111, 50);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(186, 21);
			this.comboBox1.TabIndex = 0;
			// 
			// comboBox2
			// 
			this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox2.FormattingEnabled = true;
			this.comboBox2.Location = new System.Drawing.Point(111, 77);
			this.comboBox2.Name = "comboBox2";
			this.comboBox2.Size = new System.Drawing.Size(186, 21);
			this.comboBox2.TabIndex = 1;
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(63, 53);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(42, 13);
			this.label11.TabIndex = 0;
			this.label11.Text = "Format:";
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(6, 80);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(99, 13);
			this.label12.TabIndex = 2;
			this.label12.Text = "Compression Level:";
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.butCancel);
			this.panel1.Controls.Add(this.butOK);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 320);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(406, 47);
			this.panel1.TabIndex = 23;
			// 
			// butCancel
			// 
			this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.butCancel.Location = new System.Drawing.Point(319, 12);
			this.butCancel.Name = "butCancel";
			this.butCancel.Size = new System.Drawing.Size(75, 23);
			this.butCancel.TabIndex = 1;
			this.butCancel.Text = "Cancel";
			this.butCancel.UseVisualStyleBackColor = true;
			// 
			// butOK
			// 
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Location = new System.Drawing.Point(238, 12);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(75, 23);
			this.butOK.TabIndex = 0;
			this.butOK.Text = "OK";
			this.butOK.UseVisualStyleBackColor = true;
			this.butOK.Click += new System.EventHandler(this.butOK_Click);
			// 
			// cbxAddMissingInfo
			// 
			this.ckbAddMissingInfo.AutoSize = true;
			this.ckbAddMissingInfo.Location = new System.Drawing.Point(16, 88);
			this.ckbAddMissingInfo.Name = "cbxAddMissingInfo";
			this.ckbAddMissingInfo.Size = new System.Drawing.Size(157, 17);
			this.ckbAddMissingInfo.TabIndex = 7;
			this.ckbAddMissingInfo.Text = "Add missing info to FOMods";
			this.ckbAddMissingInfo.UseVisualStyleBackColor = true;
			// 
			// SettingsForm
			// 
			this.AcceptButton = this.butOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.butCancel;
			this.ClientSize = new System.Drawing.Size(406, 367);
			this.Controls.Add(this.tbcTabs);
			this.Controls.Add(this.panel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "SettingsForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Settings";
			this.tbcTabs.ResumeLayout(false);
			this.tpgGeneral.ResumeLayout(false);
			this.groupBox5.ResumeLayout(false);
			this.groupBox5.PerformLayout();
			this.gbxAssociations.ResumeLayout(false);
			this.gbxAssociations.PerformLayout();
			this.tabPage2.ResumeLayout(false);
			this.tabPage2.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.CheckBox cbAssociateFomod;
        private System.Windows.Forms.CheckBox cbAssociateBsa;
        private System.Windows.Forms.CheckBox cbAssociateSdp;
		private System.Windows.Forms.CheckBox cbShellExtensions;
        private System.Windows.Forms.CheckBox cbDisableIPC;
		private System.Windows.Forms.CheckBox cbDisableUAC;
		private System.Windows.Forms.CheckBox ckbCheckFomodVersions;
		private System.Windows.Forms.TabControl tbcTabs;
		private System.Windows.Forms.TabPage tpgGeneral;
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
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.ComboBox comboBox2;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.GroupBox gbxAssociations;
		private System.Windows.Forms.CheckBox cbUseDocs;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button butCancel;
		private System.Windows.Forms.Button butOK;
		private System.Windows.Forms.ToolTip ttpTip;
		private System.Windows.Forms.CheckBox ckbAddMissingInfo;
    }
}