using System;
using System.Xml;
using System.Drawing;
using System.Collections.Generic;
using Fomm.Util;
using System.IO;

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

namespace Fomm.PackageManager
{
	public class fomod : IFomodInfo
	{
		class fomodLoadException : Exception { public fomodLoadException(string msg) : base(msg) { } }

		private Archive m_arcFile;

		internal readonly string filepath;

		public static readonly Version DefaultVersion = new Version(1, 0);
		public static readonly Version DefaultMinFommVersion = new Version(0, 0, 0, 0);

		private bool hasInfo;
		private bool isActive;

		private string m_strScreenshotPath = null;
		private string m_strScriptPath = null;
		private string m_strReadmePath = null;

		private readonly string baseName;
		private string m_strName;
		private string m_strAuthor;
		private string m_strDescription;
		private Version m_verVersion;
		private string m_strHumanVersion;
		private string m_strEmail;
		private string m_strWebsite;
		private Version m_verMinFommVersion;
		private string[] m_strGroups;

		#region Properties

		/// <summary>
		/// Gets or sets the name of the fomod.
		/// </summary>
		/// <value>The name of the fomod.</value>
		public string ModName
		{
			get
			{
				return m_strName;
			}
			set
			{
				m_strName = value;
			}
		}

		/// <summary>
		/// Gets or sets the human readable form of the fomod's version.
		/// </summary>
		/// <value>The human readable form of the fomod's version.</value>
		public string HumanReadableVersion
		{
			get
			{
				return m_strHumanVersion;
			}
			set
			{
				m_strHumanVersion = value;
			}
		}

		/// <summary>
		/// Gets or sets the version of the fomod.
		/// </summary>
		/// <value>The version of the fomod.</value>
		public Version MachineVersion
		{
			get
			{
				return m_verVersion;
			}
			set
			{
				m_verVersion = value;
			}
		}

		/// <summary>
		/// Gets or sets the author of the fomod.
		/// </summary>
		/// <value>The author of the fomod.</value>
		public string Author
		{
			get
			{
				return m_strAuthor;
			}
			set
			{
				m_strAuthor = value;
			}
		}

		/// <summary>
		/// Gets or sets the description of the fomod.
		/// </summary>
		/// <value>The description of the fomod.</value>
		public string Description
		{
			get
			{
				return m_strDescription;
			}
			set
			{
				m_strDescription = value;
			}
		}

		/// <summary>
		/// Gets or sets the minimum version of FOMM required to load the fomod.
		/// </summary>
		/// <value>The minimum version of FOMM required to load the fomod.</value>
		public Version MinFommVersion
		{
			get
			{
				return m_verMinFommVersion;
			}
			set
			{
				m_verMinFommVersion = value;
			}
		}

		/// <summary>
		/// Gets or sets the contact email of the fomod.
		/// </summary>
		/// <value>The contact email of the fomod.</value>
		public string Email
		{
			get
			{
				return m_strEmail;
			}
			set
			{
				m_strEmail = value;
			}
		}

		/// <summary>
		/// Gets or sets the website of the fomod.
		/// </summary>
		/// <value>The website of the fomod.</value>
		public string Website
		{
			get
			{
				return m_strWebsite;
			}
			set
			{
				m_strWebsite = value;
			}
		}

		/// <summary>
		/// Gets or sets the FOMM groups to which the fomod belongs.
		/// </summary>
		/// <value>The FOMM groups to which the fomod belongs.</value>
		public string[] Groups
		{
			get
			{
				return m_strGroups;
			}
			set
			{
				m_strGroups = value;
			}
		}

		/// <summary>
		/// Gets the base name of the fomod.
		/// </summary>
		/// <value>The base name of the fomod.</value>
		internal virtual string BaseName
		{
			get
			{
				return baseName;
			}
		}

		/// <summary>
		/// Gets the fomod file.
		/// </summary>
		/// <remarks>
		/// The fomod file is a compressed file.
		/// </remarks>
		/// <value>The fomod file.</value>
		internal Archive FomodFile
		{
			get
			{
				return m_arcFile;
			}
		}

		/// <summary>
		/// Gets whether the mod has metadata.
		/// </summary>
		/// <value>Whether the mod has metadata.</value>
		public bool HasInfo { get { return hasInfo; } }

		/// <summary>
		/// Gets or sets whether the mod is active.
		/// </summary>
		/// <value>Whether the mod is active.</value>
		public bool IsActive
		{
			get
			{
				return isActive;
			}
			set
			{
				isActive = value;
			}
		}

		/// <summary>
		/// Gets whether the mod has a custom install script.
		/// </summary>
		/// <value>Whether the mod has a custom install script.</value>
		public bool HasInstallScript
		{
			get
			{
				return !String.IsNullOrEmpty(m_strScriptPath);
			}
		}

		/// <summary>
		/// Gets whether the mod has a screenshot.
		/// </summary>
		/// <value>Whether the mod has a screenshot.</value>
		public bool HasScreenshot
		{
			get
			{
				return !String.IsNullOrEmpty(m_strScreenshotPath);
			}
		}

		/// <summary>
		/// Gets whether the mod has a custom uninstall script.
		/// </summary>
		/// <value>Whether the mod has a custom uninstall script.</value>
		public bool HasUninstallScript { get { return false; } }

		/// <summary>
		/// Gets whether the mod has a readme file.
		/// </summary>
		/// <value>Whether the mod has a readme file.</value>
		public bool HasReadme
		{
			get
			{
				return !String.IsNullOrEmpty(m_strReadmePath);
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object.
		/// </summary>
		/// <param name="path">The path to the fomod file.</param>
		internal fomod(string path)
		{
			this.filepath = path;

			m_arcFile = new Archive(path);
			ModName = System.IO.Path.GetFileNameWithoutExtension(path);
			baseName = ModName.ToLowerInvariant();
			Author = "DEFAULT";
			Description = Email = Website = string.Empty;
			HumanReadableVersion = "1.0";
			MachineVersion = DefaultVersion;
			MinFommVersion = DefaultMinFommVersion;
			Groups = new string[0];
			isActive = (InstallLog.Current.GetModKey(this.baseName) != null);

			LoadInfo();

			for (int i = 0; i < FomodScript.ScriptNames.Length; i++)
				if (m_arcFile.ContainsFile("fomod/" + FomodScript.ScriptNames[i]))
				{
					m_strScriptPath = "fomod/" + FomodScript.ScriptNames[i];
					break;
				}
		
			for (int i = 0; i < Readme.ValidExtensions.Length; i++)
				if (m_arcFile.ContainsFile("readme - " + baseName + Readme.ValidExtensions[i]))
				{
					m_strReadmePath = "Readme - " + Path.GetFileNameWithoutExtension(path) + Readme.ValidExtensions[i];
					break;
				}
			if (String.IsNullOrEmpty(m_strReadmePath))
				for (int i = 0; i < Readme.ValidExtensions.Length; i++)
					if (m_arcFile.ContainsFile("docs/readme - " + baseName + Readme.ValidExtensions[i]))
					{
						m_strReadmePath = "docs/Readme - " + Path.GetFileNameWithoutExtension(path) + Readme.ValidExtensions[i];
						break;
					}

			string[] extensions = new string[] { ".png", ".jpg", ".bmp" };
			for (int i = 0; i < extensions.Length; i++)
				if (m_arcFile.ContainsFile("fomod/screenshot" + extensions[i]))
				{
					m_strScreenshotPath = "fomod/screenshot" + extensions[i];
					break;
				}
		}

		#endregion

		#region Fomod Info Persistence

		/// <summary>
		/// Loads the fomod info from the given info file into the given fomod info object.
		/// </summary>
		/// <param name="p_xmlInfo">The XML info file from which to read the info.</param>
		/// <param name="p_finFomodInfo">The fomod info object to populate with the info.</param>
		public static void LoadInfo(XmlDocument p_xmlInfo, IFomodInfo p_finFomodInfo)
		{
			LoadInfo(p_xmlInfo, p_finFomodInfo, true);
		}

		/// <summary>
		/// Loads the fomod info from the given info file into the given fomod info object.
		/// </summary>
		/// <param name="p_xmlInfo">The XML info file from which to read the info.</param>
		/// <param name="p_finFomodInfo">The fomod info object to populate with the info.</param>
		/// <param name="p_booOverwriteExisitngValues">Whether or not to overwrite any existing values
		/// in the given <see cref="IFomodInfo"/>.</param>
		public static void LoadInfo(XmlDocument p_xmlInfo, IFomodInfo p_finFomodInfo, bool p_booOverwriteExisitngValues)
		{
			XmlNode xndRoot = null;
			foreach (XmlNode xndNode in p_xmlInfo.ChildNodes)
				if (xndNode.NodeType == XmlNodeType.Element)
				{
					xndRoot = xndNode;
					break;
				}
			if (xndRoot == null)
				throw new fomodLoadException("Root node was missing from fomod info.xml");
			if (xndRoot.Name != "fomod")
				throw new fomodLoadException("Unexpected root node type in info.xml");
			foreach (XmlNode xndNode in xndRoot.ChildNodes)
			{
				if (xndNode.NodeType == XmlNodeType.Comment) continue;
				switch (xndNode.Name)
				{
					case "Name":
						if (p_booOverwriteExisitngValues || String.IsNullOrEmpty(p_finFomodInfo.ModName))
							p_finFomodInfo.ModName = xndNode.InnerText;
						break;
					case "Version":
						if (p_booOverwriteExisitngValues || String.IsNullOrEmpty(p_finFomodInfo.HumanReadableVersion))
							p_finFomodInfo.HumanReadableVersion = xndNode.InnerText;
						if (p_booOverwriteExisitngValues || (p_finFomodInfo.MachineVersion == DefaultVersion))
						{
							XmlNode xndMachineVersion = xndNode.Attributes.GetNamedItem("MachineVersion");
							if (xndMachineVersion != null)
								p_finFomodInfo.MachineVersion = new Version(xndMachineVersion.Value);
						}
						break;
					case "Author":
						if (p_booOverwriteExisitngValues || String.IsNullOrEmpty(p_finFomodInfo.Author))
							p_finFomodInfo.Author = xndNode.InnerText;
						break;
					case "Description":
						if (p_booOverwriteExisitngValues || String.IsNullOrEmpty(p_finFomodInfo.Description))
							p_finFomodInfo.Description = xndNode.InnerText;
						break;
					case "MinFommVersion":
						if (p_booOverwriteExisitngValues || (p_finFomodInfo.MachineVersion == DefaultMinFommVersion))
							p_finFomodInfo.MinFommVersion = new Version(xndNode.InnerText);
						break;
					case "Email":
						if (p_booOverwriteExisitngValues || String.IsNullOrEmpty(p_finFomodInfo.Email))
							p_finFomodInfo.Email = xndNode.InnerText;
						break;
					case "Website":
						if (p_booOverwriteExisitngValues || String.IsNullOrEmpty(p_finFomodInfo.Website))
							p_finFomodInfo.Website = xndNode.InnerText;
						break;
					case "Groups":
						if (p_booOverwriteExisitngValues || (p_finFomodInfo.Groups.Length == 0))
						{
							string[] strGroups = new string[xndNode.ChildNodes.Count];
							for (int i = 0; i < xndNode.ChildNodes.Count; i++)
								strGroups[i] = xndNode.ChildNodes[i].InnerText;
							p_finFomodInfo.Groups = strGroups;
						}
						break;
					default:
						throw new fomodLoadException("Unexpected node type '" + xndNode.Name + "' in info.xml");
				}
			}
		}

		/// <summary>
		/// Serializes the fomod info contained in the given fomod info object into an XML document.
		/// </summary>
		/// <param name="p_finFomodInfo">The fomod info object to serialize.</param>
		/// <returns>An XML file containing the fomod info.</returns>
		public static XmlDocument SaveInfo(IFomodInfo p_finFomodInfo)
		{
			XmlDocument xmlInfo = new XmlDocument();
			xmlInfo.AppendChild(xmlInfo.CreateXmlDeclaration("1.0", "UTF-16", null));
			XmlElement xelRoot = xmlInfo.CreateElement("fomod");
			XmlElement xelTemp = null;

			xmlInfo.AppendChild(xelRoot);
			if (!String.IsNullOrEmpty(p_finFomodInfo.ModName))
			{
				xelTemp = xmlInfo.CreateElement("Name");
				xelTemp.InnerText = p_finFomodInfo.ModName;
				xelRoot.AppendChild(xelTemp);
			}
			if (!String.IsNullOrEmpty(p_finFomodInfo.Author) && !p_finFomodInfo.Author.Equals("DEFAULT"))
			{
				xelTemp = xmlInfo.CreateElement("Author");
				xelTemp.InnerText = p_finFomodInfo.Author;
				xelRoot.AppendChild(xelTemp);
			}
			if (!String.IsNullOrEmpty(p_finFomodInfo.HumanReadableVersion) || (p_finFomodInfo.MachineVersion != DefaultVersion))
			{
				xelTemp = xmlInfo.CreateElement("Version");
				xelTemp.InnerText = String.IsNullOrEmpty(p_finFomodInfo.HumanReadableVersion) ? p_finFomodInfo.MachineVersion.ToString() : p_finFomodInfo.HumanReadableVersion;
				xelTemp.Attributes.Append(xmlInfo.CreateAttribute("MachineVersion"));
				xelTemp.Attributes[0].Value = p_finFomodInfo.MachineVersion.ToString();
				xelRoot.AppendChild(xelTemp);
			}
			if (!String.IsNullOrEmpty(p_finFomodInfo.Description))
			{
				xelTemp = xmlInfo.CreateElement("Description");
				xelTemp.InnerText = p_finFomodInfo.Description;
				xelRoot.AppendChild(xelTemp);
			}
			if (!String.IsNullOrEmpty(p_finFomodInfo.Email))
			{
				xelTemp = xmlInfo.CreateElement("Email");
				xelTemp.InnerText = p_finFomodInfo.Email;
				xelRoot.AppendChild(xelTemp);
			}
			if (!String.IsNullOrEmpty(p_finFomodInfo.Website))
			{
				xelTemp = xmlInfo.CreateElement("Website");
				xelTemp.InnerText = p_finFomodInfo.Website;
				xelRoot.AppendChild(xelTemp);
			}
			if (p_finFomodInfo.MinFommVersion != DefaultMinFommVersion)
			{
				xelTemp = xmlInfo.CreateElement("MinFommVersion");
				xelTemp.InnerText = p_finFomodInfo.MinFommVersion.ToString();
				xelRoot.AppendChild(xelTemp);
			}
			if ((p_finFomodInfo.Groups != null) && (p_finFomodInfo.Groups.Length > 0))
			{
				xelTemp = xmlInfo.CreateElement("Groups");
				for (int i = 0; i < p_finFomodInfo.Groups.Length; i++)
					xelTemp.AppendChild(xmlInfo.CreateElement("element")).InnerText = p_finFomodInfo.Groups[i];
				xelRoot.AppendChild(xelTemp);
			}

			return xmlInfo;
		}

		#endregion

		protected void LoadInfo()
		{
			if (m_arcFile.ContainsFile("fomod/info.xml"))
			{
				hasInfo = true;
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(TextUtil.ByteToString(m_arcFile.GetFileContents("fomod/info.xml")));
				LoadInfo(doc, this);
				if (Program.MVersion < MinFommVersion)
					throw new fomodLoadException("This fomod requires a newer version of Fallout mod manager to load." + Environment.NewLine + "Expected " + MachineVersion);
			}
		}

		internal FomodScript GetInstallScript()
		{
			if (!HasInstallScript)
				return null;
			return new FomodScript(m_strScriptPath, TextUtil.ByteToString(m_arcFile.GetFileContents(m_strScriptPath)));
		}

		/// <summary>
		/// Gets the custom uninstall script.
		/// </summary>
		/// <remarks>
		/// Currently, custom uninstall scripts are not supported.
		/// 
		/// Indeed, if they were added significant change would have to be made
		/// to the acessibility of methods, and safeties of one sort or another
		/// would have to be put in place to insure everything is cleaned up
		/// nicely.
		/// 
		/// Implementing a custom uninstall script to support re-configuration
		/// of an installed mod seems to be a bad idea. If re-configuring an
		/// installed mod is a desired feature it should be implemented separately,
		/// and be more closey aligned to a mod install than a mod uninstall.
		/// </remarks>
		/// <returns></returns>
		internal string GetUninstallScript()
		{
			return null;
		}

		internal void SetScript(FomodScript p_fscScript)
		{
			if (p_fscScript == null)
			{
				if (HasInstallScript)
				{
					m_arcFile.DeleteFile(m_strScriptPath);
					m_strScriptPath = null;
				}
			}
			else
			{
				if (m_strScriptPath == null)
					m_strScriptPath = Path.Combine("fomod", p_fscScript.FileName);
				m_arcFile.ReplaceFile(m_strScriptPath, p_fscScript.Text);
			}
		}

		public Readme GetReadme()
		{
			if (String.IsNullOrEmpty(m_strReadmePath) || !Readme.IsValidReadme(m_strReadmePath))
				return null;
			return new Readme(m_strReadmePath, TextUtil.ByteToString(m_arcFile.GetFileContents(m_strReadmePath)));
		}

		internal void SetReadme(Readme p_rmeReadme)
		{
			if (HasReadme)
				m_arcFile.DeleteFile(m_strReadmePath);
			if (p_rmeReadme == null)
				m_strReadmePath = null;
			else
			{
				if (m_strReadmePath == null)
					m_strReadmePath = (Settings.GetBool("UseDocsFolder") ? "docs/" : "") + "Readme - " + Path.GetFileNameWithoutExtension(filepath) + ".rtf";
				m_strReadmePath = Path.ChangeExtension(m_strReadmePath, p_rmeReadme.Extension);
				m_arcFile.ReplaceFile(m_strReadmePath, p_rmeReadme.Text);
			}
		}

		internal void CommitInfo(bool SetScreenshot, Screenshot p_shtScreenshot)
		{
			XmlDocument xmlInfo = SaveInfo(this);
			hasInfo = true;

			MemoryStream ms = new MemoryStream();
			xmlInfo.Save(ms);
			m_arcFile.ReplaceFile("fomod/info.xml", ms.ToArray());
			if (SetScreenshot)
			{
				if (p_shtScreenshot == null)
				{
					if (HasScreenshot)
					{
						m_arcFile.DeleteFile(m_strScreenshotPath);
						m_strScreenshotPath = null;
					}
				}
				else
				{
					if (!HasScreenshot)
						m_strScreenshotPath = "screenshot.jpg";
					m_strScreenshotPath = Path.ChangeExtension(m_strScreenshotPath, p_shtScreenshot.Extension);
					m_arcFile.ReplaceFile(m_strScreenshotPath, p_shtScreenshot.Data);
				}
			}
		}

		public Image GetScreenshotImage()
		{
			return HasScreenshot ? GetScreenshot().Image : null;
		}

		public Screenshot GetScreenshot()
		{
			if (!HasScreenshot)
				return null;

			Screenshot shtScreenshot = new Screenshot(m_strScreenshotPath, GetFile(m_strScreenshotPath));
			return shtScreenshot;
		}

		internal void Dispose()
		{
			m_arcFile.Dispose();
		}

		internal string GetStatusString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.AppendLine("Mod name: " + ModName);
			sb.AppendLine("File name: " + baseName);
			if (Author != "DEFAULT") sb.AppendLine("Author: " + Author);
			if (HumanReadableVersion != "1.0") sb.AppendLine("Version: " + HumanReadableVersion);
			if (Email.Length > 0) sb.AppendLine("email: " + Email);
			if (Website.Length > 0) sb.AppendLine("website: " + Website);
			if (MinFommVersion != new Version(0, 0, 0, 0)) sb.AppendLine("Minimum required fomm version: " + MinFommVersion.ToString());
			if (Description.Length > 0) sb.AppendLine("Description:" + Environment.NewLine + Description);
			if (Groups.Length > 0) sb.AppendLine(Environment.NewLine + "Group tags: " + string.Join(", ", Groups));
			sb.AppendLine();
			sb.AppendLine("Has readme: " + (HasReadme ? ("Yes (" + GetReadme().Format + ")") : "No"));
			sb.AppendLine("Has script: " + (HasInstallScript ? "Yes" : "No"));
			sb.AppendLine("Has screenshot: " + (HasScreenshot ? ("Yes (" + GetScreenshot().Extension + ")") : "No"));
			sb.AppendLine("Is active: " + (isActive ? "Yes" : "No"));
			sb.AppendLine();
			sb.AppendLine("-- fomod contents list:");
			sb.AppendLine(String.Join(Environment.NewLine, m_arcFile.GetFiles(null)));
			if (isActive)
			{
				sb.AppendLine();
				sb.AppendLine("Activation data" + Environment.NewLine);

				InstallLogMergeModule ilmMergeModule = InstallLog.Current.GetMergeModule(this.baseName);
				if (ilmMergeModule.DataFiles.Count > 0)
				{
					sb.AppendLine("-- Installed data files");
					foreach (string strFile in ilmMergeModule.DataFiles)
						sb.AppendLine(strFile);
					sb.AppendLine();
				}
				if (ilmMergeModule.IniEdits.Count > 0)
				{
					sb.AppendLine("-- Ini edits");
					foreach (InstallLogMergeModule.IniEdit iniEdit in ilmMergeModule.IniEdits)
					{
						sb.AppendLine("File: " + iniEdit.File);
						sb.AppendLine("Section: " + iniEdit.Section);
						sb.AppendLine("Key: " + iniEdit.Key);
						sb.AppendLine();
					}
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// Retrieves the list of files in this FOMOD.
		/// </summary>
		/// <returns>The list of files in this FOMOD.</returns>
		public List<string> GetFileList()
		{
			PermissionsManager.CurrentPermissions.Assert();
			List<string> lstFiles = new List<string>();
			foreach (string strFile in m_arcFile.GetFiles(null))
				if (!strFile.StartsWith("fomod", StringComparison.OrdinalIgnoreCase))
					lstFiles.Add(strFile);
			return lstFiles;
		}

		/// <summary>
		/// Determines if the specified file is in the fomod.
		/// </summary>
		/// <param name="p_strFile">The file whose existence is to be determined.</param>
		/// <returns><lang cref="true"/> if the file is in the fomod; <lang dref="false"/> otherwise.</returns>
		public bool FileExists(string p_strFile)
		{
			PermissionsManager.CurrentPermissions.Assert();
			return m_arcFile.ContainsFile(p_strFile);
		}

		/// <summary>
		/// Retrieves the specified file from the fomod.
		/// </summary>
		/// <param name="p_strFile">The file to retrieve.</param>
		/// <returns>The requested file data.</returns>
		/// <exception cref="FileNotFoundException">Thrown if the specified file
		/// is not in the fomod.</exception>
		/// <exception cref="DecompressionException">Thrown if the specified file
		/// cannot be extracted from the zip.</exception>
		public byte[] GetFile(string p_strFile)
		{
			PermissionsManager.CurrentPermissions.Assert();
			if (!m_arcFile.ContainsFile(p_strFile))
				throw new FileNotFoundException("File doesn't exist in fomod", p_strFile);
			return m_arcFile.GetFileContents(p_strFile);
		}

		/// <summary>
		/// Retrieves the specified image from the fomod.
		/// </summary>
		/// <param name="p_strFile">The path of the image to extract.</param>
		/// <returns>The request image.</returns>
		/// <exception cref="FileNotFoundException">Thrown if the specified file
		/// is not in the fomod.</exception>
		/// <exception cref="DecompressionException">Thrown if the specified file
		/// cannot be extracted from the zip.</exception>
		public Image GetImage(string p_strFile)
		{
			PermissionsManager.CurrentPermissions.Assert();
			if (!m_arcFile.ContainsFile(p_strFile))
				throw new FileNotFoundException("File doesn't exist in fomod", p_strFile);
			Image imgImage = null;
			using (MemoryStream msmImage = new MemoryStream(m_arcFile.GetFileContents(p_strFile)))
			{
				imgImage = Image.FromStream(msmImage);
				msmImage.Close();
			}
			return imgImage;
		}
	}
}
