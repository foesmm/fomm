using System;
using System.Windows.Forms;

namespace fomm.PackageManager {
    public partial class ScriptEditor : Form {
        private string saved;

        private ScriptEditor(string text) {
            InitializeComponent();
            tbScript.Text=text;
            tbScript.ShowVRuler=false;
            tbScript.SetHighlighting("C#");
        }

        public static string ShowEditor(string initial) {
            ScriptEditor se=new ScriptEditor(initial);
            se.ShowDialog();
            return se.saved;
        }

        private void bSave_Click(object sender, EventArgs e) {
            saved=tbScript.Text;
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