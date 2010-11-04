using System;
using System.Xml;
using Path = System.IO.Path;
using File = System.IO.File;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using System.Windows.Forms;

namespace Fomm
{
	static class Settings
	{
		private static XmlDocument xmlDoc;
		private static readonly string xmlPath = Path.Combine(Program.fommDir, "settings.xml");

		private static XmlElement rootNode;

		public static void Init()
		{
			xmlDoc = new XmlDocument();
			if (File.Exists(xmlPath))
			{
				try
				{
					xmlDoc.Load(xmlPath);
					rootNode = (XmlElement)xmlDoc.LastChild;
				}
				catch
				{
					System.Windows.Forms.MessageBox.Show("Unable to load settings.xml", "Error");
					xmlDoc = new XmlDocument();
					xmlDoc.AppendChild(rootNode = xmlDoc.CreateElement("settings"));
				}
			}
			else
			{
				xmlDoc.AppendChild(rootNode = xmlDoc.CreateElement("settings"));
			}
		}

		public static string GetString(string name)
		{
			XmlElement xe = rootNode.SelectSingleNode("descendant::strValue[@name='" + name + "']") as XmlElement;
			if (xe == null) return null;
			else return xe.InnerText;
		}

		public static void SetString(string name, string value)
		{
			XmlElement xe = rootNode.SelectSingleNode("descendant::strValue[@name='" + name + "']") as XmlElement;
			if (xe == null)
			{
				rootNode.AppendChild(xe = xmlDoc.CreateElement("strValue"));
				xe.Attributes.Append(xmlDoc.CreateAttribute("name"));
				xe.Attributes[0].Value = name;
			}

			xe.InnerText = value;

			xmlDoc.Save(xmlPath);
		}

		public static void RemoveString(string name)
		{
			XmlElement xe = rootNode.SelectSingleNode("descendant::strValue[@name='" + name + "']") as XmlElement;
			if (xe != null) xe.ParentNode.RemoveChild(xe);
		}
		
		public static Int32 GetInt(string name, Int32 p_intDefault)
		{
			XmlElement xe = rootNode.SelectSingleNode("descendant::intValue[@name='" + name + "']") as XmlElement;
			Int32 intValue = 0;
			if ((xe != null) && Int32.TryParse(xe.InnerText, out intValue))
				return intValue;
			return p_intDefault;
		}

		public static void SetInt(string p_strName, Int32 p_intValue)
		{
			XmlElement xe = rootNode.SelectSingleNode("descendant::intValue[@name='" + p_strName + "']") as XmlElement;
			if (xe == null)
			{
				rootNode.AppendChild(xe = xmlDoc.CreateElement("intValue"));
				xe.Attributes.Append(xmlDoc.CreateAttribute("name"));
				xe.Attributes[0].Value = p_strName;
			}
			xe.InnerText = p_intValue.ToString();
			xmlDoc.Save(xmlPath);
		}
		 
		
		public static bool GetBool(string name)
		{
			XmlElement xe = rootNode.SelectSingleNode("descendant::boolValue[@name='" + name + "']") as XmlElement;
			if (xe == null) return false;
			else return xe.InnerText == "true";
		}

		public static void SetBool(string name, bool value)
		{
			XmlElement xe = rootNode.SelectSingleNode("descendant::boolValue[@name='" + name + "']") as XmlElement;
			if (xe == null)
			{
				rootNode.AppendChild(xe = xmlDoc.CreateElement("boolValue"));
				xe.Attributes.Append(xmlDoc.CreateAttribute("name"));
				xe.Attributes[0].Value = name;
			}

			xe.InnerText = value ? "true" : "false";

			xmlDoc.Save(xmlPath);
		}
	}
}
