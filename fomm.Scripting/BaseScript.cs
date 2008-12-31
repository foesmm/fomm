using System;
using fomm;
using fomm.PackageManager;
using DialogResult=System.Windows.Forms.DialogResult;

namespace fomm.Scripting {
    public abstract class BaseScript {

        public static void MessageBox(string message) { ScriptFunctions.MessageBox(message); }
        public static void MessageBox(string message, string title) { ScriptFunctions.MessageBox(message, title); }
        public static DialogResult MessageBox(string message, string title, System.Windows.Forms.MessageBoxButtons buttons) { return ScriptFunctions.MessageBox(message, title, buttons); }
        public static string[] GetFomodFileList() { return ScriptFunctions.GetFomodFileList(); }
        public static bool InstallFileFromFomod(string file) { return ScriptFunctions.InstallFileFromFomod(file); }
        public static void SetPluginActivation(string s, bool activate) { ScriptFunctions.SetPluginActivation(s, activate); }
        public static Version GetFommVersion() { return ScriptFunctions.GetFommVersion(); }
        public static bool ScriptExtenderPresent() { return ScriptFunctions.ScriptExtenderPresent(); }
        public static Version GetFoseVersion() { return ScriptFunctions.GetFoseVersion(); }
        public static string GetLastError() { return ScriptFunctions.GetLastError(); }
        public static byte[] GetFileFromFomod(string file) { return ScriptFunctions.GetFileFromFomod(file); }
        public static bool GenerateDataFile(string path, byte[] data) { return ScriptFunctions.GenerateDataFile(path, data); }
        public static string[] GetActivePlugins() { return ScriptFunctions.GetActivePlugins(); }
        public static byte[] GetExistingDataFile(string path) { return ScriptFunctions.GetExistingDataFile(path); }
        public static string GetFalloutIniString(string section, string value) { return ScriptFunctions.GetFalloutIniString(section, value); }
        public static int GetFalloutIniInt(string section, string value) { return ScriptFunctions.GetFalloutIniInt(section, value); }
        public static string GetPrefsIniString(string section, string value) { return ScriptFunctions.GetPrefsIniString(section, value); }
        public static int GetPrefsIniInt(string section, string value) { return ScriptFunctions.GetPrefsIniInt(section, value); }
        public static bool DataFileExists(string path) { return ScriptFunctions.DataFileExists(path); }
        public static int[] Select(string[] items, string[] previews, string[] descs, string title, bool many) { return ScriptFunctions.Select(items, previews, descs, title, many); }
        public static bool EditFalloutINI(string section, string key, string value, bool saveOld) { return ScriptFunctions.EditFalloutINI(section, key, value, saveOld); }
        public static bool EditShader(int package, string name, string path) { return ScriptFunctions.EditShader(package, name, path); }
        public static string[] GetExistingDataFileList(string path, string pattern, bool allFolders) { return ScriptFunctions.GetExistingDataFileList(path, pattern, allFolders); }
        public static byte[] GetDataFileFromBSA(string bsa, string file) { return ScriptFunctions.GetDataFileFromBSA(bsa, file); }
    }
}
