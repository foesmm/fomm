using System;
using ICSharpCode.SharpZipLib.Zip;
using System.Xml;
using File = System.IO.File;
using Path = System.IO.Path;
using Stream = System.IO.Stream;
using MemoryStream = System.IO.MemoryStream;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Fomm.CriticalRecords;

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
		private class DataSource : IStaticDataSource
		{
			private Stream ms;

			public DataSource(string str)
			{
				ms = new System.IO.MemoryStream(System.Text.Encoding.Default.GetBytes(str));
			}
			public DataSource(byte[] bytes)
			{
				ms = new System.IO.MemoryStream(bytes);
			}
			public DataSource(Stream s)
			{
				ms = s;
			}

			public Stream GetSource() { return ms; }

			public void Close() { ms.Close(); }
		}

		class fomodLoadException : Exception { public fomodLoadException(string msg) : base(msg) { } }

		private ZipFile m_zipFile;

		internal readonly string filepath;

		public static readonly Version DefaultVersion = new Version(1, 0);
		public static readonly Version DefaultMinFommVersion = new Version(0, 0, 0, 0);

		private bool hasInfo;
		private bool isActive;
		private bool hasScript;
		private bool hasReadme;
		private bool hasScreenshot;

		private readonly string baseName;
		private string m_strName;
		private string m_strAuthor;
		private string m_strDescription;
		private Version m_verVersion;
		private string m_strHumanVersion;
		private string m_strEmail;
		private string m_strWebsite;
		private System.Drawing.Bitmap screenshot;
		private Version m_verMinFommVersion;
		private string[] m_strGroups;

		private string readmeext;
		private string screenshotext;
		private string readmepath;

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
		internal ZipFile FomodFile { get { return m_zipFile; } }

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
		public bool HasInstallScript { get { return hasScript; } }

		/// <summary>
		/// Gets whether the mod has a custom uninstall script.
		/// </summary>
		/// <value>Whether the mod has a custom uninstall script.</value>
		public bool HasUninstallScript { get { return false; } }

		/// <summary>
		/// Gets whether the mod has a readme file.
		/// </summary>
		/// <value>Whether the mod has a readme file.</value>
		public bool HasReadme { get { return hasReadme; } }

		/// <summary>
		/// Gets the extension of the mod's readme file.
		/// </summary>
		/// <value>The extension of the mod's readme file.</value>
		public string ReadmeExt { get { return readmeext; } }

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object.
		/// </summary>
		/// <param name="path">The path to the fomod file.</param>
		internal fomod(string path)
		{
			this.filepath = path;

			m_zipFile = new ZipFile(path);
			ModName = System.IO.Path.GetFileNameWithoutExtension(path);
			baseName = ModName.ToLowerInvariant();
			Author = "DEFAULT";
			Description = Email = Website = string.Empty;
			HumanReadableVersion = "1.0";
			MachineVersion = DefaultVersion;
			MinFommVersion = DefaultMinFommVersion;
			readmepath = Settings.GetBool("UseDocsFolder") ? "docs/readme - " + baseName + ".rtf" : "readme - " + baseName + ".rtf";
			Groups = new string[0];
			isActive = (InstallLog.Current.GetModKey(this.baseName) != null);

			LoadInfo();

			hasScript = (m_zipFile.GetEntry("fomod/script.cs") != null);
			string[] extensions = new string[] { ".txt", ".rtf", ".htm", ".html" };
			for (int i = 0; i < extensions.Length; i++)
			{
				if (m_zipFile.GetEntry("readme - " + baseName + extensions[i]) != null)
				{
					hasReadme = true;
					readmeext = extensions[i];
					readmepath = "readme - " + baseName + extensions[i];
					break;
				}
			}
			if (!hasReadme)
			{
				for (int i = 0; i < extensions.Length; i++)
				{
					if (m_zipFile.GetEntry("docs/readme - " + baseName + extensions[i]) != null)
					{
						hasReadme = true;
						readmeext = extensions[i];
						readmepath = "docs/readme - " + baseName + extensions[i];
						break;
					}
				}
			}
			extensions = new string[] { ".png", ".jpg", ".bmp" };
			for (int i = 0; i < extensions.Length; i++)
			{
				if (m_zipFile.GetEntry("fomod/screenshot" + extensions[i]) != null)
				{
					hasScreenshot = true;
					screenshotext = extensions[i];
					break;
				}
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
						p_finFomodInfo.ModName = xndNode.InnerText;
						break;
					case "Version":
						p_finFomodInfo.HumanReadableVersion = xndNode.InnerText;
						XmlNode xndMachineVersion = xndNode.Attributes.GetNamedItem("MachineVersion");
						if (xndMachineVersion != null)
							p_finFomodInfo.MachineVersion = new Version(xndMachineVersion.Value);
						break;
					case "Author":
						p_finFomodInfo.Author = xndNode.InnerText;
						break;
					case "Description":
						p_finFomodInfo.Description = xndNode.InnerText;
						break;
					case "MinFommVersion":
						p_finFomodInfo.MinFommVersion = new Version(xndNode.InnerText);
						break;
					case "Email":
						p_finFomodInfo.Email = xndNode.InnerText;
						break;
					case "Website":
						p_finFomodInfo.Website = xndNode.InnerText;
						break;
					case "Groups":
						string[] strGroups = new string[xndNode.ChildNodes.Count];
						for (int i = 0; i < xndNode.ChildNodes.Count; i++)
							strGroups[i] = xndNode.ChildNodes[i].InnerText;
						p_finFomodInfo.Groups = strGroups;
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
			ZipEntry info = m_zipFile.GetEntry("fomod/info.xml");
			if (info != null)
			{
				hasInfo = true;
				XmlDocument doc = new XmlDocument();
				using (System.IO.Stream stream = m_zipFile.GetInputStream(info))
				{
					doc.Load(stream);
					stream.Close();
				}
				LoadInfo(doc, this);
				if (Program.MVersion < MinFommVersion)
					throw new fomodLoadException("This fomod requires a newer version of Fallout mod manager to load." + Environment.NewLine + "Expected " + MachineVersion);
			}
		}

		private string GetFileText(ZipEntry ze)
		{
			System.IO.Stream s = m_zipFile.GetInputStream(ze);
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			byte[] buffer = new byte[4096];
			int count;
			while ((count = s.Read(buffer, 0, 4096)) > 0)
			{
				sb.Append(System.Text.Encoding.Default.GetString(buffer, 0, count));
			}
			s.Close();
			return sb.ToString();
		}

		internal string GetInstallScript()
		{
			if (!HasInstallScript) return null;
			return GetFileText(m_zipFile.GetEntry("fomod/script.cs"));
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

		internal void SetScript(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				if (hasScript)
				{
					m_zipFile.BeginUpdate();
					m_zipFile.Delete(m_zipFile.GetEntry("fomod/script.cs"));
					m_zipFile.CommitUpdate();
					hasScript = false;
				}
			}
			else
			{
				DataSource sds = new DataSource(value);
				m_zipFile.BeginUpdate();
				m_zipFile.Add(sds, "fomod/script.cs");
				m_zipFile.CommitUpdate();
				sds.Close();
				hasScript = true;
			}
		}

		public string GetReadme()
		{
			if (!HasReadme) return null;
			return GetFileText(m_zipFile.GetEntry(readmepath));
		}

		/// <summary>
		/// Replaces the specified file with the given data.
		/// </summary>
		/// <remarks>
		/// If the file does not exist in the fomod, it is added.
		/// </remarks>
		/// <param name="p_strFile">The name of the file to replace.</param>
		/// <param name="p_bteData">The data with which to replace to file.</param>
		protected void ReplaceFile(string p_strFile, byte[] p_bteData)
		{
			m_zipFile.BeginUpdate();
			ZipEntry zpeFile = m_zipFile.GetEntry(p_strFile.Replace('\\', '/'));
			if (zpeFile != null)
				m_zipFile.Delete(zpeFile);

			DataSource sds = new DataSource(p_bteData);
			m_zipFile.Add(sds, p_strFile);
			m_zipFile.CommitUpdate();
			sds.Close();
		}

		internal void SetReadme(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				if (hasReadme)
				{
					m_zipFile.BeginUpdate();
					m_zipFile.Delete(m_zipFile.GetEntry(readmepath));
					m_zipFile.CommitUpdate();
					hasReadme = false;
				}
			}
			else
			{
				m_zipFile.BeginUpdate();
				if (hasReadme) m_zipFile.Delete(m_zipFile.GetEntry(readmepath));
				if (readmeext != ".rtf")
				{
					readmeext = ".rtf";
					readmepath = Path.ChangeExtension(readmepath, ".rtf");
				}
				DataSource sds = new DataSource(value);
				m_zipFile.Add(sds, readmepath);
				m_zipFile.CommitUpdate();
				sds.Close();
				hasReadme = true;
			}
		}

		internal void CommitInfo(bool SetScreenshot, byte[] screenshot)
		{
			XmlDocument xmlInfo = SaveInfo(this);
			DataSource sds1, sds2 = null;

			if (SetScreenshot && this.screenshot != null)
			{
				this.screenshot.Dispose();
				this.screenshot = null;
			}

			m_zipFile.BeginUpdate();
			hasInfo = true;

			MemoryStream ms = new MemoryStream();
			xmlInfo.Save(ms);
			ms.Position = 0;
			sds1 = new DataSource(ms);
			m_zipFile.Add(sds1, "fomod/info.xml");
			if (SetScreenshot)
			{
				if (screenshot == null)
				{
					if (hasScreenshot)
					{
						m_zipFile.Delete(m_zipFile.GetEntry("fomod/screenshot" + screenshotext));
						hasScreenshot = false;
					}
				}
				else
				{
					if (hasScreenshot && screenshotext != ".png")
					{
						m_zipFile.Delete(m_zipFile.GetEntry("fomod/screenshot" + screenshotext));
					}
					hasScreenshot = true;
					screenshotext = ".png";
					sds2 = new DataSource(screenshot);
					m_zipFile.Add(sds2, "fomod/screenshot.png");
				}
			}
			m_zipFile.CommitUpdate();
			sds1.Close();
			if (sds2 != null) sds2.Close();
		}

		public System.Drawing.Bitmap GetScreenshot()
		{
			if (!hasScreenshot) return null;
			if (screenshot == null)
			{
				screenshot = new System.Drawing.Bitmap(m_zipFile.GetInputStream(m_zipFile.GetEntry("fomod/screenshot" + screenshotext)));
			}
			return screenshot;
		}

		internal void Dispose()
		{
			if (screenshot != null) screenshot.Dispose();
			m_zipFile.Close();
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
			sb.AppendLine("Has readme: " + (hasReadme ? ("Yes (" + readmeext + ")") : "No"));
			sb.AppendLine("Has script: " + (hasScript ? "Yes" : "No"));
			sb.AppendLine("Has screenshot: " + (hasScreenshot ? ("Yes (" + screenshotext + ")") : "No"));
			sb.AppendLine("Is active: " + (isActive ? "Yes" : "No"));
			sb.AppendLine();
			sb.AppendLine("-- fomod contents list:");
			foreach (ZipEntry ze in m_zipFile)
			{
				if (!ze.IsFile) continue;
				sb.AppendLine(ze.Name);
			}
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
			foreach (ZipEntry ze in m_zipFile)
			{
				if (ze.IsDirectory) continue;
				if (!ze.Name.StartsWith("fomod", StringComparison.OrdinalIgnoreCase)) lstFiles.Add(ze.Name);
			}
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
			ZipEntry zpeFile = m_zipFile.GetEntry(p_strFile.Replace('\\', '/'));
			return (zpeFile != null);
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
			ZipEntry zpeFile = m_zipFile.GetEntry(p_strFile.Replace('\\', '/'));
			if (zpeFile == null)
				throw new FileNotFoundException("File doesn't exist in fomod", p_strFile);
			if (!zpeFile.CanDecompress)
				throw new DecompressionException("Can't extract file from fomod");
			Stream stmInput = m_zipFile.GetInputStream(zpeFile);
			byte[] bteBuffer = new byte[zpeFile.Size];
			int intUpto = 0, intSize;
			while ((intSize = stmInput.Read(bteBuffer, 0, (int)zpeFile.Size - intUpto)) > 0) intUpto += intSize;
			return bteBuffer;
		}

		/// <summary>
		/// Retrieves the specified image from the fomod.
		/// </summary>
		/// <param name="file">The path of the image to extract.</param>
		/// <returns>The request image.</returns>
		/// <exception cref="FileNotFoundException">Thrown if the specified file
		/// is not in the fomod.</exception>
		/// <exception cref="DecompressionException">Thrown if the specified file
		/// cannot be extracted from the zip.</exception>
		public Image GetImage(string file)
		{
			PermissionsManager.CurrentPermissions.Assert();
			ZipEntry zpeFile = m_zipFile.GetEntry(file.Replace('\\', '/'));
			if (zpeFile == null)
				throw new FileNotFoundException("File doesn't exist in fomod", file);
			if (!zpeFile.CanDecompress)
				throw new DecompressionException("Can't extract file from fomod");
			Image imgImage = null;
			using (Stream stmInput = m_zipFile.GetInputStream(zpeFile))
			{
				imgImage = Image.FromStream(stmInput);
			}
			return imgImage;
		}
	}
}
