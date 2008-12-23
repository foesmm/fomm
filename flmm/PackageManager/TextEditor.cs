using System;
using System.Windows.Forms;

namespace fomm.PackageManager {
    public partial class TextEditor : Form {
        private string saved;

        private TextEditor(string text, bool rtf) {
            InitializeComponent();
            if(rtf) rtbEdit.Rtf=text;
            else rtbEdit.Text=text;
        }

        public static string ShowEditor(string initial, bool rtf) {
            TextEditor se=new TextEditor(initial, rtf);
            se.ShowDialog();
            return se.saved;
        }

        private void bSave_Click(object sender, EventArgs e) {
            if(rtbEdit.TextLength==0) saved="";
            else saved=rtbEdit.Rtf;
        }
    }
}