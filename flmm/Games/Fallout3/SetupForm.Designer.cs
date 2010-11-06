namespace Fomm.Games.Fallout3
{
	partial class SetupForm
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
			this.panel1 = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.pnlShadow = new System.Windows.Forms.Panel();
			this.pnlLight = new System.Windows.Forms.Panel();
			this.wizSetup = new Fomm.Controls.WizardControl();
			this.vtpDirectories = new Fomm.Controls.VerticalTabPage();
			this.panel1.SuspendLayout();
			this.wizSetup.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.SystemColors.Window;
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.pnlShadow);
			this.panel1.Controls.Add(this.pnlLight);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(455, 38);
			this.panel1.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(125, 17);
			this.label1.TabIndex = 2;
			this.label1.Text = "Fallout 3 Setup";
			// 
			// pnlShadow
			// 
			this.pnlShadow.BackColor = System.Drawing.SystemColors.ControlDark;
			this.pnlShadow.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlShadow.Location = new System.Drawing.Point(0, 36);
			this.pnlShadow.Name = "pnlShadow";
			this.pnlShadow.Size = new System.Drawing.Size(455, 1);
			this.pnlShadow.TabIndex = 1;
			// 
			// pnlLight
			// 
			this.pnlLight.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.pnlLight.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlLight.Location = new System.Drawing.Point(0, 37);
			this.pnlLight.Name = "pnlLight";
			this.pnlLight.Size = new System.Drawing.Size(455, 1);
			this.pnlLight.TabIndex = 0;
			// 
			// wizSetup
			// 
			this.wizSetup.BackColor = System.Drawing.SystemColors.Control;
			this.wizSetup.Controls.Add(this.vtpDirectories);
			this.wizSetup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.wizSetup.Location = new System.Drawing.Point(0, 38);
			this.wizSetup.Name = "wizSetup";
			this.wizSetup.SelectedIndex = 0;
			this.wizSetup.SelectedTabPage = this.vtpDirectories;
			this.wizSetup.Size = new System.Drawing.Size(455, 307);
			this.wizSetup.TabIndex = 1;
			this.wizSetup.Text = "wizardControl1";
			this.wizSetup.Cancelled += new System.EventHandler(this.wizSetup_Cancelled);
			this.wizSetup.Finished += new System.EventHandler(this.wizSetup_Finished);
			this.wizSetup.SelectedTabPageChanged += new System.EventHandler<Fomm.Controls.VerticalTabControl.TabPageEventArgs>(this.wizSetup_SelectedTabPageChanged);
			// 
			// vtpDirectories
			// 
			this.vtpDirectories.BackColor = System.Drawing.SystemColors.Control;
			this.vtpDirectories.Dock = System.Windows.Forms.DockStyle.Fill;
			this.vtpDirectories.Location = new System.Drawing.Point(0, 0);
			this.vtpDirectories.Name = "vtpDirectories";
			this.vtpDirectories.PageIndex = 0;
			this.vtpDirectories.Size = new System.Drawing.Size(455, 307);
			this.vtpDirectories.TabIndex = 2;
			this.vtpDirectories.Text = "verticalTabPage1";
			// 
			// SetupForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(455, 345);
			this.Controls.Add(this.wizSetup);
			this.Controls.Add(this.panel1);
			this.Name = "SetupForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Fallout 3 Setup";
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.wizSetup.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel pnlLight;
		private System.Windows.Forms.Panel pnlShadow;
		private System.Windows.Forms.Label label1;
		private Fomm.Controls.WizardControl wizSetup;
		private Fomm.Controls.VerticalTabPage vtpDirectories;





	}
}