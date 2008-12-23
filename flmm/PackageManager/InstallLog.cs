using System;
using System.Xml;
using Path=System.IO.Path;
using File=System.IO.File;

namespace fomm.PackageManager {
    /*
     * InstallLog.xml structure
     * <installLog>
     *   <dataFiles>
     *     <file count="10">filepath</file>
     *   </dataFiles>
     *   <iniEdits>
     *   <sdpEdits>
     * </installLog>
     */ 
    static class InstallLog {
        private static readonly string xmlpath=Path.Combine(Program.fommDir, "InstallLog.xml");
        private static readonly XmlDocument xmlDoc;
        private static readonly XmlElement dataFilesNode;
        private static readonly XmlElement iniEditsNode;
        private static readonly XmlElement sdpEditsNode;

        static InstallLog() {
            xmlDoc=new XmlDocument();
            if(File.Exists(xmlpath)) {
                xmlDoc.Load(xmlpath);
                dataFilesNode=(XmlElement)xmlDoc.SelectSingleNode("installLog/dataFiles");
                iniEditsNode=(XmlElement)xmlDoc.SelectSingleNode("installLog/iniEdits");
                sdpEditsNode=(XmlElement)xmlDoc.SelectSingleNode("installLog/sdpEdits");
            } else {
                XmlNode root=xmlDoc.AppendChild(xmlDoc.CreateElement("installLog"));
                root.AppendChild(dataFilesNode=xmlDoc.CreateElement("dataFiles"));
                root.AppendChild(iniEditsNode=xmlDoc.CreateElement("iniEdits"));
                root.AppendChild(sdpEditsNode=xmlDoc.CreateElement("sdpEdits"));
            }
        }

        public static void Commit() {
            xmlDoc.Save(xmlpath);
        }

        public static void InstallDataFile(string path) {
            path=path.ToLowerInvariant();
            XmlNode node=dataFilesNode.SelectSingleNode("file[.=\""+path.Replace("&", "&amp;")+"\"]");
            if(node==null) {
                XmlElement el=xmlDoc.CreateElement("file");
                el.Attributes.Append(xmlDoc.CreateAttribute("count"));
                el.Attributes[0].Value="1";
                el.InnerText=path;
                dataFilesNode.AppendChild(el);
            } else {
                node.Attributes[0].Value=(int.Parse(node.Attributes[0].Value)+1).ToString();
            }
        }

        public static void UninstallDataFile(string path) {
            path=path.ToLowerInvariant();
            XmlNode node=dataFilesNode.SelectSingleNode("file[.=\""+path.Replace("&", "&amp;")+"\"]");
            if(node!=null) {
                int i=int.Parse(node.Attributes.GetNamedItem("count").Value)-1;
                if(i==0) {
                    node.ParentNode.RemoveChild(node);
                    File.Delete(path);
                } else node.Attributes.GetNamedItem("count").Value=i.ToString();
            }
        }
    }
}
