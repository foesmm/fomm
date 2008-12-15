namespace fomm {
    partial class UtilitiesOnlyForm {
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
            this.bBSAUnpack = new System.Windows.Forms.Button();
            this.cBSACreator = new System.Windows.Forms.Button();
            this.bTESsnip = new System.Windows.Forms.Button();
            this.bShaderEdit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // bBSAUnpack
            // 
            this.bBSAUnpack.Location = new System.Drawing.Point(12, 12);
            this.bBSAUnpack.Name = "bBSAUnpack";
            this.bBSAUnpack.Size = new System.Drawing.Size(120, 23);
            this.bBSAUnpack.TabIndex = 0;
            this.bBSAUnpack.Text = "BSA unpacker";
            this.bBSAUnpack.UseVisualStyleBackColor = true;
            this.bBSAUnpack.Click += new System.EventHandler(this.bBSAUnpack_Click);
            // 
            // cBSACreator
            // 
            this.cBSACreator.Location = new System.Drawing.Point(12, 41);
            this.cBSACreator.Name = "cBSACreator";
            this.cBSACreator.Size = new System.Drawing.Size(120, 23);
            this.cBSACreator.TabIndex = 1;
            this.cBSACreator.Text = "BSA creator";
            this.cBSACreator.UseVisualStyleBackColor = true;
            this.cBSACreator.Click += new System.EventHandler(this.cBSACreator_Click);
            // 
            // bTESsnip
            // 
            this.bTESsnip.Location = new System.Drawing.Point(12, 70);
            this.bTESsnip.Name = "bTESsnip";
            this.bTESsnip.Size = new System.Drawing.Size(120, 23);
            this.bTESsnip.TabIndex = 2;
            this.bTESsnip.Text = "Plugin editor";
            this.bTESsnip.UseVisualStyleBackColor = true;
            this.bTESsnip.Click += new System.EventHandler(this.bTESsnip_Click);
            // 
            // bShaderEdit
            // 
            this.bShaderEdit.Location = new System.Drawing.Point(12, 99);
            this.bShaderEdit.Name = "bShaderEdit";
            this.bShaderEdit.Size = new System.Drawing.Size(120, 23);
            this.bShaderEdit.TabIndex = 3;
            this.bShaderEdit.Text = "Shader editor";
            this.bShaderEdit.UseVisualStyleBackColor = true;
            this.bShaderEdit.Click += new System.EventHandler(this.bShaderEdit_Click);
            // 
            // UtilitiesOnlyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(164, 141);
            this.Controls.Add(this.bShaderEdit);
            this.Controls.Add(this.bTESsnip);
            this.Controls.Add(this.cBSACreator);
            this.Controls.Add(this.bBSAUnpack);
            this.Name = "UtilitiesOnlyForm";
            this.Text = "fomm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UtilitiesOnlyForm_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button bBSAUnpack;
        private System.Windows.Forms.Button cBSACreator;
        private System.Windows.Forms.Button bTESsnip;
        private System.Windows.Forms.Button bShaderEdit;
    }
}

