using System;
using fomm;
using fomm.PackageManager;

namespace fomm.Scripting {
    public abstract class BaseScript {

        public static void MessageBox(string message) { ScriptFunctions.MessageBox(message); }
        public static void MessageBox(string message, string title) { ScriptFunctions.MessageBox(message, title); }
        public static System.Windows.Forms.DialogResult MessageBox(string message, string title, System.Windows.Forms.MessageBoxButtons buttons) { return ScriptFunctions.MessageBox(message, title, buttons); }
        public string[] GetFomodFileList() { return ScriptFunctions.GetFomodFileList(); }
        public static bool InstallFileFromFomod(string file) { return ScriptFunctions.InstallFileFromFomod(file); }
        public static void SetPluginActivation(string s, bool activate) { ScriptFunctions.SetPluginActivation(s, activate); }
        public static Version GetFommVersion() { return ScriptFunctions.GetFommVersion(); }
        public static bool ScriptExtenderPresent() { return ScriptFunctions.ScriptExtenderPresent(); }
        public static Version GetFoseVersion() { return ScriptFunctions.GetFoseVersion(); }
        public static string GetLastError() { return ScriptFunctions.GetLastError(); }
    }
}
