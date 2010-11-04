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
			this.components = new System.ComponentModel.Container();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.pnlShadow = new System.Windows.Forms.Panel();
			this.pnlLight = new System.Windows.Forms.Panel();
			this.wizSetup = new Fomm.Controls.WizardControl();
			this.vtpDirectories = new Fomm.Controls.VerticalTabPage();
			this.butModDirectory = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.tbxModDirectory = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.erpValidator = new System.Windows.Forms.ErrorProvider(this.components);
			this.fbdModDirectory = new System.Windows.Forms.FolderBrowserDialog();
			this.panel1.SuspendLayout();
			this.wizSetup.SuspendLayout();
			this.vtpDirectories.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.erpValidator)).BeginInit();
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
			this.vtpDirectories.Controls.Add(this.butModDirectory);
			this.vtpDirectories.Controls.Add(this.label3);
			this.vtpDirectories.Controls.Add(this.tbxModDirectory);
			this.vtpDirectories.Controls.Add(this.label2);
			this.vtpDirectories.Dock = System.Windows.Forms.DockStyle.Fill;
			this.vtpDirectories.Location = new System.Drawing.Point(0, 0);
			this.vtpDirectories.Name = "vtpDirectories";
			this.vtpDirectories.PageIndex = 0;
			this.vtpDirectories.Size = new System.Drawing.Size(455, 307);
			this.vtpDirectories.TabIndex = 2;
			this.vtpDirectories.Text = "verticalTabPage1";
			// 
			// butModDirectory
			// 
			this.butModDirectory.AutoSize = true;
			this.butModDirectory.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.butModDirectory.Location = new System.Drawing.Point(403, 17);
			this.butModDirectory.Name = "butModDirectory";
			this.butModDirectory.Size = new System.Drawing.Size(26, 23);
			this.butModDirectory.TabIndex = 1;
			this.butModDirectory.Text = "...";
			this.butModDirectory.UseVisualStyleBackColor = true;
			this.butModDirectory.Click += new System.EventHandler(this.butModDirectory_Click);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 3);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(356, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "Choose the directory where you would like to store your Fallout 3 FOMods.";
			// 
			// tbxModDirectory
			// 
			this.tbxModDirectory.Location = new System.Drawing.Point(111, 19);
			this.tbxModDirectory.Name = "tbxModDirectory";
			this.tbxModDirectory.Size = new System.Drawing.Size(286, 20);
			this.tbxModDirectory.TabIndex = 0;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(29, 22);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(76, 13);
			this.label2.TabIndex = 0;
			this.label2.Text = "Mod Directory:";
			// 
			// erpValidator
			// 
			this.erpValidator.ContainerControl = this;
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
			this.vtpDirectories.ResumeLayout(false);
			this.vtpDirectories.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.erpValidator)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel pnlLight;
		private System.Windows.Forms.Panel pnlShadow;
		private System.Windows.Forms.Label label1;
		private Fomm.Controls.WizardControl wizSetup;
		private Fomm.Controls.VerticalTabPage vtpDirectories;
		private System.Windows.Forms.Button butModDirectory;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox tbxModDirectory;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ErrorProvider erpValidator;
		private System.Windows.Forms.FolderBrowserDialog fbdModDirectory;





	}
}