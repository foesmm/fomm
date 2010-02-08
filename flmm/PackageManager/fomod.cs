using System;
using ICSharpCode.SharpZipLib.Zip;
using System.Xml;
using File = System.IO.File;
using Path = System.IO.Path;
using Stream = System.IO.Stream;
using MemoryStream = System.IO.MemoryStream;
using System.Transactions;
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

		internal readonly string baseName;
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

		#region Properties

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
