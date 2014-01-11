namespace Fomm.Games.Fallout3.Tools.InstallTweaker {
    partial class xliveSettings {
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
            this.rbSse0 = new System.Windows.Forms.RadioButton();
            this.rbSse2 = new System.Windows.Forms.RadioButton();
            this.rbSse3 = new System.Windows.Forms.RadioButton();
            this.rbSse4 = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbProfile = new System.Windows.Forms.TextBox();
            this.bSseHelp = new System.Windows.Forms.Button();
            this.bProfileHelp = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // rbSse0
            // 
            this.rbSse0.AutoSize = true;
            this.rbSse0.Location = new System.Drawing.Point(12, 34);
            this.rbSse0.Name = "rbSse0";
            this.rbSse0.Size = new System.Drawing.Size(39, 17);
            this.rbSse0.TabIndex = 0;
            this.rbSse0.TabStop = true;
            this.rbSse0.Text = "Off";
            this.rbSse0.UseVisualStyleBackColor = true;
            // 
            // rbSse2
            // 
            this.rbSse2.AutoSize = true;
            this.rbSse2.Location = new System.Drawing.Point(75, 34);
            this.rbSse2.Name = "rbSse2";
            this.rbSse2.Size = new System.Drawing.Size(47, 17);
            this.rbSse2.TabIndex = 1;
            this.rbSse2.TabStop = true;
            this.rbSse2.Text = "sse2";
            this.rbSse2.UseVisualStyleBackColor = true;
            // 
            // rbSse3
            // 
            this.rbSse3.AutoSize = true;
            this.rbSse3.Location = new System.Drawing.Point(12, 57);
            this.rbSse3.Name = "rbSse3";
            this.rbSse3.Size = new System.Drawing.Size(47, 17);
            this.rbSse3.TabIndex = 2;
            this.rbSse3.TabStop = true;
            this.rbSse3.Text = "sse3";
            this.rbSse3.UseVisualStyleBackColor = true;
            // 
            // rbSse4
            // 
            this.rbSse4.AutoSize = true;
            this.rbSse4.Location = new System.Drawing.Point(75, 57);
            this.rbSse4.Name = "rbSse4";
            this.rbSse4.Size = new System.Drawing.Size(47, 17);
            this.rbSse4.TabIndex = 3;
            this.rbSse4.TabStop = true;
            this.rbSse4.Text = "sse4";
            this.rbSse4.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "D3DX sse optimization level";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Offline profile name";
            // 
            // tbProfile
            // 
            this.tbProfile.Location = new System.Drawing.Point(12, 111);
            this.tbProfile.MaxLength = 32;
            this.tbProfile.Name = "tbProfile";
            this.tbProfile.Size = new System.Drawing.Size(110, 20);
            this.tbProfile.TabIndex = 6;
            // 
            // bSseHelp
            // 
            this.bSseHelp.Location = new System.Drawing.Point(166, 34);
            this.bSseHelp.Name = "bSseHelp";
            this.bSseHelp.Size = new System.Drawing.Size(25, 25);
            this.bSseHelp.TabIndex = 7;
            this.bSseHelp.Text = "?";
            this.bSseHelp.UseVisualStyleBackColor = true;
            this.bSseHelp.Click += new System.EventHandler(this.bSseHelp_Click);
            // 
            // bProfileHelp
            // 
            this.bProfileHelp.Location = new System.Drawing.Point(166, 108);
            this.bProfileHelp.Name = "bProfileHelp";
            this.bProfileHelp.Size = new System.Drawing.Size(25, 25);
            this.bProfileHelp.TabIndex = 8;
            this.bProfileHelp.Text = "?";
            this.bProfileHelp.UseVisualStyleBackColor = true;
            this.bProfileHelp.Click += new System.EventHandler(this.bProfileHelp_Click);
            // 
            // xliveSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(223, 158);
            this.Controls.Add(this.bProfileHelp);
            this.Controls.Add(this.bSseHelp);
            this.Controls.Add(this.tbProfile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.rbSse4);
            this.Controls.Add(this.rbSse3);
            this.Controls.Add(this.rbSse2);
            this.Controls.Add(this.rbSse0);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "xliveSettings";
            this.Text = "xliveSettings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.xliveSettings_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton rbSse0;
        private System.Windows.Forms.RadioButton rbSse2;
        private System.Windows.Forms.RadioButton rbSse3;
        private System.Windows.Forms.RadioButton rbSse4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbProfile;
        private System.Windows.Forms.Button bSseHelp;
        private System.Windows.Forms.Button bProfileHelp;
    }
}