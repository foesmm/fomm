using System;
using ICSharpCode.SharpZipLib.Zip;
using System.Xml;
using File=System.IO.File;
using Path=System.IO.Path;

/*
 * XML Structure
 * <installData>
 *   <installedFiles>
 *     <file>filepath</file>
 *   </installedFiles>
 * </installData>
 */

namespace fomm.PackageManager {
    class fomod {
        private class StringDataSource : IStaticDataSource {
            private System.IO.MemoryStream ms;

            public StringDataSource(string str) {
                ms=new System.IO.MemoryStream(System.Text.Encoding.Default.GetBytes(str));
            }

            public System.IO.Stream GetSource() { return ms; }

            public void Close() { ms.Close(); }
        }

        public class fomodLoadException : Exception { public fomodLoadException(string msg) : base(msg) { } }
        private ZipFile file;

        public readonly string filepath;
        public readonly string xmlpath;

        private bool hasInfo;
        public bool HasInfo { get { return hasInfo; } }
        private bool isActive;
        public bool IsActive { get { return isActive; } }
        private bool hasScript;
        public bool HasScript { get { return hasScript; } }
        private bool hasReadme;
        public bool HasReadme { get { return hasReadme; } }
        private bool hasScreenshot;
        public bool HasScreenshot { get { return hasScreenshot; } }
        private bool hasSubfolderEsp;
        public bool HasSubfolderEsp { get { return hasSubfolderEsp; } }

        public string Name;
        public string Author;
        public string Description;
        public Version Version;
        public string VersionS;
        private System.Drawing.Bitmap screenshot;

        private string readmeext;
        public string ReadmeExt { get { return readmeext; } }
        private string screenshotext;
        private string readmepath;

        private void LoadInfo() {
            ZipEntry info=file.GetEntry("fomod/info.xml");
            if(info!=null) {
                hasInfo=true;
                XmlDocument doc=new XmlDocument();
                System.IO.Stream stream=file.GetInputStream(info);
                try {
                    doc.Load(stream);
                    XmlNode root=null;
                    foreach(XmlNode n in doc.ChildNodes) {
                        if(n.NodeType==XmlNodeType.Element) {
                            root=n;
                            break;
                        }
                    }
                    if(root==null) {
                        throw new fomodLoadException("Root node was missing from fomod info.xml");
                    }
                    if(root.Name!="fomod") {
                        throw new fomodLoadException("Unexpected root node type in info.xml");
                    }
                    XmlNode n2;
                    foreach(XmlNode n in root.ChildNodes) {
                        if(n.NodeType==XmlNodeType.Comment) continue;
                        switch(n.Name) {
                        case "Name":
                            Name=n.InnerText;
                            break;
                        case "Version":
                            VersionS=n.InnerText;
                            n2=n.Attributes.GetNamedItem("MachineVersion");
                            if(n2!=null) Version=new Version(n2.Value);
                            break;
                        case "Author":
                            Author=n.InnerText;
                            break;
                        case "Description":
                            Description=n.InnerText;
                            break;
                        case "MinFommVersion":
                            Version v=new Version(n.InnerText);
                            if(Program.MVersion<v) throw new fomodLoadException("This fomod requires a newer version of Fallout mod manager to load\n"+
                                "Expected "+n.InnerText);
                            break;
                        default:
                            throw new fomodLoadException("Unexpected node type '"+n.Name+"' in info.xml");
                        }
                    }
                } finally {
                    stream.Close();
                }
            }
        }

        public fomod(string path) {
            this.filepath=path;
            this.xmlpath=System.IO.Path.ChangeExtension(path, ".xml");

            isActive=File.Exists(xmlpath);

            file=new ZipFile(path);
            Name=System.IO.Path.GetFileNameWithoutExtension(path);
            Author="DEFAULT";
            Description=string.Empty;
            VersionS="1.0";
            Version=new Version(1, 0);
            LoadInfo();
            hasScript=(file.GetEntry("fomod/script.cs")!=null);
            string[] extensions=new string[] { ".txt", ".rtf", ".htm", ".html" };
            for(int i=0;i<extensions.Length;i++) {
                if(file.GetEntry("readme - "+Name+extensions[i])!=null) {
                    hasReadme=true;
                    readmeext=extensions[i];
                    readmepath="readme - "+Name.ToLowerInvariant()+extensions[i];
                    break;
                }
            }
            extensions=new string[] { ".png", ".jpg", ".bmp" };
            for(int i=0;i<extensions.Length;i++) {
                if(file.GetEntry("fomod/screenshot"+extensions[i])!=null) {
                    hasScreenshot=true;
                    screenshotext=extensions[i];
                    break;
                }
            }
        }

        private string GetFileText(ZipEntry ze) {
            System.IO.Stream s=file.GetInputStream(ze);
            System.Text.StringBuilder sb=new System.Text.StringBuilder();
            byte[] buffer=new byte[4096];
            int count;
            while((count=s.Read(buffer, 0, 4096))>0) {
                sb.Append(System.Text.Encoding.Default.GetString(buffer, 0, count));
            }
            s.Close();
            return sb.ToString();
        }

        public string GetScript() {
            if(!HasScript) return null;
            return GetFileText(file.GetEntry("fomod/script.cs"));
        }
        public void SetScript(string value) {
            if(value==null||value=="") {
                if(hasScript) {
                    file.BeginUpdate();
                    file.Delete("fomod/script.cs");
                    file.CommitUpdate();
                    hasScript=false;
                }
            } else {
                StringDataSource sds=new StringDataSource(value);
                file.BeginUpdate();
                file.Add(sds, "fomod/script.cs");
                file.CommitUpdate();
                sds.Close();
                hasScript=true;
            }
        }

        public string GetReadme() {
            if(!HasReadme) return null;
            return GetFileText(file.GetEntry(readmepath));
        }
        public void SetReadme(string value) {
            if(value==null||value=="") {
                if(hasReadme) {
                    file.BeginUpdate();
                    file.Delete(readmepath);
                    file.CommitUpdate();
                    hasReadme=false;
                }
            } else {
                file.BeginUpdate();
                if(readmeext!=".rtf") file.Delete(readmepath);
                readmeext=".rtf";
                readmepath=Path.ChangeExtension(readmepath, ".rtf");
                StringDataSource sds=new StringDataSource(value);
                file.Add(sds, readmepath);
                file.CommitUpdate();
                sds.Close();
                hasReadme=true;
            }
        }

        public System.Drawing.Bitmap GetScreenshot() {
            if(!hasScreenshot) return null;
            if(screenshot==null) {
                screenshot=new System.Drawing.Bitmap(file.GetInputStream(file.GetEntry("fomod/screenshot"+screenshotext)));
            }
            return screenshot;
        }

        public void Activate() {
            if(IsActive) return;
            ScriptFunctions.Setup(this, file);
            XmlDocument xmlDoc;
            if(HasScript) {
                xmlDoc=ScriptFunctions.CustomInstallScript(GetFileText(file.GetEntry("fomod/script.cs")));
            } else {
                xmlDoc=ScriptFunctions.BasicInstallScript();
            }
            if(xmlDoc==null) return;
            isActive=true;
            xmlDoc.Save(xmlpath);
            InstallLog.Commit();
        }

        public void Deactivate() {
            if(!IsActive) return;
            isActive=false;
            XmlDocument xmlDoc=new XmlDocument();
            xmlDoc.Load(xmlpath);
            XmlNode bnode=xmlDoc.FirstChild.SelectSingleNode("installedFiles");
            if(bnode!=null) {
                foreach(XmlNode node in bnode.ChildNodes) {
                    InstallLog.UninstallDataFile(node.InnerText);
                }
            }
            File.Delete(xmlpath);
            InstallLog.Commit();
        }


        public void Dispose() {
            if(screenshot!=null) screenshot.Dispose();
            file.Close();
        }
    }
}
