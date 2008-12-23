using System;
using fomm;
using fomm.PackageManager;

namespace fomm.Scripting {
    public abstract class BaseScript {

        public static void MessageBox(string message) { ScriptFunctions.MessageBox(message); }
        public static void MessageBox(string message, string title) { ScriptFunctions.MessageBox(message, title); }
        public static System.Windows.Forms.DialogResult MessageBox(string message, string title, System.Windows.Forms.MessageBoxButtons buttons) { return ScriptFunctions.MessageBox(message, title, buttons); }
        public string[] GetFileList() { return ScriptFunctions.GetFileList(); }
        public static bool InstallFileFromFomod(string file) { return ScriptFunctions.InstallFileFromFomod(file); }

    }
}
