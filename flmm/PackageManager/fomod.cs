using System;
using ICSharpCode.SharpZipLib.Zip;
using System.Xml;
using File=System.IO.File;
using Path=System.IO.Path;

/*
 * Installed data XML Structure
 * <installData>
 *   <installedFiles>
 *     <file>filepath</file>
 *   </installedFiles>
 *   <sdpEdits>
 *     <sdp package="1" shader=">hexcode</edit>
 * </installData>
 * 
 * Info XML structure
 * <fomod>
 *   <Name>bingle</Name>
 * </fomod>
 */

namespace fomm.PackageManager {
    class fomod {
        private class StringDataSource : IStaticDataSource {
            private System.IO.MemoryStream ms;

            public StringDataSource(string str) {
                ms=new System.IO.MemoryStream(System.Text.Encoding.Default.GetBytes(str));
            }
            public StringDataSource(byte[] bytes) {
                ms=new System.IO.MemoryStream(bytes);
            }

            public System.IO.Stream GetSource() { return ms; }

            public void Close() { ms.Close(); }
        }

        public class fomodLoadException : Exception { public fomodLoadException(string msg) : base(msg) { } }
        private ZipFile file;

        public readonly string filepath;
        public readonly string xmlpath;

        public static readonly Version DefaultVersion=new Version(1,0);
        public static readonly Version DefaultMinFommVersion=new Version(0,0,0,0);

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

        public readonly string baseName;
        public string Name;
        public string Author;
        public string Description;
        public Version Version;
        public string VersionS;
        public string email;
        public string website;
        private System.Drawing.Bitmap screenshot;
        public Version MinFommVersion;
        public string[] groups;

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
                            MinFommVersion=v;
                            break;
                        case "Email":
                            email=n.InnerText;
                            break;
                        case "Website":
                            website=n.InnerText;
                            break;
                        case "Groups":
                            groups=new string[n.ChildNodes.Count];
                            for(int i=0;i<n.ChildNodes.Count;i++) groups[i]=n.ChildNodes[i].InnerText;
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
            baseName=Name.ToLowerInvariant();
            Author="DEFAULT";
            Description=email=website=string.Empty;
            VersionS="1.0";
            Version=DefaultVersion;
            MinFommVersion=DefaultMinFommVersion;
            readmepath="readme - "+baseName+".rtf";
            groups=new string[0];

            LoadInfo();
            hasScript=(file.GetEntry("fomod/script.cs")!=null);
            string[] extensions=new string[] { ".txt", ".rtf", ".htm", ".html" };
            for(int i=0;i<extensions.Length;i++) {
                if(file.GetEntry("readme - "+baseName+extensions[i])!=null) {
                    hasReadme=true;
                    readmeext=extensions[i];
                    readmepath="readme - "+baseName+extensions[i];
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
                if(hasReadme&&readmeext!=".rtf") file.Delete(readmepath);
                readmeext=".rtf";
                readmepath=Path.ChangeExtension(readmepath, ".rtf");
                StringDataSource sds=new StringDataSource(value);
                file.Add(sds, readmepath);
                file.CommitUpdate();
                sds.Close();
                hasReadme=true;
            }
        }

        public void CommitInfo(bool SetScreenshot, byte[] screenshot) {
            XmlDocument xmlDoc=new XmlDocument();
            XmlElement el=xmlDoc.CreateElement("fomod"), el2;
            StringDataSource sds1, sds2=null;

            if(SetScreenshot&&this.screenshot!=null) {
                this.screenshot.Dispose();
                this.screenshot=null;
            }

            xmlDoc.AppendChild(el);
            if(Name!="") {
                el2=xmlDoc.CreateElement("Name");
                el2.InnerText=Name;
                el.AppendChild(el2);
            }
            if(Author!="DEFAULT") {
                el2=xmlDoc.CreateElement("Author");
                el2.InnerText=Author;
                el.AppendChild(el2);
            }
            if(VersionS!=""||Version!=DefaultVersion) {
                el2=xmlDoc.CreateElement("Version");
                el2.InnerText=VersionS==""?Version.ToString():VersionS;
                el2.Attributes.Append(xmlDoc.CreateAttribute("MachineVersion"));
                el2.Attributes[0].Value=Version.ToString();
                el.AppendChild(el2);
            }
            if(Description!="") {
                el2=xmlDoc.CreateElement("Description");
                el2.InnerText=Description;
                el.AppendChild(el2);
            }
            if(email!="") {
                el2=xmlDoc.CreateElement("Email");
                el2.InnerText=email;
                el.AppendChild(el2);
            }
            if(website!="") {
                el2=xmlDoc.CreateElement("Website");
                el2.InnerText=website;
                el.AppendChild(el2);
            }
            if(Description!="") {
                el2=xmlDoc.CreateElement("Description");
                el2.InnerText=Description;
                el.AppendChild(el2);
            }
            if(MinFommVersion!=DefaultMinFommVersion) {
                el2=xmlDoc.CreateElement("MinFommVersion");
                el2.InnerText=MinFommVersion.ToString();
                el.AppendChild(el2);
            }
            if(groups.Length>0) {
                el2=xmlDoc.CreateElement("Groups");
                for(int i=0;i<groups.Length;i++) el2.AppendChild(xmlDoc.CreateElement("element"));
                for(int i=0;i<groups.Length;i++) el2.ChildNodes[i].InnerText=groups[i];
                el.AppendChild(el2);
            }

            file.BeginUpdate();
            hasInfo=true;
            sds1=new StringDataSource(xmlDoc.OuterXml);
            file.Add(sds1,"fomod/info.xml");
            if(SetScreenshot) {
                if(screenshot==null) {
                    if(hasScreenshot) {
                        file.Delete("fomod/screenshot"+screenshotext);
                        hasScreenshot=false;
                    }
                } else {
                    if(hasScreenshot&&screenshotext!=".png") {
                        file.Delete("fomod/screenshot"+screenshotext);
                    }
                    hasScreenshot=true;
                    screenshotext=".png";
                    sds2=new StringDataSource(screenshot);
                    file.Add(sds2, "fomod/screenshot.png");
                }
            }
            file.CommitUpdate();
            sds1.Close();
            if(sds2!=null) sds2.Close();
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
            bnode=xmlDoc.FirstChild.SelectSingleNode("iniEdits");
            if(bnode!=null) {
                foreach(XmlNode node in bnode.ChildNodes) {
                    InstallLog.UndoIniEdit(node.Attributes.GetNamedItem("file").Value, node.Attributes.GetNamedItem("section").Value,
                        node.Attributes.GetNamedItem("key").Value, baseName);
                }
            }
            bnode=xmlDoc.FirstChild.SelectSingleNode("sdpEdits");
            if(bnode!=null) {
                foreach(XmlNode node in bnode.ChildNodes) {
                    InstallLog.UndoShaderEdit(int.Parse(node.Attributes.GetNamedItem("package").Value), node.Attributes.GetNamedItem("shader").Value);
                }
            }
            File.Delete(xmlpath);
            InstallLog.Commit();
        }

        public void Dispose() {
            if(screenshot!=null) screenshot.Dispose();
            file.Close();
        }

        public string GetStatusString() {
            System.Text.StringBuilder sb=new System.Text.StringBuilder();
            sb.AppendLine("Mod name: "+Name);
            sb.AppendLine("File name: "+baseName);
            if(Author!="DEFAULT") sb.AppendLine("Author: "+Author);
            if(VersionS!="1.0") sb.AppendLine("Version: "+VersionS);
            if(email!="") sb.AppendLine("email: "+email);
            if(website!="") sb.AppendLine("website: "+website);
            if(MinFommVersion!=new Version(0,0,0,0)) sb.AppendLine("Minimum required fomm version: "+MinFommVersion.ToString());
            if(Description!="") sb.AppendLine("Description:"+Environment.NewLine+Description);
            if(groups.Length>0) sb.AppendLine(Environment.NewLine+"Group tags: "+string.Join(", ", groups));
            sb.AppendLine();
            sb.AppendLine("Has readme: "+(hasReadme?("Yes ("+readmeext+")"):"No"));
            sb.AppendLine("Has script: "+(hasScript?"Yes":"No"));
            sb.AppendLine("Has screenshot: "+(hasScreenshot?("Yes ("+screenshotext+")"):"No"));
            sb.AppendLine("Is active: "+(isActive?"Yes":"No"));
            sb.AppendLine();
            sb.AppendLine("-- fomod contents list:");
            foreach(ZipEntry ze in file) {
                if(!ze.IsFile) continue;
                sb.AppendLine(ze.Name);
            }
            if(isActive) {
                sb.AppendLine();
                sb.AppendLine("Activation data"+Environment.NewLine);
                XmlDocument xmlDoc=new XmlDocument();
                xmlDoc.Load(xmlpath);
                XmlNode bnode=xmlDoc.FirstChild.SelectSingleNode("installedFiles");
                if(bnode!=null) {
                    sb.AppendLine("-- Installed data files");
                    foreach(XmlNode node in bnode.ChildNodes) {
                        sb.AppendLine(node.InnerText);
                    }
                    sb.AppendLine();
                }
                bnode=xmlDoc.FirstChild.SelectSingleNode("iniEdits");
                if(bnode!=null) {
                    sb.AppendLine("-- Ini edits");
                    foreach(XmlNode node in bnode.ChildNodes) {
                        sb.AppendLine("File: "+node.Attributes.GetNamedItem("file").Value);
                        sb.AppendLine("Section: "+node.Attributes.GetNamedItem("section").Value);
                        sb.AppendLine("Key: "+node.Attributes.GetNamedItem("key").Value);
                        sb.AppendLine();
                    }
                }
            }
            return sb.ToString();
        }
    }
}
