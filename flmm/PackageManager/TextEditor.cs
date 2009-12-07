using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.TextEditor.Document;

namespace Fomm.PackageManager {
    enum TextEditorType { Text, Rtf, Script, FixedFontText }

    partial class TextEditor : Form {
        private string saved;
        private static RichTextBox rtbEdit;
        private static ICSharpCode.TextEditor.TextEditorControl tbScript;
        private bool UsingScript;

        private TextEditor(string text, TextEditorType type) {
            InitializeComponent();
            this.Icon=Fomm.Properties.Resources.fomm02;
            Settings.GetWindowPosition("TextEditor", this);
            switch(type) {
            case TextEditorType.FixedFontText:
            case TextEditorType.Text:
            case TextEditorType.Rtf:
                rtbEdit=new RichTextBox();
                panel1.Controls.Add(rtbEdit);
                if(type==TextEditorType.FixedFontText) rtbEdit.Font=new System.Drawing.Font("Courier new", 10);
                if(type==TextEditorType.Rtf) rtbEdit.Rtf=text;
                else rtbEdit.Text=text;
                rtbEdit.Dock=DockStyle.Fill;
                rtbEdit.TextChanged+=textChanged;
                Text="Readme editor";
                break;
            case TextEditorType.Script:
                if(text==null) {
                    text=
@"using System;
using fomm.Scripting;

class Script : BaseScript {
	public static bool OnActivate() {
        //Install all files from the fomod and activate any esps
        PerformBasicInstall();
		return true;
	}
}
";
                }
                tbScript=new ICSharpCode.TextEditor.TextEditorControl();
                panel1.Controls.Add(tbScript);
                tbScript.Text=text;
                tbScript.ShowVRuler=false;
                if(!Program.MonoMode) tbScript.SetHighlighting("C#");
                tbScript.Dock=DockStyle.Fill;
                tbScript.TextChanged+=textChanged;
                tbScript.Document.FoldingManager.FoldingStrategy=new CodeFolder();
                tbScript.Document.FoldingManager.UpdateFoldings(null, null);
                UsingScript=true;
                ToolStripButton bSyntaxCheck = new ToolStripButton();
                bSyntaxCheck.DisplayStyle = ToolStripItemDisplayStyle.Text;
                bSyntaxCheck.Size = new System.Drawing.Size(76, 22);
                bSyntaxCheck.Text = "Check syntax";
                timer=new Timer();
                timer.Interval=1000;
                timer.Tick+=new EventHandler(timer_Tick);
                bSyntaxCheck.Click += new System.EventHandler(this.bSyntaxCheck_Click);
                toolStrip1.Items.Add(bSyntaxCheck);
                break;
            }
        }

        void timer_Tick(object sender, EventArgs e) {
            if(DateTime.Now>TimerNext) {
                tbScript.Document.FoldingManager.UpdateFoldings(null, null);
                //tbScript.Invalidate();
                tbScript.Document.FoldingManager.NotifyFoldingsChanged(null);
                timer.Stop();
            }
        }

        private DateTime TimerNext;
        private Timer timer;

        private bool changed;
        private void textChanged(object sender, EventArgs e) {
            changed=true;
            if(UsingScript) {
                TimerNext=DateTime.Now+TimeSpan.FromSeconds(1);
                if(!timer.Enabled) timer.Start();
            } else rtbEdit.TextChanged-=textChanged;
        }

        public static string ShowEditor(string initial, TextEditorType type, bool dialog) {
            TextEditor se=new TextEditor(initial, type);
            if(dialog) se.ShowDialog();
            else se.Show();
            return se.saved;
        }

        private void bSave_Click(object sender, EventArgs e) {
            if(UsingScript) {
                saved=tbScript.Text;
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

    class CodeFolder : IFoldingStrategy {
        public List<FoldMarker> GenerateFoldMarkers(IDocument document, string fileName, object parseInformation) {
            List<FoldMarker> list = new List<FoldMarker>();

            Stack<int> stack=new Stack<int>();
            //bool InComment;

            for(int i = 0;i < document.TotalNumberOfLines;i++) {
                string text = document.GetText(document.GetLineSegment(i)).Trim();
                if(text.StartsWith("}")&&stack.Count>0) {
                    int pos=stack.Pop();
                    list.Add(new FoldMarker(document, pos, document.GetLineSegment(pos).Length, i, 1));
                }
                if(text.EndsWith("{")) {
                    stack.Push(i);
                }
            }

            return list;
        }
    }
}