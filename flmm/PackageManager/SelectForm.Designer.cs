namespace Fomm.PackageManager {
    partial class SelectForm {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectForm));
            this.lbSelect = new System.Windows.Forms.ListBox();
            this.bOK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.bPreview = new System.Windows.Forms.Button();
            this.bDescription = new System.Windows.Forms.Button();
            this.tbDesc = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // lbSelect
            // 
            this.lbSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbSelect.FormattingEnabled = true;
            this.lbSelect.IntegralHeight = false;
            this.lbSelect.Location = new System.Drawing.Point(12, 32);
            this.lbSelect.Name = "lbSelect";
            this.lbSelect.Size = new System.Drawing.Size(238, 139);
            this.lbSelect.TabIndex = 0;
            this.lbSelect.SelectedIndexChanged += new System.EventHandler(this.lbSelect_SelectedIndexChanged);
            this.lbSelect.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lbSelect_KeyDown);
            // 
            // bOK
            // 
            this.bOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bOK.Location = new System.Drawing.Point(175, 177);
            this.bOK.Name = "bOK";
            this.bOK.Size = new System.Drawing.Size(75, 23);
            this.bOK.TabIndex = 1;
            this.bOK.Text = "OK";
            this.bOK.UseVisualStyleBackColor = true;
            this.bOK.Click += new System.EventHandler(this.bOK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Select one option";
            // 
            // bPreview
            // 
            this.bPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bPreview.Enabled = false;
            this.bPreview.Location = new System.Drawing.Point(93, 177);
            this.bPreview.Name = "bPreview";
            this.bPreview.Size = new System.Drawing.Size(75, 23);
            this.bPreview.TabIndex = 3;
            this.bPreview.Text = "Preview";
            this.bPreview.UseVisualStyleBackColor = true;
            this.bPreview.Click += new System.EventHandler(this.bPreview_Click);
            // 
            // bDescription
            // 
            this.bDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bDescription.Enabled = false;
            this.bDescription.Location = new System.Drawing.Point(12, 177);
            this.bDescription.Name = "bDescription";
            this.bDescription.Size = new System.Drawing.Size(75, 23);
            this.bDescription.TabIndex = 4;
            this.bDescription.Text = "Description";
            this.bDescription.UseVisualStyleBackColor = true;
            this.bDescription.Click += new System.EventHandler(this.bDescription_Click);
            // 
            // tbDesc
            // 
            this.tbDesc.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDesc.BackColor = System.Drawing.SystemColors.Window;
            this.tbDesc.DetectUrls = false;
            this.tbDesc.Location = new System.Drawing.Point(12, 32);
            this.tbDesc.Name = "tbDesc";
            this.tbDesc.ReadOnly = true;
            this.tbDesc.Size = new System.Drawing.Size(238, 139);
            this.tbDesc.TabIndex = 5;
            this.tbDesc.Text = "";
            this.tbDesc.Visible = false;
            // 
            // SelectForm
            // 
            this.AcceptButton = this.bOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bOK;
            this.ClientSize = new System.Drawing.Size(262, 212);
            this.ControlBox = false;
            this.Controls.Add(this.tbDesc);
            this.Controls.Add(this.bDescription);
            this.Controls.Add(this.bPreview);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.bOK);
            this.Controls.Add(this.lbSelect);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectForm";
            this.Text = "Select Form";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SelectForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lbSelect;
        private System.Windows.Forms.Button bOK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bPreview;
        private System.Windows.Forms.Button bDescription;
        private System.Windows.Forms.RichTextBox tbDesc;
    }
}