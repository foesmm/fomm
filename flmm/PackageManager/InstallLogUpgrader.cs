using System;
using System.IO;
using System.Xml;
using ICSharpCode.SharpZipLib.Checksums;
using ChinhDo.Transactions;
using System.Collections.Generic;
using System.Windows.Forms;
using fomm.Transactions;
using System.ComponentModel;

namespace Fomm.PackageManager
{
	/// <summary>
	/// Upgrades the install log.
	/// </summary>
	class InstallLogUpgrader : InstallLog
	{
		private static object m_objLock = new object();
		private TxFileManager m_tfmFileManager = null;
		private XmlDocument m_xmlOldInstallLog = null;
		private Dictionary<string, string> m_dicDefaultFileOwners = null;
		private BackgroundWorkerProgressDialog m_pgdProgress = null;

		/// <summary>
		/// The default constructor.
		/// </summary>
		internal InstallLogUpgrader()
			: base()
		{
		}

		/// <summary>
		/// Upgrades the install log.
		/// </summary>
		/// <remarks>
		/// This creates a <see cref="BackgroundWorkerProgressDialog"/> to do the work
		/// and display progress.
		/// </remarks>
		/// <returns><lang cref="false"/> if the user cancelled the upgrade; <lang cref="true"/> otherwise.</returns>
		public bool UpgradeInstallLog()
		{
			//this is to handle the few people who already installed a version that used
			// the new-style install log, but before it had a version
			if (Document.SelectNodes("descendant::installingMods").Count > 0)
			{
				SetInstallLogVersion(new Version("0.1.0.0"));
				Save();
				return true;
			}

			//we only want one upgrade at a time happening to minimize the chances of
			// messed up install logs.
			lock (m_objLock)
			{
				EnableLogFileRefresh = false;

				m_pgdProgress = new BackgroundWorkerProgressDialog(PerformUpgrade);
				m_pgdProgress.OverallMessage = "Upgrading Files";
				m_pgdProgress.ItemProgressStep = 1;
				m_pgdProgress.OverallProgressStep = 1;
				if (m_pgdProgress.ShowDialog() == DialogResult.Cancel)
					return false;
			}
			return true;
		}

		/// <summary>
		/// This method is called by a background worker to perform the actual upgrade.
		/// </summary>
		protected void PerformUpgrade()
		{			
			using (TransactionScope tsTransaction = new TransactionScope())
			{
				string[] strModInstallFiles = Directory.GetFiles(Program.PackageDir, "*.XMl", SearchOption.TopDirectoryOnly);
				m_pgdProgress.OverallProgressMaximum = strModInstallFiles.Length;
				
				m_dicDefaultFileOwners = new Dictionary<string, string>();
				XmlDocument xmlModInstallLog = null;
				string strModBaseName = null;
												
				m_tfmFileManager = new TxFileManager();
				m_tfmFileManager.Snapshot(InstallLogPath);
				m_xmlOldInstallLog = new XmlDocument();
				m_xmlOldInstallLog.Load(InstallLogPath);
				Reset();

				foreach (string strModInstallLog in strModInstallFiles)
				{
					if (m_pgdProgress.Cancelled())
						return;

					strModBaseName = Path.GetFileNameWithoutExtension(strModInstallLog);
					xmlModInstallLog = new XmlDocument();
					xmlModInstallLog.Load(strModInstallLog);

					//figure out how much work we need to do for this mod
					XmlNodeList xnlFiles = xmlModInstallLog.SelectNodes("descendant::installedFiles/*");
					XmlNodeList xnlIniEdits = xmlModInstallLog.SelectNodes("descendant::iniEdits/*");
					XmlNodeList xnlSdpEdits = xmlModInstallLog.SelectNodes("descendant::sdpEdits/*");
					Int32 intItemCount = xnlFiles.Count + xnlIniEdits.Count + xnlSdpEdits.Count;
					m_pgdProgress.ItemMessage = strModBaseName;
					m_pgdProgress.ItemProgress = 0;
					m_pgdProgress.ItemProgressMaximum = intItemCount;

					UpgradeInstalledFiles(xmlModInstallLog, strModInstallLog, strModBaseName);
					//we now have to tell all the remaining default owners that are are indeed
					// the owners
					foreach (KeyValuePair<string, string> kvpOwner in m_dicDefaultFileOwners)
						MakeOverwrittenModOwner(kvpOwner.Value, kvpOwner.Key);

					if (m_pgdProgress.Cancelled())
						return;

					UpgradeIniEdits(xmlModInstallLog, strModBaseName);

					if (m_pgdProgress.Cancelled())
						return;

					UpgradeSdpEdits(xmlModInstallLog, strModBaseName);

					if (m_pgdProgress.Cancelled())
						return;

					if (File.Exists(strModInstallLog + ".bak"))
						m_tfmFileManager.Delete(strModInstallLog + ".bak");
					m_tfmFileManager.Move(strModInstallLog, strModInstallLog + ".bak");

					m_pgdProgress.StepOverallProgress();
				}
				SetInstallLogVersion(new Version("0.1.0.0"));
				Save();
				tsTransaction.Complete();
				m_tfmFileManager = null;
			}
		}

		#region Sdp Edits Upgrade

		private byte[] GetOldSdpValue(Int32 p_intPackage, string p_strShader)
		{
			XmlNode node = m_xmlOldInstallLog.SelectSingleNode("descendant::sdp[@package='" + p_intPackage + "' and @shader='" + p_strShader + "']");
			if (node == null)
				return null;
			byte[] b = new byte[node.InnerText.Length / 2];
			for (int i = 0; i < b.Length; i++)
			{
				b[i] = byte.Parse("" + node.InnerText[i * 2] + node.InnerText[i * 2 + 1], System.Globalization.NumberStyles.AllowHexSpecifier);
			}
			return b;
		}

		private List<string> m_lstSeenShader = new List<string>();
		/// <summary>
		/// Upgrades the sdp edits log entries.
		/// </summary>
		/// <remarks>
		/// This analyses the mods and determines, as best as possible, who edited which shaders, and attempts
		/// to reconstruct the install order. The resulting information is then put in the new install log.
		/// </remarks>
		/// <param name="p_xmlModInstallLog">The current mod install log we are parsing to upgrade.</param>
		/// <param name="p_strModBaseName">The base name of the mod whose install log is being parsed.</param>
		private void UpgradeSdpEdits(XmlDocument p_xmlModInstallLog, string p_strModBaseName)
		{
			XmlNodeList xnlSdpEdits = p_xmlModInstallLog.SelectNodes("descendant::sdpEdits/*");
			foreach (XmlNode xndSdpEdit in xnlSdpEdits)
			{
				Int32 intPackage = Int32.Parse(xndSdpEdit.Attributes.GetNamedItem("package").Value);
				string strShader = xndSdpEdit.Attributes.GetNamedItem("shader").Value;
				byte[] bteOldValue = GetOldSdpValue(intPackage, strShader);
				//we have no way of knowing who last edited the shader - that information
				// was not tracked
				// so, let's just do first come first serve 
				if (!m_lstSeenShader.Contains(intPackage + "~" + strShader.ToLowerInvariant()))
				{
					//this is the first mod we have encountered that edited this shader,
					// so let's assume it is the lastest mod to have made the edit...
					AddShaderEdit(p_strModBaseName, intPackage, strShader, SDPArchives.GetShader(intPackage, strShader));
					//...and backup the old value as the original value
					PrependAfterOriginalShaderEdit(ORIGINAL_VALUES, intPackage, strShader, bteOldValue);
					m_lstSeenShader.Add(intPackage + "~" + strShader.ToLowerInvariant());
				}
				else
				{
					//someone else made the shader edit
					// we don't know what value was overwritten, so we will just use what we have
					// which is the old value
					PrependAfterOriginalShaderEdit(p_strModBaseName, intPackage, strShader, bteOldValue);
				}

				if (m_pgdProgress.Cancelled())
					return;
				m_pgdProgress.StepItemProgress();
			}
		}

		/// <summary>
		/// Adds a node representing that the specified mod made the specified sdp edit.
		/// </summary>
		/// <remarks>
		/// This method prepends the node to the beginning of the list of installing mods, but
		/// after the ORIGINAL_VALUES node if it exists, indicating that the specified mod is not
		/// the latest mod to edit the specified shader.
		/// </remarks>
		/// <param name="p_strModName">The base name of the mod that made the edit.</param>
		/// <param name="p_intPackage">The package containing the shader that was edited.</param>
		/// <param name="p_strShaderName">The shader that was edited.</param>
		/// <param name="p_bteData">The value to which to the shader was set.</param>
		protected void PrependAfterOriginalShaderEdit(string p_strModName, int p_intPackage, string p_strShader, byte[] p_bteData)
		{
			XmlNode xndModList = null;
			XmlNode xndInstallingMod = CreateSdpEditNode(GetModKey(p_strModName), p_intPackage, p_strShader, p_bteData, out xndModList);
			if ((xndModList.FirstChild != null) && (xndModList.FirstChild.Attributes["key"].InnerText.Equals(OriginalValuesKey)))
				xndModList.InsertAfter(xndInstallingMod, xndModList.FirstChild);
			else
				xndModList.PrependChild(xndInstallingMod);
		}

		#endregion

		#region Ini Edits Upgrade

		private string GetOldIniValue(string p_strFile, string p_strSection, string p_strKey, out string p_strModName)
		{
			p_strModName = null;
			XmlNode node = m_xmlOldInstallLog.SelectSingleNode("descendant::ini[@file='" + p_strFile + "' and @section='" + p_strSection + "' and @key='" + p_strKey + "']");
			if (node == null)
				return null;
			XmlNode modnode = node.Attributes.GetNamedItem("mod");
			if (modnode != null)
				p_strModName = modnode.Value;
			return node.InnerText;
		}

		/// <summary>
		/// Upgrades the ini edits log entries.
		/// </summary>
		/// <remarks>
		/// This analyses the mods and determines, as best as possible, who edited which keys, and attempts
		/// to reconstruct the install order. The resulting information is then put in the new install log.
		/// </remarks>
		/// <param name="p_xmlModInstallLog">The current mod install log we are parsing to upgrade.</param>
		/// <param name="p_strModBaseName">The base name of the mod whose install log is being parsed.</param>
		private void UpgradeIniEdits(XmlDocument p_xmlModInstallLog, string p_strModBaseName)
		{
			XmlNodeList xnlIniEdits = p_xmlModInstallLog.SelectNodes("descendant::iniEdits/*");
			foreach (XmlNode xndIniEdit in xnlIniEdits)
			{
				string strFile = xndIniEdit.Attributes.GetNamedItem("file").Value;
				string strSection = xndIniEdit.Attributes.GetNamedItem("section").Value;
				string strKey = xndIniEdit.Attributes.GetNamedItem("key").Value;
				string strOldIniEditor = null;
				string strOldValue = GetOldIniValue(strFile, strSection, strKey, out strOldIniEditor);
				if (p_strModBaseName.Equals(strOldIniEditor))
				{
					//this mod owns the ini edit, so append it to the list of editing mods...
					AddIniEdit(strFile, strSection, strKey, p_strModBaseName, NativeMethods.GetPrivateProfileString(strSection, strKey, "", strFile));
					//...and backup the old value as the original value
					PrependAfterOriginalIniEdit(strFile, strSection, strKey, ORIGINAL_VALUES, strOldValue);
				}
				else
				{
					//someone else made the ini edit
					// we don't know what value was overwritten, so we will just use what we have
					// which is the old value stored in the old install log
					PrependAfterOriginalIniEdit(strFile, strSection, strKey, p_strModBaseName, strOldValue);
				}

				if (m_pgdProgress.Cancelled())
					return;
				m_pgdProgress.StepItemProgress();
			}
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
		protected void PrependAfterOriginalIniEdit(string p_strFile, string p_strSection, string p_strKey, string p_strModName, string p_strValue)
		{
			XmlNode xndModList = null;
			XmlNode xndInstallingMod = CreateIniEditNode(GetModKey(p_strModName), p_strFile, p_strSection, p_strKey, p_strValue, out xndModList);
			if ((xndModList.FirstChild != null) && (xndModList.FirstChild.Attributes["key"].InnerText.Equals(OriginalValuesKey)))
				xndModList.InsertAfter(xndInstallingMod, xndModList.FirstChild);
			else
				xndModList.PrependChild(xndInstallingMod);
		}

		#endregion

		#region Installed Files Upgrade

		/// <summary>
		/// Makes the specified mod the owner of the specified file.
		/// </summary>
		/// <remarks>
		/// Moves the node representing that the specified mod installed the specified file to the end,
		/// indicating in was the last mod to install the file. It also deletes the mod's backup of the file
		/// from the overwrites folder.
		/// </remarks>
		/// <param name="p_strModName">The base name of the mod that is being made the file owner.</param>
		/// <param name="p_strDataRealtivePath">The path of the file whose owner is changing..</param>
		private void MakeOverwrittenModOwner(string p_strMadBaseName, string p_strDataRealtivePath)
		{
			string strModKey = GetModKey(p_strMadBaseName);
			string strDirectory = Path.GetDirectoryName(p_strDataRealtivePath);
			string strBackupPath = Path.GetFullPath(Path.Combine(Program.overwriteDir, strDirectory));
			strBackupPath = Path.Combine(strBackupPath, strModKey + "_" + Path.GetFileName(p_strDataRealtivePath));
			m_tfmFileManager.Delete(strBackupPath);
			RemoveDataFile(p_strMadBaseName, p_strDataRealtivePath);
			AddDataFile(p_strMadBaseName, p_strDataRealtivePath);
		}

		private void PrependModWithMissingFomodFile(string p_strDataRelativePath, string p_strModBaseName)
		{
			//another mod owns the file
			//put this mod's file into the overwrites directory.
			// we can't get the original file from the fomod,
			// so we'll use the existing file instead. this isn't
			// strictly correct, but it is inline with the behaviour
			// of the fomm version we are upgrading from
			string strDirectory = Path.GetDirectoryName(p_strDataRelativePath);
			string strBackupPath = Path.GetFullPath(Path.Combine(Program.overwriteDir, strDirectory));
			string strModKey = GetModKey(p_strModBaseName);
			if (!Directory.Exists(strBackupPath))
				m_tfmFileManager.CreateDirectory(strBackupPath);
			strBackupPath = Path.Combine(strBackupPath, strModKey + "_" + Path.GetFileName(p_strDataRelativePath));
			m_tfmFileManager.Copy(Path.Combine(Path.GetFullPath("data"), p_strDataRelativePath), strBackupPath, true);
			PrependDataFile(p_strModBaseName, p_strDataRelativePath);
		}

		/// <summary>
		/// Determines if we already know who owns the specified file.
		/// </summary>
		/// <remarks>
		/// We know who owns a specified file if the file has at least one installing mod, and
		/// the last installing mod doesn't have a corresponding file in the overwrites folder.
		/// </remarks>
		/// <param name="p_strDataRelativePath">The file for which it is to be determined if the owner is known.</param>
		/// <returns><lang cref="true"/> if the owner is known; <lang cref="false"/> otherwise.</returns>
		private bool FileOwnerIsKnown(string p_strDataRelativePath)
		{
			string strModKey = GetCurrentFileOwnerKey(p_strDataRelativePath);
			if (strModKey == null)
				return false;
			string strDirectory = Path.GetDirectoryName(p_strDataRelativePath);
			string strBackupPath = Path.GetFullPath(Path.Combine(Program.overwriteDir, strDirectory));
			strBackupPath = Path.Combine(strBackupPath, strModKey + "_" + Path.GetFileName(p_strDataRelativePath));
			return !m_tfmFileManager.FileExists(strBackupPath);
		}

		/// <summary>
		/// Upgrades the installed files log entries.
		/// </summary>
		/// <remarks>
		/// This analyses the mods and determines, as best as possible, who owns which files, and attempts
		/// to reconstruct the install order. It populates the overwrites folder with the files that, as far
		/// as can be determined, belong there. This resulting information is then put in the new install log.
		/// </remarks>
		/// <param name="p_xmlModInstallLog">The current mod install log we are parsing to upgrade.</param>
		/// <param name="p_strModInstallLogPath">The path to the current mod install log.</param>
		/// <param name="p_strModBaseName">The base name of the mod whose install log is being parsed.</param>
		private void UpgradeInstalledFiles(XmlDocument p_xmlModInstallLog, string p_strModInstallLogPath, string p_strModBaseName)
		{
			Int32 intDataPathStartPos = Path.GetFullPath("data").Length + 1;
			XmlNodeList xnlFiles = p_xmlModInstallLog.SelectNodes("descendant::installedFiles/*");
			foreach (XmlNode xndFile in xnlFiles)
			{
				AddMod(p_strModBaseName);
				string strFile = xndFile.InnerText;
				if (!File.Exists(strFile))
					continue;
				fomod fomodMod = new fomod(p_strModInstallLogPath.ToLowerInvariant().Replace(".xml", ".fomod"));
				string strDataRelativePath = strFile.Substring(intDataPathStartPos);

				Crc32 crcDiskFile = new Crc32();
				Crc32 crcFomodFile = new Crc32();
				crcDiskFile.Update(File.ReadAllBytes(strFile));
				if (!fomodMod.FileExists(strDataRelativePath))
				{
					//we don't know if this mod owns the file, so let's assume
					// it doesn't
					PrependModWithMissingFomodFile(strDataRelativePath, p_strModBaseName);

					//however, it may own the file, so let's make it the default owner for now
					// unless we already know who the owner is
					if (!FileOwnerIsKnown(strDataRelativePath))
						m_dicDefaultFileOwners[strDataRelativePath] = p_strModBaseName;
					continue;
				}
				byte[] bteFomodFile = fomodMod.GetFile(strDataRelativePath);
				crcFomodFile.Update(bteFomodFile);
				if (!crcDiskFile.Value.Equals(crcFomodFile.Value))
				{
					//another mod owns the file, so put this mod's file into
					// the overwrites directory
					string strDirectory = Path.GetDirectoryName(strDataRelativePath);
					string strBackupPath = Path.GetFullPath(Path.Combine(Program.overwriteDir, strDirectory));
					string strModKey = GetModKey(p_strModBaseName);
					if (!Directory.Exists(strBackupPath))
						m_tfmFileManager.CreateDirectory(strBackupPath);
					strBackupPath = Path.Combine(strBackupPath, strModKey + "_" + Path.GetFileName(strDataRelativePath));
					m_tfmFileManager.WriteAllBytes(strBackupPath, bteFomodFile);
					PrependDataFile(p_strModBaseName, strDataRelativePath);
				}
				else
				{
					//this mod owns the file, so append it to the list of installing mods
					AddDataFile(p_strModBaseName, strDataRelativePath);

					//we also have to displace the mod that is currently the default owner
					if (m_dicDefaultFileOwners.ContainsKey(strDataRelativePath))
						m_dicDefaultFileOwners.Remove(strDataRelativePath);
				}

				if (m_pgdProgress.Cancelled())
					return;
				m_pgdProgress.StepItemProgress();
			}
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
		protected void PrependDataFile(string p_strModName, string p_strPath)
		{
			XmlNode xndModList = null;
			XmlNode xndInstallingMod = CreateDataFileNode(GetModKey(p_strModName), p_strPath, out xndModList);
			xndModList.PrependChild(xndInstallingMod);
		}

		#endregion
	}
}
