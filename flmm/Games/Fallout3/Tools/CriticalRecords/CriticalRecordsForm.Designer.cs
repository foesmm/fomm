namespace Fomm.Games.Fallout3.Tools.CriticalRecords
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
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.tvwRecords = new System.Windows.Forms.TreeView();
      this.cbxSeverity = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.tbxReason = new System.Windows.Forms.TextBox();
      this.ckbIsCritical = new System.Windows.Forms.CheckBox();
      this.menuStrip1 = new System.Windows.Forms.MenuStrip();
      this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.openNewPluginToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.closeAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.OpenModDialog = new System.Windows.Forms.OpenFileDialog();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.menuStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      this.splitContainer1.Location = new System.Drawing.Point(0, 24);
      this.splitContainer1.Name = "splitContainer1";
      this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.tvwRecords);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.cbxSeverity);
      this.splitContainer1.Panel2.Controls.Add(this.label2);
      this.splitContainer1.Panel2.Controls.Add(this.label1);
      this.splitContainer1.Panel2.Controls.Add(this.tbxReason);
      this.splitContainer1.Panel2.Controls.Add(this.ckbIsCritical);
      this.splitContainer1.Size = new System.Drawing.Size(527, 512);
      this.splitContainer1.SplitterDistance = 390;
      this.splitContainer1.TabIndex = 0;
      // 
      // tvwRecords
      // 
      this.tvwRecords.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tvwRecords.HideSelection = false;
      this.tvwRecords.Location = new System.Drawing.Point(0, 0);
      this.tvwRecords.Name = "tvwRecords";
      this.tvwRecords.Size = new System.Drawing.Size(527, 390);
      this.tvwRecords.TabIndex = 0;
      this.tvwRecords.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvwRecords_AfterSelect);
      // 
      // cbxSeverity
      // 
      this.cbxSeverity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbxSeverity.FormattingEnabled = true;
      this.cbxSeverity.Location = new System.Drawing.Point(394, 3);
      this.cbxSeverity.Name = "cbxSeverity";
      this.cbxSeverity.Size = new System.Drawing.Size(121, 21);
      this.cbxSeverity.TabIndex = 1;
      this.cbxSeverity.SelectedIndexChanged += new System.EventHandler(this.criticalInfoChanged);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(340, 6);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(48, 13);
      this.label2.TabIndex = 3;
      this.label2.Text = "Severity:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(9, 25);
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
      this.tbxReason.Location = new System.Drawing.Point(12, 41);
      this.tbxReason.Multiline = true;
      this.tbxReason.Name = "tbxReason";
      this.tbxReason.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.tbxReason.Size = new System.Drawing.Size(503, 65);
      this.tbxReason.TabIndex = 2;
      this.tbxReason.TextChanged += new System.EventHandler(this.criticalInfoChanged);
      // 
      // ckbIsCritical
      // 
      this.ckbIsCritical.AutoSize = true;
      this.ckbIsCritical.Location = new System.Drawing.Point(12, 5);
      this.ckbIsCritical.Name = "ckbIsCritical";
      this.ckbIsCritical.Size = new System.Drawing.Size(57, 17);
      this.ckbIsCritical.TabIndex = 0;
      this.ckbIsCritical.Text = "Critical";
      this.ckbIsCritical.UseVisualStyleBackColor = true;
      this.ckbIsCritical.CheckedChanged += new System.EventHandler(this.criticalInfoChanged);
      // 
      // menuStrip1
      // 
      this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
      this.menuStrip1.Location = new System.Drawing.Point(0, 0);
      this.menuStrip1.Name = "menuStrip1";
      this.menuStrip1.Size = new System.Drawing.Size(527, 24);
      this.menuStrip1.TabIndex = 2;
      this.menuStrip1.Text = "menuStrip1";
      // 
      // fileToolStripMenuItem
      // 
      this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openNewPluginToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.closeToolStripMenuItem,
            this.closeAllToolStripMenuItem});
      this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
      this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
      this.fileToolStripMenuItem.Text = "File";
      // 
      // openNewPluginToolStripMenuItem
      // 
      this.openNewPluginToolStripMenuItem.Name = "openNewPluginToolStripMenuItem";
      this.openNewPluginToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
      this.openNewPluginToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
      this.openNewPluginToolStripMenuItem.Text = "Open";
      this.openNewPluginToolStripMenuItem.Click += new System.EventHandler(this.openNewPluginToolStripMenuItem_Click);
      // 
      // saveToolStripMenuItem
      // 
      this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
      this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
      this.saveToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
      this.saveToolStripMenuItem.Text = "Save";
      this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
      // 
      // closeToolStripMenuItem
      // 
      this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
      this.closeToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
      this.closeToolStripMenuItem.Text = "Close";
      this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
      // 
      // closeAllToolStripMenuItem
      // 
      this.closeAllToolStripMenuItem.Name = "closeAllToolStripMenuItem";
      this.closeAllToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
      this.closeAllToolStripMenuItem.Text = "Close all";
      this.closeAllToolStripMenuItem.Click += new System.EventHandler(this.closeAllToolStripMenuItem_Click);
      // 
      // OpenModDialog
      // 
      this.OpenModDialog.Filter = "Fallout 3 plugin (*.esm, *.esp)|*.esm;*.esp";
      this.OpenModDialog.Multiselect = true;
      this.OpenModDialog.RestoreDirectory = true;
      this.OpenModDialog.Title = "Select plugin(s) to open";
      // 
      // CriticalRecordsForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(527, 536);
      this.Controls.Add(this.splitContainer1);
      this.Controls.Add(this.menuStrip1);
      this.Name = "CriticalRecordsForm";
      this.Text = "Critical Records Editor (CREditor)";
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.Panel2.PerformLayout();
      this.splitContainer1.ResumeLayout(false);
      this.menuStrip1.ResumeLayout(false);
      this.menuStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.TreeView tvwRecords;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox tbxReason;
    private System.Windows.Forms.CheckBox ckbIsCritical;
    private System.Windows.Forms.ComboBox cbxSeverity;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.MenuStrip menuStrip1;
    private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem openNewPluginToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem closeAllToolStripMenuItem;
    private System.Windows.Forms.OpenFileDialog OpenModDialog;
  }
}