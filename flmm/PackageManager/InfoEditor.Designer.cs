namespace Fomm.PackageManager {
    partial class InfoEditor {
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
			this.tbName = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.tbAuthor = new System.Windows.Forms.TextBox();
			this.bScreenshot = new System.Windows.Forms.Button();
			this.bSave = new System.Windows.Forms.Button();
			this.bCancel = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.tbDescription = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.tbVersion = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.bClearScreenshot = new System.Windows.Forms.Button();
			this.tbMVersion = new System.Windows.Forms.TextBox();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.tbMinFommVersion = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.tbWebsite = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.tbEmail = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.clbGroups = new System.Windows.Forms.CheckedListBox();
			this.SuspendLayout();
			// 
			// tbName
			// 
			this.tbName.Location = new System.Drawing.Point(12, 12);
			this.tbName.Name = "tbName";
			this.tbName.Size = new System.Drawing.Size(178, 20);
			this.tbName.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(196, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(57, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Mod name";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(196, 41);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(62, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Mod Author";
			// 
			// tbAuthor
			// 
			this.tbAuthor.Location = new System.Drawing.Point(12, 38);
			this.tbAuthor.Name = "tbAuthor";
			this.tbAuthor.Size = new System.Drawing.Size(178, 20);
			this.tbAuthor.TabIndex = 2;
			// 
			// bScreenshot
			// 
			this.bScreenshot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.bScreenshot.Location = new System.Drawing.Point(12, 424);
			this.bScreenshot.Name = "bScreenshot";
			this.bScreenshot.Size = new System.Drawing.Size(115, 23);
			this.bScreenshot.TabIndex = 18;
			this.bScreenshot.Text = "Set screenshot";
			this.bScreenshot.UseVisualStyleBackColor = true;
			this.bScreenshot.Click += new System.EventHandler(this.bScreenshot_Click);
			// 
			// bSave
			// 
			this.bSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.bSave.Location = new System.Drawing.Point(260, 453);
			this.bSave.Name = "bSave";
			this.bSave.Size = new System.Drawing.Size(75, 23);
			this.bSave.TabIndex = 22;
			this.bSave.Text = "Save";
			this.bSave.UseVisualStyleBackColor = true;
			this.bSave.Click += new System.EventHandler(this.bSave_Click);
			// 
			// bCancel
			// 
			this.bCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.bCancel.Location = new System.Drawing.Point(260, 424);
			this.bCancel.Name = "bCancel";
			this.bCancel.Size = new System.Drawing.Size(75, 23);
			this.bCancel.TabIndex = 21;
			this.bCancel.Text = "Cancel";
			this.bCancel.UseVisualStyleBackColor = true;
			this.bCancel.Click += new System.EventHandler(this.bCancel_Click);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(9, 191);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(84, 13);
			this.label3.TabIndex = 14;
			this.label3.Text = "Mod Description";
			// 
			// tbDescription
			// 
			this.tbDescription.AcceptsReturn = true;
			this.tbDescription.AcceptsTab = true;
			this.tbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbDescription.Location = new System.Drawing.Point(12, 207);
			this.tbDescription.Multiline = true;
			this.tbDescription.Name = "tbDescription";
			this.tbDescription.Size = new System.Drawing.Size(323, 142);
			this.tbDescription.TabIndex = 15;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(196, 67);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(65, 13);
			this.label4.TabIndex = 5;
			this.label4.Text = "Mod version";
			// 
			// tbVersion
			// 
			this.tbVersion.Location = new System.Drawing.Point(12, 64);
			this.tbVersion.Name = "tbVersion";
			this.tbVersion.Size = new System.Drawing.Size(178, 20);
			this.tbVersion.TabIndex = 4;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(196, 93);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(101, 13);
			this.label5.TabIndex = 7;
			this.label5.Text = "Mod version (Script)";
			// 
			// bClearScreenshot
			// 
			this.bClearScreenshot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.bClearScreenshot.Location = new System.Drawing.Point(12, 453);
			this.bClearScreenshot.Name = "bClearScreenshot";
			this.bClearScreenshot.Size = new System.Drawing.Size(115, 23);
			this.bClearScreenshot.TabIndex = 19;
			this.bClearScreenshot.Text = "Clear screenshot";
			this.bClearScreenshot.UseVisualStyleBackColor = true;
			this.bClearScreenshot.Click += new System.EventHandler(this.bClearScreenshot_Click);
			// 
			// tbMVersion
			// 
			this.tbMVersion.Location = new System.Drawing.Point(12, 90);
			this.tbMVersion.Name = "tbMVersion";
			this.tbMVersion.Size = new System.Drawing.Size(178, 20);
			this.tbMVersion.TabIndex = 6;
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.Filter = "Image files|*.png;*.jpg;*.bmp";
			this.openFileDialog1.RestoreDirectory = true;
			// 
			// tbMinFommVersion
			// 
			this.tbMinFommVersion.Location = new System.Drawing.Point(12, 168);
			this.tbMinFommVersion.Name = "tbMinFommVersion";
			this.tbMinFommVersion.Size = new System.Drawing.Size(178, 20);
			this.tbMinFommVersion.TabIndex = 12;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(196, 171);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(130, 13);
			this.label6.TabIndex = 13;
			this.label6.Text = "Min required fomm version";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(196, 119);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(46, 13);
			this.label7.TabIndex = 9;
			this.label7.Text = "Website";
			// 
			// tbWebsite
			// 
			this.tbWebsite.Location = new System.Drawing.Point(12, 116);
			this.tbWebsite.Name = "tbWebsite";
			this.tbWebsite.Size = new System.Drawing.Size(178, 20);
			this.tbWebsite.TabIndex = 8;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(196, 145);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(71, 13);
			this.label8.TabIndex = 11;
			this.label8.Text = "Contact email";
			// 
			// tbEmail
			// 
			this.tbEmail.Location = new System.Drawing.Point(12, 142);
			this.tbEmail.Name = "tbEmail";
			this.tbEmail.Size = new System.Drawing.Size(178, 20);
			this.tbEmail.TabIndex = 10;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(12, 352);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(41, 13);
			this.label9.TabIndex = 16;
			this.label9.Text = "Groups";
			// 
			// clbGroups
			// 
			this.clbGroups.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.clbGroups.CheckOnClick = true;
			this.clbGroups.FormattingEnabled = true;
			this.clbGroups.IntegralHeight = false;
			this.clbGroups.Location = new System.Drawing.Point(12, 368);
			this.clbGroups.Name = "clbGroups";
			this.clbGroups.Size = new System.Drawing.Size(323, 50);
			this.clbGroups.TabIndex = 17;
			this.clbGroups.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clbGroups_ItemCheck);
			// 
			// InfoEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(347, 486);
			this.Controls.Add(this.clbGroups);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.tbEmail);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.tbWebsite);
			this.Controls.Add(this.tbMinFommVersion);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.tbMVersion);
			this.Controls.Add(this.bClearScreenshot);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.tbVersion);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.tbDescription);
			this.Controls.Add(this.bCancel);
			this.Controls.Add(this.bSave);
			this.Controls.Add(this.bScreenshot);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.tbAuthor);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.tbName);
			this.Name = "InfoEditor";
			this.Text = "InfoEditor";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.InfoEditor_FormClosing);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbAuthor;
        private System.Windows.Forms.Button bScreenshot;
        private System.Windows.Forms.Button bSave;
        private System.Windows.Forms.Button bCancel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbDescription;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbVersion;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button bClearScreenshot;
        private System.Windows.Forms.TextBox tbMVersion;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TextBox tbMinFommVersion;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbWebsite;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tbEmail;
        private System.Windows.Forms.Label label9;
		private System.Windows.Forms.CheckedListBox clbGroups;
    }
}