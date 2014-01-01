using System;
using System.Xml;
using Path = System.IO.Path;
using File = System.IO.File;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Fomm.PackageManager.ModInstallLog
{
	/*
	 * InstallLog.xml structure
	 * <installLog>
	 *   <modList>
	 *	   <mod name="" key="">
	 *       <version machineVersion="">mod version</version>
	 *	   </mod>
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
	 *   <gameSpecificEdits>
	 *	   <edit key=''>
	 *       <installingMods>
	 *	       <mod key="">old value</mod>
	 *	     </installingMods>
	 *	   </edit>
	 *	 </gameSpecificEdits>
	 * </installLog>
	 * 
	 * Other data eg:
	 *   <gameSpecificEdits>
	 *     <edit key='spd:<package name>/<shader name>'>
	 *       <installingMods>
	 *	       <mod key="">old shader value</mod>
	 *	     </installingMods>
	 *     </edit>
	 *   </gameSpecificEdits>
	 */
	internal class InstallLog : InstallLogBase
	{
		public static readonly Version CURRENT_VERSION = new Version("0.2.0.0");
		internal protected const string ORIGINAL_VALUES = "ORIGINAL_VALUES";
		internal protected const string FOMM = "FOMM";
		private static InstallLog m_ilgCurrent = null;

		public static InstallLog Current
		{
			get
			{
				if (m_ilgCurrent == null)
					Reload();
				return m_ilgCurrent;
			}
		}

		/// <summary>
		/// Forces the install log to reload itself.
		/// </summary>
		internal static void Reload()
		{
			m_ilgCurrent = new InstallLog();
		}

		private readonly string xmlpath = Path.Combine(Program.GameMode.InstallInfoDirectory, "InstallLog.xml");
		private XmlDocument xmlDoc;
		private XmlElement m_xelModListNode;
		private XmlElement dataFilesNode;
		private XmlElement iniEditsNode;
		private XmlElement gameSpecificValueEditsNode;
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

		/// <summary>
		/// Gets the <see cref="XmlDocument"/> used to interact with the xml install log.
		/// </summary>
		/// <value>The <see cref="XmlDocument"/> used to interact with the xml install log.</value>
		protected XmlDocument Document
		{
			get
			{
				return xmlDoc;
			}
		}

		/// <summary>
		/// Gets the node that lists the installed data files.
		/// </summary>
		/// <value>The node that lists the installed data files.</value>
		protected XmlElement DataFilesNode
		{
			get
			{
				return dataFilesNode;
			}
		}

		/// <summary>
		/// Sets whether or not the install log watches for external changes to the xml file.
		/// </summary>
		/// <value>Whether or not the install log watches for external changes to the xml file.</value>
		internal protected bool EnableLogFileRefresh
		{
			set
			{
				m_fswLogWatcher.EnableRaisingEvents = value;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		protected InstallLog()
		{
			m_fswLogWatcher = new FileSystemWatcher(Path.GetDirectoryName(xmlpath));
			m_fswLogWatcher.Filter = Path.GetFileName(xmlpath);
			m_fswLogWatcher.Changed += new FileSystemEventHandler(InstallLogWatcher_Changed);
			m_fswLogWatcher.EnableRaisingEvents = true;
			Load();
		}

		#endregion

		#region Initialization

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
				try
				{
					xmlDoc.Load(xmlpath);
				}
				catch (XmlException e)
				{
					Exception exContent = new Exception("Bad InstallLog:" + Environment.NewLine + File.ReadAllText(xmlpath), e);
					throw new Exception("Malformed InstallLog (" + xmlpath + ")", exContent);
				}
				m_xelModListNode = (XmlElement)xmlDoc.SelectSingleNode("installLog/modList");
				dataFilesNode = (XmlElement)xmlDoc.SelectSingleNode("installLog/dataFiles");
				iniEditsNode = (XmlElement)xmlDoc.SelectSingleNode("installLog/iniEdits");
				gameSpecificValueEditsNode = (XmlElement)xmlDoc.SelectSingleNode("installLog/gameSpecificEdits");
				if (m_xelModListNode == null)
				{
					XmlNode root = (XmlNode)xmlDoc.SelectSingleNode("installLog");
					root.InsertBefore(m_xelModListNode = xmlDoc.CreateElement("modList"), dataFilesNode);
				}
				InitMods();
			}
			else
			{
				Reset();
				SetInstallLogVersion(new Version("0.0.0.0"));
				Save();
			}
		}

		internal protected void Reset()
		{
			xmlDoc.RemoveAll();
			XmlNode root = xmlDoc.AppendChild(xmlDoc.CreateElement("installLog"));
			root.AppendChild(m_xelModListNode = xmlDoc.CreateElement("modList"));
			root.AppendChild(dataFilesNode = xmlDoc.CreateElement("dataFiles"));
			root.AppendChild(iniEditsNode = xmlDoc.CreateElement("iniEdits"));
			root.AppendChild(gameSpecificValueEditsNode = xmlDoc.CreateElement("gameSpecificEdits"));
			InitMods();
		}

		protected void InitMods()
		{
			XmlNodeList xnlMods = m_xelModListNode.ChildNodes;
			m_dicModList = new Dictionary<string, string>();
      foreach (XmlNode xndMod in xnlMods)
      {
        if ((xndMod.Attributes["name"] != null) && (xndMod.Attributes["key"] != null))
        {
          m_dicModList[xndMod.Attributes["name"].InnerText] = xndMod.Attributes["key"].InnerText;
        }
      }

			AddMod(ORIGINAL_VALUES);
			AddMod(FOMM);
		}

		#endregion

		/// <summary>
		/// Saves the Install Log.
		/// </summary>
		internal protected void Save()
		{
			bool boolWasWatching = m_fswLogWatcher.EnableRaisingEvents;
			m_fswLogWatcher.EnableRaisingEvents = false;
			xmlDoc.Save(xmlpath);
			m_fswLogWatcher.EnableRaisingEvents = boolWasWatching;
		}

		/// <summary>
		/// Gets the version of the install log.
		/// </summary>
		/// <returns>The version of the install log.</returns>
		internal Version GetInstallLogVersion()
		{
			XmlAttribute xndVersion;

      if (xmlDoc.FirstChild.Attributes != null)
      {
        if (xmlDoc.FirstChild.Attributes["fileVersion"] != null)
        {
          xndVersion = xmlDoc.FirstChild.Attributes["fileVersion"];
          return new Version(xndVersion.InnerText);
        }
      }

      if (xmlDoc.ChildNodes.Count > 1)
      {
        if (xmlDoc.ChildNodes[1].Attributes != null)
        {
          if (xmlDoc.ChildNodes[1].Attributes["fileVersion"] != null)
          {
            xndVersion = xmlDoc.ChildNodes[1].Attributes["fileVersion"];
            return new Version(xndVersion.InnerText);
          }
        }
      }

      return new Version("0.0.0.0");
		}

		/// <summary>
		/// Sets the version of the install log.
		/// </summary>
		/// <param name="p_verFileVersion">The version of the install log.</param>
		internal protected void SetInstallLogVersion(Version p_verFileVersion)
		{
			XmlAttribute xndVersion = xmlDoc.FirstChild.Attributes["fileVersion"];
			if (xndVersion == null)
				xndVersion = xmlDoc.FirstChild.Attributes.Append(xmlDoc.CreateAttribute("fileVersion"));
			xndVersion.InnerText = p_verFileVersion.ToString();
		}

		#region Mod Tracking

		/// <summary>
		/// Returns the list of mods that have been installed.
		/// </summary>
		/// <remarks>
		/// The return list is ordered alphabetically.
		/// </remarks>
		/// <returns>The list of mods that have been installed.</returns>
		internal IList<string> GetModList()
		{
			List<string> lstMods = new List<string>();
			foreach (string strMod in m_dicModList.Keys)
				lstMods.Add(strMod);
			lstMods.Sort();
			return lstMods;
		}

		/// <summary>
		/// Returns the list of mods that have been installed, with their
		/// versions.
		/// </summary>
		/// <remarks>
		/// The return list is ordered alphabetically.
		/// </remarks>
		/// <returns>The list of mods that have been installed, with their
		/// versions.</returns>
		internal IList<FomodInfo> GetVersionedModList()
		{
			List<FomodInfo> lstMods = new List<FomodInfo>();
			XmlNodeList xnlMods = m_xelModListNode.ChildNodes;
			XmlNode xndVersion = null;
			string strBaseName = null;
			foreach (XmlNode xndMod in xnlMods)
			{
				strBaseName = xndMod.Attributes["name"].InnerText;
				if (strBaseName.Equals(ORIGINAL_VALUES) || strBaseName.Equals(FOMM))
					continue;
				xndVersion = xndMod.SelectSingleNode("version");
				if ((xndVersion == null) || (xndVersion.Attributes["machineVersion"] == null))
					throw new InstallLogException("Cannot find version for mod '" + strBaseName + "'. Install Log Version: " + GetInstallLogVersion());
				lstMods.Add(new FomodInfo(strBaseName, xndVersion.InnerText, new Version(xndVersion.Attributes["machineVersion"].InnerText)));
			}
			lstMods.Sort();
			return lstMods;
		}

		/// <summary>
		/// Returns install information about the specified <see cref="fomod"/>.
		/// </summary>
		/// <param name="p_strBaseName">The base name of the <see cref="fomod"/> for which to return information.</param>
		/// <returns>Install information about the specified <see cref="fomod"/>, or <lang cref="null"/> if the
		/// specified fomod is not installed.</returns>
		internal FomodInfo GetModInfo(string p_strBaseName)
		{
			XmlNode xndVersion = m_xelModListNode.SelectSingleNode("mod[@name=\"" + p_strBaseName + "\"]/version");
			if (xndVersion == null)
				return null;
			return new FomodInfo(p_strBaseName, xndVersion.InnerText, new Version(xndVersion.Attributes["machineVersion"].InnerText));
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
		internal string GetModName(string p_strModKey)
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
		protected XmlNode AddMod(string p_strModName)
		{
			XmlNode xndMod = null;
			if (!m_dicModList.ContainsKey(p_strModName))
			{
				xndMod = m_xelModListNode.AppendChild(xmlDoc.CreateElement("mod"));
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
			return xndMod;
		}

		/// <summary>
		/// Adds a mod to the install log.
		/// </summary>
		/// <remarks>
		/// Adding a mod to the install log assigns it a key. Keys are used to track file and
		/// edit versions.
		/// </remarks>
		/// <param name="p_fomodMod">The <see cref="fomod"/> being added.</param>
		internal protected void AddMod(fomod p_fomodMod)
		{
			if (!m_dicModList.ContainsKey(p_fomodMod.BaseName))
			{
				XmlNode xndMod = AddMod(p_fomodMod.BaseName);
				if (xndMod == null)
					return;

				XmlNode xndVersion = xndMod.AppendChild(xmlDoc.CreateElement("version"));
				xndVersion.Attributes.Append(xmlDoc.CreateAttribute("machineVersion"));
				xndVersion.Attributes["machineVersion"].InnerText = p_fomodMod.MachineVersion.ToString();
				xndVersion.InnerText = p_fomodMod.HumanReadableVersion;
			}
		}

		/// <summary>
		/// Updates a mod's information in the install log.
		/// </summary>
		/// <remarks>
		/// This updates the given mod's version in the install log without changing its key.
		/// </remarks>
		/// <param name="p_fomodMod">The <see cref="fomod"/> being updated.</param>
		/// <returns><lang cref="true"/> if the given fomod was found and it's information updated;
		/// <lang cref="false"/> otherwise.</returns>
		internal protected bool UpdateMod(fomod p_fomodMod)
		{
			if (m_dicModList.ContainsKey(p_fomodMod.BaseName))
			{
				XmlNode xndMod = m_xelModListNode.SelectSingleNode("mod[@key=\"" + GetModKey(p_fomodMod.BaseName) + "\"]");
				XmlNode xndVersion = xndMod.SelectSingleNode("version");
				if (xndVersion == null)
				{
					xndVersion = xndMod.AppendChild(xmlDoc.CreateElement("version"));
					xndVersion.Attributes.Append(xmlDoc.CreateAttribute("machineVersion"));
				}
				xndVersion.Attributes["machineVersion"].InnerText = p_fomodMod.MachineVersion.ToString();
				xndVersion.InnerText = p_fomodMod.HumanReadableVersion;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Updates a mod's information in the install log.
		/// </summary>
		/// <remarks>
		/// This updates the given mod's version and base name in the install log without changing its key.
		/// </remarks>
		/// <param name="p_strOldBaseName">The old base name of the mod whose information is to be updated.</param>
		/// <param name="p_fomodMod">The <see cref="fomod"/> containing the new information.</param>
		/// <returns><lang cref="true"/> if the given fomod was found and it's information updated;
		/// <lang cref="false"/> otherwise.</returns>
		protected bool UpdateMod(string p_strOldBaseName, fomod p_fomodMod)
		{
			if (p_fomodMod.BaseName.Equals(p_strOldBaseName))
				return UpdateMod(p_fomodMod);
			if (m_dicModList.ContainsKey(p_strOldBaseName))
			{
				string strKey = GetModKey(p_strOldBaseName);
				XmlNode xndMod = m_xelModListNode.SelectSingleNode("mod[@key=\"" + strKey + "\"]");
				xndMod.Attributes["name"].InnerText = p_fomodMod.BaseName;
				m_dicModList.Remove(p_strOldBaseName);
				m_dicModList.Add(p_fomodMod.BaseName, strKey);
				return UpdateMod(p_fomodMod);
			}
			return false;
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
			XmlNodeList xnlInstallingMods = dataFilesNode.SelectNodes("file[@path=\"" + strNormalizedPath.ToLowerInvariant() + "\"]/installingMods/*");
			foreach (XmlNode xndInallingMod in xnlInstallingMods)
				lstInstallers.Add(GetModName(xndInallingMod.Attributes["key"].InnerText));
			return lstInstallers;
		}

		internal void SetInstallingModsOrder(string p_strPath, IList<string> p_lstOrderedMods)
		{
			lock (dataFilesNode)
			{
				IList<string> lstCurrentOrder = GetInstallingMods(p_strPath);
				if (lstCurrentOrder.Count != p_lstOrderedMods.Count)
					throw new ArgumentException("The given list mods order must include all installing mods.", "p_lstOrderedMods");
				foreach (string strMod in p_lstOrderedMods)
					if (!lstCurrentOrder.Contains(strMod))
						throw new ArgumentException("The given list mods order must include all installing mods.", "p_lstOrderedMods");
				XmlNode xndInstallingMods = dataFilesNode.SelectSingleNode("file[@path=\"" + p_strPath.ToLowerInvariant() + "\"]/installingMods");
				xndInstallingMods.RemoveAll();
				foreach (string strMod in p_lstOrderedMods)
					AddDataFile(strMod, p_strPath);
				Save();
			}
		}

		public string GetCurrentFileOwnerKey(string p_strPath)
		{
			string strNormalizedPath = NormalizePath(p_strPath);
			XmlNode xndModList = dataFilesNode.SelectSingleNode("file[@path=\"" + strNormalizedPath.ToLowerInvariant() + "\"]/installingMods");
			if (xndModList == null)
				return null;
			XmlNode xndInstallingMod = xndModList.LastChild;
			return xndInstallingMod.Attributes["key"].InnerText;
		}

		public string GetCurrentFileOwnerName(string p_strPath)
		{
			string strKey = GetCurrentFileOwnerKey(p_strPath);
			if (strKey == null)
				return null;
			return GetModName(strKey);
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

		#endregion

		#region File Install Logging

		/// <summary>
		/// Creates a node representing that the specified mod installed the specified file.
		/// </summary>
		/// <param name="p_strModKey">The key of the mod that installed the file.</param>
		/// <param name="p_strPath">The path of the file that was installed.</param>
		/// <param name="p_xndModList">An out pramater returning the node containing the list of mods that
		/// have installed the specified file. This is useful for inserting the created node.</param>
		/// <returns>A node representing that the specified mod installed the specified file. The out
		/// parameter <paramref name="p_xndModList"/> returns the node containing the list of mods that
		/// have installed the specified file.</returns>
		protected XmlNode CreateDataFileNode(string p_strModKey, string p_strPath, out XmlNode p_xndModList)
		{
			p_strPath = NormalizePath(p_strPath.ToLowerInvariant());
			XmlNode xndInstallingMod = null;
			lock (dataFilesNode)
			{
				XmlNode xndFile = dataFilesNode.SelectSingleNode("file[@path=\"" + p_strPath + "\"]");
				if (xndFile == null)
				{
					xndFile = dataFilesNode.AppendChild(xmlDoc.CreateElement("file"));
					xndFile.Attributes.Append(xmlDoc.CreateAttribute("path"));
					xndFile.Attributes[0].Value = p_strPath;
					p_xndModList = xndFile.AppendChild(xmlDoc.CreateElement("installingMods"));
				}
				else
				{
					p_xndModList = xndFile.SelectSingleNode("installingMods");
					xndInstallingMod = p_xndModList.SelectSingleNode("mod[@key=\"" + p_strModKey + "\"]");
					if (xndInstallingMod != null)
						p_xndModList.RemoveChild(xndInstallingMod);
				}
				if (xndInstallingMod == null)
				{
					xndInstallingMod = xmlDoc.CreateElement("mod");
					xndInstallingMod.Attributes.Append(xmlDoc.CreateAttribute("key"));
					xndInstallingMod.Attributes["key"].InnerText = p_strModKey;
				}
			}
			return xndInstallingMod;
		}

		/// <summary>
		/// Adds a node representing that the specified mod installed the specified file.
		/// </summary>
		/// <remarks>
		/// This method appends the node to the end of the list of installing mods, indicating
		/// that the specified mod is the latest mod to install the specified file.
		/// 
		/// If the specified mod has already installed the specified file, it is moved to the end of the
		/// list, making if the current owner.
		/// </remarks>
		/// <param name="p_strModName">The base name of the mod that installed the file.</param>
		/// <param name="p_strPath">The path of the file that was installed.</param>
		internal protected void AddDataFile(string p_strModName, string p_strPath)
		{
			XmlNode xndModList = null;
			XmlNode xndInstallingMod = CreateDataFileNode(m_dicModList[p_strModName], p_strPath, out xndModList);
			lock (dataFilesNode)
			{
				xndModList.AppendChild(xndInstallingMod);
			}
		}

		/// <summary>
		/// Replaces a node representing that the specified mod installed the specified file.
		/// </summary>
		/// <remarks>
		/// If the specified mod already installed the specified file, nothing is done. Otherwise,
		/// this method appends the node to the end of the list of installing mods, indicating
		/// that the specified mod is the latest mod to install the specified file.
		/// </remarks>
		/// <param name="p_strModName">The base name of the mod that installed the file.</param>
		/// <param name="p_strPath">The path of the file that was installed.</param>
		/// <seealso cref="AddDataFile(string p_strModName, string p_strPath)"/>
		protected void ReplaceDataFile(string p_strModName, string p_strPath)
		{
			p_strPath = NormalizePath(p_strPath.ToLowerInvariant());
			XmlNode xndInstallingMod = dataFilesNode.SelectSingleNode("file[@path=\"" + p_strPath + "\"]/installingMods/mod[@key=\"" + GetModKey(p_strModName) + "\"]");
			if (xndInstallingMod == null)
				AddDataFile(p_strModName, p_strPath);
		}

		/// <summary>
		/// Adds a node representing that the specified mod installed the specified file.
		/// </summary>
		/// <remarks>
		/// This method prepends the node to the beginning of the list of installing mods, indicating
		/// that the specified mod is not the latest mod to install the specified file.
		/// </remarks>
		/// <param name="p_strModName">The base name of the mod that installed the file.</param>
		/// <param name="p_strPath">The path of the file that was installed.</param>
		internal protected void PrependDataFile(string p_strModName, string p_strPath)
		{
			XmlNode xndModList = null;
			XmlNode xndInstallingMod = CreateDataFileNode(GetModKey(p_strModName), p_strPath, out xndModList);
			xndModList.PrependChild(xndInstallingMod);
		}

		/// <summary>
		/// Removes the node representing that the specified mod installed the specified file.
		/// </summary>
		/// <param name="p_strModName">The base name of the mod that installed the file.</param>
		/// <param name="p_strPath">The path of the file that was installed.</param>
		protected void RemoveDataFile(string p_strModName, string p_strPath)
		{
			p_strPath = NormalizePath(p_strPath.ToLowerInvariant());
			lock (dataFilesNode)
			{
				XmlNode xndInstallingMod = dataFilesNode.SelectSingleNode("file[@path=\"" + p_strPath + "\"]/installingMods/mod[@key=\"" + GetModKey(p_strModName) + "\"]");
				if (xndInstallingMod != null)
				{
					XmlNode xndInstallingMods = xndInstallingMod.ParentNode;
					XmlNode xndFile = xndInstallingMods.ParentNode;
					xndInstallingMods.RemoveChild(xndInstallingMod);
					if ((xndInstallingMods.ChildNodes.Count == 0) || (xndInstallingMods.LastChild.Attributes["key"].InnerText.Equals(GetModKey(ORIGINAL_VALUES))))
						xndFile.ParentNode.RemoveChild(xndFile);
				}
			}
		}

		#endregion

		#region Ini Edit Version Management

		/// <summary>
		/// Returns the list of mods that have edited the spcified Ini value.
		/// </summary>
		/// <remarks>
		/// The returned list is ordered by install date. In other words, the first
		/// mod in the list was the first to edit the value, and the last mod in
		/// the list was the most recent. This implies that the current version of
		/// the specified edit was installed by the last mod in the list. 
		/// </remarks>
		/// <param name="p_strFile">The Ini file containing the key whose editors are to be retrieved.</param>
		/// <param name="p_strSection">The section containing the key whose editors are to be retrieved.</param>
		/// <param name="p_strKey">The key whose editors are to be retrieved.</param>
		/// <returns>The list of mods that have edited the specified Ini value.</returns>
		internal List<string> GetInstallingMods(string p_strFile, string p_strSection, string p_strKey)
		{
			List<string> lstInstallers = new List<string>();
			XmlNodeList xnlInstallingMods = iniEditsNode.SelectNodes("ini[@file=\"" + p_strFile.ToLowerInvariant() + "\" and @section=\"" + p_strSection.ToLowerInvariant() + "\" and @key=\"" + p_strKey.ToLowerInvariant() + "\"]/installingMods/*");
			foreach (XmlNode xndInallingMod in xnlInstallingMods)
				lstInstallers.Add(GetModName(xndInallingMod.Attributes["key"].InnerText));
			return lstInstallers;
		}

		public string GetCurrentIniEditorModName(string file, string section, string key)
		{
			string strKey = GetCurrentIniEditorModKey(file, section, key);
			if (strKey == null)
				return null;
			return GetModName(strKey);
		}

		public string GetCurrentIniEditorModKey(string file, string section, string key)
		{
			XmlNode xndModList = iniEditsNode.SelectSingleNode("ini[@file=\"" + file.ToLowerInvariant() + "\" and @section=\"" + section.ToLowerInvariant() + "\" and @key=\"" + key.ToLowerInvariant() + "\"]/installingMods");
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
			XmlNode xndModList = iniEditsNode.SelectSingleNode("ini[@file=\"" + file.ToLowerInvariant() + "\" and @section=\"" + section.ToLowerInvariant() + "\" and @key=\"" + key.ToLowerInvariant() + "\"]/installingMods");
			if (xndModList == null)
				return null;
			XmlNode xndInstallingMod = xndModList.LastChild;
			xndInstallingMod = xndInstallingMod.PreviousSibling;
			if (xndInstallingMod == null)
				return null;
			return xndInstallingMod.InnerText;
		}

		#endregion

		#region Ini Edit Logging

		/// <summary>
		/// Creates a node representing that the specified mod made the specified Ini edit.
		/// </summary>
		/// <param name="p_strModKey">The key of the mod that made the edit.</param>
		/// <param name="p_strFile">The Ini file that was edited.</param>
		/// <param name="p_strSection">The section in the Ini file that was edited.</param>
		/// <param name="p_strKey">The key in the Ini file that was edited.</param>
		/// <param name="p_strValue">The value to which to the key was set.</param>
		/// <param name="p_xndModList">An out pramater returning the node containing the list of mods that
		/// have edited the specified key. This is useful for inserting the created node.</param>
		/// <returns>A node representing that the specified mod made the specified Ini edit. The out
		/// parameter <paramref name="p_xndModList"/> returns the node containing the list of mods that
		/// have edited the specified key.</returns>
		protected XmlNode CreateIniEditNode(string p_strModKey, string p_strFile, string p_strSection, string p_strKey, string p_strValue, out XmlNode p_xndModList)
		{
			p_strFile = p_strFile.ToLowerInvariant();
			p_strSection = p_strSection.ToLowerInvariant();
			p_strKey = p_strKey.ToLowerInvariant();
			XmlNode xndInstallingMod = null;
			lock (iniEditsNode)
			{
				XmlNode xndIni = iniEditsNode.SelectSingleNode("ini[@file=\"" + p_strFile + "\" and @section=\"" + p_strSection + "\" and @key=\"" + p_strKey + "\"]");
				if (xndIni == null)
				{
					xndIni = iniEditsNode.AppendChild(xmlDoc.CreateElement("ini"));
					xndIni.Attributes.Append(xmlDoc.CreateAttribute("file"));
					xndIni.Attributes.Append(xmlDoc.CreateAttribute("section"));
					xndIni.Attributes.Append(xmlDoc.CreateAttribute("key"));
					xndIni.Attributes[0].Value = p_strFile;
					xndIni.Attributes[1].Value = p_strSection;
					xndIni.Attributes[2].Value = p_strKey;
					p_xndModList = xndIni.AppendChild(xmlDoc.CreateElement("installingMods"));
				}
				else
				{
					p_xndModList = xndIni.SelectSingleNode("installingMods");
					xndInstallingMod = p_xndModList.SelectSingleNode("mod[@key=\"" + p_strModKey + "\"]");
					if (xndInstallingMod != null)
						p_xndModList.RemoveChild(xndInstallingMod);
				}
				if (xndInstallingMod == null)
				{
					xndInstallingMod = xmlDoc.CreateElement("mod");
					xndInstallingMod.Attributes.Append(xmlDoc.CreateAttribute("key"));
					xndInstallingMod.Attributes["key"].InnerText = p_strModKey;
				}
				xndInstallingMod.InnerText = p_strValue;
			}
			return xndInstallingMod;
		}

		/// <summary>
		/// Adds a node representing that the specified mod made the specified Ini edit.
		/// </summary>
		/// <remarks>
		/// This method appends the node to the end of the list of installing mods, indicating
		/// that the specified mod is the latest mod to edit the specified Ini value.
		/// </remarks>
		/// <param name="p_strFile">The Ini file that was edited.</param>
		/// <param name="p_strSection">The section in the Ini file that was edited.</param>
		/// <param name="p_strKey">The key in the Ini file that was edited.</param>
		/// <param name="p_strModName">The base name of the mod that made the edit.</param>
		/// <param name="p_strValue">The value to which to the key was set.</param>
		internal protected void AddIniEdit(string p_strFile, string p_strSection, string p_strKey, string p_strModName, string p_strValue)
		{
			XmlNode xndModList = null;
			XmlNode xndInstallingMod = CreateIniEditNode(m_dicModList[p_strModName], p_strFile, p_strSection, p_strKey, p_strValue, out xndModList);
			lock (iniEditsNode)
			{
				xndModList.AppendChild(xndInstallingMod);
			}
		}

		/// <summary>
		/// Replaces a node representing that the specified mod made the specified Ini edit.
		/// </summary>
		/// <remarks>
		/// If the specified mod already edited the specified file, the value of the edit is updated,
		/// but the install order is not changed. Otherwise, this method appends the node to the end of the
		/// list of installing mods, indicating that the specified mod is the latest mod to edit the
		/// specified Ini value.
		/// </remarks>
		/// <param name="p_strFile">The Ini file that was edited.</param>
		/// <param name="p_strSection">The section in the Ini file that was edited.</param>
		/// <param name="p_strKey">The key in the Ini file that was edited.</param>
		/// <param name="p_strModName">The base name of the mod that made the edit.</param>
		/// <param name="p_strValue">The value to which to the key was set.</param>
		/// <seealso cref="AddIniEdit(string p_strFile, string p_strSection, string p_strKey, string p_strModName, string p_strValue)"/>
		internal protected void ReplaceIniEdit(string p_strFile, string p_strSection, string p_strKey, string p_strModName, string p_strValue)
		{
			p_strFile = p_strFile.ToLowerInvariant();
			p_strSection = p_strSection.ToLowerInvariant();
			p_strKey = p_strKey.ToLowerInvariant();
			XmlNode xndInstallingMod = iniEditsNode.SelectSingleNode("ini[@file=\"" + p_strFile + "\" and @section=\"" + p_strSection + "\" and @key=\"" + p_strKey + "\"]/installingMods/mod[@key=\"" + GetModKey(p_strModName) + "\"]");
			if (xndInstallingMod != null)
				xndInstallingMod.InnerText = p_strValue;
			else
				AddIniEdit(p_strFile, p_strSection, p_strKey, p_strModName, p_strValue);
		}

		/// <summary>
		/// Adds a node representing that the specified mod made the specified Ini edit.
		/// </summary>
		/// <remarks>
		/// This method prepends the node to the beginning of the list of installing mods, but
		/// after the ORIGINAL_VALUES node if it exists, indicating that the specified mod is not
		/// the latest mod to edit the specified Ini value.
		/// </remarks>
		/// <param name="p_strFile">The Ini file that was edited.</param>
		/// <param name="p_strSection">The section in the Ini file that was edited.</param>
		/// <param name="p_strKey">The key in the Ini file that was edited.</param>
		/// <param name="p_strModName">The base name of the mod that made the edit.</param>
		/// <param name="p_strValue">The value to which to the key was set.</param>
		internal protected void PrependAfterOriginalIniEdit(string p_strFile, string p_strSection, string p_strKey, string p_strModName, string p_strValue)
		{
			XmlNode xndModList = null;
			XmlNode xndInstallingMod = CreateIniEditNode(GetModKey(p_strModName), p_strFile, p_strSection, p_strKey, p_strValue, out xndModList);
			if ((xndModList.FirstChild != null) && (xndModList.FirstChild.Attributes["key"].InnerText.Equals(OriginalValuesKey)))
				xndModList.InsertAfter(xndInstallingMod, xndModList.FirstChild);
			else
				xndModList.PrependChild(xndInstallingMod);
		}

		/// <summary>
		/// Removes the node representing that the specified mod edited the specified Ini value.
		/// </summary>
		/// <param name="p_strModName">The base name of the mod that edited the value.</param>
		/// <param name="p_strFile">The Ini file that was edited.</param>
		/// <param name="p_strSection">The section in the Ini file that was edited.</param>
		/// <param name="p_strKey">The key in the Ini file that was edited.</param>
		protected void RemoveIniEdit(string p_strModName, string p_strFile, string p_strSection, string p_strKey)
		{
			p_strFile = p_strFile.ToLowerInvariant();
			p_strSection = p_strSection.ToLowerInvariant();
			p_strKey = p_strKey.ToLowerInvariant();
			lock (iniEditsNode)
			{
				XmlNode xndInstallingMod = iniEditsNode.SelectSingleNode("ini[@file=\"" + p_strFile + "\" and @section=\"" + p_strSection + "\" and @key=\"" + p_strKey + "\"]/installingMods/mod[@key=\"" + GetModKey(p_strModName) + "\"]");
				if (xndInstallingMod != null)
				{
					XmlNode xndInstallingMods = xndInstallingMod.ParentNode;
					XmlNode xndIniEdit = xndInstallingMods.ParentNode;
					xndInstallingMods.RemoveChild(xndInstallingMod);
					if ((xndInstallingMods.ChildNodes.Count == 0) || (xndInstallingMods.LastChild.Attributes["key"].InnerText.Equals(GetModKey(ORIGINAL_VALUES))))
						xndIniEdit.ParentNode.RemoveChild(xndIniEdit);
				}
			}
		}

		#endregion

		#region Game-Specific Value Edit Version Management

		/// <summary>
		/// Returns the list of mods that have edited the spcified game-specific value.
		/// </summary>
		/// <remarks>
		/// The returned list is ordered by install date. In other words, the first
		/// mod in the list was the first to edit the game-specific value, and the last mod in
		/// the list was the most recent. This implies that the current version of
		/// the specified edit was installed by the last mod in the list. 
		/// </remarks>
		/// <param name="p_strValueKey">The key of the game-specific value whose editors are to be retrieved.</param>
		/// <returns>The list of mods that have edited the specified game-specific value.</returns>
		internal List<string> GetGameSpecifcValueInstallingMods(string p_strValueKey)
		{
			List<string> lstInstallers = new List<string>();
			XmlNodeList xnlInstallingMods = gameSpecificValueEditsNode.SelectNodes("edit[@key=\"" + p_strValueKey.ToLowerInvariant() + "\"]/installingMods/*");
			foreach (XmlNode xndInallingMod in xnlInstallingMods)
				lstInstallers.Add(GetModName(xndInallingMod.Attributes["key"].InnerText));
			return lstInstallers;
		}

		public string GetCurrentGameSpecifcValueEditorModName(string p_strValueKey)
		{
			string strKey = GetCurrentGameSpecifcValueEditorModKey(p_strValueKey);
			if (strKey == null)
				return null;
			return GetModName(strKey);
		}

		public string GetCurrentGameSpecifcValueEditorModKey(string p_strValueKey)
		{
			XmlNode xndModList = gameSpecificValueEditsNode.SelectSingleNode("edit[@key=\"" + p_strValueKey.ToLowerInvariant() + "\"]/installingMods");
			if (xndModList == null)
				return null;
			XmlNode xndInstallingMod = xndModList.LastChild;
			return xndInstallingMod.Attributes["key"].InnerText;
		}

		/// <summary>
		/// Gets the data of the specified game-specific value before it was most recently overwritten.
		/// </summary>
		/// <param name="p_strValueKey">The key of the game-specific value whose previous data is to be retrieved.</param>
		/// <returns>The data of the specified game-specific value before it was most recently overwritten, or
		/// <lang cref="null"/> if there was no previous value.</returns>
		public byte[] GetPreviousGameSpecifcValueData(string p_strValueKey)
		{
			XmlNode xndModList = gameSpecificValueEditsNode.SelectSingleNode("edit[@key=\"" + p_strValueKey.ToLowerInvariant() + "\"]/installingMods");
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

		#endregion

		#region Game-Specific Value Logging

		/// <summary>
		/// Creates a node representing that the specified mod made the specified edit to a game-specific value.
		/// </summary>
		/// <param name="p_strModKey">The key of the mod that made the edit.</param>
		/// <param name="p_strValueKey">The key of the game-specific value that was edited.</param>
		/// <param name="p_bteData">The data to which to the value was set.</param>
		/// <param name="p_xndModList">An out pramater returning the node containing the list of mods that
		/// have edited the specified game-specific value. This is useful for inserting the created node.</param>
		/// <returns>A node representing the specified mod that made the specified game-specific edit. The out
		/// parameter <paramref name="p_xndModList"/> returns the node containing the list of mods that
		/// have edited the specified game-specific value.</returns>
		protected XmlNode CreateGameSpecificValueEditNode(string p_strModKey, string p_strValueKey, byte[] p_bteData, out XmlNode p_xndModList)
		{
			string strLoweredValueKey = p_strValueKey.ToLowerInvariant();
			XmlNode xndInstallingMod = null;
			lock (gameSpecificValueEditsNode)
			{
				XmlNode xndGameSpecificValueEdit = gameSpecificValueEditsNode.SelectSingleNode("edit[@key=\"" + strLoweredValueKey + "\"]");
				if (xndGameSpecificValueEdit == null)
				{
					xndGameSpecificValueEdit = gameSpecificValueEditsNode.AppendChild(xmlDoc.CreateElement("edit"));
					xndGameSpecificValueEdit.Attributes.Append(xmlDoc.CreateAttribute("key"));
					xndGameSpecificValueEdit.Attributes[0].Value = strLoweredValueKey;
					p_xndModList = xndGameSpecificValueEdit.AppendChild(xmlDoc.CreateElement("installingMods"));
				}
				else
				{
					p_xndModList = xndGameSpecificValueEdit.SelectSingleNode("installingMods");
					xndInstallingMod = p_xndModList.SelectSingleNode("mod[@key=\"" + p_strModKey + "\"]");
					if (xndInstallingMod != null)
						p_xndModList.RemoveChild(xndInstallingMod);
				}
				if (xndInstallingMod == null)
				{
					xndInstallingMod = xmlDoc.CreateElement("mod");
					xndInstallingMod.Attributes.Append(xmlDoc.CreateAttribute("key"));
					xndInstallingMod.Attributes["key"].InnerText = p_strModKey;
				}
				StringBuilder stbData = new StringBuilder(p_bteData.Length * 2);
				for (int i = 0; i < p_bteData.Length; i++)
					stbData.Append(p_bteData[i].ToString("x2"));
				xndInstallingMod.InnerText = stbData.ToString();
			}
			return xndInstallingMod;
		}

		/// <summary>
		/// Adds a node representing that the specified mod made the specified edit to a game-specific value.
		/// </summary>
		/// <remarks>
		/// This method appends the node to the end of the list of installing mods, indicating
		/// that the specified mod is the latest mod to edit the specified game-specific value.
		/// </remarks>
		/// <param name="p_strModName">The base name of the mod that made the edit.</param>
		/// <param name="p_strValueKey">The key of the game-specific value that was edited.</param>
		/// <param name="p_bteData">The data to which to the value was set.</param>
		internal protected void AddGameSpecificValueEdit(string p_strModName, string p_strValueKey, byte[] p_bteData)
		{
			XmlNode xndModList = null;
			XmlNode xndInstallingMod = CreateGameSpecificValueEditNode(GetModKey(p_strModName), p_strValueKey, p_bteData, out xndModList);
			lock (gameSpecificValueEditsNode)
			{
				xndModList.AppendChild(xndInstallingMod);
			}
		}

		/// <summary>
		/// Replaces a node representing that the specified mod made the specified edit to a game-specific value.
		/// </summary>
		/// <remarks>
		/// If the specified mod already edited the specified game-specific value, the value of the edit is updated,
		/// but the install order is not changed. Otherwise, this method appends the node to the end of the
		/// list of installing mods, indicating that the specified mod is the latest mod to edit the
		/// specified game-specific value.
		/// </remarks>
		/// <param name="p_strModName">The base name of the mod that made the edit.</param>
		/// <param name="p_strValueKey">The key of the game-specific value that was edited.</param>
		/// <param name="p_bteData">The data to which to the value was set.</param>
		/// <seealso cref="AddGameSpecificValueEdit(string p_strModName, string p_strValueKey, byte[] p_bteData)"/>
		internal protected void ReplaceGameSpecificValueEdit(string p_strModName, string p_strValueKey, byte[] p_bteData)
		{
			string strLoweredValueKey = p_strValueKey.ToLowerInvariant();
			XmlNode xndInstallingMod = gameSpecificValueEditsNode.SelectSingleNode("edit[@key=\"" + strLoweredValueKey + "\"]/installingMods/mod[@key=\"" + GetModKey(p_strModName) + "\"]");
			if (xndInstallingMod != null)
			{
				StringBuilder stbData = new StringBuilder(p_bteData.Length * 2);
				for (int i = 0; i < p_bteData.Length; i++)
					stbData.Append(p_bteData[i].ToString("x2"));
				xndInstallingMod.InnerText = stbData.ToString();
			}
			else
				AddGameSpecificValueEdit(p_strModName, p_strValueKey, p_bteData);
		}

		/// <summary>
		/// Adds a node representing that the specified mod made the specified edit to a game-specific value.
		/// </summary>
		/// <remarks>
		/// This method prepends the node to the beginning of the list of installing mods, but
		/// after the ORIGINAL_VALUES node if it exists, indicating that the specified mod is not
		/// the latest mod to edit the specified the game-specific value..
		/// </remarks>
		/// <param name="p_strModName">The base name of the mod that made the edit.</param>
		/// <param name="p_strValueKey">The key of the game-specific value that was edited.</param>
		/// <param name="p_bteData">The data to which to the value was set.</param>
		internal protected void PrependAfterOriginalGameSpecificValueEdit(string p_strModName, string p_strValueKey, byte[] p_bteData)
		{
			XmlNode xndModList = null;
			XmlNode xndInstallingMod = CreateGameSpecificValueEditNode(GetModKey(p_strModName), p_strValueKey, p_bteData, out xndModList);
			if ((xndModList.FirstChild != null) && (xndModList.FirstChild.Attributes["key"].InnerText.Equals(OriginalValuesKey)))
				xndModList.InsertAfter(xndInstallingMod, xndModList.FirstChild);
			else
				xndModList.PrependChild(xndInstallingMod);
		}

		/// <summary>
		/// Removes the node representing that the specified mod edited the specified game-specific value.
		/// </summary>
		/// <param name="p_strModName">The base name of the mod that edited the shader.</param>
		/// <param name="p_strValueKey">The key of the game-specific value that was edited.</param>
		protected void RemoveGameSpecificValueEdit(string p_strModName, string p_strValueKey)
		{
			string strLoweredValueKey = p_strValueKey.ToLowerInvariant();
			lock (gameSpecificValueEditsNode)
			{
				XmlNode xndInstallingMod = gameSpecificValueEditsNode.SelectSingleNode("edit[@key=\"" + strLoweredValueKey + "\"]/installingMods/mod[@key=\"" + GetModKey(p_strModName) + "\"]");
				if (xndInstallingMod != null)
				{
					XmlNode xndInstallingMods = xndInstallingMod.ParentNode;
					XmlNode xndGameSpecificValueEdit = xndInstallingMods.ParentNode;
					xndInstallingMods.RemoveChild(xndInstallingMod);
					if ((xndInstallingMods.ChildNodes.Count == 0) || (xndInstallingMods.LastChild.Attributes["key"].InnerText.Equals(GetModKey(ORIGINAL_VALUES))))
						xndGameSpecificValueEdit.ParentNode.RemoveChild(xndGameSpecificValueEdit);
				}
			}
		}

		#endregion

		#region Merge Module Management

		/// <summary>
		/// Merges the installed and edited components in the given <see cref="InstallLogMergeModule"/>
		/// into the install log for the specified mod.
		/// </summary>
		/// <param name="p_fomodMod">The <see cref="fomod"/> for which the
		/// installs and edits where made.</param>
		/// <param name="p_ilmMergeModule">The installs and edits that where made as part of the
		/// <see cref="fomod"/>'s installation.</param>
		public void Merge(fomod p_fomodMod, InstallLogMergeModule p_ilmMergeModule)
		{
			AddMod(p_fomodMod);
			processMergeModule(p_fomodMod.BaseName, p_ilmMergeModule);
		}

		/// <summary>
		/// Merges the installed and edited components in the given <see cref="InstallLogMergeModule"/>
		/// into the install log for the specified mod, as an in-place upgrade.
		/// </summary>
		/// <remarks>
		/// When a <see cref="InstallLogMergeModule"/> is merged as an in-place upgrade, any files/changes
		/// that exist in the install log for the given fomod are replaced where they are in the install
		/// order, rather than being made the file/change owner (unless they already where the file/change
		/// owner). Note, however, that if the merge module contains new files/changes that the previous fomod
		/// version did not contain the fomods will become the owner of the new files/changes.
		/// 
		/// Also, changes that are logged for the speicifed fomod that are not in the given
		/// <see cref="InstallLogMergeModule"/> are removed from the install log.
		/// </remarks>
		/// <param name="p_fomodMod">The <see cref="fomod"/> for which the
		/// installs and edits where made.</param>
		/// <param name="p_strOldBaseName">The old base name of the fomod that is being upgraded. This is the
		/// base name that will be replaced with the base name of the given fomod.</param>
		/// <param name="p_ilmMergeModule">The installs and edits that where made as part of the
		/// <see cref="fomod"/>'s upgrade.</param>
		public void MergeUpgrade(fomod p_fomodMod, string p_strOldBaseName, InstallLogMergeModule p_ilmMergeModule)
		{
			if (!UpdateMod(p_strOldBaseName, p_fomodMod))
				AddMod(p_fomodMod);

			//remove changes that were not made in the upgrade
			InstallLogMergeModule ilmPreviousChanges = GetMergeModule(p_fomodMod.BaseName);
			foreach (string strFile in ilmPreviousChanges.DataFiles)
				if (!p_ilmMergeModule.ContainsFile(strFile))
					RemoveDataFile(p_fomodMod.BaseName, strFile);
			foreach (InstallLogMergeModule.IniEdit iniEdit in ilmPreviousChanges.IniEdits)
				if (!p_ilmMergeModule.IniEdits.Contains(iniEdit))
					RemoveIniEdit(p_fomodMod.BaseName, iniEdit.File, iniEdit.Section, iniEdit.Key);
			foreach (InstallLogMergeModule.GameSpecificValueEdit gsvEdit in ilmPreviousChanges.GameSpecificValueEdits)
				if (!p_ilmMergeModule.GameSpecificValueEdits.Contains(gsvEdit))
					RemoveGameSpecificValueEdit(p_fomodMod.BaseName, gsvEdit.Key);

			//add/replace changes
			foreach (string strFile in p_ilmMergeModule.ReplacedOriginalDataFiles)
				AddDataFile(ORIGINAL_VALUES, strFile);
			foreach (string strFile in p_ilmMergeModule.DataFiles)
				ReplaceDataFile(p_fomodMod.BaseName, strFile);
			foreach (InstallLogMergeModule.IniEdit iniEdit in p_ilmMergeModule.ReplacedOriginalIniValues)
				AddIniEdit(iniEdit.File, iniEdit.Section, iniEdit.Key, ORIGINAL_VALUES, iniEdit.Value);
			foreach (InstallLogMergeModule.IniEdit iniEdit in p_ilmMergeModule.IniEdits)
				ReplaceIniEdit(iniEdit.File, iniEdit.Section, iniEdit.Key, p_fomodMod.BaseName, iniEdit.Value);
			foreach (InstallLogMergeModule.GameSpecificValueEdit gsvEdit in p_ilmMergeModule.ReplacedGameSpecificValueData)
				AddGameSpecificValueEdit(ORIGINAL_VALUES, gsvEdit.Key, gsvEdit.Data);
			foreach (InstallLogMergeModule.GameSpecificValueEdit gsvEdit in p_ilmMergeModule.GameSpecificValueEdits)
				ReplaceGameSpecificValueEdit(p_fomodMod.BaseName, gsvEdit.Key, gsvEdit.Data);

			Save();
		}

		/// <summary>
		/// Merges the installed and edited components in the given <see cref="InstallLogMergeModule"/>
		/// into the install log for the specified mod.
		/// </summary>
		/// <param name="p_strModName">The base name of the unversioned <see cref="fomod"/> for which the
		/// installs and edits where made.</param>
		/// <param name="p_ilmMergeModule">The installs and edits that where made as part of the
		/// <see cref="fomod"/>'s installation.</param>
		internal void UnversionedFomodMerge(string p_strModName, InstallLogMergeModule p_ilmMergeModule)
		{
			AddMod(p_strModName);
			processMergeModule(p_strModName, p_ilmMergeModule);
		}

		/// <summary>
		/// Merges the installed and edited components in the given <see cref="InstallLogMergeModule"/>
		/// into the install log for the specified mod.
		/// </summary>
		/// <param name="p_strModName">The base name of the unversioned <see cref="fomod"/> for which the
		/// installs and edits where made.</param>
		/// <param name="p_ilmMergeModule">The installs and edits that where made as part of the
		/// <see cref="fomod"/>'s installation.</param>
		private void processMergeModule(string p_strModName, InstallLogMergeModule p_ilmMergeModule)
		{
			foreach (string strFile in p_ilmMergeModule.ReplacedOriginalDataFiles)
				AddDataFile(ORIGINAL_VALUES, strFile);
			foreach (string strFile in p_ilmMergeModule.DataFiles)
				AddDataFile(p_strModName, strFile);
			foreach (InstallLogMergeModule.IniEdit iniEdit in p_ilmMergeModule.ReplacedOriginalIniValues)
				AddIniEdit(iniEdit.File, iniEdit.Section, iniEdit.Key, ORIGINAL_VALUES, iniEdit.Value);
			foreach (InstallLogMergeModule.IniEdit iniEdit in p_ilmMergeModule.IniEdits)
				AddIniEdit(iniEdit.File, iniEdit.Section, iniEdit.Key, p_strModName, iniEdit.Value);
			foreach (InstallLogMergeModule.GameSpecificValueEdit gsvEdit in p_ilmMergeModule.ReplacedGameSpecificValueData)
				AddGameSpecificValueEdit(ORIGINAL_VALUES, gsvEdit.Key, gsvEdit.Data);
			foreach (InstallLogMergeModule.GameSpecificValueEdit gsvEdit in p_ilmMergeModule.GameSpecificValueEdits)
				AddGameSpecificValueEdit(p_strModName, gsvEdit.Key, gsvEdit.Data);
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
				xnlComponentMods = dataFilesNode.SelectNodes("descendant::mod[@key=\"" + m_dicModList[p_strModName] + "\"]");
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
				xnlComponentMods = iniEditsNode.SelectNodes("descendant::mod[@key=\"" + m_dicModList[p_strModName] + "\"]");
				foreach (XmlNode xndIniEdit in xnlComponentMods)
				{
					xndInstallingMods = xndIniEdit.ParentNode;
					xndComponent = xndInstallingMods.ParentNode;
					xndInstallingMods.RemoveChild(xndIniEdit);
					if ((xndInstallingMods.ChildNodes.Count == 0) || (xndInstallingMods.LastChild.Attributes["key"].InnerText.Equals(GetModKey(ORIGINAL_VALUES))))
						xndComponent.ParentNode.RemoveChild(xndComponent);
				}
			}
			lock (gameSpecificValueEditsNode)
			{
				xnlComponentMods = gameSpecificValueEditsNode.SelectNodes("descendant::mod[@key=\"" + m_dicModList[p_strModName] + "\"]");
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
			XmlNodeList xnlComponentMods = dataFilesNode.SelectNodes("descendant::mod[@key=\"" + m_dicModList[p_strModName] + "\"]");
			foreach (XmlNode xndFile in xnlComponentMods)
			{
				xndComponent = xndFile.ParentNode.ParentNode;
				ilmMergeModule.AddFile(xndComponent.Attributes["path"].InnerText);
			}
			xnlComponentMods = iniEditsNode.SelectNodes("descendant::mod[@key=\"" + m_dicModList[p_strModName] + "\"]");
			foreach (XmlNode xndIniEdit in xnlComponentMods)
			{
				xndComponent = xndIniEdit.ParentNode.ParentNode;
				ilmMergeModule.AddIniEdit(xndComponent.Attributes["file"].InnerText,
											xndComponent.Attributes["section"].InnerText,
											xndComponent.Attributes["key"].InnerText,
											xndIniEdit.InnerText);
			}
			xnlComponentMods = gameSpecificValueEditsNode.SelectNodes("descendant::mod[@key=\"" + m_dicModList[p_strModName] + "\"]");
			foreach (XmlNode xndGameSpecificValueEdit in xnlComponentMods)
			{
				xndComponent = xndGameSpecificValueEdit.ParentNode.ParentNode;

				string strData = xndGameSpecificValueEdit.InnerText;
				byte[] bteData = new byte[strData.Length / 2];
				for (int i = 0; i < bteData.Length; i++)
					bteData[i] = byte.Parse("" + strData[i * 2] + strData[i * 2 + 1], System.Globalization.NumberStyles.AllowHexSpecifier);

				ilmMergeModule.AddGameSpecificValueEdit(xndComponent.Attributes["key"].InnerText, bteData);
			}

			return ilmMergeModule;
		}

		#endregion

	}
}
