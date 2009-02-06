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
            this.SuspendLayout();
            // 
            // cbFomod
            // 
            this.cbFomod.AutoSize = true;
            this.cbFomod.Location = new System.Drawing.Point(205, 12);
            this.cbFomod.Name = "cbFomod";
            this.cbFomod.Size = new System.Drawing.Size(101, 17);
            this.cbFomod.TabIndex = 0;
            this.cbFomod.Text = "Override default";
            this.cbFomod.UseVisualStyleBackColor = true;
            this.cbFomod.CheckedChanged += new System.EventHandler(this.cbFomod_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "fomod Directory";
            // 
            // tbFomod
            // 
            this.tbFomod.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFomod.Location = new System.Drawing.Point(12, 35);
            this.tbFomod.Name = "tbFomod";
            this.tbFomod.ReadOnly = true;
            this.tbFomod.Size = new System.Drawing.Size(294, 20);
            this.tbFomod.TabIndex = 2;
            // 
            // bBrowseFomod
            // 
            this.bBrowseFomod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bBrowseFomod.Enabled = false;
            this.bBrowseFomod.Location = new System.Drawing.Point(312, 33);
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
            this.bBrowseFallout.Location = new System.Drawing.Point(312, 88);
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
            this.tbFallout.Location = new System.Drawing.Point(12, 90);
            this.tbFallout.Name = "tbFallout";
            this.tbFallout.ReadOnly = true;
            this.tbFallout.Size = new System.Drawing.Size(294, 20);
            this.tbFallout.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Fallout Directory";
            // 
            // cbFallout
            // 
            this.cbFallout.AutoSize = true;
            this.cbFallout.Location = new System.Drawing.Point(205, 67);
            this.cbFallout.Name = "cbFallout";
            this.cbFallout.Size = new System.Drawing.Size(101, 17);
            this.cbFallout.TabIndex = 4;
            this.cbFallout.Text = "Override default";
            this.cbFallout.UseVisualStyleBackColor = true;
            this.cbFallout.CheckedChanged += new System.EventHandler(this.cbFallout_CheckedChanged);
            // 
            // tbLaunch
            // 
            this.tbLaunch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLaunch.Location = new System.Drawing.Point(12, 145);
            this.tbLaunch.Name = "tbLaunch";
            this.tbLaunch.ReadOnly = true;
            this.tbLaunch.Size = new System.Drawing.Size(375, 20);
            this.tbLaunch.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 123);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(111, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Launch command line";
            // 
            // cbLaunch
            // 
            this.cbLaunch.AutoSize = true;
            this.cbLaunch.Location = new System.Drawing.Point(205, 122);
            this.cbLaunch.Name = "cbLaunch";
            this.cbLaunch.Size = new System.Drawing.Size(101, 17);
            this.cbLaunch.TabIndex = 8;
            this.cbLaunch.Text = "Override default";
            this.cbLaunch.UseVisualStyleBackColor = true;
            this.cbLaunch.CheckedChanged += new System.EventHandler(this.cbLaunch_CheckedChanged);
            // 
            // cbAssociateFomod
            // 
            this.cbAssociateFomod.AutoSize = true;
            this.cbAssociateFomod.Location = new System.Drawing.Point(15, 171);
            this.cbAssociateFomod.Name = "cbAssociateFomod";
            this.cbAssociateFomod.Size = new System.Drawing.Size(131, 17);
            this.cbAssociateFomod.TabIndex = 14;
            this.cbAssociateFomod.Text = "Associate with fomods";
            this.cbAssociateFomod.UseVisualStyleBackColor = true;
            this.cbAssociateFomod.CheckedChanged += new System.EventHandler(this.cbAssociateFomod_CheckedChanged);
            // 
            // cbAssociateBsa
            // 
            this.cbAssociateBsa.AutoSize = true;
            this.cbAssociateBsa.Location = new System.Drawing.Point(15, 194);
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
            this.cbAssociateSdp.Location = new System.Drawing.Point(164, 171);
            this.cbAssociateSdp.Name = "cbAssociateSdp";
            this.cbAssociateSdp.Size = new System.Drawing.Size(124, 17);
            this.cbAssociateSdp.TabIndex = 16;
            this.cbAssociateSdp.Text = "Associate with SDPs";
            this.cbAssociateSdp.UseVisualStyleBackColor = true;
            this.cbAssociateSdp.CheckedChanged += new System.EventHandler(this.cbAssociateSdp_CheckedChanged);
            // 
            // cbShellExtensions
            // 
            this.cbShellExtensions.AutoSize = true;
            this.cbShellExtensions.Location = new System.Drawing.Point(164, 194);
            this.cbShellExtensions.Name = "cbShellExtensions";
            this.cbShellExtensions.Size = new System.Drawing.Size(231, 17);
            this.cbShellExtensions.TabIndex = 17;
            this.cbShellExtensions.Text = "Add shell extensions for supported file types";
            this.cbShellExtensions.UseVisualStyleBackColor = true;
            this.cbShellExtensions.CheckedChanged += new System.EventHandler(this.cbShellExtensions_CheckedChanged);
            // 
            // SetupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(399, 230);
            this.Controls.Add(this.cbShellExtensions);
            this.Controls.Add(this.cbAssociateSdp);
            this.Controls.Add(this.cbAssociateBsa);
            this.Controls.Add(this.cbAssociateFomod);
            this.Controls.Add(this.tbLaunch);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbLaunch);
            this.Controls.Add(this.bBrowseFallout);
            this.Controls.Add(this.tbFallout);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbFallout);
            this.Controls.Add(this.bBrowseFomod);
            this.Controls.Add(this.tbFomod);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbFomod);
            this.Name = "SetupForm";
            this.Text = "SetupForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SetupForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}