using System;
using System.Security.Permissions;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Xml;
using System.Windows.Forms;

//Get/set global variables
//Block original files from being overwritten

namespace Fomm.PackageManager {
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
            permissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess, new string[] {
                Program.exeDir, Program.Fallout3SaveDir, Program.tmpPath, Program.LocalDataPath, Environment.CurrentDirectory
            }));
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
            ddsParserInited=false;
            textures=new List<IntPtr>();
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
            if(ddsParserInited) {
                for(int i=0;i<textures.Count;i++) NativeMethods.ddsRelease(textures[i]);
                textures=null;
                NativeMethods.ddsClose();
                ddsParserInited=false;
            }
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
            if(dataFileNode.SelectSingleNode("descendant::file[.=\""+ldatapath+"\"]")==null) {
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

        private static bool EditINI(string file, string section, string key, string value, bool saveOld) {
            permissions.Assert();
            if(iniEditsNode==null) {
                rootNode.AppendChild(iniEditsNode=xmlDoc.CreateElement("iniEdits"));
            }
            if(iniEditsNode.SelectSingleNode("descendant::ini[@file='"+file+"' and @section='"+section+"' and @key='"+key+"']")!=null) {
                LastError="You've already set this ini key";
                return false;
            }
            string oldmod, oldvalue=InstallLog.GetIniEdit(file, section, key, out oldmod);
            if(oldmod!=null) {
                if(System.Windows.Forms.MessageBox.Show("Key '"+key+"' in section '"+section+"' of fallout.ini has already been overwritten by '"+oldmod+"'\n"+
                    "Overwrite again with this mod?\n"+
                    "Current value '"+oldvalue+"', new value '"+value+"'", "", MessageBoxButtons.YesNo)!=DialogResult.Yes) {
                    LastError="User chose not to overwrite old value";
                    return false;
                } else if(!saveOld) {
                    InstallLog.UndoIniEdit(file, section, key);
                }
            }
            if(saveOld) {
                XmlElement node=xmlDoc.CreateElement("ini");
                node.Attributes.Append(xmlDoc.CreateAttribute("file"));
                node.Attributes.Append(xmlDoc.CreateAttribute("section"));
                node.Attributes.Append(xmlDoc.CreateAttribute("key"));
                node.Attributes[0].Value=file;
                node.Attributes[1].Value=section;
                node.Attributes[2].Value=key;
                iniEditsNode.AppendChild(node);
                InstallLog.AddIniEdit(file, section, key, mod.baseName);
            }
            NativeMethods.WritePrivateProfileStringA(section, key, value, file);
            return true;
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
                if(!ze.Name.StartsWith("fomod", StringComparison.OrdinalIgnoreCase)) files.Add(ze.Name);
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

        public static Version GetFalloutVersion() {
            permissions.Assert();
            if(File.Exists("Fallout3.exe")) return new Version(System.Diagnostics.FileVersionInfo.GetVersionInfo("Fallout3.exe").FileVersion.Replace(", ", "."));
            else if(File.Exists("Fallout3ng.exe")) return new Version(System.Diagnostics.FileVersionInfo.GetVersionInfo("Fallout3ng.exe").FileVersion.Replace(", ", "."));
            else return null;
        }

        public static Version GetGeckVersion() {
            permissions.Assert();
            if(!File.Exists("geck.exe")) return null;
            return new Version(System.Diagnostics.FileVersionInfo.GetVersionInfo("geck.exe").FileVersion.Replace(", ", "."));
        }

        public static string GetLastError() { return LastError; }

        public static string[] GetAllPlugins() {
            permissions.Assert();
            List<string> lfiles=new List<string>();
            lfiles.AddRange(Directory.GetFiles("data", "*.esm"));
            lfiles.AddRange(Directory.GetFiles("data", "*.esp"));
            FileInfo[] files=new FileInfo[lfiles.Count];
            for(int i=0;i<files.Length;i++) files[i]=new FileInfo(lfiles[i]);
            Array.Sort<FileInfo>(files, delegate(FileInfo a, FileInfo b)
            {
                return a.LastWriteTime.CompareTo(b.LastWriteTime);
            });
            string[] result=new string[files.Length];
            for(int i=0;i<files.Length;i++) result[i]=files[i].Name;
            return result;
        }

        public static string[] GetActivePlugins() {
            permissions.Assert();
            if(activePlugins==null) LoadActivePlugins();
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

        public static void SetLoadOrder(int[] plugins) {
            string[] names=GetAllPlugins();
            if(plugins.Length!=names.Length) {
                LastError="Length of new load order array was different to the total number of plugins";
                return;
            }
            for(int i=0;i<plugins.Length;i++) {
                if(plugins[i]<0||plugins[i]>=plugins.Length) {
                    LastError="A plugin index was out of range";
                    return;
                }
            }
            permissions.Assert();
            DateTime timestamp=new DateTime(2008, 1, 1);
            TimeSpan twomins=TimeSpan.FromMinutes(2);

            for(int i=0;i<names.Length;i++) {
                //if(Array.BinarySearch<int>(plugins, i)>=0) continue;
                File.SetLastWriteTime(Path.Combine("data\\", names[plugins[i]]), timestamp);
                timestamp+=twomins;
            }
        }
        public static void SetLoadOrder(int[] plugins, int position) {
            string[] names=GetAllPlugins();
            permissions.Assert();
            Array.Sort<int>(plugins);
            DateTime timestamp=new DateTime(2008, 1, 1);
            TimeSpan twomins=TimeSpan.FromMinutes(2);
            
            for(int i=0;i<position;i++) {
                if(Array.BinarySearch<int>(plugins, i)>=0) continue;
                File.SetLastWriteTime(Path.Combine("data\\", names[i]), timestamp);
                timestamp+=twomins;
            }
            for(int i=0;i<plugins.Length;i++) {
                File.SetLastWriteTime(Path.Combine("data\\", names[plugins[i]]), timestamp);
                timestamp+=twomins;
            }
            for(int i=position;i<names.Length;i++) {
                if(Array.BinarySearch<int>(plugins, i)>=0) continue;
                File.SetLastWriteTime(Path.Combine("data\\", names[i]), timestamp);
                timestamp+=twomins;
            }
        }

        public static string GetFalloutIniString(string section, string value) {
            permissions.Assert();
            return NativeMethods.GetPrivateProfileString(section, value, null, Program.FOIniPath);
        }

        public static int GetFalloutIniInt(string section, string value) {
            permissions.Assert();
            return NativeMethods.GetPrivateProfileIntA(section, value, 0, Program.FOIniPath);
        }

        public static string GetPrefsIniString(string section, string value) {
            permissions.Assert();
            return NativeMethods.GetPrivateProfileString(section, value, null, Program.FOPrefsIniPath);
        }

        public static int GetPrefsIniInt(string section, string value) {
            permissions.Assert();
            return NativeMethods.GetPrivateProfileIntA(section, value, 0, Program.FOPrefsIniPath);
        }

        public static string GetGeckIniString(string section, string value) {
            permissions.Assert();
            return NativeMethods.GetPrivateProfileString(section, value, null, Program.GeckIniPath);
        }

        public static int GetGeckIniInt(string section, string value) {
            permissions.Assert();
            return NativeMethods.GetPrivateProfileIntA(section, value, 0, Program.GeckIniPath);
        }

        public static string GetGeckPrefsIniString(string section, string value) {
            permissions.Assert();
            return NativeMethods.GetPrivateProfileString(section, value, null, Program.GeckPrefsIniPath);
        }

        public static int GetGeckPrefsIniInt(string section, string value) {
            permissions.Assert();
            return NativeMethods.GetPrivateProfileIntA(section, value, 0, Program.GeckPrefsIniPath);
        }

        public static string GetRendererInfo(string value) {
            permissions.Assert();
            string[] lines=File.ReadAllLines(Program.FORendererFile);
            for(int i=1;i<lines.Length;i++) {
                if(!lines[i].Contains(":")) continue;
                string val=lines[i].Remove(lines[i].IndexOf(':')).Trim();
                if(val==value) return lines[i].Substring(lines[i].IndexOf(':')+1).Trim();
            }
            return null;
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
            return EditINI(Program.FOIniPath, section, key, value, saveOld);
        }
        public static bool EditPrefsINI(string section, string key, string value, bool saveOld) {
            return EditINI(Program.FOPrefsIniPath, section, key, value, saveOld);
        }
        public static bool EditGeckINI(string section, string key, string value, bool saveOld) {
            return EditINI(Program.GeckPrefsIniPath, section, key, value, saveOld);
        }
        public static bool EditGeckPrefsINI(string section, string key, string value, bool saveOld) {
            return EditINI(Program.GeckPrefsIniPath, section, key, value, saveOld);
        }
        public static bool EditShader(int package, string name, byte[] data) {
            permissions.Assert();
            if(InstallLog.IsShaderEdited(package, name)) {
                if(System.Windows.Forms.MessageBox.Show("Shader '"+name+"' in package '"+package+"' has already been edited by another mod\n"+
                    "Overwrite the changes?", "", MessageBoxButtons.YesNo)!=DialogResult.Yes) {
                    LastError="User chose not to overwrite old changes";
                    return false;
                }
            }
            byte[] oldData;
            if(!SDPArchives.EditShader(package, name, data, out oldData)) {
                LastError="Failed to edit the shader";
                return false;
            }
            if(sdpEditsNode==null) {
                rootNode.AppendChild(sdpEditsNode=xmlDoc.CreateElement("sdpEdits"));
            }
            InstallLog.AddShaderEdit(package, name, oldData);
            XmlElement node=xmlDoc.CreateElement("sdp");
            node.Attributes.Append(xmlDoc.CreateAttribute("package"));
            node.Attributes.Append(xmlDoc.CreateAttribute("shader"));
            node.Attributes.Append(xmlDoc.CreateAttribute("crc"));
            node.Attributes[0].Value=package.ToString();
            node.Attributes[1].Value=name;
            ICSharpCode.SharpZipLib.Checksums.Crc32 crc=new ICSharpCode.SharpZipLib.Checksums.Crc32();
            crc.Update(data);
            node.Attributes[2].Value=crc.Value.ToString();
            sdpEditsNode.AppendChild(node);
            return true;
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

        public static string[] GetBSAFileList(string bsa) {
            if(!IsSafeFilePath(bsa)||bsa.Contains("\\")||bsa.Contains("/")) {
                LastError="Invalid file path";
                return null;
            }
            permissions.Assert();
            if(!bsas.ContainsKey(bsa)) {
                try {
                    bsas[bsa]=new BSAArchive(Path.Combine("data", bsa));
                } catch(BSAArchive.BSALoadException) {
                    LastError="Failed to load BSA";
                    return null;
                }
            }
            return (string[])bsas[bsa].FileNames.Clone();
        }

        public static Form CreateCustomForm() {
            permissions.Assert();
            return new Form();
        }

        public static void SetupScriptCompiler(Fomm.TESsnip.Plugin[] plugins) {
            permissions.Assert();
            Fomm.ScriptCompiler.ScriptCompiler.Setup(plugins);
        }
        public static void CompileResultScript(Fomm.TESsnip.SubRecord sr, out Fomm.TESsnip.Record r2, out string msg) {
            Fomm.ScriptCompiler.ScriptCompiler.CompileResultScript(sr, out r2, out msg);
        }
        public static void CompileScript(Fomm.TESsnip.Record r2, out string msg) {
            Fomm.ScriptCompiler.ScriptCompiler.Compile(r2, out msg);
        }

        public static bool IsAIActive() { return ArchiveInvalidation.IsActive(); }

        private static bool ddsParserInited;
        private static List<IntPtr> textures;

        public static IntPtr LoadTexture(byte[] bytes) {
            permissions.Assert();
            if(!ddsParserInited) {
                NativeMethods.ddsInit(Application.OpenForms[0].Handle);
                ddsParserInited=true;
            }
            IntPtr ptr=NativeMethods.ddsLoad(bytes, bytes.Length);
            if(ptr!=IntPtr.Zero) textures.Add(ptr);
            return ptr;
        }
        public static IntPtr CreateTexture(int width, int height) {
            permissions.Assert();
            if(!ddsParserInited) {
                NativeMethods.ddsInit(Application.OpenForms[0].Handle);
                ddsParserInited=true;
            }
            IntPtr ptr=NativeMethods.ddsCreate(width, height);
            if(ptr!=IntPtr.Zero) textures.Add(ptr);
            return ptr;
        }
        public static byte[] SaveTexture(IntPtr tex, int format, bool mipmaps) {
            if(!ddsParserInited||!textures.Contains(tex)) return null;
            permissions.Assert();
            int length;
            IntPtr data=NativeMethods.ddsSave(tex, format, mipmaps?1:0, out length);
            if(data==IntPtr.Zero) return null;
            byte[] result=new byte[length];
            System.Runtime.InteropServices.Marshal.Copy(data, result, 0, length);
            return result;
        }
        public static void CopyTexture(IntPtr source, System.Drawing.Rectangle sourceRect, IntPtr dest, System.Drawing.Rectangle destRect) {
            if(!ddsParserInited||!textures.Contains(source)||!textures.Contains(dest)) return;
            permissions.Assert();
            NativeMethods.ddsBlt(source, sourceRect.Left, sourceRect.Top, sourceRect.Width, sourceRect.Height, dest, destRect.Left,
                destRect.Top, destRect.Width, destRect.Height);
        }
        public static void GetTextureSize(IntPtr tex, out int width, out int height) {
            width=0;
            height=0;
            if(!ddsParserInited||!textures.Contains(tex)) return;
            permissions.Assert();
            NativeMethods.ddsGetSize(tex, out width, out height);
        }
        public static byte[] GetTextureData(IntPtr tex, out int pitch) {
            pitch=0;
            if(!ddsParserInited||!textures.Contains(tex)) return null;
            permissions.Assert();
            int length;
            IntPtr ptr=NativeMethods.ddsLock(tex, out length, out pitch);
            if(ptr==IntPtr.Zero) return null;
            byte[] result=new byte[length];
            System.Runtime.InteropServices.Marshal.Copy(ptr, result, 0, length);
            NativeMethods.ddsUnlock(tex);
            return result;
        }
        public static void SetTextureData(IntPtr tex, byte[] data) {
            if(!ddsParserInited||!textures.Contains(tex)) return;
            permissions.Assert();
            NativeMethods.ddsSetData(tex, data, data.Length);
        }
        public static void ReleaseTexture(IntPtr tex) {
            if(!ddsParserInited||!textures.Contains(tex)) return;
            permissions.Assert();
            NativeMethods.ddsRelease(tex);
            textures.Remove(tex);
        }
    }
}
