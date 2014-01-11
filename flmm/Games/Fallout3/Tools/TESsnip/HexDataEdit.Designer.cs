namespace Fomm.Games.Fallout3.Tools.TESsnip {
    partial class HexDataEdit {
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
            this.bSave = new System.Windows.Forms.Button();
            this.bCancel = new System.Windows.Forms.Button();
            this.tbName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbFloat = new System.Windows.Forms.TextBox();
            this.tbInt = new System.Windows.Forms.TextBox();
            this.tbWord = new System.Windows.Forms.TextBox();
            this.bCFloat = new System.Windows.Forms.Button();
            this.bCInt = new System.Windows.Forms.Button();
            this.bCWord = new System.Windows.Forms.Button();
            this.cbInsert = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tbFormID = new System.Windows.Forms.TextBox();
            this.bLookup = new System.Windows.Forms.Button();
            this.bCFormID = new System.Windows.Forms.Button();
            this.tbEDID = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.hexBox1 = new Be.Windows.Forms.HexBox();
            this.bFromFile = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // bSave
            // 
            this.bSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bSave.Location = new System.Drawing.Point(493, 403);
            this.bSave.Name = "bSave";
            this.bSave.Size = new System.Drawing.Size(75, 23);
            this.bSave.TabIndex = 1;
            this.bSave.Text = "Save";
            this.bSave.UseVisualStyleBackColor = true;
            this.bSave.Click += new System.EventHandler(this.bSave_Click);
            // 
            // bCancel
            // 
            this.bCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bCancel.Location = new System.Drawing.Point(412, 403);
            this.bCancel.Name = "bCancel";
            this.bCancel.Size = new System.Drawing.Size(75, 23);
            this.bCancel.TabIndex = 2;
            this.bCancel.Text = "Cancel";
            this.bCancel.UseVisualStyleBackColor = true;
            this.bCancel.Click += new System.EventHandler(this.bCancel_Click);
            // 
            // tbName
            // 
            this.tbName.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.tbName.Location = new System.Drawing.Point(12, 12);
            this.tbName.MaxLength = 4;
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(100, 20);
            this.tbName.TabIndex = 5;
            this.tbName.Leave += new System.EventHandler(this.tbName_Leave);
            this.tbName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbName_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(121, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Name";
            // 
            // tbFloat
            // 
            this.tbFloat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbFloat.Location = new System.Drawing.Point(38, 349);
            this.tbFloat.Name = "tbFloat";
            this.tbFloat.Size = new System.Drawing.Size(118, 20);
            this.tbFloat.TabIndex = 8;
            // 
            // tbInt
            // 
            this.tbInt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbInt.Location = new System.Drawing.Point(38, 375);
            this.tbInt.Name = "tbInt";
            this.tbInt.Size = new System.Drawing.Size(118, 20);
            this.tbInt.TabIndex = 9;
            // 
            // tbWord
            // 
            this.tbWord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbWord.Location = new System.Drawing.Point(38, 401);
            this.tbWord.Name = "tbWord";
            this.tbWord.Size = new System.Drawing.Size(118, 20);
            this.tbWord.TabIndex = 10;
            // 
            // bCFloat
            // 
            this.bCFloat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bCFloat.Location = new System.Drawing.Point(12, 349);
            this.bCFloat.Name = "bCFloat";
            this.bCFloat.Size = new System.Drawing.Size(20, 20);
            this.bCFloat.TabIndex = 11;
            this.bCFloat.UseVisualStyleBackColor = true;
            this.bCFloat.Click += new System.EventHandler(this.bCFloat_Click);
            // 
            // bCInt
            // 
            this.bCInt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bCInt.Location = new System.Drawing.Point(12, 375);
            this.bCInt.Name = "bCInt";
            this.bCInt.Size = new System.Drawing.Size(20, 20);
            this.bCInt.TabIndex = 12;
            this.bCInt.UseVisualStyleBackColor = true;
            this.bCInt.Click += new System.EventHandler(this.bCInt_Click);
            // 
            // bCWord
            // 
            this.bCWord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bCWord.Location = new System.Drawing.Point(12, 401);
            this.bCWord.Name = "bCWord";
            this.bCWord.Size = new System.Drawing.Size(20, 20);
            this.bCWord.TabIndex = 13;
            this.bCWord.UseVisualStyleBackColor = true;
            this.bCWord.Click += new System.EventHandler(this.bCShort_Click);
            // 
            // cbInsert
            // 
            this.cbInsert.AutoSize = true;
            this.cbInsert.Location = new System.Drawing.Point(230, 14);
            this.cbInsert.Name = "cbInsert";
            this.cbInsert.Size = new System.Drawing.Size(81, 17);
            this.cbInsert.TabIndex = 14;
            this.cbInsert.Text = "Insert mode";
            this.cbInsert.UseVisualStyleBackColor = true;
            this.cbInsert.CheckedChanged += new System.EventHandler(this.cbInsert_CheckedChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(162, 353);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(27, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "float";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(162, 378);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(18, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "int";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(162, 404);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(30, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "short";
            // 
            // tbFormID
            // 
            this.tbFormID.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbFormID.Location = new System.Drawing.Point(295, 349);
            this.tbFormID.Name = "tbFormID";
            this.tbFormID.Size = new System.Drawing.Size(90, 20);
            this.tbFormID.TabIndex = 18;
            // 
            // bLookup
            // 
            this.bLookup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bLookup.Location = new System.Drawing.Point(214, 375);
            this.bLookup.Name = "bLookup";
            this.bLookup.Size = new System.Drawing.Size(75, 23);
            this.bLookup.TabIndex = 19;
            this.bLookup.Text = "Look up";
            this.bLookup.UseVisualStyleBackColor = true;
            this.bLookup.Click += new System.EventHandler(this.bLookup_Click);
            // 
            // bCFormID
            // 
            this.bCFormID.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bCFormID.Location = new System.Drawing.Point(269, 349);
            this.bCFormID.Name = "bCFormID";
            this.bCFormID.Size = new System.Drawing.Size(20, 20);
            this.bCFormID.TabIndex = 20;
            this.bCFormID.UseVisualStyleBackColor = true;
            this.bCFormID.Click += new System.EventHandler(this.bCFormID_Click);
            // 
            // tbEDID
            // 
            this.tbEDID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbEDID.Location = new System.Drawing.Point(295, 375);
            this.tbEDID.Name = "tbEDID";
            this.tbEDID.ReadOnly = true;
            this.tbEDID.Size = new System.Drawing.Size(273, 20);
            this.tbEDID.TabIndex = 21;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(391, 353);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 13);
            this.label5.TabIndex = 22;
            this.label5.Text = "FormID";
            // 
            // hexBox1
            // 
            this.hexBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hexBox1.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hexBox1.LineInfoForeColor = System.Drawing.Color.Empty;
            this.hexBox1.Location = new System.Drawing.Point(12, 38);
            this.hexBox1.Name = "hexBox1";
            this.hexBox1.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
            this.hexBox1.Size = new System.Drawing.Size(556, 305);
            this.hexBox1.StringViewVisible = true;
            this.hexBox1.TabIndex = 7;
            this.hexBox1.UseFixedBytesPerLine = true;
            this.hexBox1.SelectionStartChanged += new System.EventHandler(this.hexBox1_SelectionStartChanged);
            this.hexBox1.InsertActiveChanged += new System.EventHandler(this.hexBox1_InsertActiveChanged);
            // 
            // bFromFile
            // 
            this.bFromFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bFromFile.Location = new System.Drawing.Point(214, 404);
            this.bFromFile.Name = "bFromFile";
            this.bFromFile.Size = new System.Drawing.Size(123, 23);
            this.bFromFile.TabIndex = 23;
            this.bFromFile.Text = "Set data from file";
            this.bFromFile.UseVisualStyleBackColor = true;
            this.bFromFile.Click += new System.EventHandler(this.bFromFile_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.AddExtension = false;
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Any file|*.*";
            this.openFileDialog1.RestoreDirectory = true;
            this.openFileDialog1.Title = "Pick file to import";
            // 
            // HexDataEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(580, 438);
            this.Controls.Add(this.bFromFile);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbEDID);
            this.Controls.Add(this.bCFormID);
            this.Controls.Add(this.bLookup);
            this.Controls.Add(this.tbFormID);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbInsert);
            this.Controls.Add(this.bCWord);
            this.Controls.Add(this.bCInt);
            this.Controls.Add(this.bCFloat);
            this.Controls.Add(this.tbWord);
            this.Controls.Add(this.tbInt);
            this.Controls.Add(this.tbFloat);
            this.Controls.Add(this.hexBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.bCancel);
            this.Controls.Add(this.bSave);
            this.Name = "HexDataEdit";
            this.Text = "Editing: ";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bSave;
        private System.Windows.Forms.Button bCancel;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label label1;
        private Be.Windows.Forms.HexBox hexBox1;
        private System.Windows.Forms.TextBox tbFloat;
        private System.Windows.Forms.TextBox tbInt;
        private System.Windows.Forms.TextBox tbWord;
        private System.Windows.Forms.Button bCFloat;
        private System.Windows.Forms.Button bCInt;
        private System.Windows.Forms.Button bCWord;
        private System.Windows.Forms.CheckBox cbInsert;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbFormID;
        private System.Windows.Forms.Button bLookup;
        private System.Windows.Forms.Button bCFormID;
        private System.Windows.Forms.TextBox tbEDID;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button bFromFile;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}