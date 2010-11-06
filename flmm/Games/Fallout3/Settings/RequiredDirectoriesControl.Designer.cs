namespace Fomm.Games.Fallout3.Settings
{
	partial class RequiredDirectoriesControl
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
			this.butSelectInfoDirectory = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.tbxInstallInfo = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.butSelectModDirectory = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.tbxModDirectory = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.erpValidator = new System.Windows.Forms.ErrorProvider(this.components);
			this.fbdDirectory = new System.Windows.Forms.FolderBrowserDialog();
			((System.ComponentModel.ISupportInitialize)(this.erpValidator)).BeginInit();
			this.SuspendLayout();
			// 
			// butSelectInfoDirectory
			// 
			this.butSelectInfoDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butSelectInfoDirectory.AutoSize = true;
			this.butSelectInfoDirectory.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.butSelectInfoDirectory.Location = new System.Drawing.Point(394, 57);
			this.butSelectInfoDirectory.Name = "butSelectInfoDirectory";
			this.butSelectInfoDirectory.Size = new System.Drawing.Size(26, 23);
			this.butSelectInfoDirectory.TabIndex = 12;
			this.butSelectInfoDirectory.Text = "...";
			this.butSelectInfoDirectory.UseVisualStyleBackColor = true;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(3, 43);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(362, 13);
			this.label4.TabIndex = 14;
			this.label4.Text = "Choose the directory where you would like to store your Fallout 3 install info.";
			// 
			// tbxInstallInfo
			// 
			this.tbxInstallInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbxInstallInfo.Location = new System.Drawing.Point(102, 59);
			this.tbxInstallInfo.Name = "tbxInstallInfo";
			this.tbxInstallInfo.Size = new System.Drawing.Size(286, 20);
			this.tbxInstallInfo.TabIndex = 10;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(38, 62);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(58, 13);
			this.label5.TabIndex = 13;
			this.label5.Text = "Install Info:";
			// 
			// butSelectModDirectory
			// 
			this.butSelectModDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butSelectModDirectory.AutoSize = true;
			this.butSelectModDirectory.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.butSelectModDirectory.Location = new System.Drawing.Point(394, 14);
			this.butSelectModDirectory.Name = "butSelectModDirectory";
			this.butSelectModDirectory.Size = new System.Drawing.Size(26, 23);
			this.butSelectModDirectory.TabIndex = 9;
			this.butSelectModDirectory.Text = "...";
			this.butSelectModDirectory.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(3, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(356, 13);
			this.label3.TabIndex = 11;
			this.label3.Text = "Choose the directory where you would like to store your Fallout 3 FOMods.";
			// 
			// tbxModDirectory
			// 
			this.tbxModDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbxModDirectory.Location = new System.Drawing.Point(102, 16);
			this.tbxModDirectory.Name = "tbxModDirectory";
			this.tbxModDirectory.Size = new System.Drawing.Size(286, 20);
			this.tbxModDirectory.TabIndex = 7;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(20, 19);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(76, 13);
			this.label2.TabIndex = 8;
			this.label2.Text = "Mod Directory:";
			// 
			// erpValidator
			// 
			this.erpValidator.ContainerControl = this;
			// 
			// RequiredDirectoriesControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.butSelectInfoDirectory);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.tbxInstallInfo);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.butSelectModDirectory);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.tbxModDirectory);
			this.Controls.Add(this.label2);
			this.Name = "RequiredDirectoriesControl";
			this.Size = new System.Drawing.Size(443, 405);
			((System.ComponentModel.ISupportInitialize)(this.erpValidator)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button butSelectInfoDirectory;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox tbxInstallInfo;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button butSelectModDirectory;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox tbxModDirectory;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ErrorProvider erpValidator;
		private System.Windows.Forms.FolderBrowserDialog fbdDirectory;
	}
}
