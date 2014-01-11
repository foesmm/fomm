namespace Fomm
{
	partial class GameModeSelector
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
			this.radFallout3 = new System.Windows.Forms.RadioButton();
			this.radFalloutNV = new System.Windows.Forms.RadioButton();
			this.cbxRemember = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.butOK = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// radFallout3
			// 
			this.radFallout3.AutoSize = true;
			this.radFallout3.Location = new System.Drawing.Point(19, 19);
			this.radFallout3.Name = "radFallout3";
			this.radFallout3.Size = new System.Drawing.Size(65, 17);
			this.radFallout3.TabIndex = 0;
			this.radFallout3.TabStop = true;
			this.radFallout3.Text = "Fallout 3";
			this.radFallout3.UseVisualStyleBackColor = true;
			// 
			// radFalloutNV
			// 
			this.radFalloutNV.AutoSize = true;
			this.radFalloutNV.Location = new System.Drawing.Point(19, 42);
			this.radFalloutNV.Name = "radFalloutNV";
			this.radFalloutNV.Size = new System.Drawing.Size(117, 17);
			this.radFalloutNV.TabIndex = 1;
			this.radFalloutNV.TabStop = true;
			this.radFalloutNV.Text = "Fallout: New Vegas";
			this.radFalloutNV.UseVisualStyleBackColor = true;
			// 
			// cbxRemember
			// 
			this.cbxRemember.AutoSize = true;
			this.cbxRemember.Location = new System.Drawing.Point(102, 86);
			this.cbxRemember.Name = "cbxRemember";
			this.cbxRemember.Size = new System.Drawing.Size(136, 17);
			this.cbxRemember.TabIndex = 1;
			this.cbxRemember.Text = "Don\'t ask me next time.";
			this.cbxRemember.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.radFallout3);
			this.groupBox1.Controls.Add(this.radFalloutNV);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(226, 68);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Select the game you would like to manage:";
			// 
			// butOK
			// 
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Location = new System.Drawing.Point(163, 112);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(75, 23);
			this.butOK.TabIndex = 2;
			this.butOK.Text = "OK";
			this.butOK.UseVisualStyleBackColor = true;
			this.butOK.Click += new System.EventHandler(this.butOK_Click);
			// 
			// GameModeSelector
			// 
			this.AcceptButton = this.butOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(250, 147);
			this.Controls.Add(this.butOK);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.cbxRemember);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "GameModeSelector";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Game Selection";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RadioButton radFallout3;
		private System.Windows.Forms.RadioButton radFalloutNV;
		private System.Windows.Forms.CheckBox cbxRemember;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button butOK;
	}
}