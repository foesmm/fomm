using System;
using System.Xml;
using Path = System.IO.Path;
using File = System.IO.File;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Fomm.PackageManager
{
	/*
	 * InstallLog.xml structure
	 * <installLog>
	 *   <modList>
	 *	   <mod name="" key=""/>
	 *   </modList>
	 *   <dataFiles>
	 *     <file path="<filepath>">
	 *	     <installingMods>
	 *	       <mod key=""/>
	 *	     </installingMods>
	 *     </file>
	 *   </dataFiles>
	 *   <iniEdits>
	 *     <ini file='' section='' key=''>
	 *       <installingMods>
	 *	       <mod key="">old value</mod>
	 *	     </installingMods>
	 *     </ini>
	 *   </iniEdits>
	 *   <sdpEdits>
	 *     <spd package='' shader=''>
	 *       <installingMods>
	 *	       <mod key="">old value</mod>
	 *	     </installingMods>
	 *     </spd>
	 *   </sdpEdits>
	 * </installLog>
	 */
	internal class InstallLog : InstallLogBase
	{
		private const string ORIGINAL_VALUES = "ORIGINAL_VALUES";
		private static readonly InstallLog m_ilgCurrent = new InstallLog();

		public static InstallLog Current
		{
			get
			{
				return m_ilgCurrent;
			}
		}

		private readonly string xmlpath = Path.Combine(Program.fommDir, "InstallLog.xml");
		private XmlDocument xmlDoc;
		private XmlElement m_xelModListNode;
		private XmlElement dataFilesNode;
		private XmlElement iniEditsNode;
		private XmlElement sdpEditsNode;
		private Dictionary<string, string> m_dicModList = null;
		private FileSystemWatcher m_fswLogWatcher = null;

		#region Properties

		/// <summary>
		/// Gets the path to the install log.
		/// </summary>
		/// <value>The path to the install log.</value>
		internal string InstallLogPath
		{
			get
			{
				return xmlpath;
			}
		}

		/// <summary>
		/// Gets the key used for original values.
		/// </summary>
		/// <value>The key used for original values.</value>
		internal string OriginalValuesKey
		{
			get
			{
				return GetModKey(ORIGINAL_VALUES);
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		private InstallLog()
		{
			Load();
			m_fswLogWatcher = new FileSystemWatcher(Path.GetDirectoryName(xmlpath));
			m_fswLogWatcher.Filter = Path.GetFileName(xmlpath);
			m_fswLogWatcher.Changed += new FileSystemEventHandler(InstallLogWatcher_Changed);
			m_fswLogWatcher.EnableRaisingEvents = true;
		}

		#endregion

		/// <summary>
		/// Handles the <see cref="FileSystemWatcher.Changed"/> event of the Install Log watcher.
		/// </summary>
		/// <remarks>
		/// If the Install Log changes out side of the programme, let's reload it to make sure we
		/// aren't using old data.
		/// </remarks>
		/// <param name="sender">The object that triggered the event</param>
		/// <param name="e">A <see cref="TreeViewEventArgs"/> describing the event arguments.</param>
		private void InstallLogWatcher_Changed(object sender, FileSystemEventArgs e)
		{
			//sometimes the machine responds too quickly and tries to load the file before
			// whatever change caused this event to be raised has finished.
			// the result is an IOException. so, if we get an IOException lets what for
			// a fraction of a second and try again. but let's only try 3 times: we don't
			// want to get into an infinite loop.
			for (Int32 i = 0; i < 3; )
			{
				try
				{
					Load();
					break;
				}
				catch (IOException)
				{
					Thread.Sleep(250);
					i++;
				}
			}
		}

		/// <summary>
		/// Loads the data from the Install Log.
		/// </summary>
		private void Load()
		{
			xmlDoc = new XmlDocument();
			if (File.Exists(xmlpath))
			{
				xmlDoc.Load(xmlpath);
				m_xelModListNode = (XmlElement)xmlDoc.SelectSingleNode("installLog/modList");
				dataFilesNode = (XmlElement)xmlDoc.SelectSingleNode("installLog/dataFiles");
				iniEditsNode = (XmlElement)xmlDoc.SelectSingleNode("installLog/iniEdits");
				sdpEditsNode = (XmlElement)xmlDoc.SelectSingleNode("installLog/sdpEdits");
			}
			else
			{
				XmlNode root = xmlDoc.AppendChild(xmlDoc.CreateElement("installLog"));
				root.AppendChild(dataFilesNode = xmlDoc.CreateElement("dataFiles"));
				root.AppendChild(iniEditsNode = xmlDoc.CreateElement("iniEdits"));
				root.AppendChild(sdpEditsNode = xmlDoc.CreateElement("sdpEdits"));
			}
			//we create this node separately in case we are upgrading an existing
			// install log that has all the nodes except the mod list node
			if (m_xelModListNode == null)
			{
				XmlNode root = (XmlNode)xmlDoc.SelectSingleNode("installLog");
				root.InsertBefore(m_xelModListNode = xmlDoc.CreateElement("modList"), dataFilesNode);
			}

			XmlNodeList xnlMods = m_xelModListNode.ChildNodes;
			m_dicModList = new Dictionary<string, string>();
			foreach (XmlNode xndMod in xnlMods)
				m_dicModList[xndMod.Attributes["name"].InnerText] = xndMod.Attributes["key"].InnerText;

			AddMod(ORIGINAL_VALUES);
		}

		/// <summary>
		/// Saves the Install Log.
		/// </summary>
		private void Save()
		{
			m_fswLogWatcher.EnableRaisingEvents = false;
			xmlDoc.Save(xmlpath);
			m_fswLogWatcher.EnableRaisingEvents = true;
		}

		#region Mod Tracking

		/// <summary>
		/// Returns the list of mods that have been installed.
		/// </summary>
		/// <remarks>
		/// The return list is ordered alphabetically.
		/// </remarks>
		/// <returns>The list of mods that have been installed.</returns>
		internal List<string> GetModList()
		{
			List<string> lstMods = new List<string>();
			foreach (string strMod in m_dicModList.Keys)
				lstMods.Add(strMod);
			lstMods.Sort();
			return lstMods;
		}

		/// <summary>
		/// Gets the key that was assigned to the specified mod.
		/// </summary>
		/// <param name="p_strModName">The base name of the <see cref="fomod"/> whose key is to be retrieved.</param>
		/// <returns>The key that was assigned to the specified mod, or <lang cref="null"/> if
		/// the specified mod has no key.</returns>
		internal string GetModKey(string p_strModName)
		{
			string strKey = null;
			m_dicModList.TryGetValue(p_strModName, out strKey);
			return strKey;
		}

		/// <summary>
		/// Gets the base name of the <see cref="fomod"/> which was assigned the given key.
		/// </summary>
		/// <param name="p_strModKey">The key of the <see cref="fomod"/> whose name is to be retrieved.</param>
		/// <returns>The base name of the <see cref="fomod"/> which was assigned the given key.</returns>
		protected string GetModName(string p_strModKey)
		{
			foreach (KeyValuePair<string, string> kvpMod in m_dicModList)
				if (kvpMod.Value.Equals(p_strModKey))
					return kvpMod.Key;
			return null;
		}

		/// <summary>
		/// Adds a mod to the install log.
		/// </summary>
		/// <remarks>
		/// Adding a mod to the install log assigns it a key. Keys are used to track file and
		/// edit versions.
		/// </remarks>
		/// <param name="p_strModName">The base name of the <see cref="fomod"/> being added.</param>
		protected void AddMod(string p_strModName)
		{
			if (!m_dicModList.ContainsKey(p_strModName))
			{
				XmlNode xndMod = m_xelModListNode.AppendChild(xmlDoc.CreateElement("mod"));
				xndMod.Attributes.Append(xmlDoc.CreateAttribute("name"));
				xndMod.Attributes.Append(xmlDoc.CreateAttribute("key"));
				xndMod.Attributes["name"].InnerText = p_strModName;
				string strKey = null;
				do
				{
					strKey = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
				} while (m_dicModList.ContainsValue(strKey));
				xndMod.Attributes["key"].InnerText = strKey;
				m_dicModList[p_strModName] = strKey;
			}
		}

		/// <summary>
		/// Removes a mod from the install log.
		/// </summary>
		/// <remarks>
		/// This shouldn't be called anywhere except from <see cref="UnmergeModule"/> to prevent
		/// log corruption.
		/// TODO: Consider integrating this method's functionality into <see cref="UnmergeModule"/>.
		/// </remarks>
		/// <param name="p_strModName">The base name of the <see cref="fomod"/>
		/// that is being removed from the log.</param>
		protected void RemoveMod(string p_strModName)
		{
			if (m_dicModList.ContainsKey(p_strModName))
			{
				XmlNode xndMod = m_xelModListNode.SelectSingleNode("mod[@name=\"" + p_strModName + "\"]");
				m_xelModListNode.RemoveChild(xndMod);
				m_dicModList.Remove(p_strModName);
			}
		}

		#endregion

		#region File Version Management

		/// <summary>
		/// Returns the list of files that have been installed.
		/// </summary>
		/// <remarks>
		/// The return list is ordered alphabetically.
		/// </remarks>
		/// <returns>The list of files that have been installed.</returns>
		internal List<string> GetFileList()
		{
			List<string> lstFiles = new List<string>();

			foreach (XmlNode xndFile in dataFilesNode.ChildNodes)
				lstFiles.Add(xndFile.Attributes["path"].InnerText);
			lstFiles.Sort();

			return lstFiles;
		}

		/// <summary>
		/// Returns the list of files that have been installed by the specified mod.
		/// </summary>
		/// <remarks>
		/// The return list is ordered alphabetically.
		/// </remarks>
		/// <param name="p_strModName">The name of the mod for which to retrieve the list of installed files.</param>
		/// <returns>The list of files that have been installed by the specified mod.</returns>
		internal List<string> GetFileList(string p_strModName)
		{
			List<string> lstFiles = GetMergeModule(p_strModName).DataFiles;
			lstFiles.Sort();
			return lstFiles;
		}

		/// <summary>
		/// Returns the list of mods that have installed the specified file.
		/// </summary>
		/// <remarks>
		/// The returned list is ordered by install date. In other words, the first
		/// mod in the list was the first to install the file, and the last mod in
		/// the list was the most recent. This implies that the current version of
		/// the specified file was installed by the last mod in the list. 
		/// </remarks>
		/// <param name="p_strPath">The file whose installers are to be retrieved.</param>
		/// <returns>The list of mods that have installed the specified file.</returns>
		internal List<string> GetInstallingMods(string p_strPath)
		{
			string strNormalizedPath = NormalizePath(p_strPath);
			List<string> lstInstallers = new List<string>();
			XmlNodeList xnlInstallingMods = dataFilesNode.SelectNodes("file[@path='" + strNormalizedPath.ToLowerInvariant() + "']/installingMods/*");
			foreach (XmlNode xndInallingMod in xnlInstallingMods)
				lstInstallers.Add(GetModName(xndInallingMod.Attributes["key"].InnerText));
			return lstInstallers;
		}

		internal void SetInstallingModsOrder(string p_strPath, List<string> p_lstOrderedMods)
		{
			lock (dataFilesNode)
			{
				List<string> lstCurrentOrder = GetInstallingMods(p_strPath);
				if (lstCurrentOrder.Count != p_lstOrderedMods.Count)
					throw new ArgumentException("The given list mods order must include all installing mods.", "p_lstOrderedMods");
				foreach (string strMod in p_lstOrderedMods)
					if (!lstCurrentOrder.Contains(strMod))
						throw new ArgumentException("The given list mods order must include all installing mods.", "p_lstOrderedMods");
				XmlNode xndInstallingMods = dataFilesNode.SelectSingleNode("file[@path='" + p_strPath.ToLowerInvariant() + "']/installingMods");
				xndInstallingMods.RemoveAll();
				foreach (string strMod in p_lstOrderedMods)
					AddDataFile(strMod, p_strPath);
				Save();
			}
		}

		#endregion

		#region File Install Logging

		public string GetCurrentFileOwnerKey(string p_strPath)
		{
			string strNormalizedPath = NormalizePath(p_strPath);
			XmlNode xndModList = dataFilesNode.SelectSingleNode("file[@path=\"" + strNormalizedPath.ToLowerInvariant() + "\"]/installingMods");
			if (xndModList == null)
				return null;
			XmlNode xndInstallingMod = xndModList.LastChild;
			return xndInstallingMod.Attributes["key"].InnerText;
		}

		/// <summary>
		/// Gets the key of the mod that owned the specified file prior to the current owner.
		/// </summary>
		/// <param name="p_strPath">The path to the file whose previous owner is to be determined.</param>
		/// <returns>The key of the mod who was the previous owner of the specified file, or
		/// <lang cref="null"/> if there was no previous owner.</returns>
		public string GetPreviousFileOwnerKey(string p_strPath)
		{
			string strNormalizedPath = NormalizePath(p_strPath);
			XmlNode xndModList = dataFilesNode.SelectSingleNode("file[@path=\"" + strNormalizedPath.ToLowerInvariant() + "\"]/installingMods");
			if (xndModList == null)
				return null;
			XmlNode xndInstallingMod = xndModList.LastChild;
			xndInstallingMod = xndInstallingMod.PreviousSibling;
			if (xndInstallingMod == null)
				return null;
			return xndInstallingMod.Attributes["key"].InnerText;
		}

		protected void AddDataFile(string modName, string path)
		{
			path = NormalizePath(path.ToLowerInvariant());
			XmlNode xndModList = null;
			XmlNode xndInstallingMod = null;
			lock (dataFilesNode)
			{
				XmlNode xndFile = dataFilesNode.SelectSingleNode("file[@path=\"" + path + "\"]");
				if (xndFile == null)
				{
					xndFile = dataFilesNode.AppendChild(xmlDoc.CreateElement("file"));
					xndFile.Attributes.Append(xmlDoc.CreateAttribute("path"));
					xndFile.Attributes[0].Value = path;
					xndModList = xndFile.AppendChild(xmlDoc.CreateElement("installingMods"));
				}
				else
				{
					xndModList = xndFile.SelectSingleNode("installingMods");
					xndInstallingMod = xndModList.SelectSingleNode("mod[@key='" + m_dicModList[modName] + "']");
					if (xndInstallingMod != null)
						xndModList.RemoveChild(xndInstallingMod);
				}
				if (xndInstallingMod == null)
				{
					xndInstallingMod = xmlDoc.CreateElement("mod");
					xndInstallingMod.Attributes.Append(xmlDoc.CreateAttribute("key"));
					xndInstallingMod.Attributes["key"].InnerText = m_dicModList[modName];
				}
				xndModList.AppendChild(xndInstallingMod);
			}
		}

		#endregion

		#region Ini File Edit Logging

		public string GetCurrentIniEditorModName(string file, string section, string key)
		{
			string strKey = GetCurrentIniEditorModKey(file, section, key);
			if (strKey == null)
				return null;
			return GetModName(strKey);
		}

		public string GetCurrentIniEditorModKey(string file, string section, string key)
		{
			XmlNode xndModList = iniEditsNode.SelectSingleNode("ini[@file='" + file.ToLowerInvariant() + "' and @section='" + section.ToLowerInvariant() + "' and @key='" + key.ToLowerInvariant() + "']/installingMods");
			if (xndModList == null)
				return null;
			XmlNode xndInstallingMod = xndModList.LastChild;
			return xndInstallingMod.Attributes["key"].InnerText;
		}

		/// <summary>
		/// Gets the value of the specified key before it was most recently overwritten.
		/// </summary>
		/// <param name="p_strFile">The Ini file containing the key whose previous value is to be retrieved.</param>
		/// <param name="p_strSection">The section containing the key whose previous value is to be retrieved.</param>
		/// <param name="p_strKey">The key whose previous value is to be retrieved.</param>
		/// <returns>The value of the specified key before it was most recently overwritten, or
		/// <lang cref="null"/> if there was no previous value.</returns>
		public string GetPreviousIniValue(string file, string section, string key)
		{
			XmlNode xndModList = iniEditsNode.SelectSingleNode("ini[@file='" + file.ToLowerInvariant() + "' and @section='" + section.ToLowerInvariant() + "' and @key='" + key.ToLowerInvariant() + "']/installingMods");
			if (xndModList == null)
				return null;
			XmlNode xndInstallingMod = xndModList.LastChild;
			xndInstallingMod = xndInstallingMod.PreviousSibling;
			if (xndInstallingMod == null)
				return null;
			return xndInstallingMod.InnerText;
		}

		protected void AddIniEdit(string file, string section, string key, string mod, string value)
		{
			file = file.ToLowerInvariant();
			section = section.ToLowerInvariant();
			key = key.ToLowerInvariant();
			XmlNode xndModList = null;
			XmlNode xndInstallingMod = null;
			lock (iniEditsNode)
			{
				XmlNode xndIni = iniEditsNode.SelectSingleNode("ini[@file='" + file + "' and @section='" + section + "' and @key='" + key + "']");
				if (xndIni == null)
				{
					xndIni = iniEditsNode.AppendChild(xmlDoc.CreateElement("ini"));
					xndIni.Attributes.Append(xmlDoc.CreateAttribute("file"));
					xndIni.Attributes.Append(xmlDoc.CreateAttribute("section"));
					xndIni.Attributes.Append(xmlDoc.CreateAttribute("key"));
					xndIni.Attributes[0].Value = file;
					xndIni.Attributes[1].Value = section;
					xndIni.Attributes[2].Value = key;
					xndModList = xndIni.AppendChild(xmlDoc.CreateElement("installingMods"));
				}
				else
				{
					xndModList = xndIni.SelectSingleNode("installingMods");
					xndInstallingMod = xndModList.SelectSingleNode("mod[@key='" + m_dicModList[mod] + "']");
					if (xndInstallingMod != null)
						xndModList.RemoveChild(xndInstallingMod);
				}
				if (xndInstallingMod == null)
				{
					xndInstallingMod = xmlDoc.CreateElement("mod");
					xndInstallingMod.Attributes.Append(xmlDoc.CreateAttribute("key"));
					xndInstallingMod.Attributes["key"].InnerText = m_dicModList[mod];
				}
				xndInstallingMod.InnerText = value;
				xndModList.AppendChild(xndInstallingMod);
			}
		}

		#endregion

		#region Shader Edit Logging

		public string GetCurrentShaderEditorModName(int p_intPackage, string p_strName)
		{
			string strKey = GetCurrentShaderEditorModKey(p_intPackage, p_strName);
			if (strKey == null)
				return null;
			return GetModName(strKey);
		}

		public string GetCurrentShaderEditorModKey(int p_intPackage, string p_strShader)
		{
			XmlNode xndModList = sdpEditsNode.SelectSingleNode("sdp[@package='" + p_intPackage + "' and @shader='" + p_strShader.ToLowerInvariant() + "']/installingMods");
			if (xndModList == null)
				return null;
			XmlNode xndInstallingMod = xndModList.LastChild;
			return xndInstallingMod.Attributes["key"].InnerText;
		}

		/// <summary>
		/// Gets the data of the specified shader before it was most recently overwritten.
		/// </summary>
		/// <param name="p_intPackage">The package containing the shader whose previous data is to be retrieved.</param>
		/// <param name="p_strShaderName">The shader whose previous data is to be retrieved.</param>
		/// <returns>The data of the specified shader before it was most recently overwritten, or
		/// <lang cref="null"/> if there was no previous value.</returns>
		public byte[] GetPreviousSdpData(int p_intPackage, string p_strShaderName)
		{
			XmlNode xndModList = sdpEditsNode.SelectSingleNode("sdp[@package='" + p_intPackage + "' and @shader='" + p_strShaderName.ToLowerInvariant() + "']/installingMods");
			if (xndModList == null)
				return null;
			XmlNode xndInstallingMod = xndModList.LastChild;
			xndInstallingMod = xndInstallingMod.PreviousSibling;
			if (xndInstallingMod == null)
				return null;
			string strData = xndInstallingMod.InnerText;
			byte[] bteData = new byte[strData.Length / 2];
			for (int i = 0; i < bteData.Length; i++)
				bteData[i] = byte.Parse("" + strData[i * 2] + strData[i * 2 + 1], System.Globalization.NumberStyles.AllowHexSpecifier);
			return bteData;
		}

		protected void AddShaderEdit(string modName, int p_intPackage, string p_strShader, byte[] p_bteOldData)
		{
			string strLoweredShader = p_strShader.ToLowerInvariant();
			XmlNode xndModList = null;
			XmlNode xndInstallingMod = null;
			lock (sdpEditsNode)
			{
				XmlNode xndSpd = sdpEditsNode.SelectSingleNode("sdp[@package='" + p_intPackage + "' and @shader='" + strLoweredShader + "']");
				if (xndSpd == null)
				{
					xndSpd = sdpEditsNode.AppendChild(xmlDoc.CreateElement("sdp"));
					xndSpd.Attributes.Append(xmlDoc.CreateAttribute("package"));
					xndSpd.Attributes.Append(xmlDoc.CreateAttribute("shader"));
					xndSpd.Attributes[0].Value = p_intPackage.ToString();
					xndSpd.Attributes[1].Value = strLoweredShader;
					xndModList = xndSpd.AppendChild(xmlDoc.CreateElement("installingMods"));

				}
				else
				{
					xndModList = xndSpd.SelectSingleNode("installingMods");
					xndInstallingMod = xndModList.SelectSingleNode("mod[@key='" + m_dicModList[modName] + "']");
					if (xndInstallingMod != null)
						xndModList.RemoveChild(xndInstallingMod);
				}
				if (xndInstallingMod == null)
				{
					xndInstallingMod = xmlDoc.CreateElement("mod");
					xndInstallingMod.Attributes.Append(xmlDoc.CreateAttribute("key"));
					xndInstallingMod.Attributes["key"].InnerText = m_dicModList[modName];
				}
				StringBuilder stbOldData = new StringBuilder(p_bteOldData.Length * 2);
				for (int i = 0; i < p_bteOldData.Length; i++)
					stbOldData.Append(p_bteOldData[i].ToString("x2"));
				xndInstallingMod.InnerText = stbOldData.ToString();
				xndModList.AppendChild(xndInstallingMod);
			}
		}

		#endregion

		#region Merge Module Management

		/// <summary>
		/// Merges the installed and edited components in the given <see cref="InstallLogMergeModule"/>
		/// into the install log for the specified mod.
		/// </summary>
		/// <param name="p_strModName">The base name of the <see cref="fomod"/> for which the
		/// installs and edits where made.</param>
		/// <param name="p_ilmMergeModule">The installs and edits that where made as part of the
		/// <see cref="fomod"/>'s installation.</param>
		public void Merge(string p_strModName, InstallLogMergeModule p_ilmMergeModule)
		{
			AddMod(p_strModName);
			foreach (string strFile in p_ilmMergeModule.ReplacedOriginalDataFiles)
				AddDataFile(ORIGINAL_VALUES, strFile);
			foreach (string strFile in p_ilmMergeModule.DataFiles)
				AddDataFile(p_strModName, strFile);
			foreach (InstallLogMergeModule.IniEdit iniEdit in p_ilmMergeModule.ReplacedOriginalIniValues)
				AddIniEdit(iniEdit.File, iniEdit.Section, iniEdit.Key, ORIGINAL_VALUES, iniEdit.Value);
			foreach (InstallLogMergeModule.IniEdit iniEdit in p_ilmMergeModule.IniEdits)
				AddIniEdit(iniEdit.File, iniEdit.Section, iniEdit.Key, p_strModName, iniEdit.Value);
			foreach (InstallLogMergeModule.SdpEdit spdEdit in p_ilmMergeModule.ReplacedOriginalSdpData)
				AddShaderEdit(ORIGINAL_VALUES, spdEdit.Package, spdEdit.ShaderName, spdEdit.Data);
			foreach (InstallLogMergeModule.SdpEdit spdEdit in p_ilmMergeModule.SdpEdits)
				AddShaderEdit(p_strModName, spdEdit.Package, spdEdit.ShaderName, spdEdit.Data);
			Save();
		}

		/// <summary>
		/// Removes the log entries for the components that were installed and edited during
		/// the installation of the specified <see cref="fomod"/>.
		/// </summary>
		/// <param name="p_strModName">The base name of the <see cref="fomod"/> for which to remove
		/// the log entries for the components that were installed and edited.</param>
		internal void UnmergeModule(string p_strModName)
		{
			InstallLogMergeModule ilmMergeModule = new InstallLogMergeModule();
			XmlNode xndComponent = null;
			XmlNode xndInstallingMods = null;
			XmlNodeList xnlComponentMods = null;
			lock (dataFilesNode)
			{
				xnlComponentMods = dataFilesNode.SelectNodes("descendant::mod[@key='" + m_dicModList[p_strModName] + "']");
				foreach (XmlNode xndFile in xnlComponentMods)
				{
					xndInstallingMods = xndFile.ParentNode;
					xndComponent = xndInstallingMods.ParentNode;
					xndInstallingMods.RemoveChild(xndFile);
					if ((xndInstallingMods.ChildNodes.Count == 0) || (xndInstallingMods.LastChild.Attributes["key"].InnerText.Equals(GetModKey(ORIGINAL_VALUES))))
						xndComponent.ParentNode.RemoveChild(xndComponent);
				}
			}
			lock (iniEditsNode)
			{
				xnlComponentMods = iniEditsNode.SelectNodes("descendant::mod[@key='" + m_dicModList[p_strModName] + "']");
				foreach (XmlNode xndIniEdit in xnlComponentMods)
				{
					xndInstallingMods = xndIniEdit.ParentNode;
					xndComponent = xndInstallingMods.ParentNode;
					xndInstallingMods.RemoveChild(xndIniEdit);
					if ((xndInstallingMods.ChildNodes.Count == 0) || (xndInstallingMods.LastChild.Attributes["key"].InnerText.Equals(GetModKey(ORIGINAL_VALUES))))
						xndComponent.ParentNode.RemoveChild(xndComponent);
				}
			}
			lock (sdpEditsNode)
			{
				xnlComponentMods = sdpEditsNode.SelectNodes("descendant::mod[@key='" + m_dicModList[p_strModName] + "']");
				foreach (XmlNode xndSdpEdit in xnlComponentMods)
				{
					xndInstallingMods = xndSdpEdit.ParentNode;
					xndComponent = xndInstallingMods.ParentNode;
					xndInstallingMods.RemoveChild(xndSdpEdit);
					if ((xndInstallingMods.ChildNodes.Count == 0) || (xndInstallingMods.LastChild.Attributes["key"].InnerText.Equals(GetModKey(ORIGINAL_VALUES))))
						xndComponent.ParentNode.RemoveChild(xndComponent);
				}
			}
			RemoveMod(p_strModName);
			Save();
		}

		/// <summary>
		/// Creates an <see cref="InstallLogMergeModule"/> that describes the components that were
		/// installed and edited during the installation of the specified <see cref="fomod"/>.
		/// </summary>
		/// <param name="p_strModName">The base name of the <see cref="fomod"/> for which to retrieve
		/// the components that were installed and edited.</param>
		/// <returns>An <see cref="InstallLogMergeModule"/> that describes the components that were
		/// installed and edited during the installation of the specified <see cref="fomod"/>.</returns>
		internal InstallLogMergeModule GetMergeModule(string p_strModName)
		{
			InstallLogMergeModule ilmMergeModule = new InstallLogMergeModule();
			XmlNode xndComponent = null;
			XmlNodeList xnlComponentMods = dataFilesNode.SelectNodes("descendant::mod[@key='" + m_dicModList[p_strModName] + "']");
			foreach (XmlNode xndFile in xnlComponentMods)
			{
				xndComponent = xndFile.ParentNode.ParentNode;
				ilmMergeModule.AddFile(xndComponent.Attributes["path"].InnerText);
			}
			xnlComponentMods = iniEditsNode.SelectNodes("descendant::mod[@key='" + m_dicModList[p_strModName] + "']");
			foreach (XmlNode xndIniEdit in xnlComponentMods)
			{
				xndComponent = xndIniEdit.ParentNode.ParentNode;
				ilmMergeModule.AddIniEdit(xndComponent.Attributes["file"].InnerText,
											xndComponent.Attributes["section"].InnerText,
											xndComponent.Attributes["key"].InnerText,
											xndIniEdit.InnerText);
			}
			xnlComponentMods = sdpEditsNode.SelectNodes("descendant::mod[@key='" + m_dicModList[p_strModName] + "']");
			foreach (XmlNode xndSdpEdit in xnlComponentMods)
			{
				xndComponent = xndSdpEdit.ParentNode.ParentNode;

				string strData = xndSdpEdit.InnerText;
				byte[] bteData = new byte[strData.Length / 2];
				for (int i = 0; i < bteData.Length; i++)
					bteData[i] = byte.Parse("" + strData[i * 2] + strData[i * 2 + 1], System.Globalization.NumberStyles.AllowHexSpecifier);

				ilmMergeModule.AddSdpEdit(Int32.Parse(xndComponent.Attributes["package"].InnerText),
											xndComponent.Attributes["shader"].InnerText,
											bteData);
			}

			return ilmMergeModule;
		}

		#endregion
	}
}
