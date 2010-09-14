namespace Fomm.PackageManager
{
	partial class FomodInfoControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.clbGroups = new System.Windows.Forms.CheckedListBox();
			this.label9 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.tbEmail = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.tbWebsite = new System.Windows.Forms.TextBox();
			this.tbMinFommVersion = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.tbMVersion = new System.Windows.Forms.TextBox();
			this.butClearScreenshot = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.tbVersion = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.tbDescription = new System.Windows.Forms.TextBox();
			this.butSetScreenshot = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.tbAuthor = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.tbName = new System.Windows.Forms.TextBox();
			this.pbxScreenshot = new System.Windows.Forms.PictureBox();
			this.ofdScreenshot = new System.Windows.Forms.OpenFileDialog();
			this.erpErrors = new System.Windows.Forms.ErrorProvider(this.components);
			((System.ComponentModel.ISupportInitialize)(this.pbxScreenshot)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.erpErrors)).BeginInit();
			this.SuspendLayout();
			// 
			// clbGroups
			// 
			this.clbGroups.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.clbGroups.CheckOnClick = true;
			this.clbGroups.FormattingEnabled = true;
			this.clbGroups.IntegralHeight = false;
			this.clbGroups.Location = new System.Drawing.Point(3, 359);
			this.clbGroups.Name = "clbGroups";
			this.clbGroups.Size = new System.Drawing.Size(364, 142);
			this.clbGroups.TabIndex = 40;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(0, 343);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(41, 13);
			this.label9.TabIndex = 39;
			this.label9.Text = "Groups";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(51, 136);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(75, 13);
			this.label8.TabIndex = 34;
			this.label8.Text = "Contact Email:";
			// 
			// tbEmail
			// 
			this.tbEmail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbEmail.Location = new System.Drawing.Point(132, 133);
			this.tbEmail.Name = "tbEmail";
			this.tbEmail.Size = new System.Drawing.Size(219, 20);
			this.tbEmail.TabIndex = 33;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(77, 110);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(49, 13);
			this.label7.TabIndex = 32;
			this.label7.Text = "Website:";
			// 
			// tbWebsite
			// 
			this.tbWebsite.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbWebsite.Location = new System.Drawing.Point(132, 107);
			this.tbWebsite.Name = "tbWebsite";
			this.tbWebsite.Size = new System.Drawing.Size(219, 20);
			this.tbWebsite.TabIndex = 31;
			this.tbWebsite.Validating += new System.ComponentModel.CancelEventHandler(this.tbWebsite_Validating);
			// 
			// tbMinFommVersion
			// 
			this.tbMinFommVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbMinFommVersion.Location = new System.Drawing.Point(132, 159);
			this.tbMinFommVersion.Name = "tbMinFommVersion";
			this.tbMinFommVersion.Size = new System.Drawing.Size(219, 20);
			this.tbMinFommVersion.TabIndex = 35;
			this.tbMinFommVersion.Validating += new System.ComponentModel.CancelEventHandler(this.tbMinFommVersion_Validating);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(0, 162);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(126, 13);
			this.label6.TabIndex = 36;
			this.label6.Text = "Required FOMM Version:";
			// 
			// tbMVersion
			// 
			this.tbMVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbMVersion.Location = new System.Drawing.Point(132, 81);
			this.tbMVersion.Name = "tbMVersion";
			this.tbMVersion.Size = new System.Drawing.Size(219, 20);
			this.tbMVersion.TabIndex = 29;
			this.tbMVersion.Validating += new System.ComponentModel.CancelEventHandler(this.tbMVersion_Validating);
			// 
			// butClearScreenshot
			// 
			this.butClearScreenshot.Location = new System.Drawing.Point(127, 507);
			this.butClearScreenshot.Name = "butClearScreenshot";
			this.butClearScreenshot.Size = new System.Drawing.Size(115, 23);
			this.butClearScreenshot.TabIndex = 42;
			this.butClearScreenshot.Text = "Clear Screenshot";
			this.butClearScreenshot.UseVisualStyleBackColor = true;
			this.butClearScreenshot.Click += new System.EventHandler(this.butClearScreenshot_Click);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(21, 84);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(105, 13);
			this.label5.TabIndex = 30;
			this.label5.Text = "Mod Version (Script):";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(57, 58);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(69, 13);
			this.label4.TabIndex = 28;
			this.label4.Text = "Mod Version:";
			// 
			// tbVersion
			// 
			this.tbVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbVersion.Location = new System.Drawing.Point(132, 55);
			this.tbVersion.Name = "tbVersion";
			this.tbVersion.Size = new System.Drawing.Size(219, 20);
			this.tbVersion.TabIndex = 27;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(0, 182);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(84, 13);
			this.label3.TabIndex = 37;
			this.label3.Text = "Mod Description";
			// 
			// tbDescription
			// 
			this.tbDescription.AcceptsReturn = true;
			this.tbDescription.AcceptsTab = true;
			this.tbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbDescription.Location = new System.Drawing.Point(3, 198);
			this.tbDescription.Multiline = true;
			this.tbDescription.Name = "tbDescription";
			this.tbDescription.Size = new System.Drawing.Size(364, 142);
			this.tbDescription.TabIndex = 38;
			// 
			// butSetScreenshot
			// 
			this.butSetScreenshot.Location = new System.Drawing.Point(6, 507);
			this.butSetScreenshot.Name = "butSetScreenshot";
			this.butSetScreenshot.Size = new System.Drawing.Size(115, 23);
			this.butSetScreenshot.TabIndex = 41;
			this.butSetScreenshot.Text = "Set Screenshot";
			this.butSetScreenshot.UseVisualStyleBackColor = true;
			this.butSetScreenshot.Click += new System.EventHandler(this.butSetScreenshot_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(61, 32);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(65, 13);
			this.label2.TabIndex = 26;
			this.label2.Text = "Mod Author:";
			// 
			// tbAuthor
			// 
			this.tbAuthor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbAuthor.Location = new System.Drawing.Point(132, 29);
			this.tbAuthor.Name = "tbAuthor";
			this.tbAuthor.Size = new System.Drawing.Size(219, 20);
			this.tbAuthor.TabIndex = 25;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(64, 6);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(62, 13);
			this.label1.TabIndex = 24;
			this.label1.Text = "Mod Name:";
			// 
			// tbName
			// 
			this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbName.Location = new System.Drawing.Point(132, 3);
			this.tbName.Name = "tbName";
			this.tbName.Size = new System.Drawing.Size(219, 20);
			this.tbName.TabIndex = 23;
			// 
			// pbxScreenshot
			// 
			this.pbxScreenshot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.pbxScreenshot.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pbxScreenshot.Location = new System.Drawing.Point(3, 536);
			this.pbxScreenshot.MinimumSize = new System.Drawing.Size(2, 150);
			this.pbxScreenshot.Name = "pbxScreenshot";
			this.pbxScreenshot.Size = new System.Drawing.Size(364, 150);
			this.pbxScreenshot.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pbxScreenshot.TabIndex = 43;
			this.pbxScreenshot.TabStop = false;
			// 
			// ofdScreenshot
			// 
			this.ofdScreenshot.Filter = "Image files|*.png;*.jpg;*.bmp";
			this.ofdScreenshot.RestoreDirectory = true;
			// 
			// erpErrors
			// 
			this.erpErrors.ContainerControl = this;
			// 
			// FomodInfo
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.Controls.Add(this.pbxScreenshot);
			this.Controls.Add(this.clbGroups);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.tbEmail);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.tbWebsite);
			this.Controls.Add(this.tbMinFommVersion);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.tbMVersion);
			this.Controls.Add(this.butClearScreenshot);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.tbVersion);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.tbDescription);
			this.Controls.Add(this.butSetScreenshot);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.tbAuthor);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.tbName);
			this.Name = "FomodInfo";
			this.Size = new System.Drawing.Size(371, 689);
			((System.ComponentModel.ISupportInitialize)(this.pbxScreenshot)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.erpErrors)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.CheckedListBox clbGroups;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox tbEmail;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox tbWebsite;
		private System.Windows.Forms.TextBox tbMinFommVersion;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox tbMVersion;
		private System.Windows.Forms.Button butClearScreenshot;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox tbVersion;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox tbDescription;
		private System.Windows.Forms.Button butSetScreenshot;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox tbAuthor;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox tbName;
		private System.Windows.Forms.PictureBox pbxScreenshot;
		private System.Windows.Forms.OpenFileDialog ofdScreenshot;
		private System.Windows.Forms.ErrorProvider erpErrors;

	}
}
