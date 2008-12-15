namespace fomm {
    partial class MainForm {
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.bLaunch = new System.Windows.Forms.Button();
            this.lvEspList = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tbPluginInfo = new System.Windows.Forms.TextBox();
            this.bPackageManager = new System.Windows.Forms.Button();
            this.bEnableAI = new System.Windows.Forms.Button();
            this.bSaveGames = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // bBSAUnpack
            // 
            this.bBSAUnpack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bBSAUnpack.Location = new System.Drawing.Point(336, 137);
            this.bBSAUnpack.Name = "bBSAUnpack";
            this.bBSAUnpack.Size = new System.Drawing.Size(120, 23);
            this.bBSAUnpack.TabIndex = 0;
            this.bBSAUnpack.Text = "BSA unpacker";
            this.bBSAUnpack.UseVisualStyleBackColor = true;
            this.bBSAUnpack.Click += new System.EventHandler(this.bBSAUnpack_Click);
            // 
            // cBSACreator
            // 
            this.cBSACreator.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cBSACreator.Location = new System.Drawing.Point(336, 166);
            this.cBSACreator.Name = "cBSACreator";
            this.cBSACreator.Size = new System.Drawing.Size(120, 23);
            this.cBSACreator.TabIndex = 1;
            this.cBSACreator.Text = "BSA creator";
            this.cBSACreator.UseVisualStyleBackColor = true;
            this.cBSACreator.Click += new System.EventHandler(this.cBSACreator_Click);
            // 
            // bTESsnip
            // 
            this.bTESsnip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bTESsnip.Location = new System.Drawing.Point(336, 195);
            this.bTESsnip.Name = "bTESsnip";
            this.bTESsnip.Size = new System.Drawing.Size(120, 23);
            this.bTESsnip.TabIndex = 2;
            this.bTESsnip.Text = "Plugin editor";
            this.bTESsnip.UseVisualStyleBackColor = true;
            this.bTESsnip.Click += new System.EventHandler(this.bTESsnip_Click);
            // 
            // bShaderEdit
            // 
            this.bShaderEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bShaderEdit.Location = new System.Drawing.Point(336, 224);
            this.bShaderEdit.Name = "bShaderEdit";
            this.bShaderEdit.Size = new System.Drawing.Size(120, 23);
            this.bShaderEdit.TabIndex = 3;
            this.bShaderEdit.Text = "Shader editor";
            this.bShaderEdit.UseVisualStyleBackColor = true;
            this.bShaderEdit.Click += new System.EventHandler(this.bShaderEdit_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox1.Location = new System.Drawing.Point(336, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(120, 90);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // bLaunch
            // 
            this.bLaunch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bLaunch.Location = new System.Drawing.Point(336, 108);
            this.bLaunch.Name = "bLaunch";
            this.bLaunch.Size = new System.Drawing.Size(120, 23);
            this.bLaunch.TabIndex = 5;
            this.bLaunch.Text = "Launch Fallout";
            this.bLaunch.UseVisualStyleBackColor = true;
            this.bLaunch.Click += new System.EventHandler(this.bLaunch_Click);
            // 
            // lvEspList
            // 
            this.lvEspList.AllowDrop = true;
            this.lvEspList.AutoArrange = false;
            this.lvEspList.CheckBoxes = true;
            this.lvEspList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lvEspList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvEspList.Location = new System.Drawing.Point(0, 0);
            this.lvEspList.MultiSelect = false;
            this.lvEspList.Name = "lvEspList";
            this.lvEspList.Size = new System.Drawing.Size(318, 198);
            this.lvEspList.TabIndex = 0;
            this.lvEspList.UseCompatibleStateImageBehavior = false;
            this.lvEspList.View = System.Windows.Forms.View.Details;
            this.lvEspList.SelectedIndexChanged += new System.EventHandler(this.lvEspList_SelectedIndexChanged);
            this.lvEspList.DragDrop += new System.Windows.Forms.DragEventHandler(this.lvEspList_DragDrop);
            this.lvEspList.DragEnter += new System.Windows.Forms.DragEventHandler(this.lvEspList_DragEnter);
            this.lvEspList.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.lvEspList_GiveFeedback);
            this.lvEspList.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.lvEspList_ItemDrag);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "File";
            this.columnHeader1.Width = 219;
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
            this.splitContainer1.Panel1.Controls.Add(this.lvEspList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tbPluginInfo);
            this.splitContainer1.Size = new System.Drawing.Size(318, 323);
            this.splitContainer1.SplitterDistance = 198;
            this.splitContainer1.TabIndex = 6;
            // 
            // tbPluginInfo
            // 
            this.tbPluginInfo.BackColor = System.Drawing.SystemColors.Window;
            this.tbPluginInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbPluginInfo.Location = new System.Drawing.Point(0, 0);
            this.tbPluginInfo.Multiline = true;
            this.tbPluginInfo.Name = "tbPluginInfo";
            this.tbPluginInfo.ReadOnly = true;
            this.tbPluginInfo.Size = new System.Drawing.Size(318, 121);
            this.tbPluginInfo.TabIndex = 0;
            this.tbPluginInfo.Text = "Drag/Drop to modify load order";
            // 
            // bPackageManager
            // 
            this.bPackageManager.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bPackageManager.Location = new System.Drawing.Point(336, 253);
            this.bPackageManager.Name = "bPackageManager";
            this.bPackageManager.Size = new System.Drawing.Size(120, 23);
            this.bPackageManager.TabIndex = 9;
            this.bPackageManager.Text = "Package manager";
            this.bPackageManager.UseVisualStyleBackColor = true;
            this.bPackageManager.Click += new System.EventHandler(this.bPackageManager_Click);
            // 
            // bEnableAI
            // 
            this.bEnableAI.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bEnableAI.Location = new System.Drawing.Point(336, 282);
            this.bEnableAI.Name = "bEnableAI";
            this.bEnableAI.Size = new System.Drawing.Size(120, 23);
            this.bEnableAI.TabIndex = 10;
            this.bEnableAI.Text = "Toggle invalidation";
            this.bEnableAI.UseVisualStyleBackColor = true;
            this.bEnableAI.Click += new System.EventHandler(this.bEnableAI_Click);
            // 
            // bSaveGames
            // 
            this.bSaveGames.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bSaveGames.Location = new System.Drawing.Point(336, 311);
            this.bSaveGames.Name = "bSaveGames";
            this.bSaveGames.Size = new System.Drawing.Size(120, 23);
            this.bSaveGames.TabIndex = 11;
            this.bSaveGames.Text = "Save game list";
            this.bSaveGames.UseVisualStyleBackColor = true;
            this.bSaveGames.Click += new System.EventHandler(this.bSaveGames_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(468, 346);
            this.Controls.Add(this.bSaveGames);
            this.Controls.Add(this.bEnableAI);
            this.Controls.Add(this.bPackageManager);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.bLaunch);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.bShaderEdit);
            this.Controls.Add(this.bTESsnip);
            this.Controls.Add(this.cBSACreator);
            this.Controls.Add(this.bBSAUnpack);
            this.Name = "MainForm";
            this.Text = "Fallout Mod Manager";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button bBSAUnpack;
        private System.Windows.Forms.Button cBSACreator;
        private System.Windows.Forms.Button bTESsnip;
        private System.Windows.Forms.Button bShaderEdit;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button bLaunch;
        private System.Windows.Forms.ListView lvEspList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox tbPluginInfo;
        private System.Windows.Forms.Button bPackageManager;
        private System.Windows.Forms.Button bEnableAI;
        private System.Windows.Forms.Button bSaveGames;
    }
}

