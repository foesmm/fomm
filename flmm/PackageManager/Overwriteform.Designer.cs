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
            this.SuspendLayout();
            // 
            // bYesToAll
            // 
            this.bYesToAll.Location = new System.Drawing.Point(12, 73);
            this.bYesToAll.Name = "bYesToAll";
            this.bYesToAll.Size = new System.Drawing.Size(75, 23);
            this.bYesToAll.TabIndex = 0;
            this.bYesToAll.Text = "Yes to all";
            this.bYesToAll.UseVisualStyleBackColor = true;
            this.bYesToAll.Click += new System.EventHandler(this.bYesToAll_Click);
            // 
            // bYesToFolder
            // 
            this.bYesToFolder.Location = new System.Drawing.Point(93, 73);
            this.bYesToFolder.Name = "bYesToFolder";
            this.bYesToFolder.Size = new System.Drawing.Size(75, 23);
            this.bYesToFolder.TabIndex = 1;
            this.bYesToFolder.Text = "Yes to folder";
            this.bYesToFolder.UseVisualStyleBackColor = true;
            this.bYesToFolder.Click += new System.EventHandler(this.bYesToFolder_Click);
            // 
            // bYes
            // 
            this.bYes.Location = new System.Drawing.Point(174, 73);
            this.bYes.Name = "bYes";
            this.bYes.Size = new System.Drawing.Size(75, 23);
            this.bYes.TabIndex = 2;
            this.bYes.Text = "Yes";
            this.bYes.UseVisualStyleBackColor = true;
            this.bYes.Click += new System.EventHandler(this.bYes_Click);
            // 
            // bNoToAll
            // 
            this.bNoToAll.Location = new System.Drawing.Point(255, 73);
            this.bNoToAll.Name = "bNoToAll";
            this.bNoToAll.Size = new System.Drawing.Size(75, 23);
            this.bNoToAll.TabIndex = 3;
            this.bNoToAll.Text = "No to all";
            this.bNoToAll.UseVisualStyleBackColor = true;
            this.bNoToAll.Click += new System.EventHandler(this.bNoToAll_Click);
            // 
            // bNoToFolder
            // 
            this.bNoToFolder.Location = new System.Drawing.Point(336, 73);
            this.bNoToFolder.Name = "bNoToFolder";
            this.bNoToFolder.Size = new System.Drawing.Size(75, 23);
            this.bNoToFolder.TabIndex = 4;
            this.bNoToFolder.Text = "No to folder";
            this.bNoToFolder.UseVisualStyleBackColor = true;
            this.bNoToFolder.Click += new System.EventHandler(this.bNoToFolder_Click);
            // 
            // bNo
            // 
            this.bNo.Location = new System.Drawing.Point(417, 73);
            this.bNo.Name = "bNo";
            this.bNo.Size = new System.Drawing.Size(75, 23);
            this.bNo.TabIndex = 5;
            this.bNo.Text = "No";
            this.bNo.UseVisualStyleBackColor = true;
            this.bNo.Click += new System.EventHandler(this.bNo_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "label1";
            // 
            // Overwriteform
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(506, 108);
            this.ControlBox = false;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.bNo);
            this.Controls.Add(this.bNoToFolder);
            this.Controls.Add(this.bNoToAll);
            this.Controls.Add(this.bYes);
            this.Controls.Add(this.bYesToFolder);
            this.Controls.Add(this.bYesToAll);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Overwriteform";
            this.Text = "Overwriteform";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Overwriteform_FormClosing);
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
    }
}