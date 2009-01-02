using System;
using System.Windows.Forms;

namespace fomm.PackageManager {
    enum TextEditorType { Text, Rtf, Script }

    partial class TextEditor : Form {
        private string saved;
        private static RichTextBox rtbEdit;
        private static ICSharpCode.TextEditor.TextEditorControl tbScript;
        private bool UsingScript;

        private TextEditor(string text, TextEditorType type) {
            InitializeComponent();
            Settings.GetWindowPosition("TextEditor", this);
            switch(type) {
            case TextEditorType.Text:
            case TextEditorType.Rtf:
                rtbEdit=new RichTextBox();
                panel1.Controls.Add(rtbEdit);
                if(type==TextEditorType.Rtf) rtbEdit.Rtf=text;
                else rtbEdit.Text=text;
                rtbEdit.Dock=DockStyle.Fill;
                rtbEdit.TextChanged+=textChanged;
                Text="Readme editor";
                break;
            case TextEditorType.Script:
                tbScript=new ICSharpCode.TextEditor.TextEditorControl();
                panel1.Controls.Add(tbScript);
                tbScript.Text=text;
                tbScript.ShowVRuler=false;
                tbScript.SetHighlighting("C#");
                tbScript.Dock=DockStyle.Fill;
                tbScript.TextChanged+=textChanged;
                UsingScript=true;
                ToolStripButton bSyntaxCheck = new ToolStripButton();
                bSyntaxCheck.DisplayStyle = ToolStripItemDisplayStyle.Text;
                bSyntaxCheck.Size = new System.Drawing.Size(76, 22);
                bSyntaxCheck.Text = "Check syntax";
                bSyntaxCheck.Click += new System.EventHandler(this.bSyntaxCheck_Click);
                toolStrip1.Items.Add(bSyntaxCheck);
                break;
            }
        }

        private bool changed;
        void textChanged(object sender, EventArgs e) {
            changed=true;
            if(UsingScript) tbScript.TextChanged-=textChanged;
            else rtbEdit.TextChanged-=textChanged;
        }

        public static string ShowEditor(string initial, TextEditorType type) {
            TextEditor se=new TextEditor(initial, type);
            se.ShowDialog();
            return se.saved;
        }

        private void bSave_Click(object sender, EventArgs e) {
            if(UsingScript) {
                saved=tbScript.Text;
                tbScript.TextChanged+=textChanged;
            } else {
                if(rtbEdit.TextLength==0) saved="";
                else saved=rtbEdit.Rtf;
                rtbEdit.TextChanged+=textChanged;
            }
            changed=false;
        }

        private void TextEditor_FormClosing(object sender, FormClosingEventArgs e) {
            if(changed) {
                switch(MessageBox.Show("Save changes?", "", MessageBoxButtons.YesNoCancel)) {
                case DialogResult.Yes:
                    bSave_Click(null, null);
                    break;
                case DialogResult.No:
                    break;
                default:
                    e.Cancel=true;
                    return;
                }
            }
            Settings.SetWindowPosition("TextEditor", this);
        }

        private void bSyntaxCheck_Click(object sender, EventArgs e) {
            string stdout;
            string errors=ScriptCompiler.CheckSyntax(tbScript.Text, out stdout);
            if(errors!=null) {
                MessageBox.Show(errors);
            } else {
                MessageBox.Show("No errors found");
            }
        }
    }
}