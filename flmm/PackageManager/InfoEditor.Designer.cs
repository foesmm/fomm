namespace Fomm.PackageManager {
    partial class InfoEditor {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InfoEditor));
			this.butSave = new System.Windows.Forms.Button();
			this.butCancel = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.butEditReadme = new System.Windows.Forms.Button();
			this.finInfo = new Fomm.PackageManager.FomodInfo();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(163, 12);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 23);
			this.butSave.TabIndex = 0;
			this.butSave.Text = "Save";
			this.butSave.UseVisualStyleBackColor = true;
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// butCancel
			// 
			this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.butCancel.Location = new System.Drawing.Point(244, 12);
			this.butCancel.Name = "butCancel";
			this.butCancel.Size = new System.Drawing.Size(75, 23);
			this.butCancel.TabIndex = 1;
			this.butCancel.Text = "Cancel";
			this.butCancel.UseVisualStyleBackColor = true;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.butEditReadme);
			this.panel1.Controls.Add(this.butCancel);
			this.panel1.Controls.Add(this.butSave);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 401);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(331, 47);
			this.panel1.TabIndex = 23;
			// 
			// butEditReadme
			// 
			this.butEditReadme.Location = new System.Drawing.Point(12, 12);
			this.butEditReadme.Name = "butEditReadme";
			this.butEditReadme.Size = new System.Drawing.Size(78, 23);
			this.butEditReadme.TabIndex = 2;
			this.butEditReadme.Text = "Edit Readme";
			this.butEditReadme.UseVisualStyleBackColor = true;
			this.butEditReadme.Click += new System.EventHandler(this.butEditReadme_Click);
			// 
			// finInfo
			// 
			this.finInfo.Author = "";
			this.finInfo.AutoScroll = true;
			this.finInfo.Description = "";
			this.finInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.finInfo.Email = "";
			this.finInfo.Groups = new string[0];
			this.finInfo.HumanReadableVersion = "";
			this.finInfo.Location = new System.Drawing.Point(0, 0);
			this.finInfo.MachineVersion = ((System.Version)(resources.GetObject("finInfo.MachineVersion")));
			this.finInfo.MinFommVersion = ((System.Version)(resources.GetObject("finInfo.MinFommVersion")));
			this.finInfo.ModName = "";
			this.finInfo.Name = "finInfo";
			this.finInfo.Screenshot = null;
			this.finInfo.Size = new System.Drawing.Size(331, 401);
			this.finInfo.TabIndex = 24;
			this.finInfo.Website = "";
			// 
			// InfoEditor
			// 
			this.AcceptButton = this.butSave;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.butCancel;
			this.ClientSize = new System.Drawing.Size(331, 448);
			this.Controls.Add(this.finInfo);
			this.Controls.Add(this.panel1);
			this.Name = "InfoEditor";
			this.Text = "Info Editor";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.InfoEditor_FormClosing);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

		private System.Windows.Forms.Button butSave;
		private System.Windows.Forms.Button butCancel;
		private System.Windows.Forms.Panel panel1;
		private FomodInfo finInfo;
		private System.Windows.Forms.Button butEditReadme;
    }
}