namespace Fomm.Games.Fallout3.Settings
{
	partial class GeneralSettingsPage
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
			this.label1 = new System.Windows.Forms.Label();
			this.tbxModDirectory = new System.Windows.Forms.TextBox();
			this.butSelectModDirectory = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.tbxWorkingDirectory = new System.Windows.Forms.TextBox();
			this.butSelectWorkingDirectory = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.tbxCommandArguments = new System.Windows.Forms.TextBox();
			this.tbxCommand = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.fbdModDirectory = new System.Windows.Forms.FolderBrowserDialog();
			this.fbdWorkingDirectory = new System.Windows.Forms.FolderBrowserDialog();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(22, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(76, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Mod Directory:";
			// 
			// tbxModDirectory
			// 
			this.tbxModDirectory.Location = new System.Drawing.Point(104, 5);
			this.tbxModDirectory.Name = "tbxModDirectory";
			this.tbxModDirectory.Size = new System.Drawing.Size(257, 20);
			this.tbxModDirectory.TabIndex = 0;
			// 
			// butSelectModDirectory
			// 
			this.butSelectModDirectory.AutoSize = true;
			this.butSelectModDirectory.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.butSelectModDirectory.Location = new System.Drawing.Point(367, 3);
			this.butSelectModDirectory.Name = "butSelectModDirectory";
			this.butSelectModDirectory.Size = new System.Drawing.Size(26, 23);
			this.butSelectModDirectory.TabIndex = 1;
			this.butSelectModDirectory.Text = "...";
			this.butSelectModDirectory.UseVisualStyleBackColor = true;
			this.butSelectModDirectory.Click += new System.EventHandler(this.butSelectModDirectory_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 38);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(95, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Fallout 3 Directory:";
			// 
			// tbxWorkingDirectory
			// 
			this.tbxWorkingDirectory.Location = new System.Drawing.Point(104, 35);
			this.tbxWorkingDirectory.Name = "tbxWorkingDirectory";
			this.tbxWorkingDirectory.Size = new System.Drawing.Size(257, 20);
			this.tbxWorkingDirectory.TabIndex = 2;
			// 
			// butSelectWorkingDirectory
			// 
			this.butSelectWorkingDirectory.AutoSize = true;
			this.butSelectWorkingDirectory.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.butSelectWorkingDirectory.Location = new System.Drawing.Point(367, 32);
			this.butSelectWorkingDirectory.Name = "butSelectWorkingDirectory";
			this.butSelectWorkingDirectory.Size = new System.Drawing.Size(26, 23);
			this.butSelectWorkingDirectory.TabIndex = 3;
			this.butSelectWorkingDirectory.Text = "...";
			this.butSelectWorkingDirectory.UseVisualStyleBackColor = true;
			this.butSelectWorkingDirectory.Click += new System.EventHandler(this.butSelectWorkingDirectory_Click);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(16, 22);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(57, 13);
			this.label3.TabIndex = 6;
			this.label3.Text = "Command:";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.tbxCommandArguments);
			this.groupBox1.Controls.Add(this.tbxCommand);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Location = new System.Drawing.Point(25, 61);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(346, 78);
			this.groupBox1.TabIndex = 7;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Custom Launch Command";
			// 
			// tbxCommandArguments
			// 
			this.tbxCommandArguments.Location = new System.Drawing.Point(79, 45);
			this.tbxCommandArguments.Name = "tbxCommandArguments";
			this.tbxCommandArguments.Size = new System.Drawing.Size(257, 20);
			this.tbxCommandArguments.TabIndex = 5;
			// 
			// tbxCommand
			// 
			this.tbxCommand.Location = new System.Drawing.Point(79, 19);
			this.tbxCommand.Name = "tbxCommand";
			this.tbxCommand.Size = new System.Drawing.Size(257, 20);
			this.tbxCommand.TabIndex = 4;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(13, 48);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(60, 13);
			this.label4.TabIndex = 7;
			this.label4.Text = "Arguments:";
			// 
			// GeneralSettingsPage
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.butSelectWorkingDirectory);
			this.Controls.Add(this.tbxWorkingDirectory);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.butSelectModDirectory);
			this.Controls.Add(this.tbxModDirectory);
			this.Controls.Add(this.label1);
			this.Name = "GeneralSettingsPage";
			this.Size = new System.Drawing.Size(398, 151);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox tbxModDirectory;
		private System.Windows.Forms.Button butSelectModDirectory;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox tbxWorkingDirectory;
		private System.Windows.Forms.Button butSelectWorkingDirectory;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox tbxCommandArguments;
		private System.Windows.Forms.TextBox tbxCommand;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.FolderBrowserDialog fbdModDirectory;
		private System.Windows.Forms.FolderBrowserDialog fbdWorkingDirectory;

	}
}
