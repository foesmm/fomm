namespace Fomm.Games.Fallout3.Tools.TESsnip {
    partial class GroupEditor {
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
            this.cmbGroupType = new System.Windows.Forms.ComboBox();
            this.tbRecType = new System.Windows.Forms.TextBox();
            this.tbBlock = new System.Windows.Forms.TextBox();
            this.tbX = new System.Windows.Forms.TextBox();
            this.tbY = new System.Windows.Forms.TextBox();
            this.tbParent = new System.Windows.Forms.TextBox();
            this.tbDateStamp = new System.Windows.Forms.TextBox();
            this.tbFlags = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.bCancel = new System.Windows.Forms.Button();
            this.bSave = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cmbGroupType
            // 
            this.cmbGroupType.FormattingEnabled = true;
            this.cmbGroupType.Items.AddRange(new object[] {
            "Top",
            "World Children",
            "Interior Cell Block",
            "Interior Cell Subblock",
            "Exterior Cell Block",
            "Exterior Cell Subblock",
            "Cell Children",
            "Topic Children",
            "Cell Persistent Children",
            "Cell Temporary Children",
            "Cell Visible Distant Children"});
            this.cmbGroupType.Location = new System.Drawing.Point(12, 12);
            this.cmbGroupType.Name = "cmbGroupType";
            this.cmbGroupType.Size = new System.Drawing.Size(151, 21);
            this.cmbGroupType.TabIndex = 0;
            this.cmbGroupType.SelectedIndexChanged += new System.EventHandler(this.cmbGroupType_SelectedIndexChanged);
            this.cmbGroupType.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cmbGroupType_KeyPress);
            // 
            // tbRecType
            // 
            this.tbRecType.Location = new System.Drawing.Point(12, 39);
            this.tbRecType.MaxLength = 4;
            this.tbRecType.Name = "tbRecType";
            this.tbRecType.Size = new System.Drawing.Size(100, 20);
            this.tbRecType.TabIndex = 1;
            this.tbRecType.Leave += new System.EventHandler(this.tbRecType_Leave);
            // 
            // tbBlock
            // 
            this.tbBlock.Location = new System.Drawing.Point(12, 65);
            this.tbBlock.Name = "tbBlock";
            this.tbBlock.Size = new System.Drawing.Size(100, 20);
            this.tbBlock.TabIndex = 2;
            // 
            // tbX
            // 
            this.tbX.Location = new System.Drawing.Point(12, 91);
            this.tbX.MaxLength = 2;
            this.tbX.Name = "tbX";
            this.tbX.Size = new System.Drawing.Size(47, 20);
            this.tbX.TabIndex = 3;
            // 
            // tbY
            // 
            this.tbY.Location = new System.Drawing.Point(65, 91);
            this.tbY.MaxLength = 2;
            this.tbY.Name = "tbY";
            this.tbY.Size = new System.Drawing.Size(47, 20);
            this.tbY.TabIndex = 4;
            // 
            // tbParent
            // 
            this.tbParent.Location = new System.Drawing.Point(12, 117);
            this.tbParent.MaxLength = 8;
            this.tbParent.Name = "tbParent";
            this.tbParent.Size = new System.Drawing.Size(100, 20);
            this.tbParent.TabIndex = 5;
            // 
            // tbDateStamp
            // 
            this.tbDateStamp.Location = new System.Drawing.Point(12, 170);
            this.tbDateStamp.MaxLength = 8;
            this.tbDateStamp.Name = "tbDateStamp";
            this.tbDateStamp.Size = new System.Drawing.Size(100, 20);
            this.tbDateStamp.TabIndex = 6;
            // 
            // tbFlags
            // 
            this.tbFlags.Location = new System.Drawing.Point(12, 196);
            this.tbFlags.MaxLength = 8;
            this.tbFlags.Name = "tbFlags";
            this.tbFlags.Size = new System.Drawing.Size(100, 20);
            this.tbFlags.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(169, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Group type";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(118, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Parent record type";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(118, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Block number";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(118, 94);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(66, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "X, Y position";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(118, 120);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Parent";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(118, 173);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(58, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Datestamp";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(118, 199);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(32, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "Flags";
            // 
            // bCancel
            // 
            this.bCancel.Location = new System.Drawing.Point(183, 238);
            this.bCancel.Name = "bCancel";
            this.bCancel.Size = new System.Drawing.Size(75, 23);
            this.bCancel.TabIndex = 15;
            this.bCancel.Text = "Cancel";
            this.bCancel.UseVisualStyleBackColor = true;
            this.bCancel.Click += new System.EventHandler(this.bCancel_Click);
            // 
            // bSave
            // 
            this.bSave.Location = new System.Drawing.Point(102, 238);
            this.bSave.Name = "bSave";
            this.bSave.Size = new System.Drawing.Size(75, 23);
            this.bSave.TabIndex = 16;
            this.bSave.Text = "Save";
            this.bSave.UseVisualStyleBackColor = true;
            this.bSave.Click += new System.EventHandler(this.bSave_Click);
            // 
            // GroupEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.bSave);
            this.Controls.Add(this.bCancel);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbFlags);
            this.Controls.Add(this.tbDateStamp);
            this.Controls.Add(this.tbParent);
            this.Controls.Add(this.tbY);
            this.Controls.Add(this.tbX);
            this.Controls.Add(this.tbBlock);
            this.Controls.Add(this.tbRecType);
            this.Controls.Add(this.cmbGroupType);
            this.Name = "GroupEditor";
            this.Text = "GroupEditor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbGroupType;
        private System.Windows.Forms.TextBox tbRecType;
        private System.Windows.Forms.TextBox tbBlock;
        private System.Windows.Forms.TextBox tbX;
        private System.Windows.Forms.TextBox tbY;
        private System.Windows.Forms.TextBox tbParent;
        private System.Windows.Forms.TextBox tbDateStamp;
        private System.Windows.Forms.TextBox tbFlags;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button bCancel;
        private System.Windows.Forms.Button bSave;
    }
}