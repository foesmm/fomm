using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Fomm.TESsnip {
    internal partial class HexDataEdit : Form {
        public static bool Canceled;
        public static byte[] result;
        public static string resultName;

        private System.Text.StringBuilder sb=new System.Text.StringBuilder();
        private Be.Windows.Forms.DynamicByteProvider dbytes;
        private Be.Windows.Forms.ByteCollection bytes;

        private dFormIDLookupS formIDLookup;

        public HexDataEdit(string RecName, byte[] data, dFormIDLookupS formIDLookup) {
            InitializeComponent();
            this.formIDLookup=formIDLookup;
            Text+=RecName;
            tbName.Text=RecName;
            Text+=" (hex mode)";
            dbytes=new Be.Windows.Forms.DynamicByteProvider(data);
            bytes=dbytes.Bytes;
            hexBox1.ByteProvider=dbytes;
            Canceled=true;
        }

        private void bSave_Click(object sender, EventArgs e) {
            Canceled=false;
            result=bytes.GetBytes();
            resultName=tbName.Text;
            Close();
        }

        private void bCancel_Click(object sender, EventArgs e) {
            Canceled=true;
            Close();
        }

        private void tbName_KeyPress(object sender, KeyPressEventArgs e) {
            if(!char.IsControl(e.KeyChar)&&!char.IsDigit(e.KeyChar)&&!char.IsLetter(e.KeyChar)&&e.KeyChar!='_') e.Handled=true;
        }

        private void tbName_Leave(object sender, EventArgs e) {
            if(tbName.Text.Length<4) tbName.Text=tbName.Text.PadRight(4, '_');
        }

        private void hexBox1_SelectionStartChanged(object sender, EventArgs e) {
            int pos=(int)hexBox1.SelectionStart;
            if(bytes.Count>=pos+4) {
                tbFloat.Text=TypeConverter.h2f(bytes[pos], bytes[pos+1], bytes[pos+2], bytes[pos+3]).ToString();
                tbFloat.Enabled=true;
                tbInt.Text=TypeConverter.h2si(bytes[pos], bytes[pos+1], bytes[pos+2], bytes[pos+3]).ToString();
                tbInt.Enabled=true;
                bCFloat.Enabled=true;
                bCInt.Enabled=true;
                tbFormID.Text=TypeConverter.h2i(bytes[pos], bytes[pos+1], bytes[pos+2], bytes[pos+3]).ToString("X8");
                tbFormID.Enabled=true;
                bCFormID.Enabled=true;
                bLookup.Enabled=true;
            } else {
                tbFloat.Enabled=false;
                tbInt.Enabled=false;
                bCFloat.Enabled=false;
                bCInt.Enabled=false;
                tbFormID.Enabled=false;
                bCFormID.Enabled=false;
                bLookup.Enabled=false;
            }
            if(bytes.Count>=pos+2) {
                tbWord.Text=TypeConverter.h2ss(bytes[pos], bytes[pos+1]).ToString();
                tbWord.Enabled=true;
                bCWord.Enabled=true;
            } else {
                tbWord.Enabled=false;
                bCWord.Enabled=false;
            }
        }

        private void bCFloat_Click(object sender, EventArgs e) {
            float f;
            if(!float.TryParse(tbFloat.Text, out f)) {
                MessageBox.Show("Invalid float");
                return;
            }
            byte[] b=TypeConverter.f2h(f);
            int pos=(int)hexBox1.SelectionStart;
            bytes[pos+0]=b[0];
            bytes[pos+1]=b[1];
            bytes[pos+2]=b[2];
            bytes[pos+3]=b[3];
            hexBox1.Refresh();
        }

        private void bCInt_Click(object sender, EventArgs e) {
            int i;
            if(!int.TryParse(tbInt.Text, out i)) {
                MessageBox.Show("Invalid int");
                return;
            }
            byte[] b=TypeConverter.si2h(i);
            int pos=(int)hexBox1.SelectionStart;
            bytes[pos+0]=b[0];
            bytes[pos+1]=b[1];
            bytes[pos+2]=b[2];
            bytes[pos+3]=b[3];
            hexBox1.Refresh();
            
        }

        private void bCShort_Click(object sender, EventArgs e) {
            short s;
            if(!short.TryParse(tbFloat.Text, out s)) {
                MessageBox.Show("Invalid word");
                return;
            }
            byte[] b=TypeConverter.ss2h(s);
            int pos=(int)hexBox1.SelectionStart;
            bytes[pos+0]=b[0];
            bytes[pos+1]=b[1];
            hexBox1.Refresh();
        }

        private void cbInsert_CheckedChanged(object sender, EventArgs e) {
            hexBox1.InsertActive=cbInsert.Checked;
        }

        private void hexBox1_InsertActiveChanged(object sender, EventArgs e) {
            if(cbInsert.Checked!=hexBox1.InsertActive) cbInsert.Checked=hexBox1.InsertActive;
        }

        private void bCFormID_Click(object sender, EventArgs e) {
            uint i;
            if(!uint.TryParse(tbFormID.Text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out i)) {
                MessageBox.Show("Invalid form ID");
                return;
            }
            byte[] b=TypeConverter.i2h(i);
            int pos=(int)hexBox1.SelectionStart;
            bytes[pos+0]=b[0];
            bytes[pos+1]=b[1];
            bytes[pos+2]=b[2];
            bytes[pos+3]=b[3];
            hexBox1.Refresh();
        }

        private void bLookup_Click(object sender, EventArgs e) {
            tbEDID.Text=formIDLookup(tbFormID.Text);
        }

        private void bFromFile_Click(object sender, EventArgs e) {
            if(openFileDialog1.ShowDialog()!=DialogResult.OK) return;
            byte[] newdata=System.IO.File.ReadAllBytes(openFileDialog1.FileName);
            bytes.Clear();
            bytes.AddRange(newdata);
            hexBox1.Refresh();
        }

    }
}