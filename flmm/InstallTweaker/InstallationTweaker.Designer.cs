namespace Fomm.InstallTweaker {
    partial class InstallationTweaker {
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
            this.cbDisableLive = new System.Windows.Forms.CheckBox();
            this.cbShrinkTextures = new System.Windows.Forms.CheckBox();
            this.bApply = new System.Windows.Forms.Button();
            this.bCancel = new System.Windows.Forms.Button();
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.bReset = new System.Windows.Forms.Button();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.bXliveSettings = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cbDisableLive
            // 
            this.cbDisableLive.AutoSize = true;
            this.cbDisableLive.Location = new System.Drawing.Point(12, 47);
            this.cbDisableLive.Name = "cbDisableLive";
            this.cbDisableLive.Size = new System.Drawing.Size(84, 17);
            this.cbDisableLive.TabIndex = 0;
            this.cbDisableLive.Text = "Disable Live";
            this.cbDisableLive.UseVisualStyleBackColor = true;
            this.cbDisableLive.CheckedChanged += new System.EventHandler(this.cbDisableLive_CheckedChanged);
            // 
            // cbShrinkTextures
            // 
            this.cbShrinkTextures.AutoSize = true;
            this.cbShrinkTextures.Location = new System.Drawing.Point(158, 47);
            this.cbShrinkTextures.Name = "cbShrinkTextures";
            this.cbShrinkTextures.Size = new System.Drawing.Size(96, 17);
            this.cbShrinkTextures.TabIndex = 1;
            this.cbShrinkTextures.Text = "Shrink textures";
            this.cbShrinkTextures.UseVisualStyleBackColor = true;
            this.cbShrinkTextures.CheckedChanged += new System.EventHandler(this.cbShrinkTextures_CheckedChanged);
            // 
            // bApply
            // 
            this.bApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bApply.Location = new System.Drawing.Point(221, 233);
            this.bApply.Name = "bApply";
            this.bApply.Size = new System.Drawing.Size(75, 23);
            this.bApply.TabIndex = 2;
            this.bApply.Text = "Apply";
            this.bApply.UseVisualStyleBackColor = true;
            this.bApply.Click += new System.EventHandler(this.bApply_Click);
            // 
            // bCancel
            // 
            this.bCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bCancel.Location = new System.Drawing.Point(140, 233);
            this.bCancel.Name = "bCancel";
            this.bCancel.Size = new System.Drawing.Size(75, 23);
            this.bCancel.TabIndex = 3;
            this.bCancel.Text = "Close";
            this.bCancel.UseVisualStyleBackColor = true;
            this.bCancel.Click += new System.EventHandler(this.bCancel_Click);
            // 
            // tbDescription
            // 
            this.tbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDescription.BackColor = System.Drawing.SystemColors.Window;
            this.tbDescription.Location = new System.Drawing.Point(12, 99);
            this.tbDescription.Multiline = true;
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.ReadOnly = true;
            this.tbDescription.Size = new System.Drawing.Size(284, 128);
            this.tbDescription.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(287, 35);
            this.label1.TabIndex = 7;
            this.label1.Text = "Always reset this utility before installing any official patches!";
            // 
            // bReset
            // 
            this.bReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bReset.Location = new System.Drawing.Point(12, 233);
            this.bReset.Name = "bReset";
            this.bReset.Size = new System.Drawing.Size(75, 23);
            this.bReset.TabIndex = 8;
            this.bReset.Text = "Reset";
            this.bReset.UseVisualStyleBackColor = true;
            this.bReset.Click += new System.EventHandler(this.bReset_Click);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            // 
            // bXliveSettings
            // 
            this.bXliveSettings.Enabled = false;
            this.bXliveSettings.Location = new System.Drawing.Point(12, 70);
            this.bXliveSettings.Name = "bXliveSettings";
            this.bXliveSettings.Size = new System.Drawing.Size(75, 23);
            this.bXliveSettings.TabIndex = 9;
            this.bXliveSettings.Text = "Settings";
            this.bXliveSettings.UseVisualStyleBackColor = true;
            this.bXliveSettings.Click += new System.EventHandler(this.bXliveSettings_Click);
            // 
            // InstallationTweaker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(308, 268);
            this.Controls.Add(this.bXliveSettings);
            this.Controls.Add(this.bReset);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbDescription);
            this.Controls.Add(this.bCancel);
            this.Controls.Add(this.bApply);
            this.Controls.Add(this.cbShrinkTextures);
            this.Controls.Add(this.cbDisableLive);
            this.Name = "InstallationTweaker";
            this.Text = "InstallationTweaker";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.InstallationTweaker_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cbDisableLive;
        private System.Windows.Forms.CheckBox cbShrinkTextures;
        private System.Windows.Forms.Button bApply;
        private System.Windows.Forms.Button bCancel;
        private System.Windows.Forms.TextBox tbDescription;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bReset;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Button bXliveSettings;
    }
}