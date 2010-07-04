using System;
using SevenZip;
using System.Collections.Generic;
using System.IO;
using Fomm.Util;
using System.Text;

namespace Fomm.PackageManager
{
	/// <summary>
	/// Encapsulates the interactions with an archive file.
	/// </summary>
	public class Archive : IDisposable
	{
		/// <summary>
		/// The path prefix use to identify a file as being contained in an archive.
		/// </summary>
		public const string ARCHIVE_PREFIX = "arch:";

		private static Dictionary<string, Dictionary<string, ArchiveFileInfo>> m_dicFileInfoCache = new Dictionary<string, Dictionary<string, ArchiveFileInfo>>(StringComparer.InvariantCultureIgnoreCase);

		private string m_strPath = null;
		private SevenZipCompressor m_szcCompressor = null;
		private List<string> m_strFiles = null;
		private Dictionary<string, ArchiveFileInfo> m_dicFileInfo = null;
		private bool m_booCanEdit = false;

		#region Properties

		/// <summary>
		/// Gets the path of the archive.
		/// </summary>
		/// <value>The path of the archive.</value>
		public string ArchivePath
		{
			get
			{
				return m_strPath;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor the initializes the object with the given values.
		/// </summary>
		/// <param name="p_strPath">The path to the archive file.</param>
		public Archive(string p_strPath)
		{
			m_strPath = p_strPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			using (SevenZipExtractor szeExtractor = new SevenZipExtractor(m_strPath))
			{
				if (Enum.IsDefined(typeof(OutArchiveFormat), szeExtractor.Format.ToString()))
				{
					m_szcCompressor = new SevenZipCompressor();
					m_szcCompressor.CompressionMode = CompressionMode.Append;
					m_szcCompressor.ArchiveFormat = (OutArchiveFormat)Enum.Parse(typeof(OutArchiveFormat), szeExtractor.Format.ToString());
					m_booCanEdit = true;
				}
			}
			lock (m_dicFileInfoCache)
			{
				if (m_dicFileInfoCache.ContainsKey(m_strPath))
				{
					m_dicFileInfo = m_dicFileInfoCache[m_strPath];
					m_strFiles = new List<string>(m_dicFileInfo.Keys);
				}
				else
				{
					m_dicFileInfo = new Dictionary<string, ArchiveFileInfo>(StringComparer.InvariantCultureIgnoreCase);
					m_strFiles = new List<string>();
					LoadFileIndices();
					m_dicFileInfoCache[m_strPath] = m_dicFileInfo;
				}
			}
		}

		#endregion

		/// <summary>
		/// Caches information about the files in the archive.
		/// </summary>
		protected void LoadFileIndices()
		{
			lock (m_dicFileInfoCache)
			{
				m_dicFileInfo.Clear();
				m_strFiles.Clear();
				using (SevenZipExtractor szeExtractor = new SevenZipExtractor(m_strPath))
				{
					foreach (ArchiveFileInfo afiFile in szeExtractor.ArchiveFileData)
						if (!afiFile.IsDirectory)
						{
							m_dicFileInfo[afiFile.FileName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)] = afiFile;
							m_strFiles.Add(afiFile.FileName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
						}
				}
			}
		}

		/// <summary>
		/// Parses the given path to extract the path to the archive file, and the path to
		/// a file within said archive.
		/// </summary>
		/// <param name="p_strPath">The file path to parse.</param>
		/// <returns>The path to an archive file, and the path to a file within said archive.</returns>
		public static KeyValuePair<string, string> ParseArchivePath(string p_strPath)
		{
			if (!p_strPath.StartsWith(ARCHIVE_PREFIX))
				return new KeyValuePair<string, string>(null, null);
			Int32 intEndIndex = p_strPath.IndexOf("//", ARCHIVE_PREFIX.Length);
			if (intEndIndex < 0)
				intEndIndex = p_strPath.Length;
			string strArchive = p_strPath.Substring(ARCHIVE_PREFIX.Length, intEndIndex - ARCHIVE_PREFIX.Length);
			string strPath = p_strPath.Substring(intEndIndex + 2);
			return new KeyValuePair<string, string>(strArchive, strPath);
		}

		/// <summary>
		/// Generates a path to a file in an archive.
		/// </summary>
		/// <param name="p_strArchivePath">The path of the archive file.</param>
		/// <param name="p_strInternalPath">The path of the file in the archive.</param>
		/// <returns></returns>
		public static string GenerateArchivePath(string p_strArchivePath, string p_strInternalPath)
		{
			return String.Format("{0}{1}//{2}", Archive.ARCHIVE_PREFIX, p_strArchivePath, p_strInternalPath);
		}

		/// <summary>
		/// Determins if the given path is a directory in this archive.
		/// </summary>
		/// <param name="p_strPath">The path to examine.</param>
		/// <returns><lang cref="true"/> if the given path is a directory in this archive;
		/// <lang cref="false"/> otherwise.</returns>
		public bool IsDirectory(string p_strPath)
		{
			string strPath = p_strPath.Trim(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
			strPath = strPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			string strPathWithSep = strPath + Path.DirectorySeparatorChar;

			if (m_dicFileInfo.ContainsKey(strPath))
				return false;

			foreach (string strFile in m_dicFileInfo.Keys)
				if (strFile.StartsWith(strPathWithSep, StringComparison.InvariantCultureIgnoreCase))
					return true;

			ArchiveFileInfo afiFile = default(ArchiveFileInfo);
			string strArchiveFileName = null;
			using (SevenZipExtractor szeExtractor = new SevenZipExtractor(m_strPath))
				foreach (ArchiveFileInfo afiTmp in szeExtractor.ArchiveFileData)
				{
					strArchiveFileName = afiTmp.FileName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
					if (strArchiveFileName.Equals(strPath, StringComparison.InvariantCultureIgnoreCase))
					{
						afiFile = afiTmp;
						break;
					}
				}
			return (afiFile == null) ? false : afiFile.IsDirectory;
		}

		/// <summary>
		/// Gets a list of directories that are in the specified directory in this archive.
		/// </summary>
		/// <param name="p_strDirectory">The directory in the archive whose descendents are to be returned.</param>
		/// <returns>A list of directories that are in the specified directory in this archive.</returns>
		public string[] GetDirectories(string p_strDirectory)
		{
			string strPrefix = p_strDirectory;
			if (String.IsNullOrEmpty(p_strDirectory))
				strPrefix = "";
			strPrefix = strPrefix.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			strPrefix = strPrefix.Trim(new char[] { Path.DirectorySeparatorChar });
			if (strPrefix.Length > 0)
				strPrefix += Path.DirectorySeparatorChar;
			Set<string> lstFolders = new Set<string>();
			Int32 intStopIndex = 0;
			foreach (string strFile in m_strFiles)
			{
				if (strFile.StartsWith(strPrefix, StringComparison.InvariantCultureIgnoreCase))
				{
					intStopIndex = strFile.IndexOf(Path.DirectorySeparatorChar, strPrefix.Length);
					if (intStopIndex < 0)
						continue;
					lstFolders.Add(strFile.Substring(0, intStopIndex));
				}
			}
			return lstFolders.ToArray();
		}

		/// <summary>
		/// Gets a list of files that are in the specified directory in this archive.
		/// </summary>
		/// <param name="p_strDirectory">The directory in the archive whose descendents are to be returned.</param>
		/// <returns>A list of files that are in the specified directory in this archive.</returns>
		public string[] GetFiles(string p_strDirectory)
		{
			if (String.IsNullOrEmpty(p_strDirectory))
				return m_strFiles.ToArray();
			string strPrefix = p_strDirectory;
			strPrefix = strPrefix.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			strPrefix = strPrefix.Trim(new char[] { Path.DirectorySeparatorChar });
			if (strPrefix.Length > 0)
				strPrefix += Path.DirectorySeparatorChar;
			Set<string> lstFiles = new Set<string>();
			Int32 intStopIndex = 0;
			foreach (string strFile in m_strFiles)
			{
				if (strFile.StartsWith(strPrefix, StringComparison.InvariantCultureIgnoreCase))
				{
					intStopIndex = strFile.IndexOf(Path.DirectorySeparatorChar, strPrefix.Length);
					if (intStopIndex > 0)
						continue;
					lstFiles.Add(strFile);
				}
			}
			return lstFiles.ToArray();
		}

		/// <summary>
		/// Determins if the archive contains the specified file.
		/// </summary>
		/// <param name="p_strPath">The path of the file whose presence in the archive is to be determined.</param>
		/// <returns><lang cref="true"/> if the file is in the archive;
		/// <lang cref="false"/> otherwise.</returns>
		public bool ContainsFile(string p_strPath)
		{
			string strPath = p_strPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).ToLowerInvariant();
			return m_dicFileInfo.ContainsKey(strPath);
		}

		/// <summary>
		/// Gets the contents of the specified file in the archive.
		/// </summary>
		/// <param name="p_strPath">The file whose contents are to be retrieved.</param>
		/// <returns>The contents of the specified file in the archive.</returns>
		public byte[] GetFileContents(string p_strPath)
		{
			string strPath = p_strPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).ToLowerInvariant();
			if (!m_dicFileInfo.ContainsKey(strPath))
				throw new FileNotFoundException("The requested file does not exist in the archive.", p_strPath);

			byte[] bteFile = null;
			using (SevenZipExtractor szeExtractor = new SevenZipExtractor(m_strPath))
			{
				ArchiveFileInfo afiFile = m_dicFileInfo[strPath];
				bteFile = new byte[afiFile.Size];
				using (MemoryStream msmFile = new MemoryStream())
				{
					szeExtractor.ExtractFile(afiFile.Index, msmFile);
					msmFile.Position = 0;
					for (Int32 intOffset = 0, intRead = 0; intOffset < bteFile.Length && ((intRead = msmFile.Read(bteFile, intOffset, bteFile.Length - intOffset)) >= 0); intOffset += intRead) ;
					msmFile.Close();
				}
			}
			return bteFile;
		}

		/// <summary>
		/// Replaces the specified file in the archive with the given data.
		/// </summary>
		/// <remarks>
		/// If the specified file doesn't exist in the archive, the file is added.
		/// </remarks>
		/// <param name="p_strFileName">The path to the file to replace in the archive.</param>
		/// <param name="p_strData">The new file data.</param>
		public void ReplaceFile(string p_strFileName, string p_strData)
		{
			ReplaceFile(p_strFileName, Encoding.Default.GetBytes(p_strData));
		}

		/// <summary>
		/// Replaces the specified file in the archive with the given data.
		/// </summary>
		/// <remarks>
		/// If the specified file doesn't exist in the archive, the file is added.
		/// </remarks>
		/// <param name="p_strFileName">The path to the file to replace in the archive.</param>
		/// <param name="p_bteData">The new file data.</param>
		/// <exception cref="InvalidOperationException">Thrown if modification of archives of the current
		/// archive type is not supported.</exception>
		public void ReplaceFile(string p_strFileName, byte[] p_bteData)
		{
			if (!m_booCanEdit)
				using (SevenZipExtractor szeExtractor = new SevenZipExtractor(m_strPath))
					throw new InvalidOperationException("Cannot modify archive of type: " + szeExtractor.Format);
			string strPath = p_strFileName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).ToLowerInvariant();
			if (m_dicFileInfo.ContainsKey(strPath))
			{
				Dictionary<int, string> dicDelete = new Dictionary<int, string>() { { m_dicFileInfo[strPath].Index, null } };
				m_szcCompressor.ModifyArchive(m_strPath, dicDelete);
			}
			using (MemoryStream msmData = new MemoryStream(p_bteData))
			{
				m_szcCompressor.CompressStreamDictionary(new Dictionary<string, Stream>() { { p_strFileName, msmData } }, m_strPath);
				msmData.Close();
			}
			LoadFileIndices();
		}

		/// <summary>
		/// Deletes the specified file from the archive.
		/// </summary>
		/// <remarks>
		/// If the specified file doesn't exist in the archive, nothing is done.
		/// </remarks>
		/// <param name="p_strFileName">The path to the file to delete from the archive.</param>
		/// <exception cref="InvalidOperationException">Thrown if modification of archives of the current
		/// archive type is not supported.</exception>
		public void DeleteFile(string p_strFileName)
		{
			if (!m_booCanEdit)
				using (SevenZipExtractor szeExtractor = new SevenZipExtractor(m_strPath))
					throw new InvalidOperationException("Cannot modify archive of type: " + szeExtractor.Format);
			string strPath = p_strFileName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).ToLowerInvariant();
			if (m_dicFileInfo.ContainsKey(strPath))
			{
				Dictionary<int, string> dicDelete = new Dictionary<int, string>() { { m_dicFileInfo[strPath].Index, null } };
				m_szcCompressor.ModifyArchive(m_strPath, dicDelete);
			}
			LoadFileIndices();
		}

		#region IDisposable Members

		/// <summary>
		/// Disposes of the resources used by the object.
		/// </summary>
		public void Dispose()
		{
		}

		#endregion
	}
}
