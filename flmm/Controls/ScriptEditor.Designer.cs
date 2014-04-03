namespace Fomm.Controls
{
  partial class ScriptEditor
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScriptEditor));
      this.toolStrip1 = new System.Windows.Forms.ToolStrip();
      this.tsbCheckSyntax = new System.Windows.Forms.ToolStripButton();
      this.cedEditor = new Fomm.Controls.CodeEditor();
      this.cedEditor.SetHighlighting("C#");
      this.toolStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // toolStrip1
      // 
      this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbCheckSyntax});
      this.toolStrip1.Location = new System.Drawing.Point(0, 0);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.Size = new System.Drawing.Size(482, 25);
      this.toolStrip1.TabIndex = 0;
      this.toolStrip1.Text = "toolStrip1";
      // 
      // tsbCheckSyntax
      // 
      this.tsbCheckSyntax.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.tsbCheckSyntax.Image = ((System.Drawing.Image)(resources.GetObject("tsbCheckSyntax.Image")));
      this.tsbCheckSyntax.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.tsbCheckSyntax.Name = "tsbCheckSyntax";
      this.tsbCheckSyntax.Size = new System.Drawing.Size(81, 22);
      this.tsbCheckSyntax.Text = "Check Syntax";
      this.tsbCheckSyntax.Click += new System.EventHandler(this.tsbCheckSyntax_Click);
      // 
      // cedEditor
      // 
      this.cedEditor.Dock = System.Windows.Forms.DockStyle.Fill;
      this.cedEditor.IsReadOnly = false;
      this.cedEditor.Location = new System.Drawing.Point(0, 25);
      this.cedEditor.Name = "cedEditor";
      this.cedEditor.Size = new System.Drawing.Size(482, 319);
      this.cedEditor.TabIndex = 1;
      // 
      // ScriptEditor
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.cedEditor);
      this.Controls.Add(this.toolStrip1);
      this.Name = "ScriptEditor";
      this.Size = new System.Drawing.Size(482, 344);
      this.toolStrip1.ResumeLayout(false);
      this.toolStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ToolStrip toolStrip1;
    private System.Windows.Forms.ToolStripButton tsbCheckSyntax;
    private CodeEditor cedEditor;
  }
}
