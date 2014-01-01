using System;
using System.Windows.Forms;

namespace Fomm.Games.Fallout3.Tools.TESsnip {
    internal partial class GroupEditor : Form {
        private GroupRecord gr;


        public GroupEditor(GroupRecord gr) {
            this.gr=gr;
            InitializeComponent();
            this.Icon=Fomm.Properties.Resources.fomm02;
            cmbGroupType.ContextMenu=new ContextMenu();
            cmbGroupType.SelectedIndex=(int)gr.groupType;
            tbRecType.Text=gr.ContentsType;
            byte[] data=gr.GetData();
            tbX.Text=TypeConverter.h2ss(data[2], data[3]).ToString();
            tbY.Text=TypeConverter.h2ss(data[0], data[1]).ToString();
            tbBlock.Text=TypeConverter.h2i(data[0], data[1], data[2], data[3]).ToString();
            tbParent.Text=TypeConverter.h2i(data[0], data[1], data[2], data[3]).ToString("X8");
            tbDateStamp.Text=gr.dateStamp.ToString("X8");
            tbFlags.Text=gr.flags.ToString("X8");
        }

        public static void Display(GroupRecord gr) {
            GroupEditor ge=new GroupEditor(gr);
            ge.ShowDialog();
        }

        private void cmbGroupType_SelectedIndexChanged(object sender, EventArgs e) {
            switch(cmbGroupType.SelectedIndex) {
            case 0:
                tbRecType.Enabled=true;
                tbX.Enabled=false;
                tbY.Enabled=false;
                tbParent.Enabled=false;
                tbBlock.Enabled=false;
                break;
            case 2:
            case 3:
                tbRecType.Enabled=false;
                tbX.Enabled=false;
                tbY.Enabled=false;
                tbParent.Enabled=false;
                tbBlock.Enabled=true;
                break;
            case 4:
            case 5:
                tbRecType.Enabled=false;
                tbX.Enabled=true;
                tbY.Enabled=true;
                tbParent.Enabled=false;
                tbBlock.Enabled=false;
                break;
            case 1:
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
                tbRecType.Enabled=false;
                tbX.Enabled=false;
                tbY.Enabled=false;
                tbParent.Enabled=true;
                tbBlock.Enabled=false;
                break;
            }
        }

        private void cmbGroupType_KeyPress(object sender, KeyPressEventArgs e) {
            e.Handled=true;
        }

        private void bCancel_Click(object sender, EventArgs e) {
            Close();
        }

        private void bSave_Click(object sender, EventArgs e) {
            uint flags, datestamp;
            if(!uint.TryParse(tbDateStamp.Text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out datestamp)) {
                MessageBox.Show("Invalid value specified for datestamp");
                return;
            }
            if(!uint.TryParse(tbFlags.Text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out flags)) {
                MessageBox.Show("Invalid value specified for flags");
                return;
            }
            uint grouptype=(uint)cmbGroupType.SelectedIndex;
            byte[] data;
            switch(cmbGroupType.SelectedIndex) {
            case 0:
                data=System.Text.Encoding.ASCII.GetBytes(tbRecType.Text);
                break;
            case 2:
            case 3:
                uint block;
                if(!uint.TryParse(tbBlock.Text, out block)) {
                    MessageBox.Show("Invalid value specified for block id");
                    return;
                }
                data=TypeConverter.i2h(block);
                break;
            case 4:
            case 5:
                short x, y;
                if(!short.TryParse(tbX.Text, out x)) {
                    MessageBox.Show("Invalid value specified for x coord");
                    return;
                }
                if(!short.TryParse(tbY.Text, out y)) {
                    MessageBox.Show("Invalid value specified for y coord");
                    return;
                }
                data=new byte[4];
                TypeConverter.ss2h(x, data, 2);
                TypeConverter.ss2h(y, data, 0);
                break;
            case 1:
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
                uint parent;
                if(!uint.TryParse(tbParent.Text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out parent)) {
                    MessageBox.Show("Invalid value specified for parent");
                    return;
                }
                data=TypeConverter.i2h(parent);
                break;
            default:
                MessageBox.Show("Sanity check failed; invalid group type");
                return;
            }
            gr.flags=flags;
            gr.dateStamp=datestamp;
            gr.groupType=grouptype;
            gr.SetData(data);
        }

        private void tbRecType_Leave(object sender, EventArgs e) {
            if(tbRecType.Text.Length<4) tbRecType.Text=tbRecType.Text.PadRight(4, '_');
        }
    }
}