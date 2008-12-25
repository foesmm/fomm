namespace fomm.PackageManager {
    partial class PackageManager {
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lvModList = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.tbModInfo = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.bEditScript = new System.Windows.Forms.Button();
            this.bEditReadme = new System.Windows.Forms.Button();
            this.bEditInfo = new System.Windows.Forms.Button();
            this.bActivate = new System.Windows.Forms.Button();
            this.bAddNew = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 12);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lvModList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tbModInfo);
            this.splitContainer1.Size = new System.Drawing.Size(344, 322);
            this.splitContainer1.SplitterDistance = 197;
            this.splitContainer1.TabIndex = 18;
            // 
            // lvModList
            // 
            this.lvModList.AutoArrange = false;
            this.lvModList.CheckBoxes = true;
            this.lvModList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.lvModList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvModList.FullRowSelect = true;
            this.lvModList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvModList.HideSelection = false;
            this.lvModList.Location = new System.Drawing.Point(0, 0);
            this.lvModList.MultiSelect = false;
            this.lvModList.Name = "lvModList";
            this.lvModList.Size = new System.Drawing.Size(344, 197);
            this.lvModList.TabIndex = 0;
            this.lvModList.UseCompatibleStateImageBehavior = false;
            this.lvModList.View = System.Windows.Forms.View.Details;
            this.lvModList.SelectedIndexChanged += new System.EventHandler(this.lvModList_SelectedIndexChanged);
            this.lvModList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lvModList_ItemCheck);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 219;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Version";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Author";
            // 
            // tbModInfo
            // 
            this.tbModInfo.BackColor = System.Drawing.SystemColors.Window;
            this.tbModInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbModInfo.Location = new System.Drawing.Point(0, 0);
            this.tbModInfo.Multiline = true;
            this.tbModInfo.Name = "tbModInfo";
            this.tbModInfo.ReadOnly = true;
            this.tbModInfo.Size = new System.Drawing.Size(344, 121);
            this.tbModInfo.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox1.Location = new System.Drawing.Point(362, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(120, 90);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 16;
            this.pictureBox1.TabStop = false;
            // 
            // bEditScript
            // 
            this.bEditScript.Location = new System.Drawing.Point(362, 224);
            this.bEditScript.Name = "bEditScript";
            this.bEditScript.Size = new System.Drawing.Size(120, 23);
            this.bEditScript.TabIndex = 20;
            this.bEditScript.Text = "Edit script";
            this.bEditScript.UseVisualStyleBackColor = true;
            this.bEditScript.Click += new System.EventHandler(this.bEditScript_Click);
            // 
            // bEditReadme
            // 
            this.bEditReadme.Location = new System.Drawing.Point(362, 195);
            this.bEditReadme.Name = "bEditReadme";
            this.bEditReadme.Size = new System.Drawing.Size(120, 23);
            this.bEditReadme.TabIndex = 21;
            this.bEditReadme.Text = "View readme";
            this.bEditReadme.UseVisualStyleBackColor = true;
            this.bEditReadme.Click += new System.EventHandler(this.bEditReadme_Click);
            // 
            // bEditInfo
            // 
            this.bEditInfo.Location = new System.Drawing.Point(362, 253);
            this.bEditInfo.Name = "bEditInfo";
            this.bEditInfo.Size = new System.Drawing.Size(120, 23);
            this.bEditInfo.TabIndex = 22;
            this.bEditInfo.Text = "Edit info";
            this.bEditInfo.UseVisualStyleBackColor = true;
            this.bEditInfo.Click += new System.EventHandler(this.bEditInfo_Click);
            // 
            // bActivate
            // 
            this.bActivate.Location = new System.Drawing.Point(362, 108);
            this.bActivate.Name = "bActivate";
            this.bActivate.Size = new System.Drawing.Size(120, 23);
            this.bActivate.TabIndex = 23;
            this.bActivate.Text = "Activate";
            this.bActivate.UseVisualStyleBackColor = true;
            this.bActivate.Click += new System.EventHandler(this.bActivate_Click);
            // 
            // bAddNew
            // 
            this.bAddNew.Location = new System.Drawing.Point(362, 137);
            this.bAddNew.Name = "bAddNew";
            this.bAddNew.Size = new System.Drawing.Size(120, 23);
            this.bAddNew.TabIndex = 24;
            this.bAddNew.Text = "Add new";
            this.bAddNew.UseVisualStyleBackColor = true;
            this.bAddNew.Click += new System.EventHandler(this.bAddNew_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "mod archive (*.fomod, *.zip)|*.fomod;*.zip";
            this.openFileDialog1.RestoreDirectory = true;
            // 
            // PackageManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(494, 346);
            this.Controls.Add(this.bAddNew);
            this.Controls.Add(this.bActivate);
            this.Controls.Add(this.bEditInfo);
            this.Controls.Add(this.bEditReadme);
            this.Controls.Add(this.bEditScript);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.pictureBox1);
            this.Name = "PackageManager";
            this.Text = "PackageManager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PackageManager_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView lvModList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.TextBox tbModInfo;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Button bEditScript;
        private System.Windows.Forms.Button bEditReadme;
        private System.Windows.Forms.Button bEditInfo;
        private System.Windows.Forms.Button bActivate;
        private System.Windows.Forms.Button bAddNew;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}