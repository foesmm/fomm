using System;
using System.Security.Permissions;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Xml;
using System.Windows.Forms;

namespace fomm.PackageManager {
    public static class ScriptFunctions {
        private static readonly System.Security.PermissionSet permissions;

        private static fomod mod;
        private static ZipFile file;

        private static readonly List<string> OverwriteFolders=new List<string>();
        private static readonly List<string> DontOverwriteFolders=new List<string>();
        private static bool DontOverwriteAll;
        private static bool OverwriteAll;
        private static XmlDocument xmlDoc;
        private static XmlElement rootNode;
        private static XmlElement dataFileNode;

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
            xmlDoc=new XmlDocument();
            rootNode=xmlDoc.CreateElement("installData");
            xmlDoc.AppendChild(rootNode);

            dataFileNode=null;
        }

        internal static XmlDocument BasicInstallScript() {
            foreach(string file in GetFomodFileList()) InstallFileFromFomod(file);
            return xmlDoc;
        }

        internal static XmlDocument CustomInstallScript(string script) {
            permissions.Assert();
            ScriptCompiler.Execute(script);
            return xmlDoc;
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

        public static bool InstallFileFromFomod(string file) {
            permissions.Assert();
            string datapath=Path.GetFullPath(Path.Combine("Data", file));
            string ldatapath=datapath.ToLowerInvariant();
            ZipEntry ze=ScriptFunctions.file.GetEntry(file);
            //if(ze==null) return false;
            if(!Directory.Exists(Path.GetDirectoryName(datapath))) {
                Directory.CreateDirectory(Path.GetDirectoryName(datapath));
            } else {
                if(!TestDoOverwrite(datapath)) return false;
                else File.Delete(datapath);
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
            if(dataFileNode==null) {
                dataFileNode=xmlDoc.CreateElement("installedFiles");
                rootNode.AppendChild(dataFileNode);
            }
            if(dataFileNode.SelectSingleNode("descendant::file[.=\""+ldatapath.Replace("&", "&amp;")+"\"]")==null) {
                dataFileNode.AppendChild(xmlDoc.CreateElement("file"));
                dataFileNode.LastChild.InnerText=ldatapath;
            }
            InstallLog.InstallDataFile(ldatapath);
            return true;
        }

        //public static string GetLastError() { return LastError; }
    }
}
