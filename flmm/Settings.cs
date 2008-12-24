using System;
using System.Xml;
using Path=System.IO.Path;
using File=System.IO.File;
using Point=System.Drawing.Point;
using Size=System.Drawing.Size;

namespace fomm {
    static class Settings {
        private static XmlDocument xmlDoc;
        private static readonly string xmlPath=Path.Combine(Program.fommDir, "settings.xml");

        private static XmlElement rootNode;

        public static void Init() {
            xmlDoc=new XmlDocument();
            if(File.Exists(xmlPath)) {
                xmlDoc.Load(xmlPath);
                rootNode=(XmlElement)xmlDoc.LastChild;
            } else {
                xmlDoc.AppendChild(rootNode=xmlDoc.CreateElement("settings"));
            }
        }

        public static void GetWindowPosition(string window, System.Windows.Forms.Form f) {
            XmlElement xe=rootNode.SelectSingleNode("descendant::window[@name='"+window+"']") as XmlElement;
            if(xe==null) return;
            if(xe.Attributes.GetNamedItem("maximized").Value=="true") {
                f.WindowState=System.Windows.Forms.FormWindowState.Maximized;
            } else {
                f.Location=new Point(int.Parse(xe.Attributes.GetNamedItem("left").Value), int.Parse(xe.Attributes.GetNamedItem("top").Value));
                f.ClientSize=new Size(int.Parse(xe.Attributes.GetNamedItem("width").Value), int.Parse(xe.Attributes.GetNamedItem("height").Value));
                f.StartPosition=System.Windows.Forms.FormStartPosition.Manual;
            }
        }

        public static void SetWindowPosition(string window, System.Windows.Forms.Form f) {
            Point location=f.Location;
            Size size=f.ClientSize;
            bool maximized=f.WindowState==System.Windows.Forms.FormWindowState.Maximized;
            XmlElement xe=rootNode.SelectSingleNode("descendant::window[@name='"+window+"']") as XmlElement;
            if(xe==null) {
                rootNode.AppendChild(xe=xmlDoc.CreateElement("window"));
                xe.Attributes.Append(xmlDoc.CreateAttribute("name"));
                xe.Attributes[0].Value=window;
            }
            XmlAttribute xa=xmlDoc.CreateAttribute("left");
            xa.Value=location.X.ToString();
            xe.Attributes.SetNamedItem(xa);
            xa=xmlDoc.CreateAttribute("top");
            xa.Value=location.Y.ToString();
            xe.Attributes.SetNamedItem(xa);
            xa=xmlDoc.CreateAttribute("width");
            xa.Value=size.Width.ToString();
            xe.Attributes.SetNamedItem(xa);
            xa=xmlDoc.CreateAttribute("height");
            xa.Value=size.Height.ToString();
            xe.Attributes.SetNamedItem(xa);
            xa=xmlDoc.CreateAttribute("maximized");
            xa.Value=maximized?"true":"false";
            xe.Attributes.SetNamedItem(xa);

            xmlDoc.Save(xmlPath);
        }
    }
}
