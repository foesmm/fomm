namespace Fomm.PackageManager.Controls
{
  partial class FomodScriptEditor
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
      this.ddtScript = new Fomm.Controls.DropDownTabControl();
      this.dtpCSharp = new Fomm.Controls.DropDownTabPage();
      this.sedScript = new Fomm.Controls.ScriptEditor();
      this.dtpXML = new Fomm.Controls.DropDownTabPage();
      this.xedScript = new Fomm.Controls.XmlEditor();
      this.xedScript.SetHighlighting("XML");
      this.panel4 = new System.Windows.Forms.Panel();
      this.cbxVersion = new System.Windows.Forms.ComboBox();
      this.label9 = new System.Windows.Forms.Label();
      this.ddtScript.SuspendLayout();
      this.dtpCSharp.SuspendLayout();
      this.dtpXML.SuspendLayout();
      this.panel4.SuspendLayout();
      this.SuspendLayout();
      // 
      // ddtScript
      // 
      this.ddtScript.BackColor = System.Drawing.SystemColors.Control;
      this.ddtScript.Controls.Add(this.dtpXML);
      this.ddtScript.Controls.Add(this.dtpCSharp);
      this.ddtScript.Dock = System.Windows.Forms.DockStyle.Fill;
      this.ddtScript.Location = new System.Drawing.Point(0, 0);
      this.ddtScript.Name = "ddtScript";
      this.ddtScript.SelectedIndex = 0;
      this.ddtScript.SelectedTabPage = this.dtpCSharp;
      this.ddtScript.Size = new System.Drawing.Size(448, 398);
      this.ddtScript.TabIndex = 2;
      this.ddtScript.TabWidth = 121;
      this.ddtScript.Text = "Script Type:";
      // 
      // dtpCSharp
      // 
      this.dtpCSharp.BackColor = System.Drawing.SystemColors.Control;
      this.dtpCSharp.Controls.Add(this.sedScript);
      this.dtpCSharp.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dtpCSharp.Location = new System.Drawing.Point(0, 45);
      this.dtpCSharp.Name = "dtpCSharp";
      this.dtpCSharp.Padding = new System.Windows.Forms.Padding(3);
      this.dtpCSharp.PageIndex = 0;
      this.dtpCSharp.Size = new System.Drawing.Size(448, 353);
      this.dtpCSharp.TabIndex = 1;
      this.dtpCSharp.Text = "C#";
      // 
      // sedScript
      // 
      this.sedScript.Dock = System.Windows.Forms.DockStyle.Fill;
      this.sedScript.Location = new System.Drawing.Point(3, 3);
      this.sedScript.Name = "sedScript";
      this.sedScript.Size = new System.Drawing.Size(442, 347);
      this.sedScript.TabIndex = 0;
      // 
      // dtpXML
      // 
      this.dtpXML.BackColor = System.Drawing.SystemColors.Control;
      this.dtpXML.Controls.Add(this.xedScript);
      this.dtpXML.Controls.Add(this.panel4);
      this.dtpXML.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dtpXML.Location = new System.Drawing.Point(0, 45);
      this.dtpXML.Name = "dtpXML";
      this.dtpXML.Padding = new System.Windows.Forms.Padding(3);
      this.dtpXML.PageIndex = 1;
      this.dtpXML.Size = new System.Drawing.Size(448, 353);
      this.dtpXML.TabIndex = 2;
      this.dtpXML.Text = "XML Config";
      // 
      // xedScript
      // 
      this.xedScript.Dock = System.Windows.Forms.DockStyle.Fill;
      this.xedScript.IsReadOnly = false;
      this.xedScript.Location = new System.Drawing.Point(3, 37);
      this.xedScript.Name = "xedScript";
      this.xedScript.Size = new System.Drawing.Size(442, 313);
      this.xedScript.TabIndex = 1;
      this.xedScript.GotAutoCompleteList += new System.EventHandler<Fomm.Controls.RegeneratableAutoCompleteListEventArgs>(xedScript_GotAutoCompleteList);
      // 
      // panel4
      // 
      this.panel4.Controls.Add(this.cbxVersion);
      this.panel4.Controls.Add(this.label9);
      this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel4.Location = new System.Drawing.Point(3, 3);
      this.panel4.Name = "panel4";
      this.panel4.Size = new System.Drawing.Size(442, 34);
      this.panel4.TabIndex = 0;
      // 
      // cbxVersion
      // 
      this.cbxVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbxVersion.FormattingEnabled = true;
      this.cbxVersion.Location = new System.Drawing.Point(151, 6);
      this.cbxVersion.Name = "cbxVersion";
      this.cbxVersion.Size = new System.Drawing.Size(121, 21);
      this.cbxVersion.TabIndex = 1;
      this.cbxVersion.SelectedIndexChanged += new System.EventHandler(this.cbxVersion_SelectedIndexChanged);
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(10, 9);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(135, 13);
      this.label9.TabIndex = 0;
      this.label9.Text = "XML Configuration Version:";
      // 
      // FomodScriptEditor
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.ddtScript);
      this.Name = "FomodScriptEditor";
      this.Size = new System.Drawing.Size(448, 398);
      this.ddtScript.ResumeLayout(false);
      this.dtpCSharp.ResumeLayout(false);
      this.dtpXML.ResumeLayout(false);
      this.panel4.ResumeLayout(false);
      this.panel4.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private Fomm.Controls.DropDownTabControl ddtScript;
    private Fomm.Controls.DropDownTabPage dtpCSharp;
    private Fomm.Controls.ScriptEditor sedScript;
    private Fomm.Controls.DropDownTabPage dtpXML;
    private Fomm.Controls.XmlEditor xedScript;
    private System.Windows.Forms.Panel panel4;
    private System.Windows.Forms.ComboBox cbxVersion;
    private System.Windows.Forms.Label label9;
  }
}
