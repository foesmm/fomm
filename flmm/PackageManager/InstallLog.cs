using System;
using System.Xml;
using Path=System.IO.Path;
using File=System.IO.File;

namespace Fomm.PackageManager {
    /*
     * InstallLog.xml structure
     * <installLog>
     *   <dataFiles>
     *     <file count="10">filepath</file>
     *   </dataFiles>
     *   <iniEdits>
     *     <ini file='' section='' key='' mod=''>old value</ini>
     *   </iniEdits>
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
            XmlNode node=dataFilesNode.SelectSingleNode("file[.=\""+path+"\"]");
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
            XmlNode node=dataFilesNode.SelectSingleNode("file[.=\""+path+"\"]");
            if(node==null) return;
            int i=int.Parse(node.Attributes.GetNamedItem("count").Value)-1;
            if(i==0) {
                node.ParentNode.RemoveChild(node);
                File.Delete(path);
            } else node.Attributes.GetNamedItem("count").Value=i.ToString();
        }

        public static void AddIniEdit(string file, string section, string key, string mod/*, string value*/) {
            XmlNode node=iniEditsNode.SelectSingleNode("ini[@file='"+file+"' and @section='"+section+"' and @key='"+key+"']");
            if(node==null) {
                node=xmlDoc.CreateElement("ini");
                node.Attributes.Append(xmlDoc.CreateAttribute("file"));
                node.Attributes.Append(xmlDoc.CreateAttribute("section"));
                node.Attributes.Append(xmlDoc.CreateAttribute("key"));
                node.Attributes.Append(xmlDoc.CreateAttribute("mod"));
                node.Attributes[0].Value=file;
                node.Attributes[1].Value=section;
                node.Attributes[2].Value=key;
                node.Attributes[3].Value=mod;
                node.InnerText=NativeMethods.GetPrivateProfileString(section, key, "", file);
                iniEditsNode.AppendChild(node);
            } else {
                node.Attributes.GetNamedItem("mod").Value=mod;
            }
        }

        public static string GetIniEdit(string file, string section, string key, out string mod) {
            mod=null;
            XmlNode node=iniEditsNode.SelectSingleNode("ini[@file='"+file+"' and @section='"+section+"' and @key='"+key+"']");
            if(node==null) return null;
            XmlNode modnode=node.Attributes.GetNamedItem("mod");
            if(modnode!=null) mod=modnode.Value;
            return node.InnerText;
        }

        public static void UndoIniEdit(string file, string section, string key) {
            XmlNode node=iniEditsNode.SelectSingleNode("ini[@file='"+file+"' and @section='"+section+"' and @key='"+key+"']");
            if(node==null) return;
            NativeMethods.WritePrivateProfileStringA(section, key, node.InnerText, file);
            iniEditsNode.RemoveChild(node);
        }
        public static void UndoIniEdit(string file, string section, string key, string mod) {
            XmlNode node=iniEditsNode.SelectSingleNode("ini[@file='"+file+"' and @section='"+section+"' and @key='"+key+"' and @mod='"+mod+"']");
            if(node==null) return;
            NativeMethods.WritePrivateProfileStringA(section, key, node.InnerText, file);
            iniEditsNode.RemoveChild(node);
        }

        public static void AddShaderEdit(int package, string shader, byte[] old) {
            XmlNode node=sdpEditsNode.SelectSingleNode("sdp[@package='"+package+"' and @shader='"+shader+"']");
            if(node==null) {
                XmlElement el=xmlDoc.CreateElement("sdp");
                el.Attributes.Append(xmlDoc.CreateAttribute("package"));
                el.Attributes.Append(xmlDoc.CreateAttribute("shader"));
                el.Attributes[0].Value=package.ToString();
                el.Attributes[1].Value=shader;
                System.Text.StringBuilder sb=new System.Text.StringBuilder(old.Length*2);
                for(int i=0;i<old.Length;i++) sb.Append(old[i].ToString("x2"));
                el.InnerText=sb.ToString();
                sdpEditsNode.AppendChild(el);
            }
        }
        public static bool IsShaderEdited(int package, string shader) {
            XmlNode node=sdpEditsNode.SelectSingleNode("sdp[@package='"+package+"' and @shader='"+shader+"']");
            return node!=null;
        }
        public static void UndoShaderEdit(int package, string shader, uint crc) {
            XmlNode node=sdpEditsNode.SelectSingleNode("sdp[@package='"+package+"' and @shader='"+shader+"']");
            if(node==null) return;
            byte[] b=new byte[node.InnerText.Length/2];
            for(int i=0;i<b.Length;i++) {
                b[i]=byte.Parse(""+node.InnerText[i*2]+node.InnerText[i*2+1], System.Globalization.NumberStyles.AllowHexSpecifier);
            }
            if(SDPArchives.RestoreShader(package, shader, b, crc)) sdpEditsNode.RemoveChild(node);
        }
    }
}
