namespace Fomm.PackageManager.FomodBuilder
{
  partial class ReadmeGeneratorForm
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
      this.rfsSelector = new Fomm.PackageManager.FomodBuilder.ReadmeFileSelector();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.butCancel);
      this.panel1.Controls.Add(this.butOK);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 296);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(418, 42);
      this.panel1.TabIndex = 0;
      // 
      // butCancel
      // 
      this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.butCancel.Location = new System.Drawing.Point(331, 7);
      this.butCancel.Name = "butCancel";
      this.butCancel.Size = new System.Drawing.Size(75, 23);
      this.butCancel.TabIndex = 1;
      this.butCancel.Text = "Cancel";
      this.butCancel.UseVisualStyleBackColor = true;
      // 
      // butOK
      // 
      this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.butOK.Location = new System.Drawing.Point(250, 7);
      this.butOK.Name = "butOK";
      this.butOK.Size = new System.Drawing.Size(75, 23);
      this.butOK.TabIndex = 0;
      this.butOK.Text = "OK";
      this.butOK.UseVisualStyleBackColor = true;
      this.butOK.Click += new System.EventHandler(this.butOK_Click);
      // 
      // rfsSelector
      // 
      this.rfsSelector.Dock = System.Windows.Forms.DockStyle.Fill;
      this.rfsSelector.Location = new System.Drawing.Point(0, 0);
      this.rfsSelector.Name = "rfsSelector";
      this.rfsSelector.Size = new System.Drawing.Size(418, 296);
      this.rfsSelector.TabIndex = 1;
      // 
      // ReadmeGeneratorForm
      // 
      this.AcceptButton = this.butOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.butCancel;
      this.ClientSize = new System.Drawing.Size(418, 338);
      this.Controls.Add(this.rfsSelector);
      this.Controls.Add(this.panel1);
      this.Name = "ReadmeGeneratorForm";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Readme Generator";
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button butCancel;
    private System.Windows.Forms.Button butOK;
    private ReadmeFileSelector rfsSelector;
  }
}