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
	public class fomod
	{
		private class StringDataSource : IStaticDataSource
		{
			private Stream ms;

			public StringDataSource(string str)
			{
				ms = new System.IO.MemoryStream(System.Text.Encoding.Default.GetBytes(str));
			}
			public StringDataSource(byte[] bytes)
			{
				ms = new System.IO.MemoryStream(bytes);
			}
			public StringDataSource(Stream s)
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
		internal string Name;
		internal string Author;
		internal string Description;
		internal Version Version;
		internal string VersionS;
		internal string email;
		internal string website;
		private System.Drawing.Bitmap screenshot;
		internal Version MinFommVersion;
		internal string[] groups;

		private string readmeext;
		private string screenshotext;
		private string readmepath;

		private Dictionary<string, string> m_dicCriticalRecordPluginInstalledNames = null;
		private Dictionary<string, Dictionary<UInt32, string>> m_dicCriticalRecords = null;

		#region Properties

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

		/// <summary>
		/// Gets the mod's critical records.
		/// </summary>
		/// <remarks>
		/// Critical recrods are plugin records that the author is marking as recrods that should
		/// not be overridden.
		/// </remarks>
		/// <value>The mod's critical records.</value>
		internal Dictionary<string, Dictionary<UInt32, string>> CriticalRecords
		{
			get
			{
				return m_dicCriticalRecords;
			}
			set
			{
				m_dicCriticalRecords = value;
			}
		}

		/// <summary>
		/// Gets the installed names of the mod's plugins, as used for critical records.
		/// </summary>
		/// <value>The installed names of the mod's plugins, as used for critical records.</value>
		internal Dictionary<string, string> CriticalRecordPluginInstalledNames
		{
			get
			{
				return m_dicCriticalRecordPluginInstalledNames;
			}
			set
			{
				m_dicCriticalRecordPluginInstalledNames = value;
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

			m_zipFile = new ZipFile(path);
			Name = System.IO.Path.GetFileNameWithoutExtension(path);
			baseName = Name.ToLowerInvariant();
			Author = "DEFAULT";
			Description = email = website = string.Empty;
			VersionS = "1.0";
			Version = DefaultVersion;
			MinFommVersion = DefaultMinFommVersion;
			readmepath = Settings.GetBool("UseDocsFolder") ? "docs/readme - " + baseName + ".rtf" : "readme - " + baseName + ".rtf";
			groups = new string[0];
			isActive = (InstallLog.Current.GetModKey(this.baseName) != null);

			m_dicCriticalRecords = new Dictionary<string, Dictionary<uint, string>>();
			m_dicCriticalRecordPluginInstalledNames = new Dictionary<string, string>();
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

		private void LoadInfo()
		{
			ZipEntry info = m_zipFile.GetEntry("fomod/info.xml");
			if (info != null)
			{
				hasInfo = true;
				XmlDocument doc = new XmlDocument();
				System.IO.Stream stream = m_zipFile.GetInputStream(info);
				try
				{
					//System.IO.TextReader tr=new System.IO.StreamReader(stream);
					doc.Load(stream);
					XmlNode root = null;
					foreach (XmlNode n in doc.ChildNodes)
					{
						if (n.NodeType == XmlNodeType.Element)
						{
							root = n;
							break;
						}
					}
					if (root == null)
					{
						throw new fomodLoadException("Root node was missing from fomod info.xml");
					}
					if (root.Name != "fomod")
					{
						throw new fomodLoadException("Unexpected root node type in info.xml");
					}
					XmlNode n2;
					foreach (XmlNode n in root.ChildNodes)
					{
						if (n.NodeType == XmlNodeType.Comment) continue;
						switch (n.Name)
						{
							case "Name":
								Name = n.InnerText;
								break;
							case "Version":
								VersionS = n.InnerText;
								n2 = n.Attributes.GetNamedItem("MachineVersion");
								if (n2 != null) Version = new Version(n2.Value);
								break;
							case "Author":
								Author = n.InnerText;
								break;
							case "Description":
								Description = n.InnerText;
								break;
							case "MinFommVersion":
								Version v = new Version(n.InnerText);
								if (Program.MVersion < v) throw new fomodLoadException("This fomod requires a newer version of Fallout mod manager to load\n" +
									   "Expected " + n.InnerText);
								MinFommVersion = v;
								break;
							case "Email":
								email = n.InnerText;
								break;
							case "Website":
								website = n.InnerText;
								break;
							case "Groups":
								groups = new string[n.ChildNodes.Count];
								for (int i = 0; i < n.ChildNodes.Count; i++) groups[i] = n.ChildNodes[i].InnerText;
								break;
							case "CriticalRecords":
								string strPluginName = null;
								UInt32 uintFormId = 0;
								foreach (XmlNode xndPlugin in n.ChildNodes[0])
								{
									if ((xndPlugin.Attributes["name"] == null) || (String.IsNullOrEmpty(xndPlugin.Attributes["name"].InnerText)))
										throw new fomodLoadException("The 'name' attribute is required for the CriticalRecords/installedNames/plugin nodes.");
									if ((xndPlugin.Attributes["installedName"] == null) || (String.IsNullOrEmpty(xndPlugin.Attributes["installedName"].InnerText)))
										throw new fomodLoadException("The 'installedName' attribute is required for the CriticalRecords/installedNames/plugin nodes.");
									m_dicCriticalRecordPluginInstalledNames[xndPlugin.Attributes["name"].InnerText.ToLowerInvariant()] = xndPlugin.Attributes["installedName"].InnerText;
								}
								foreach (XmlNode xndPlugin in n.ChildNodes[1])
								{
									if ((xndPlugin.Attributes["name"] == null) || (String.IsNullOrEmpty(xndPlugin.Attributes["name"].InnerText)))
										throw new fomodLoadException("The 'name' attribute is required for the CriticalRecords/records/plugin nodes.");
									strPluginName = xndPlugin.Attributes["name"].InnerText.ToLowerInvariant();
									if (!m_dicCriticalRecords.ContainsKey(strPluginName))
										m_dicCriticalRecords[strPluginName] = new Dictionary<UInt32, string>();
									foreach (XmlNode xndRecord in xndPlugin.ChildNodes)
									{
										if ((xndRecord.Attributes["formId"] == null) || (String.IsNullOrEmpty(xndRecord.Attributes["formId"].InnerText)))
											throw new fomodLoadException("The 'formId' attribute is required for the CriticalRecords/plugin/record nodes.");
										uintFormId = UInt32.Parse(xndRecord.Attributes["formId"].InnerText, System.Globalization.NumberStyles.HexNumber);
										m_dicCriticalRecords[strPluginName][uintFormId] = xndRecord.InnerText;
									}
								}
								break;
							default:
								throw new fomodLoadException("Unexpected node type '" + n.Name + "' in info.xml");
						}
					}
				}
				finally
				{
					stream.Close();
				}
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
				StringDataSource sds = new StringDataSource(value);
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
				StringDataSource sds = new StringDataSource(value);
				m_zipFile.Add(sds, readmepath);
				m_zipFile.CommitUpdate();
				sds.Close();
				hasReadme = true;
			}
		}

		internal void CommitInfo(bool SetScreenshot, byte[] screenshot)
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "UTF-16", null));
			XmlElement el = xmlDoc.CreateElement("fomod"), el2;
			StringDataSource sds1, sds2 = null;

			if (SetScreenshot && this.screenshot != null)
			{
				this.screenshot.Dispose();
				this.screenshot = null;
			}

			xmlDoc.AppendChild(el);
			if (Name.Length > 0)
			{
				el2 = xmlDoc.CreateElement("Name");
				el2.InnerText = Name;
				el.AppendChild(el2);
			}
			if (Author != "DEFAULT")
			{
				el2 = xmlDoc.CreateElement("Author");
				el2.InnerText = Author;
				el.AppendChild(el2);
			}
			if (VersionS.Length > 0 || Version != DefaultVersion)
			{
				el2 = xmlDoc.CreateElement("Version");
				el2.InnerText = (VersionS.Length == 0) ? Version.ToString() : VersionS;
				el2.Attributes.Append(xmlDoc.CreateAttribute("MachineVersion"));
				el2.Attributes[0].Value = Version.ToString();
				el.AppendChild(el2);
			}
			if (Description.Length > 0)
			{
				el2 = xmlDoc.CreateElement("Description");
				el2.InnerText = Description;
				el.AppendChild(el2);
			}
			if (email.Length > 0)
			{
				el2 = xmlDoc.CreateElement("Email");
				el2.InnerText = email;
				el.AppendChild(el2);
			}
			if (website.Length > 0)
			{
				el2 = xmlDoc.CreateElement("Website");
				el2.InnerText = website;
				el.AppendChild(el2);
			}
			if (MinFommVersion != DefaultMinFommVersion)
			{
				el2 = xmlDoc.CreateElement("MinFommVersion");
				el2.InnerText = MinFommVersion.ToString();
				el.AppendChild(el2);
			}
			if (groups.Length > 0)
			{
				el2 = xmlDoc.CreateElement("Groups");
				for (int i = 0; i < groups.Length; i++) el2.AppendChild(xmlDoc.CreateElement("element"));
				for (int i = 0; i < groups.Length; i++) el2.ChildNodes[i].InnerText = groups[i];
				el.AppendChild(el2);
			}
			if (m_dicCriticalRecords.Count > 0)
			{
				el2 = xmlDoc.CreateElement("CriticalRecords");
				XmlNode xndInstalledName = el2.AppendChild(xmlDoc.CreateElement("installedNames"));
				XmlNode xndPluginName = null;
				foreach (KeyValuePair<string, string> kvpNames in m_dicCriticalRecordPluginInstalledNames)
				{
					xndPluginName = xndInstalledName.AppendChild(xmlDoc.CreateElement("plugin"));
					xndPluginName.Attributes.Append(xmlDoc.CreateAttribute("name"));
					xndPluginName.Attributes.Append(xmlDoc.CreateAttribute("installedName"));
					xndPluginName.Attributes["name"].InnerText = kvpNames.Key;
					xndPluginName.Attributes["installedName"].InnerText = kvpNames.Value;
				}

				XmlNode xndRecords = el2.AppendChild(xmlDoc.CreateElement("records"));
				XmlNode xndPlugin = null;
				XmlNode xndRecord = null;
				foreach (string strPlugin in m_dicCriticalRecords.Keys)
				{
					xndPlugin = xndRecords.AppendChild(xmlDoc.CreateElement("plugin"));
					xndPlugin.Attributes.Append(xmlDoc.CreateAttribute("name"));
					xndPlugin.Attributes["name"].InnerText = strPlugin;
					foreach (UInt32 uintFormId in m_dicCriticalRecords[strPlugin].Keys)
					{
						xndRecord = xndPlugin.AppendChild(xmlDoc.CreateElement("record"));
						xndRecord.Attributes.Append(xmlDoc.CreateAttribute("formId"));
						xndRecord.Attributes["formId"].InnerText = uintFormId.ToString("x8");
						xndRecord.InnerText = m_dicCriticalRecords[strPlugin][uintFormId];
					}
				}
				el.AppendChild(el2);
			}

			m_zipFile.BeginUpdate();
			hasInfo = true;

			MemoryStream ms = new MemoryStream();
			xmlDoc.Save(ms);
			ms.Position = 0;
			sds1 = new StringDataSource(ms);
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
					sds2 = new StringDataSource(screenshot);
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
			sb.AppendLine("Mod name: " + Name);
			sb.AppendLine("File name: " + baseName);
			if (Author != "DEFAULT") sb.AppendLine("Author: " + Author);
			if (VersionS != "1.0") sb.AppendLine("Version: " + VersionS);
			if (email.Length > 0) sb.AppendLine("email: " + email);
			if (website.Length > 0) sb.AppendLine("website: " + website);
			if (MinFommVersion != new Version(0, 0, 0, 0)) sb.AppendLine("Minimum required fomm version: " + MinFommVersion.ToString());
			if (Description.Length > 0) sb.AppendLine("Description:" + Environment.NewLine + Description);
			if (groups.Length > 0) sb.AppendLine(Environment.NewLine + "Group tags: " + string.Join(", ", groups));
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

		#region Critical Records

		/// <summary>
		/// Gets the installed name of the specified plugin.
		/// </summary>
		/// <remarks>
		/// The installed name of a plugin is the file name of the plugin once installed on a
		/// user's machine.
		/// </remarks>
		/// <param name="p_strPluginName">The name of the plugin for which to retrieve the installed name.</param>
		/// <returns>The installed name of the specified plugin.</returns>
		public string GetCriticalRecordPluginInstalledName(string p_strPluginName)
		{
			string strLowered = p_strPluginName.ToLowerInvariant();
			if (!m_dicCriticalRecordPluginInstalledNames.ContainsKey(strLowered))
				return null;
			return m_dicCriticalRecordPluginInstalledNames[strLowered];
		}

		/// <summary>
		/// Gets the original names of the specified plugin installed name.
		/// </summary>
		/// <remarks>
		/// The installed name of a plugin is the file name of the plugin once installed on a
		/// user's machine. The orignal names are the names of the files in the fomod.
		/// </remarks>
		/// <param name="p_strPluginInstalledName">The installed name of the plugin for which to
		/// retrieve the original names.</param>
		/// <returns>The original names of the specified plugin.</returns>
		public IList<string> GetCriticalRecordPluginNames(string p_strPluginInstalledName)
		{
			string strLowered = p_strPluginInstalledName.ToLowerInvariant();
			List<string> lstNames = new List<string>();
			foreach (KeyValuePair<string, string> kvpNames in m_dicCriticalRecordPluginInstalledNames)
			{
				if (kvpNames.Value.ToLowerInvariant().Equals(p_strPluginInstalledName))
					if (!lstNames.Contains(kvpNames.Key))
						lstNames.Add(kvpNames.Key);
			}
			return lstNames;
		}

		/// <summary>
		/// Sets the installed name of the specified plugin.
		/// </summary>
		/// <remarks>
		/// The installed name of a plugin is the file name of the plugin once installed on a
		/// user's machine.
		/// </remarks>
		/// <param name="p_strPluginName">The name of the plugin for which to set the installed name.</param>
		/// <param name="p_strPluginInstalledName">The installed name of the specified plugin.</param>
		public void SetCriticalRecordPluginInstalledName(string p_strPluginName, string p_strPluginInstalledName)
		{
			string strOldInstalledName = null;
			m_dicCriticalRecordPluginInstalledNames.TryGetValue(p_strPluginName.ToLowerInvariant(), out strOldInstalledName);
			m_dicCriticalRecordPluginInstalledNames[p_strPluginName.ToLowerInvariant()] = p_strPluginInstalledName;
			if ((strOldInstalledName != null) && !p_strPluginInstalledName.Equals(strOldInstalledName))
			{
				strOldInstalledName = strOldInstalledName.ToLowerInvariant();
				if (m_dicCriticalRecords.ContainsKey(strOldInstalledName))
				{
					string strLoweredNew = p_strPluginInstalledName.ToLowerInvariant();
					if (!m_dicCriticalRecords.ContainsKey(strLoweredNew))
						m_dicCriticalRecords[strLoweredNew] = new Dictionary<UInt32, string>();
					foreach (KeyValuePair<UInt32, string> kvpRecord in m_dicCriticalRecords[strOldInstalledName])
						m_dicCriticalRecords[strLoweredNew][kvpRecord.Key] = kvpRecord.Value;
					m_dicCriticalRecords.Remove(strOldInstalledName);
				}
			}
		}

		/// <summary>
		/// Determines if the specified record is marked as critical for the specifid plugin.
		/// </summary>
		/// <param name="p_strPluginInstalledName">The installed name of the plugin for which it is to
		/// be determined if the specified record is critical.</param>
		/// <param name="p_uintFormId">The record for which it is to be determined whether it is
		/// critical.</param>
		/// <returns><lang cref="true"/> if the specified record is critical for the specified plugin;
		/// <lang cref="false"/> otherwise.</returns>
		public bool IsRecordCritical(string p_strPluginInstalledName, UInt32 p_uintFormId)
		{
			if (!m_dicCriticalRecords.ContainsKey(p_strPluginInstalledName.ToLowerInvariant()))
				return false;
			return m_dicCriticalRecords[p_strPluginInstalledName.ToLowerInvariant()].ContainsKey(p_uintFormId);
		}

		/// <summary>
		/// Gets the reason the the specified record has been marked as critical.
		/// </summary>
		/// <param name="p_strPluginInstalledName">The installed name of the plugin for whose record we are
		/// to retrieve the critical record reason.</param>
		/// <param name="p_uintFormId">The record for which  to retrieve the critical record reason.</param>
		/// <returns>The reason the the specified record has been marked as critical.</returns>
		public string GetCriticalRecordReason(string p_strPluginInstalledName, UInt32 p_uintFormId)
		{
			if (!IsRecordCritical(p_strPluginInstalledName, p_uintFormId))
				return null;
			return m_dicCriticalRecords[p_strPluginInstalledName.ToLowerInvariant()][p_uintFormId];
		}

		/// <summary>
		/// Marks a record as critical, supplying a reason to the critical marking.
		/// </summary>
		/// <param name="p_strPluginInstalledName">The installed name of the plugin whose record is being
		/// marked as critical.</param>
		/// <param name="p_uintFormId">The form id that is being marked as critical.</param>
		/// <param name="p_strReason">The reasonthe record is being marked as critical.</param>
		public void SetCriticalRecord(string p_strPluginInstalledName, UInt32 p_uintFormId, string p_strReason)
		{
			if (!m_dicCriticalRecordPluginInstalledNames.ContainsValue(p_strPluginInstalledName))
				throw new ArgumentException("The given installed plugin name is not known. Call SetCriticalRecordPluginInstalledName(string p_strPluginName, string p_strPluginInstalledName) first.", "p_strPluginInstalledName");
			string strLowered = p_strPluginInstalledName.ToLowerInvariant();
			if (!m_dicCriticalRecords.ContainsKey(strLowered))
				m_dicCriticalRecords[strLowered] = new Dictionary<UInt32, string>();
			m_dicCriticalRecords[strLowered][p_uintFormId] = p_strReason;
		}

		/// <summary>
		/// Unsets the sepecifed record as critical.
		/// </summary>
		/// <param name="p_strPluginInstalledName">The installed name of the plugin whose record is being
		/// unmarked as critical.</param>
		/// <param name="p_uintFormId">The form id that is being unmarked as critical.</param>
		public void UnsetCriticalRecord(string p_strPluginInstalledName, UInt32 p_uintFormId)
		{
			string strLowered = p_strPluginInstalledName.ToLowerInvariant();
			if (!m_dicCriticalRecords.ContainsKey(strLowered))
				return;
			m_dicCriticalRecords[strLowered].Remove(p_uintFormId);
			if (m_dicCriticalRecords[strLowered].Count == 0)
				m_dicCriticalRecords.Remove(strLowered);
		}

		#endregion
	}
}
