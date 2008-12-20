using System;
using ICSharpCode.SharpZipLib.Zip;
using System.Xml;

namespace fomm.PackageManager {
    class fomod {
        public class fomodLoadException : Exception { public fomodLoadException(string msg) : base(msg) { } }
        private ZipFile file;

        public bool HasInfo;
        public string Name;
        public string Author;
        public string Description;
        public Version Version;
        public string VersionS;

        private void LoadInfo() {
            ZipEntry info=file.GetEntry("fomod/info.xml");
            if(info!=null) {
                HasInfo=true;
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
            file=new ZipFile(path);
            Name=System.IO.Path.GetFileNameWithoutExtension(path);
            Author="DEFAULT";
            Description=string.Empty;
            VersionS="1.0";
            Version=new Version(1, 0);
            LoadInfo();
        }
    }
}
