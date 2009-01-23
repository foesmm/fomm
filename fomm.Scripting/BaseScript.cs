using System;
using fomm;
using fomm.PackageManager;
using DialogResult=System.Windows.Forms.DialogResult;

namespace fomm.Scripting {
    public struct SelectOption {
        public string Item;
        public string Preview;
        public string Desc;

        public SelectOption(string item, string preview, string desc) {
            Item=item;
            Preview=preview;
            Desc=desc;
        }
    }

    public abstract class BaseScript {

        public static void PerformBasicInstall() {
            char[] seperators=new char[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar };
            foreach(string file in GetFomodFileList()) {
                InstallFileFromFomod(file);
                string ext=System.IO.Path.GetExtension(file).ToLowerInvariant();
                if((ext==".esp"||ext==".esm")&&file.IndexOfAny(seperators)==-1) {
                    SetPluginActivation(file, true);
                }
            }
        }

        public static void MessageBox(string message) { ScriptFunctions.MessageBox(message); }
        public static void MessageBox(string message, string title) { ScriptFunctions.MessageBox(message, title); }
        public static DialogResult MessageBox(string message, string title, System.Windows.Forms.MessageBoxButtons buttons) { return ScriptFunctions.MessageBox(message, title, buttons); }

        public static int[] Select(SelectOption[] options, string title, bool many) {
            bool hasPreviews=false;
            bool hasDescs=false;
            foreach(SelectOption so in options) {
                if(so.Preview!=null) hasPreviews=true;
                if(so.Desc!=null) hasDescs=true;
            }
            string[] items=new string[options.Length];
            string[] previews=hasPreviews?new string[options.Length]:null;
            string[] descs=hasDescs?new string[options.Length]:null;
            for(int i=0;i<options.Length;i++) {
                items[i]=options[i].Item;
                if(hasPreviews) previews[i]=options[i].Preview;
                if(hasDescs) descs[i]=options[i].Desc;
            }
            return ScriptFunctions.Select(items, previews, descs, title, many);
        }
        public static int[] Select(string[] items, string[] previews, string[] descs, string title, bool many) { return ScriptFunctions.Select(items, previews, descs, title, many); }

        public static Version GetFommVersion() { return ScriptFunctions.GetFommVersion(); }
        public static bool ScriptExtenderPresent() { return ScriptFunctions.ScriptExtenderPresent(); }
        public static Version GetFoseVersion() { return ScriptFunctions.GetFoseVersion(); }
        public static Version GetFalloutVersion() { return ScriptFunctions.GetFalloutVersion(); }
        public static Version GetGeckVersion() { return ScriptFunctions.GetGeckVersion(); }

        public static string[] GetFomodFileList() { return ScriptFunctions.GetFomodFileList(); }
        public static bool InstallFileFromFomod(string file) { return ScriptFunctions.InstallFileFromFomod(file); }
        public static byte[] GetFileFromFomod(string file) { return ScriptFunctions.GetFileFromFomod(file); }

        public static string[] GetExistingDataFileList(string path, string pattern, bool allFolders) { return ScriptFunctions.GetExistingDataFileList(path, pattern, allFolders); }
        public static bool DataFileExists(string path) { return ScriptFunctions.DataFileExists(path); }
        public static byte[] GetExistingDataFile(string path) { return ScriptFunctions.GetExistingDataFile(path); }
        public static bool GenerateDataFile(string path, byte[] data) { return ScriptFunctions.GenerateDataFile(path, data); }

        public static string[] GetBSAFileList(string bsa) { return ScriptFunctions.GetBSAFileList(bsa); }
        public static byte[] GetDataFileFromBSA(string bsa, string file) { return ScriptFunctions.GetDataFileFromBSA(bsa, file); }

        public static string[] GetAllPlugins() { return ScriptFunctions.GetAllPlugins(); }
        public static string[] GetActivePlugins() { return ScriptFunctions.GetActivePlugins(); }
        public static void SetPluginActivation(string s, bool activate) { ScriptFunctions.SetPluginActivation(s, activate); }
        public static void SetLoadOrder(int[] plugins) { ScriptFunctions.SetLoadOrder(plugins); }
        public static void SetLoadOrder(int[] plugins, int position) { ScriptFunctions.SetLoadOrder(plugins, position); }

        public static string GetFalloutIniString(string section, string value) { return ScriptFunctions.GetFalloutIniString(section, value); }
        public static int GetFalloutIniInt(string section, string value) { return ScriptFunctions.GetFalloutIniInt(section, value); }
        public static bool EditFalloutINI(string section, string key, string value, bool saveOld) { return ScriptFunctions.EditFalloutINI(section, key, value, saveOld); }

        public static string GetPrefsIniString(string section, string value) { return ScriptFunctions.GetPrefsIniString(section, value); }
        public static int GetPrefsIniInt(string section, string value) { return ScriptFunctions.GetPrefsIniInt(section, value); }
        public static bool EditPrefsINI(string section, string key, string value, bool saveOld) { return ScriptFunctions.EditPrefsINI(section, key, value, saveOld); }

        public static string GetGeckIniString(string section, string value) { return ScriptFunctions.GetGeckIniString(section, value); }
        public static int GetGeckIniInt(string section, string value) { return ScriptFunctions.GetGeckIniInt(section, value); }
        public static bool EditGeckINI(string section, string key, string value, bool saveOld) { return ScriptFunctions.EditGeckINI(section, key, value, saveOld); }

        public static string GetGeckPrefsIniString(string section, string value) { return ScriptFunctions.GetGeckPrefsIniString(section, value); }
        public static int GetGeckPrefsIniInt(string section, string value) { return ScriptFunctions.GetGeckPrefsIniInt(section, value); }
        public static bool EditGeckPrefsINI(string section, string key, string value, bool saveOld) { return ScriptFunctions.EditGeckPrefsINI(section, key, value, saveOld); }

        public static string GetRendererInfo(string value) { return ScriptFunctions.GetRendererInfo(value); }

        public static bool EditShader(int package, string name, byte[] data) { return ScriptFunctions.EditShader(package, name, data); }

        public static string GetLastError() { return ScriptFunctions.GetLastError(); }

        public static System.Windows.Forms.Form CreateCustomForm() { return ScriptFunctions.CreateCustomForm(); }
    }
}
