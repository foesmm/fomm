namespace Fomm.Games.Fallout3.Tools.TESsnip
{
  partial class AddMasterForm
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
      this.label1 = new System.Windows.Forms.Label();
      this.tbxMaster = new System.Windows.Forms.TextBox();
      this.butChooseMaster = new System.Windows.Forms.Button();
      this.butOK = new System.Windows.Forms.Button();
      this.butCancel = new System.Windows.Forms.Button();
      this.ofdChooseMaster = new System.Windows.Forms.OpenFileDialog();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 15);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(42, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Master:";
      // 
      // tbxMaster
      // 
      this.tbxMaster.Location = new System.Drawing.Point(60, 12);
      this.tbxMaster.Name = "tbxMaster";
      this.tbxMaster.Size = new System.Drawing.Size(387, 20);
      this.tbxMaster.TabIndex = 1;
      // 
      // butChooseMaster
      // 
      this.butChooseMaster.AutoSize = true;
      this.butChooseMaster.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.butChooseMaster.Location = new System.Drawing.Point(453, 10);
      this.butChooseMaster.Name = "butChooseMaster";
      this.butChooseMaster.Size = new System.Drawing.Size(26, 23);
      this.butChooseMaster.TabIndex = 2;
      this.butChooseMaster.Text = "...";
      this.butChooseMaster.UseVisualStyleBackColor = true;
      this.butChooseMaster.Click += new System.EventHandler(this.butChooseMaster_Click);
      // 
      // butOK
      // 
      this.butOK.Location = new System.Drawing.Point(323, 39);
      this.butOK.Name = "butOK";
      this.butOK.Size = new System.Drawing.Size(75, 23);
      this.butOK.TabIndex = 3;
      this.butOK.Text = "OK";
      this.butOK.UseVisualStyleBackColor = true;
      this.butOK.Click += new System.EventHandler(this.butOK_Click);
      // 
      // butCancel
      // 
      this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.butCancel.Location = new System.Drawing.Point(404, 39);
      this.butCancel.Name = "butCancel";
      this.butCancel.Size = new System.Drawing.Size(75, 23);
      this.butCancel.TabIndex = 4;
      this.butCancel.Text = "Cancel";
      this.butCancel.UseVisualStyleBackColor = true;
      // 
      // ofdChooseMaster
      // 
      this.ofdChooseMaster.DefaultExt = "esp";
      this.ofdChooseMaster.FileName = "openFileDialog1";
      this.ofdChooseMaster.Filter = "Plugin files|*.esp|Master files|*.esm|All files|*.*";
      // 
      // AddMasterForm
      // 
      this.AcceptButton = this.butOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.butCancel;
      this.ClientSize = new System.Drawing.Size(491, 73);
      this.Controls.Add(this.butCancel);
      this.Controls.Add(this.butOK);
      this.Controls.Add(this.butChooseMaster);
      this.Controls.Add(this.tbxMaster);
      this.Controls.Add(this.label1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "AddMasterForm";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Add Master";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox tbxMaster;
    private System.Windows.Forms.Button butChooseMaster;
    private System.Windows.Forms.Button butOK;
    private System.Windows.Forms.Button butCancel;
    private System.Windows.Forms.OpenFileDialog ofdChooseMaster;
  }
}