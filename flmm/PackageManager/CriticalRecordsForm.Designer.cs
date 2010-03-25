namespace Fomm.PackageManager
{
	partial class CriticalRecordsForm
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
			this.butCancel = new System.Windows.Forms.Button();
			this.butOK = new System.Windows.Forms.Button();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.tvwRecords = new System.Windows.Forms.TreeView();
			this.label1 = new System.Windows.Forms.Label();
			this.tbxReason = new System.Windows.Forms.TextBox();
			this.ckbIsCritical = new System.Windows.Forms.CheckBox();
			this.panel2 = new System.Windows.Forms.Panel();
			this.label2 = new System.Windows.Forms.Label();
			this.tbxInstalledName = new System.Windows.Forms.TextBox();
			this.panel1.SuspendLayout();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.butCancel);
			this.panel1.Controls.Add(this.butOK);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 490);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(527, 46);
			this.panel1.TabIndex = 1;
			// 
			// butCancel
			// 
			this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.butCancel.Location = new System.Drawing.Point(440, 11);
			this.butCancel.Name = "butCancel";
			this.butCancel.Size = new System.Drawing.Size(75, 23);
			this.butCancel.TabIndex = 1;
			this.butCancel.Text = "Cancel";
			this.butCancel.UseVisualStyleBackColor = true;
			this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
			// 
			// butOK
			// 
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Location = new System.Drawing.Point(359, 11);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(75, 23);
			this.butOK.TabIndex = 0;
			this.butOK.Text = "OK";
			this.butOK.UseVisualStyleBackColor = true;
			this.butOK.Click += new System.EventHandler(this.butOK_Click);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.tvwRecords);
			this.splitContainer1.Panel1.Controls.Add(this.panel2);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.label1);
			this.splitContainer1.Panel2.Controls.Add(this.tbxReason);
			this.splitContainer1.Panel2.Controls.Add(this.ckbIsCritical);
			this.splitContainer1.Panel2.Leave += new System.EventHandler(this.splitContainer1_Panel2_Leave);
			this.splitContainer1.Size = new System.Drawing.Size(527, 490);
			this.splitContainer1.SplitterDistance = 380;
			this.splitContainer1.TabIndex = 0;
			// 
			// tvwRecords
			// 
			this.tvwRecords.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tvwRecords.HideSelection = false;
			this.tvwRecords.Location = new System.Drawing.Point(0, 0);
			this.tvwRecords.Name = "tvwRecords";
			this.tvwRecords.Size = new System.Drawing.Size(527, 351);
			this.tvwRecords.TabIndex = 0;
			this.tvwRecords.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvwRecords_AfterSelect);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 23);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(172, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Reason for being a Critical Record:";
			// 
			// tbxReason
			// 
			this.tbxReason.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbxReason.Location = new System.Drawing.Point(12, 39);
			this.tbxReason.Multiline = true;
			this.tbxReason.Name = "tbxReason";
			this.tbxReason.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tbxReason.Size = new System.Drawing.Size(503, 61);
			this.tbxReason.TabIndex = 1;
			// 
			// ckbIsCritical
			// 
			this.ckbIsCritical.AutoSize = true;
			this.ckbIsCritical.Location = new System.Drawing.Point(12, 3);
			this.ckbIsCritical.Name = "ckbIsCritical";
			this.ckbIsCritical.Size = new System.Drawing.Size(57, 17);
			this.ckbIsCritical.TabIndex = 0;
			this.ckbIsCritical.Text = "Critical";
			this.ckbIsCritical.UseVisualStyleBackColor = true;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.tbxInstalledName);
			this.panel2.Controls.Add(this.label2);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel2.Location = new System.Drawing.Point(0, 351);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(527, 29);
			this.panel2.TabIndex = 1;
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 9);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(112, 13);
			this.label2.TabIndex = 0;
			this.label2.Text = "Installed Plugin Name:";
			// 
			// tbxInstalledName
			// 
			this.tbxInstalledName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbxInstalledName.Location = new System.Drawing.Point(130, 6);
			this.tbxInstalledName.Name = "tbxInstalledName";
			this.tbxInstalledName.Size = new System.Drawing.Size(385, 20);
			this.tbxInstalledName.TabIndex = 1;
			this.tbxInstalledName.TextChanged += new System.EventHandler(this.tbxInstalledName_TextChanged);
			// 
			// CriticalRecordsForm
			// 
			this.AcceptButton = this.butOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.butCancel;
			this.ClientSize = new System.Drawing.Size(527, 536);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.panel1);
			this.Name = "CriticalRecordsForm";
			this.Text = "CriticalRecordsForm";
			this.panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			this.splitContainer1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.TreeView tvwRecords;
		private System.Windows.Forms.Button butCancel;
		private System.Windows.Forms.Button butOK;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox tbxReason;
		private System.Windows.Forms.CheckBox ckbIsCritical;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.TextBox tbxInstalledName;
		private System.Windows.Forms.Label label2;
	}
}