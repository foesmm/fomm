using System;
using System.Security.Permissions;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Xml;
using System.Windows.Forms;

//Edit sdp
//List data files in data folder
//Get file from bsa
//Block original files from being overwritten

namespace fomm.PackageManager {
    public static class ScriptFunctions {
        private static readonly System.Security.PermissionSet permissions;

        private static fomod mod;
        private static ZipFile file;

        private static readonly List<string> OverwriteFolders=new List<string>();
        private static readonly List<string> DontOverwriteFolders=new List<string>();
        private static readonly Dictionary<string, BSAArchive> bsas=new Dictionary<string, BSAArchive>();
        private static List<string> activePlugins;
        private static bool DontOverwriteAll;
        private static bool OverwriteAll;
        private static XmlDocument xmlDoc;
        private static XmlElement rootNode;
        private static XmlElement dataFileNode;
        private static XmlElement iniEditsNode;
        private static XmlElement sdpEditsNode;
        private static string LastError;

        static ScriptFunctions() {
            permissions=new System.Security.PermissionSet(PermissionState.None);
            List<string> paths=new List<string>(4);
            paths.Add(Program.exeDir);
            paths.Add(Program.Fallout3SaveDir);
            paths.Add(Program.tmpPath);
            paths.Add(Environment.CurrentDirectory);
            permissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess, paths.ToArray()));
            permissions.AddPermission(new UIPermission(UIPermissionWindow.AllWindows));
        }

        internal static void Setup(fomod mod, ZipFile file) {
            ScriptFunctions.mod=mod;
            ScriptFunctions.file=file;
            DontOverwriteAll=false;
            OverwriteAll=false;
            OverwriteFolders.Clear();
            DontOverwriteFolders.Clear();
            bsas.Clear();
            xmlDoc=new XmlDocument();
            rootNode=xmlDoc.CreateElement("installData");
            xmlDoc.AppendChild(rootNode);
            activePlugins=null;

            dataFileNode=null;
        }

        internal static XmlDocument BasicInstallScript() {
            char[] seperators=new char[] {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar};
            foreach(string file in GetFomodFileList()) {
                InstallFileFromFomod(file);
                string ext=Path.GetExtension(file).ToLowerInvariant();
                if((ext==".esp"||ext==".esm")&&file.IndexOfAny(seperators)==-1) {
                    SetPluginActivation(file, true);
                }
            }
            Cleanup();
            return xmlDoc;
        }

        internal static XmlDocument CustomInstallScript(string script) {
            //permissions.Assert();
            bool b=ScriptCompiler.Execute(script);
            Cleanup();
            if(b) return xmlDoc;
            else return null;
        }

        private static void Cleanup() {
            CommitActivePlugins();
            activePlugins=null;
            rootNode=dataFileNode=iniEditsNode=sdpEditsNode=null;
            OverwriteFolders.Clear();
            DontOverwriteFolders.Clear();
            foreach(BSAArchive ba in bsas.Values) ba.Dispose();
            bsas.Clear();
        }

        private static void CommitActivePlugins() {
            if(activePlugins==null) return;
            File.WriteAllLines(Program.PluginsFile, activePlugins.ToArray());
        }

        private static bool IsSafeFilePath(string path) {
            if(path.IndexOfAny(Path.GetInvalidPathChars())!=-1) return false;
            if(Path.IsPathRooted(path)) return false;
            if(path.Contains("..")) return false;
            return true;
        }

        private static bool TestDoOverwrite(string path) {
            if(!File.Exists(path)) return true;
            string lpath=path.ToLowerInvariant();
            if(OverwriteFolders.Contains(lpath)) return true;
            if(DontOverwriteFolders.Contains(lpath)) return false;
            if(OverwriteAll) return true;
            if(DontOverwriteAll) return false;
            switch(Overwriteform.ShowDialog("Data file '"+path+"' already exists.\nOverwrite?", true)) {
            case OverwriteResult.Yes:
                return true;
            case OverwriteResult.No:
                return false;
            case OverwriteResult.NoToAll:
                DontOverwriteAll=true;
                return false;
            case OverwriteResult.YesToAll:
                OverwriteAll=true;
                return true;
            case OverwriteResult.NoToFolder:
                Queue<string> folders=new Queue<string>();
                folders.Enqueue(Path.GetDirectoryName(lpath));
                while(folders.Count>0) {
                    lpath=folders.Dequeue();
                    if(!OverwriteFolders.Contains(lpath)) {
                        DontOverwriteFolders.Add(lpath);
                        foreach(string s in Directory.GetDirectories(lpath)) {
                            folders.Enqueue(s.ToLowerInvariant());
                        }
                    }
                }
                return false;
            case OverwriteResult.YesToFolder:
                folders=new Queue<string>();
                folders.Enqueue(Path.GetDirectoryName(lpath));
                while(folders.Count>0) {
                    lpath=folders.Dequeue();
                    if(!DontOverwriteFolders.Contains(lpath)) {
                        OverwriteFolders.Add(lpath);
                        foreach(string s in Directory.GetDirectories(lpath)) {
                            folders.Enqueue(s.ToLowerInvariant());
                        }
                    }
                }
                return true;
            default:
                throw new Exception("Sanity check failed: OverwriteDialog returned a value not present in the OverwriteResult enum");
            }
        }

        private static void AddDataFile(string datapath) {
            string ldatapath=datapath.ToLowerInvariant();
            if(dataFileNode==null) {
                rootNode.AppendChild(dataFileNode=xmlDoc.CreateElement("installedFiles"));
            }
            if(dataFileNode.SelectSingleNode("descendant::file[.=\""+ldatapath.Replace("&", "&amp;")+"\"]")==null) {
                dataFileNode.AppendChild(xmlDoc.CreateElement("file"));
                dataFileNode.LastChild.InnerText=ldatapath;
                InstallLog.InstallDataFile(ldatapath);
            }
        }

        private static void LoadActivePlugins() {
            if(File.Exists(Program.PluginsFile)) {
                string[] lines=File.ReadAllLines(Program.PluginsFile);
                for(int i=0;i<lines.Length;i++) lines[i]=lines[i].Trim().ToLowerInvariant();
                activePlugins=new List<string>(lines);
            } else {
                activePlugins=new List<string>();
            }
        }

        public static void MessageBox(string message) {
            permissions.Assert();
            System.Windows.Forms.MessageBox.Show(message);
        }
        public static void MessageBox(string message, string title) {
            permissions.Assert();
            System.Windows.Forms.MessageBox.Show(message, title);
        }
        public static DialogResult MessageBox(string message, string title, MessageBoxButtons buttons) {
            permissions.Assert();
            return System.Windows.Forms.MessageBox.Show(message, title, buttons);
        }

        public static string[] GetFomodFileList() {
            permissions.Assert();
            List<string> files=new List<string>();
            foreach(ZipEntry ze in file) {
                if(ze.IsDirectory) continue;
                if(!ze.Name.StartsWith("fomod", StringComparison.InvariantCultureIgnoreCase)) files.Add(ze.Name);
            }
            return files.ToArray();
        }

        public static byte[] GetFileFromFomod(string file) {
            permissions.Assert();
            ZipEntry ze=ScriptFunctions.file.GetEntry(file.Replace('\\', '/'));
            if(ze==null||!ze.CanDecompress) return null;
            Stream s=ScriptFunctions.file.GetInputStream(ze);
            byte[] buffer=new byte[ze.Size];
            int upto=0, size;
            while((size=s.Read(buffer, 0, (int)ze.Size-upto))>0) upto+=size;
            return buffer;
        }

        public static bool GenerateDataFile(string path, byte[] data) {
            if(!IsSafeFilePath(path)) {
                LastError="Illegal file path";
                return false;
            }
            permissions.Assert();
            string datapath=Path.GetFullPath(Path.Combine("Data", path));
            if(!Directory.Exists(Path.GetDirectoryName(datapath))) {
                Directory.CreateDirectory(Path.GetDirectoryName(datapath));
            } else {
                if(!TestDoOverwrite(datapath)) {
                    LastError="User chose not to overwrite";
                    return false;
                } else File.Delete(datapath);
            }
            File.WriteAllBytes(datapath, data);
            AddDataFile(datapath);
            return true;
        }

        public static bool DataFileExists(string path) {
            if(!IsSafeFilePath(path)) {
                LastError="Illegal file path";
                return false;
            }
            permissions.Assert();
            string datapath=Path.Combine("Data", path);
            return File.Exists(datapath);
        }

        public static string[] GetExistingDataFileList(string path, string pattern, bool allFolders) {
            if(!IsSafeFilePath(path)) {
                LastError="Illegal file path";
                return null;
            }
            permissions.Assert();
            return Directory.GetFiles(Path.Combine("Data", path), pattern, allFolders?SearchOption.AllDirectories:SearchOption.TopDirectoryOnly);
        }

        public static byte[] GetExistingDataFile(string path) {
            if(!IsSafeFilePath(path)) {
                LastError="Illegal file path";
                return null;
            }
            permissions.Assert();
            string datapath=Path.Combine("Data", path);
            if(!File.Exists(datapath)) {
                LastError="File not found";
                return null;
            }
            return File.ReadAllBytes(datapath);
        }

        public static bool InstallFileFromFomod(string file) {
            permissions.Assert();
            string datapath=Path.GetFullPath(Path.Combine("Data", file));
            string ldatapath=datapath.ToLowerInvariant();
            ZipEntry ze=ScriptFunctions.file.GetEntry(file.Replace('\\', '/'));
            if(ze==null) {
                LastError="File doesn't exist in fomod";
                return false;
            }
            if(!Directory.Exists(Path.GetDirectoryName(datapath))) {
                Directory.CreateDirectory(Path.GetDirectoryName(datapath));
            } else {
                if(!TestDoOverwrite(datapath)) {
                    LastError="User chose not to overwrite";
                    return false;
                } else File.Delete(datapath);
            }
            FileStream fs=File.Create(datapath);
            Stream s=ScriptFunctions.file.GetInputStream(ze);
            byte[] buffer=new byte[4096];
            int bytes;
            while((bytes=s.Read(buffer, 0, 4096))>0) {
                fs.Write(buffer, 0, bytes);
            }
            fs.Close();
            s.Close();
            AddDataFile(datapath);
            AddDataFile(datapath);
            return true;
        }

        public static void SetPluginActivation(string s, bool activate) {
            permissions.Assert();
            if(activePlugins==null) LoadActivePlugins();
            s=s.ToLowerInvariant();
            if(s.IndexOfAny(Path.GetInvalidFileNameChars())!=-1) {
                LastError="Illegal plugin path";
                return;
            }
            if(!File.Exists("Data\\"+s)) {
                LastError="Plugin '"+s+"' does not exist";
                return;
            }
            if(activate) {
                if(!activePlugins.Contains(s)) activePlugins.Add(s);
            } else {
                activePlugins.Remove(s);
            }
        }

        public static Version GetFommVersion() {
            permissions.Assert();
            return Program.MVersion;
        }

        public static bool ScriptExtenderPresent() {
            permissions.Assert();
            return File.Exists("fose_loader.exe");
        }

        public static Version GetFoseVersion() {
            permissions.Assert();
            if(!File.Exists("fose_loader.exe")) return null;
            return new Version(System.Diagnostics.FileVersionInfo.GetVersionInfo("fose_loader.exe").FileVersion.Replace(", ", "."));
        }

        public static string GetLastError() { return LastError; }

        public static string[] GetActivePlugins() {
            permissions.Assert();
            FileInfo[] files=new FileInfo[activePlugins.Count];
            for(int i=0;i<files.Length;i++) files[i]=new FileInfo(Path.Combine("data", activePlugins[i]));
            Array.Sort<FileInfo>(files, delegate(FileInfo a, FileInfo b)
            {
                return a.LastWriteTime.CompareTo(b.LastWriteTime);
            });
            string[] result=new string[files.Length];
            for(int i=0;i<files.Length;i++) result[i]=files[i].Name;
            return result;
        }

        public static string GetFalloutIniString(string section, string value) {
            permissions.Assert();
            return Imports.GetPrivateProfileString(section, value, null, Program.FOIniPath);
        }

        public static int GetFalloutIniInt(string section, string value) {
            permissions.Assert();
            return Imports.GetPrivateProfileIntA(section, value, 0, Program.FOIniPath);
        }

        public static string GetPrefsIniString(string section, string value) {
            permissions.Assert();
            return Imports.GetPrivateProfileString(section, value, null, Program.FOPrefsIniPath);
        }

        public static int GetPrefsIniInt(string section, string value) {
            permissions.Assert();
            return Imports.GetPrivateProfileIntA(section, value, 0, Program.FOPrefsIniPath);
        }

        public static int[] Select(string[] items, string[] previews, string[] descs, string title, bool many) {
            permissions.Assert();
            System.Drawing.Image[] ipreviews=null;
            if(previews!=null) {
                ipreviews=new System.Drawing.Image[previews.Length];
                int failcount=0;
                for(int i=0;i<previews.Length;i++) {
                    if(previews[i]==null) continue;
                    ZipEntry ze=file.GetEntry(previews[i].Replace('\\', '/'));
                    if(ze==null) failcount++;
                    else ipreviews[i]=System.Drawing.Image.FromStream(file.GetInputStream(ze));
                }
                if(failcount>0) {
                    LastError="There were "+failcount+" filenames specified for preview images which could not be loaded";
                }
            }
            SelectForm sf=new SelectForm(items, title, many, ipreviews, descs);
            sf.ShowDialog();
            int[] result=new int[sf.SelectedIndex.Length];
            for(int i=0;i<sf.SelectedIndex.Length;i++) result[i]=sf.SelectedIndex[i];
            return result;
        }

        public static bool EditFalloutINI(string section, string key, string value, bool saveOld) {
            permissions.Assert();
            if(iniEditsNode==null) {
                rootNode.AppendChild(iniEditsNode=xmlDoc.CreateElement("iniEdits"));
            }
            if(iniEditsNode.SelectSingleNode("descendant::ini[@file='"+Program.FOIniPath+"' and @section='"+section+"' and @key='"+key+"']")!=null) {
                LastError="You've already set this ini key";
                return false;
            }
            string oldmod, oldvalue=InstallLog.GetIniEdit(Program.FOIniPath, section, key, out oldmod);
            if(oldmod!=null) {
                if(System.Windows.Forms.MessageBox.Show("Key '"+key+"' in section '"+section+"' of fallout.ini has already been overwritten by '"+oldmod+"'\n"+
                    "Overwrite again with this mod?", "", MessageBoxButtons.YesNo)!=DialogResult.Yes) {
                    LastError="User chose not to overwrite old value";
                    return false;
                } else if(!saveOld) {
                    InstallLog.UndoIniEdit(Program.FOIniPath, section, key);
                }
            }
            if(saveOld) {
                XmlElement node=xmlDoc.CreateElement("ini");
                node.Attributes.Append(xmlDoc.CreateAttribute("file"));
                node.Attributes.Append(xmlDoc.CreateAttribute("section"));
                node.Attributes.Append(xmlDoc.CreateAttribute("key"));
                node.Attributes[0].Value=Program.FOIniPath;
                node.Attributes[1].Value=section;
                node.Attributes[2].Value=key;
                iniEditsNode.AppendChild(node);
                InstallLog.AddIniEdit(Program.FOIniPath, section, key, mod.baseName, value);
            }
            Imports.WritePrivateProfileStringA(section, key, value, Program.FOIniPath);
            return true;
        }
        public static bool EditShader(int package, string name, string path) {
            permissions.Assert();
            return false;
            //srd.SDPEdits.Add(new SDPEditInfo(package, name, DataFiles+path));
        }

        public static byte[] GetDataFileFromBSA(string bsa, string file) {
            if(!IsSafeFilePath(bsa)||!IsSafeFilePath(file)||bsa.Contains("\\")||bsa.Contains("/")) {
                LastError="Invalid file path";
                return null;
            }
            permissions.Assert();
            bsa=bsa.ToLowerInvariant();
            if(!bsas.ContainsKey(bsa)) {
                try {
                    bsas[bsa]=new BSAArchive(Path.Combine("data", bsa));
                } catch(BSAArchive.BSALoadException) {
                    LastError="Failed to load BSA";
                    return null;
                }
            }
            return bsas[bsa].GetFile(file);
        }
    }
}
