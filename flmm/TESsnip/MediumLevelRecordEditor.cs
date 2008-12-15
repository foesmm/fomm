using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace fomm.TESsnip {
    public partial class MediumLevelRecordEditor : Form {
        private SubRecord sr;
        private SubrecordStructure ss;
        private List<TextBox> boxes;
        private List<ElementValueType> valueTypes;
        private List<Panel> elements;
        private dFormIDLookupS formIDLookup;
        private dFormIDScan formIDScan;

        private Dictionary<int, string> removedStrings=new Dictionary<int, string>();

        private int repeatcount=0;

        private Dictionary<string, string[]> cachedFormIDs=new Dictionary<string, string[]>();

        private class cbTag {
            public readonly int group;
            public readonly TextBox textBox;

            public cbTag(int group, TextBox textBox) {
                this.group=group;
                this.textBox=textBox;
            }
        }

        private class bTag {
            public readonly TextBox formID;
            public readonly TextBox edid;

            public bTag(TextBox form, TextBox edid) {
                this.formID=form;
                this.edid=edid;
            }
        }

        private class comboBoxItem {
            public readonly string name;
            public readonly string value;

            public comboBoxItem(string name, string value) {
                this.name=name;
                this.value=value;
            }

            public override string ToString() {
                return name;
            }
        }

        private class repeatCbTag {
            public readonly TextBox tb;
            public readonly int panel;

            public repeatCbTag(TextBox tb, int panel) {
                this.tb=tb;
                this.panel=panel;
            }
        }

        private void AddElement(ElementStructure es) {
            int a=-1, b=0, c=0;
            AddElement(es, ref a, null, ref b, ref c);
        }
        private void AddElement(ElementStructure es, ref int offset, byte[] data, ref int groupOffset, ref int CurrentGroup) {
            Panel panel1=new Panel();
            panel1.AutoSize=true;
            panel1.Width=fpanel1.Width;
            panel1.Height=1;
            int ypos=0;

            TextBox tb=new TextBox();
            boxes.Add(tb);
            if(es.group!=0) {
                CheckBox cb=new CheckBox();
                cb.Text="Use this value?";
                panel1.Controls.Add(cb);
                cb.Location=new System.Drawing.Point(10, ypos);
                ypos+=24;
                cb.Tag=new cbTag(es.group, tb);
                if(CurrentGroup!=es.group) cb.Checked=true;
                else tb.Enabled=false;
                cb.CheckedChanged+=new EventHandler(CheckBox_CheckedChanged);
            }
            if(es.optional||es.repeat&&repeatcount>0) {
                CheckBox cb=new CheckBox();
                cb.Text="Use this value?";
                panel1.Controls.Add(cb);
                cb.Location=new System.Drawing.Point(10, ypos);
                ypos+=24;
                cb.Tag=new repeatCbTag(tb, elements.Count);
                if(data==null) {
                    tb.Enabled=false;
                } else {
                    cb.Checked=true;
                }
                cb.CheckedChanged+=new EventHandler(RepeatCheckBox_CheckedChanged);
            }
            if((CurrentGroup==0&&es.group!=0)||(CurrentGroup!=0&&es.group!=0&&CurrentGroup!=es.group)) {
                CurrentGroup=es.group;
                groupOffset=offset;
            } else if(CurrentGroup!=0&&es.group==0) {
                CurrentGroup=0;
            } else if(CurrentGroup!=0&&CurrentGroup==es.group) {
                offset=groupOffset;
            }
            valueTypes.Add(es.type);
            if(data!=null) {
                switch(es.type) {
                case ElementValueType.Int:
                    tb.Text=TypeConverter.h2si(data[offset], data[offset+1], data[offset+2], data[offset+3]).ToString();
                    offset+=4;
                    break;
                case ElementValueType.FormID:
                    tb.Text=TypeConverter.h2i(data[offset], data[offset+1], data[offset+2], data[offset+3]).ToString("X8");
                    offset+=4;
                    break;
                case ElementValueType.Float:
                    tb.Text=TypeConverter.h2f(data[offset], data[offset+1], data[offset+2], data[offset+3]).ToString();
                    offset+=4;
                    break;
                case ElementValueType.Short:
                    tb.Text=TypeConverter.h2ss(data[offset], data[offset+1]).ToString();
                    offset+=2;
                    break;
                case ElementValueType.Byte:
                    tb.Text=data[offset].ToString();
                    offset++;
                    break;
                case ElementValueType.String:
                    string s="";
                    while(data[offset]!=0) s+=(char)data[offset++];
                    offset++;
                    tb.Text=s;
                    tb.Width+=200;
                    break;
                case ElementValueType.fstring:
                    tb.Text=sr.GetStrData();
                    tb.Width+=200;
                    break;
                default:
                    throw new ApplicationException();
                }
            } else {
                if(es.type==ElementValueType.String||es.type==ElementValueType.fstring) tb.Width+=200;
                if(removedStrings.ContainsKey(boxes.Count-1)) tb.Text=removedStrings[boxes.Count-1];
            }
            Label l=new Label();
            l.AutoSize=true;
            string tmp=es.type.ToString();
            l.Text=tmp+": "+es.name+(es.desc!=null?(" ("+es.desc+")"):"");
            panel1.Controls.Add(tb);
            tb.Location=new System.Drawing.Point(10, ypos);
            if(es.multiline) {
                tb.Multiline=true;
                ypos+=tb.Height*5;
                tb.Height*=6;
            }
            panel1.Controls.Add(l);
            l.Location=new System.Drawing.Point(tb.Right+10, ypos+3);
            string[] options=null;
            if(es.type==ElementValueType.FormID) {
                ypos+=28;
                Button b=new Button();
                b.Text="FormID lookup";
                b.Click+=new EventHandler(LookupFormID_Click);
                panel1.Controls.Add(b);
                b.Location=new System.Drawing.Point(20, ypos);
                TextBox tb2=new TextBox();
                tb2.Width+=200;
                tb2.ReadOnly=true;
                panel1.Controls.Add(tb2);
                tb2.Location=new System.Drawing.Point(b.Right+10, ypos);
                b.Tag=new bTag(tb, tb2);
                if(es.FormIDType!=null) {
                    if(cachedFormIDs.ContainsKey(es.FormIDType)) {
                        options=cachedFormIDs[es.FormIDType];
                    } else {
                        options=formIDScan(es.FormIDType);
                        cachedFormIDs[es.FormIDType]=options;
                    }
                }
            } else if(es.options!=null) {
                options=es.options;
            }
            if(options!=null) {
                ypos+=28;
                ComboBox cmb=new ComboBox();
                cmb.Tag=tb;
                cmb.Width+=200;
                for(int j=0;j<options.Length;j+=2) {
                    cmb.Items.Add(new comboBoxItem(options[j], options[j+1]));
                }
                cmb.KeyPress+=new KeyPressEventHandler(cb_KeyPress);
                cmb.ContextMenu=new ContextMenu();
                cmb.SelectedIndexChanged+=new EventHandler(cb_SelectedIndexChanged);
                panel1.Controls.Add(cmb);
                cmb.Location=new System.Drawing.Point(20, ypos);
            }

            fpanel1.Controls.Add(panel1);
            elements.Add(panel1);
        }

        public MediumLevelRecordEditor(SubRecord sr, SubrecordStructure ss, dFormIDLookupS formIDLookup, dFormIDScan formIDScan) {
            InitializeComponent();
            SuspendLayout();
            this.sr=sr;
            this.ss=ss;
            this.formIDLookup=formIDLookup;
            this.formIDScan=formIDScan;

            int offset=0;
            byte[] data=sr.GetReadonlyData();
            boxes=new List<TextBox>(ss.elements.Length);
            valueTypes=new List<ElementValueType>(ss.elements.Length);
            elements=new List<Panel>();
            int groupOffset=0;
            int CurrentGroup=0;
            try {
                for(int i=0;i<ss.elements.Length;i++) {
                    if(ss.elements[i].optional&&offset==data.Length) {
                        AddElement(ss.elements[i]);
                    } else {
                        AddElement(ss.elements[i], ref offset, data, ref groupOffset, ref CurrentGroup);
                        if(ss.elements[i].repeat) {
                            repeatcount++;
                            if(offset<data.Length) i--;
                        }
                    }
                }
                if(ss.elements[ss.elements.Length-1].repeat&&repeatcount>0) {
                    AddElement(ss.elements[ss.elements.Length-1]);
                }
            } catch {
                MessageBox.Show("The subrecord doesn't appear to conform to the expected structure.\n"+
                    "Saving is disabled, and the formatted information may be incorrect", "Warning");
                bSave.Enabled=false;
            }
            ResumeLayout();
        }

        void cb_SelectedIndexChanged(object sender, EventArgs e) {
            ComboBox cmb=(ComboBox)sender;
            comboBoxItem cbi=(comboBoxItem)cmb.SelectedItem;
            ((TextBox)cmb.Tag).Text=cbi.value;
        }

        void cb_KeyPress(object sender, KeyPressEventArgs e) {
            e.Handled=true;
        }

        private bool CheckingChange;
        private bool IgnoreChange;
        void CheckBox_CheckedChanged(object sender, EventArgs e) {
            if(IgnoreChange) return;
            CheckBox changed=(CheckBox)sender;
            if(changed.Checked==false) {
                if(!CheckingChange) {
                    IgnoreChange=true;
                    changed.Checked=true;
                    IgnoreChange=false;
                    return;
                } else {
                    TextBox toDisable=((cbTag)changed.Tag).textBox;
                    foreach(Control c in fpanel1.Controls) {
                        TextBox tb=c as TextBox;
                        if(tb==toDisable) {
                            toDisable.Enabled=false;
                            return;
                        }
                    }
                    throw new ApplicationException();
                }
            }
            int Group=((cbTag)changed.Tag).group;
            TextBox toEnable=((cbTag)changed.Tag).textBox;
            CheckingChange=true;
            foreach(Panel p in fpanel1.Controls) {
                foreach(Control c in p.Controls) {
                    CheckBox cb=c as CheckBox;
                    if(cb!=null&&cb!=changed) {
                        if(((cbTag)changed.Tag).group==Group) cb.Checked=false;
                    }
                    TextBox tb=c as TextBox;
                    if(tb==toEnable) {
                        toEnable.Enabled=true;
                    }
                }
            }
            CheckingChange=false;
        }

        void RepeatCheckBox_CheckedChanged(object sender, EventArgs e) {
            CheckBox cb=(CheckBox)sender;
            repeatCbTag tag=(repeatCbTag)cb.Tag;
            tag.tb.Enabled=cb.Checked;
            if(cb.Checked) {
                if(ss.elements[ss.elements.Length-1].repeat) AddElement(ss.elements[ss.elements.Length-1]);
            } else {
                for(int i=tag.panel+1;i<elements.Count;i++) {
                    removedStrings[i]=boxes[i].Text;
                    fpanel1.Controls.Remove(elements[i]);
                }
                boxes.RemoveRange(tag.panel+1, elements.Count-(tag.panel+1));
                valueTypes.RemoveRange(tag.panel+1, elements.Count-(tag.panel+1));
                elements.RemoveRange(tag.panel+1, elements.Count-(tag.panel+1));
            }
        }

        void bCancel_Click(object sender, EventArgs e) {
            Close();
        }

        void bSave_Click(object sender, EventArgs e) {
            System.Collections.Generic.List<byte> bytes=new System.Collections.Generic.List<byte>();
            for(int j=0;j<boxes.Count;j++) {
                if(!boxes[j].Enabled) continue;
                switch(valueTypes[j]) {
                case ElementValueType.Byte: {
                        byte b;
                        if(!byte.TryParse(boxes[j].Text, out b)) {
                            MessageBox.Show("Invalid byte: "+boxes[j].Text, "Error");
                            return;
                        }
                        bytes.Add(b);
                        break;
                    }
                case ElementValueType.Short: {
                        short s;
                        if(!short.TryParse(boxes[j].Text, out s)) {
                            MessageBox.Show("Invalid short: "+boxes[j].Text, "Error");
                            return;
                        }
                        byte[] conv=TypeConverter.ss2h(s);
                        bytes.Add(conv[0]);
                        bytes.Add(conv[1]);
                        break;
                    }
                case ElementValueType.Int: {
                        int i;
                        if(!int.TryParse(boxes[j].Text, out i)) {
                            MessageBox.Show("Invalid int: "+boxes[j].Text, "Error");
                            return;
                        }
                        byte[] conv=TypeConverter.si2h(i);
                        bytes.AddRange(conv);
                        break;
                    }
                case ElementValueType.Float: {
                        float f;
                        if(!float.TryParse(boxes[j].Text, out f)) {
                            MessageBox.Show("Invalid float: "+boxes[j].Text, "Error");
                            return;
                        }
                        byte[] conv=TypeConverter.f2h(f);
                        bytes.AddRange(conv);
                        break;
                    }
                case ElementValueType.FormID: {
                        uint i;
                        if(!uint.TryParse(boxes[j].Text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out i)) {
                            MessageBox.Show("Invalid formID: "+boxes[j].Text, "Error");
                            return;
                        }
                        byte[] conv=TypeConverter.i2h(i);
                        bytes.AddRange(conv);
                        break;
                    }
                case ElementValueType.String: {
                        byte[] conv=System.Text.Encoding.Default.GetBytes(boxes[j].Text);
                        bytes.AddRange(conv);
                        bytes.Add(0);
                        break;
                    }
                case ElementValueType.fstring: {
                        byte[] conv=System.Text.Encoding.Default.GetBytes(boxes[j].Text);
                        bytes.AddRange(conv);
                        break;
                    }
                default:
                    throw new ApplicationException();
                }
            }
            sr.SetData(bytes.ToArray());
            Close();
        }

        void LookupFormID_Click(object sender, EventArgs e) {
            bTag tag=(bTag)((Button)sender).Tag;
            tag.edid.Text=formIDLookup(tag.formID.Text);
        }
    }
}