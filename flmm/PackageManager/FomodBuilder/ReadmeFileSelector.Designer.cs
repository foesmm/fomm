namespace Fomm.PackageManager.FomodBuilder
{
  partial class ReadmeFileSelector
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
      this.panel1 = new System.Windows.Forms.Panel();
      this.lnkHelp = new System.Windows.Forms.LinkLabel();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.panel2 = new System.Windows.Forms.Panel();
      this.label2 = new System.Windows.Forms.Label();
      this.rtbHelp = new System.Windows.Forms.RichTextBox();
      this.panel3 = new System.Windows.Forms.Panel();
      this.label1 = new System.Windows.Forms.Label();
      this.cbxFormat = new System.Windows.Forms.ComboBox();
      this.sftSources = new Fomm.PackageManager.FomodBuilder.SourceFileTree();
      this.lvwReadmeFiles = new L0ki.Controls.ReordableItemListView();
      this.chdFileName = new System.Windows.Forms.ColumnHeader();
      this.panel1.SuspendLayout();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.panel2.SuspendLayout();
      this.panel3.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.lnkHelp);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel1.Location = new System.Drawing.Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(486, 20);
      this.panel1.TabIndex = 0;
      // 
      // lnkHelp
      // 
      this.lnkHelp.AutoSize = true;
      this.lnkHelp.Location = new System.Drawing.Point(3, 3);
      this.lnkHelp.Name = "lnkHelp";
      this.lnkHelp.Size = new System.Drawing.Size(58, 13);
      this.lnkHelp.TabIndex = 0;
      this.lnkHelp.TabStop = true;
      this.lnkHelp.Text = "Open Help";
      this.lnkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkHelp_LinkClicked);
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.IsSplitterFixed = true;
      this.splitContainer1.Location = new System.Drawing.Point(0, 120);
      this.splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.sftSources);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.lvwReadmeFiles);
      this.splitContainer1.Panel2.Controls.Add(this.panel2);
      this.splitContainer1.Size = new System.Drawing.Size(486, 256);
      this.splitContainer1.SplitterDistance = 238;
      this.splitContainer1.SplitterWidth = 10;
      this.splitContainer1.TabIndex = 2;
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.label2);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel2.Location = new System.Drawing.Point(0, 0);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(238, 20);
      this.panel2.TabIndex = 1;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(3, 3);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(83, 13);
      this.label2.TabIndex = 0;
      this.label2.Text = "Readme Files";
      // 
      // rtbHelp
      // 
      this.rtbHelp.BackColor = System.Drawing.SystemColors.Control;
      this.rtbHelp.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.rtbHelp.Dock = System.Windows.Forms.DockStyle.Top;
      this.rtbHelp.Location = new System.Drawing.Point(0, 20);
      this.rtbHelp.Name = "rtbHelp";
      this.rtbHelp.ReadOnly = true;
      this.rtbHelp.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
      this.rtbHelp.Size = new System.Drawing.Size(486, 100);
      this.rtbHelp.TabIndex = 1;
      this.rtbHelp.TabStop = false;
      this.rtbHelp.Text = "";
      // 
      // panel3
      // 
      this.panel3.Controls.Add(this.label1);
      this.panel3.Controls.Add(this.cbxFormat);
      this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel3.Location = new System.Drawing.Point(0, 376);
      this.panel3.Name = "panel3";
      this.panel3.Size = new System.Drawing.Size(486, 33);
      this.panel3.TabIndex = 3;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(3, 9);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(85, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Readme Format:";
      // 
      // cbxFormat
      // 
      this.cbxFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbxFormat.FormattingEnabled = true;
      this.cbxFormat.Location = new System.Drawing.Point(94, 6);
      this.cbxFormat.Name = "cbxFormat";
      this.cbxFormat.Size = new System.Drawing.Size(121, 21);
      this.cbxFormat.TabIndex = 0;
      // 
      // sftSources
      // 
      this.sftSources.Cursor = System.Windows.Forms.Cursors.Default;
      this.sftSources.Dock = System.Windows.Forms.DockStyle.Fill;
      this.sftSources.Location = new System.Drawing.Point(0, 0);
      this.sftSources.Name = "sftSources";
      this.sftSources.Size = new System.Drawing.Size(238, 256);
      this.sftSources.Sources = new string[0];
      this.sftSources.TabIndex = 0;
      // 
      // lvwReadmeFiles
      // 
      this.lvwReadmeFiles.AllowDrop = true;
      this.lvwReadmeFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chdFileName});
      this.lvwReadmeFiles.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lvwReadmeFiles.FullRowSelect = true;
      this.lvwReadmeFiles.Location = new System.Drawing.Point(0, 20);
      this.lvwReadmeFiles.Name = "lvwReadmeFiles";
      this.lvwReadmeFiles.ShowGroups = false;
      this.lvwReadmeFiles.Size = new System.Drawing.Size(238, 236);
      this.lvwReadmeFiles.TabIndex = 0;
      this.lvwReadmeFiles.UseCompatibleStateImageBehavior = false;
      this.lvwReadmeFiles.View = System.Windows.Forms.View.Details;
      this.lvwReadmeFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.lvwReadmeFiles_DragDrop);
      this.lvwReadmeFiles.DragOver += new System.Windows.Forms.DragEventHandler(this.lvwReadmeFiles_DragOver);
      // 
      // chdFileName
      // 
      this.chdFileName.Text = "File";
      this.chdFileName.Width = 200;
      // 
      // ReadmeFileSelector
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.splitContainer1);
      this.Controls.Add(this.panel3);
      this.Controls.Add(this.rtbHelp);
      this.Controls.Add(this.panel1);
      this.Name = "ReadmeFileSelector";
      this.Size = new System.Drawing.Size(486, 409);
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.panel2.PerformLayout();
      this.panel3.ResumeLayout(false);
      this.panel3.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private SourceFileTree sftSources;
    private System.Windows.Forms.LinkLabel lnkHelp;
    private L0ki.Controls.ReordableItemListView lvwReadmeFiles;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.RichTextBox rtbHelp;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ComboBox cbxFormat;
    private System.Windows.Forms.ColumnHeader chdFileName;
  }
}
