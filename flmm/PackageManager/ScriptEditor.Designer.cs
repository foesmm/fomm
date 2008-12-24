namespace fomm.PackageManager {
    partial class ScriptEditor {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScriptEditor));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.bSave = new System.Windows.Forms.ToolStripButton();
            this.bSyntaxCheck = new System.Windows.Forms.ToolStripButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tbScript = new ICSharpCode.TextEditor.TextEditorControl();
            this.toolStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bSave,
            this.bSyntaxCheck});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(503, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // bSave
            // 
            this.bSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.bSave.Image = ((System.Drawing.Image)(resources.GetObject("bSave.Image")));
            this.bSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bSave.Name = "bSave";
            this.bSave.Size = new System.Drawing.Size(35, 22);
            this.bSave.Text = "Save";
            this.bSave.Click += new System.EventHandler(this.bSave_Click);
            // 
            // bSyntaxCheck
            // 
            this.bSyntaxCheck.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.bSyntaxCheck.Image = ((System.Drawing.Image)(resources.GetObject("bSyntaxCheck.Image")));
            this.bSyntaxCheck.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bSyntaxCheck.Name = "bSyntaxCheck";
            this.bSyntaxCheck.Size = new System.Drawing.Size(76, 22);
            this.bSyntaxCheck.Text = "Check syntax";
            this.bSyntaxCheck.Click += new System.EventHandler(this.bSyntaxCheck_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tbScript);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 25);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(503, 399);
            this.panel1.TabIndex = 2;
            // 
            // tbScript
            // 
            this.tbScript.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbScript.IsReadOnly = false;
            this.tbScript.Location = new System.Drawing.Point(0, 0);
            this.tbScript.Name = "tbScript";
            this.tbScript.Size = new System.Drawing.Size(503, 399);
            this.tbScript.TabIndex = 1;
            // 
            // ScriptEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(503, 424);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "ScriptEditor";
            this.Text = "Script Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ScriptEditor_FormClosing);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton bSave;
        private System.Windows.Forms.ToolStripButton bSyntaxCheck;
        private System.Windows.Forms.Panel panel1;
        private ICSharpCode.TextEditor.TextEditorControl tbScript;
    }
}