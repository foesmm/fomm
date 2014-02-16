namespace Fomm.PackageManager {
    partial class Overwriteform {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
      this.bYesToAll = new System.Windows.Forms.Button();
      this.bYesToFolder = new System.Windows.Forms.Button();
      this.bYes = new System.Windows.Forms.Button();
      this.bNoToAll = new System.Windows.Forms.Button();
      this.bNoToFolder = new System.Windows.Forms.Button();
      this.bNo = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.panel1 = new System.Windows.Forms.Panel();
      this.bNoToMod = new System.Windows.Forms.Button();
      this.bYesToMod = new System.Windows.Forms.Button();
      this.panel2 = new System.Windows.Forms.Panel();
      this.panel1.SuspendLayout();
      this.panel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // bYesToAll
      // 
      this.bYesToAll.Location = new System.Drawing.Point(411, 5);
      this.bYesToAll.Name = "bYesToAll";
      this.bYesToAll.Size = new System.Drawing.Size(75, 23);
      this.bYesToAll.TabIndex = 7;
      this.bYesToAll.Text = "Yes to all";
      this.bYesToAll.UseVisualStyleBackColor = true;
      this.bYesToAll.Click += new System.EventHandler(this.bYesToAll_Click);
      // 
      // bYesToFolder
      // 
      this.bYesToFolder.Location = new System.Drawing.Point(281, 5);
      this.bYesToFolder.Name = "bYesToFolder";
      this.bYesToFolder.Size = new System.Drawing.Size(75, 23);
      this.bYesToFolder.TabIndex = 5;
      this.bYesToFolder.Text = "Yes to folder";
      this.bYesToFolder.UseVisualStyleBackColor = true;
      this.bYesToFolder.Click += new System.EventHandler(this.bYesToFolder_Click);
      // 
      // bYes
      // 
      this.bYes.Location = new System.Drawing.Point(21, 5);
      this.bYes.Name = "bYes";
      this.bYes.Size = new System.Drawing.Size(75, 23);
      this.bYes.TabIndex = 1;
      this.bYes.Text = "Yes";
      this.bYes.UseVisualStyleBackColor = true;
      this.bYes.Click += new System.EventHandler(this.bYes_Click);
      // 
      // bNoToAll
      // 
      this.bNoToAll.Location = new System.Drawing.Point(411, 34);
      this.bNoToAll.Name = "bNoToAll";
      this.bNoToAll.Size = new System.Drawing.Size(75, 23);
      this.bNoToAll.TabIndex = 8;
      this.bNoToAll.Text = "No to all";
      this.bNoToAll.UseVisualStyleBackColor = true;
      this.bNoToAll.Click += new System.EventHandler(this.bNoToAll_Click);
      // 
      // bNoToFolder
      // 
      this.bNoToFolder.Location = new System.Drawing.Point(281, 34);
      this.bNoToFolder.Name = "bNoToFolder";
      this.bNoToFolder.Size = new System.Drawing.Size(75, 23);
      this.bNoToFolder.TabIndex = 6;
      this.bNoToFolder.Text = "No to folder";
      this.bNoToFolder.UseVisualStyleBackColor = true;
      this.bNoToFolder.Click += new System.EventHandler(this.bNoToFolder_Click);
      // 
      // bNo
      // 
      this.bNo.Location = new System.Drawing.Point(21, 34);
      this.bNo.Name = "bNo";
      this.bNo.Size = new System.Drawing.Size(75, 23);
      this.bNo.TabIndex = 2;
      this.bNo.Text = "No";
      this.bNo.UseVisualStyleBackColor = true;
      this.bNo.Click += new System.EventHandler(this.bNo_Click);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(11, 9);
      this.label1.MaximumSize = new System.Drawing.Size(483, 76);
      this.label1.MinimumSize = new System.Drawing.Size(70, 27);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(70, 27);
      this.label1.TabIndex = 7;
      this.label1.Text = "label1";
      // 
      // panel1
      // 
      this.panel1.AutoSize = true;
      this.panel1.Controls.Add(this.bNoToMod);
      this.panel1.Controls.Add(this.bYesToMod);
      this.panel1.Controls.Add(this.bNo);
      this.panel1.Controls.Add(this.bYesToAll);
      this.panel1.Controls.Add(this.bNoToFolder);
      this.panel1.Controls.Add(this.bYesToFolder);
      this.panel1.Controls.Add(this.bNoToAll);
      this.panel1.Controls.Add(this.bYes);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 98);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(506, 60);
      this.panel1.TabIndex = 8;
      // 
      // bNoToMod
      // 
      this.bNoToMod.Location = new System.Drawing.Point(151, 34);
      this.bNoToMod.Name = "bNoToMod";
      this.bNoToMod.Size = new System.Drawing.Size(75, 23);
      this.bNoToMod.TabIndex = 4;
      this.bNoToMod.Text = "No to mod";
      this.bNoToMod.UseVisualStyleBackColor = true;
      this.bNoToMod.Click += new System.EventHandler(this.bNoToMod_Click);
      // 
      // bYesToMod
      // 
      this.bYesToMod.Location = new System.Drawing.Point(151, 5);
      this.bYesToMod.Name = "bYesToMod";
      this.bYesToMod.Size = new System.Drawing.Size(75, 23);
      this.bYesToMod.TabIndex = 3;
      this.bYesToMod.Text = "Yes to mod";
      this.bYesToMod.UseVisualStyleBackColor = true;
      this.bYesToMod.Click += new System.EventHandler(this.bYesToMod_Click);
      // 
      // panel2
      // 
      this.panel2.AutoSize = true;
      this.panel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.panel2.Controls.Add(this.label1);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel2.Location = new System.Drawing.Point(0, 0);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(506, 98);
      this.panel2.TabIndex = 9;
      // 
      // Overwriteform
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.ClientSize = new System.Drawing.Size(506, 158);
      this.ControlBox = false;
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.KeyPreview = true;
      this.MinimumSize = new System.Drawing.Size(512, 28);
      this.Name = "Overwriteform";
      this.Text = "Confirm Overwrite";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Overwriteform_FormClosing);
      this.panel1.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.panel2.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bYesToAll;
        private System.Windows.Forms.Button bYesToFolder;
        private System.Windows.Forms.Button bYes;
        private System.Windows.Forms.Button bNoToAll;
        private System.Windows.Forms.Button bNoToFolder;
		private System.Windows.Forms.Button bNo;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.Button bNoToMod;
    private System.Windows.Forms.Button bYesToMod;
    }
}