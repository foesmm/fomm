namespace Fomm.PackageManager.FomodBuilder
{
  partial class PremadeFomodPackForm
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
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.tbxPFP = new System.Windows.Forms.TextBox();
      this.tbxSources = new System.Windows.Forms.TextBox();
      this.butPFP = new System.Windows.Forms.Button();
      this.butSources = new System.Windows.Forms.Button();
      this.butOK = new System.Windows.Forms.Button();
      this.butCancel = new System.Windows.Forms.Button();
      this.fbdSources = new System.Windows.Forms.FolderBrowserDialog();
      this.ofdPFP = new System.Windows.Forms.OpenFileDialog();
      this.erpError = new System.Windows.Forms.ErrorProvider(this.components);
      ((System.ComponentModel.ISupportInitialize)(this.erpError)).BeginInit();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 9);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(118, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Premade FOMod Pack:";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(12, 48);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(100, 13);
      this.label2.TabIndex = 1;
      this.label2.Text = "Source Files Folder:";
      // 
      // tbxPFP
      // 
      this.tbxPFP.Location = new System.Drawing.Point(12, 25);
      this.tbxPFP.Name = "tbxPFP";
      this.tbxPFP.Size = new System.Drawing.Size(374, 20);
      this.tbxPFP.TabIndex = 0;
      // 
      // tbxSources
      // 
      this.tbxSources.Location = new System.Drawing.Point(12, 64);
      this.tbxSources.Name = "tbxSources";
      this.tbxSources.Size = new System.Drawing.Size(374, 20);
      this.tbxSources.TabIndex = 2;
      // 
      // butPFP
      // 
      this.butPFP.AutoSize = true;
      this.butPFP.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.butPFP.Location = new System.Drawing.Point(392, 23);
      this.butPFP.Name = "butPFP";
      this.butPFP.Size = new System.Drawing.Size(26, 23);
      this.butPFP.TabIndex = 1;
      this.butPFP.Text = "...";
      this.butPFP.UseVisualStyleBackColor = true;
      this.butPFP.Click += new System.EventHandler(this.butPFP_Click);
      // 
      // butSources
      // 
      this.butSources.AutoSize = true;
      this.butSources.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.butSources.Location = new System.Drawing.Point(392, 62);
      this.butSources.Name = "butSources";
      this.butSources.Size = new System.Drawing.Size(26, 23);
      this.butSources.TabIndex = 3;
      this.butSources.Text = "...";
      this.butSources.UseVisualStyleBackColor = true;
      this.butSources.Click += new System.EventHandler(this.butSources_Click);
      // 
      // butOK
      // 
      this.butOK.Location = new System.Drawing.Point(262, 101);
      this.butOK.Name = "butOK";
      this.butOK.Size = new System.Drawing.Size(75, 23);
      this.butOK.TabIndex = 4;
      this.butOK.Text = "OK";
      this.butOK.UseVisualStyleBackColor = true;
      this.butOK.Click += new System.EventHandler(this.butOK_Click);
      // 
      // butCancel
      // 
      this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.butCancel.Location = new System.Drawing.Point(343, 101);
      this.butCancel.Name = "butCancel";
      this.butCancel.Size = new System.Drawing.Size(75, 23);
      this.butCancel.TabIndex = 5;
      this.butCancel.Text = "Cancel";
      this.butCancel.UseVisualStyleBackColor = true;
      // 
      // ofdPFP
      // 
      this.ofdPFP.RestoreDirectory = true;
      // 
      // erpError
      // 
      this.erpError.ContainerControl = this;
      // 
      // PremadeFomodPackForm
      // 
      this.AcceptButton = this.butOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.butCancel;
      this.ClientSize = new System.Drawing.Size(436, 135);
      this.Controls.Add(this.butCancel);
      this.Controls.Add(this.butOK);
      this.Controls.Add(this.butSources);
      this.Controls.Add(this.butPFP);
      this.Controls.Add(this.tbxSources);
      this.Controls.Add(this.tbxPFP);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Name = "PremadeFomodPackForm";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Premade FOMod Pack";
      ((System.ComponentModel.ISupportInitialize)(this.erpError)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox tbxPFP;
    private System.Windows.Forms.TextBox tbxSources;
    private System.Windows.Forms.Button butPFP;
    private System.Windows.Forms.Button butSources;
    private System.Windows.Forms.Button butOK;
    private System.Windows.Forms.Button butCancel;
    private System.Windows.Forms.FolderBrowserDialog fbdSources;
    private System.Windows.Forms.OpenFileDialog ofdPFP;
    private System.Windows.Forms.ErrorProvider erpError;
  }
}