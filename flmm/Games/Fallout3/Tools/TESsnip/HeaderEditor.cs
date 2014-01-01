using System;
using System.Windows.Forms;

namespace Fomm.Games.Fallout3.Tools.TESsnip {
    internal partial class HeaderEditor : Form {
        private uint Flags1;
        private uint Flags2;
        private uint Flags3;
        private uint FormID;
        private Record R;

        private static uint GetInt(object i) {
            return (uint)(1<<(int)i);
        }

        private HeaderEditor(Record r) {
            InitializeComponent();
            this.Icon=Fomm.Properties.Resources.fomm02;
            R=r;
            tbName.Text=r.Name;
            Flags1=r.Flags1;
            Flags2=r.Flags2;
            Flags3=r.Flags3;
            FormID=r.FormID;
            tbFormID.Text=FormID.ToString("x8");
            tbFlags2.Text=Flags2.ToString("x8");
            tbFlags3.Text=Flags3.ToString("x8");
            foreach(Control c in Controls) {
                if(c is CheckBox) {
                    c.Tag=int.Parse((string)c.Tag);
                    if((Flags1&GetInt(c.Tag))>0) ((CheckBox)c).Checked=true;
                }
            }
            cb18.Enabled=false;
        }

        public static void Display(Record r) {
            HeaderEditor hr=new HeaderEditor(r);
            hr.ShowDialog();
        }

        private void bCancel_Click(object sender, EventArgs e) {
            Close();
        }

        private void Save() {
            Flags1=0;
            foreach(Control c in Controls) {
                if(c is CheckBox&&((CheckBox)c).Checked) {
                    Flags1|=GetInt(c.Tag);
                }
            }
        }

        private void bSave_Click(object sender, EventArgs e) {
            Save();
            R.Flags1=Flags1;
            R.Flags2=Flags2;
            R.Flags3=Flags3;
            R.FormID=FormID;
            R.Name=tbName.Text;
            Close();
        }

        private void HexOnlyKeyPress(object sender, KeyPressEventArgs e) {
            if(!char.IsControl(e.KeyChar)&&!char.IsDigit(e.KeyChar)) {
                char c=char.ToLowerInvariant(e.KeyChar);
                if(c!='a'&&c!='b'&&c!='c'&&c!='d'&&c!='e'&&c!='f') e.Handled=true;
            }
        }

        private void HexValidCheck(object sender, EventArgs e) {
            TextBox tb=(TextBox)sender;
            if(tb.Text.Length<8) tb.Text=tb.Text.PadLeft(8, '0');
            uint i;
            if(!uint.TryParse(tb.Text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out i)) {
                MessageBox.Show("Invalid hex value", "Error");
                if(tb==tbFormID) tb.Text=FormID.ToString("x8");
                else if(tb==tbFlags2) tb.Text=Flags2.ToString("x8");
                else if(tb==tbFlags3) tb.Text=Flags3.ToString("x8");
            } else {
                if(tb==tbFormID) FormID=i;
                else if(tb==tbFlags2) Flags2=i;
                else if(tb==tbFlags3) Flags3=i;
            }
        }

        private void tbName_KeyPress(object sender, KeyPressEventArgs e) {
            if(!char.IsControl(e.KeyChar)&&!char.IsDigit(e.KeyChar)&&!char.IsLetter(e.KeyChar)&&e.KeyChar!='_') e.Handled=true;
        }

        private void tbName_Leave(object sender, EventArgs e) {
            if(tbName.Text.Length<4) tbName.Text=tbName.Text.PadRight(4, '_');
        }
    }
}