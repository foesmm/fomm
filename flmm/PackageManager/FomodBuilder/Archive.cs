using System;
using SevenZip;
using System.Collections.Generic;
using System.IO;
using Fomm.Util;

namespace Fomm.PackageManager.FomodBuilder
{
	/// <summary>
	/// Encapsulates the interactions with an archive file.
	/// </summary>
	public class Archive
	{
		/// <summary>
		/// The path prefix use to identify a file as being contained in an archive.
		/// </summary>
		public const string ARCHIVE_PREFIX = "arch:";

		private string m_strPath = null;
		private SevenZipExtractor m_szeExtractor = null;
		private List<string> m_strFiles = new List<string>();
		private Dictionary<string, Int32> m_dicFileIndex = new Dictionary<string, int>();

		#region Constructors

		/// <summary>
		/// A simple constructor the initializes the object with the given values.
		/// </summary>
		/// <param name="p_strPath">The path to the archive file.</param>
		public Archive(string p_strPath)
		{
			m_strPath = p_strPath;
			m_szeExtractor = new SevenZipExtractor(m_strPath);
			foreach (ArchiveFileInfo afiFile in m_szeExtractor.ArchiveFileData)
				if (!afiFile.IsDirectory)
				{
					m_dicFileIndex[afiFile.FileName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)] = afiFile.Index;
					m_strFiles.Add(afiFile.FileName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
				}
		}

		#endregion

		/// <summary>
		/// Parses the given path to extract the path to the archive file, and the path to
		/// a file within said archive.
		/// </summary>
		/// <param name="p_strPath">The file path to parse.</param>
		/// <returns>The path to an archive file, and the path to a file within said archive.</returns>
		public static KeyValuePair<string, string> ParseArchive(string p_strPath)
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
		/// Determins if the given path is a directory in this archive.
		/// </summary>
		/// <param name="p_strPath">The path to examine.</param>
		/// <returns><lang cref="true"/> if the given path is a directory in this archive;
		/// <lang cref="false"/> otherwise.</returns>
		public bool IsDirectory(string p_strPath)
		{
			string strPathWithSep = p_strPath.Trim(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
			string strPathWithAltSep = strPathWithSep + Path.AltDirectorySeparatorChar;
			strPathWithSep += Path.DirectorySeparatorChar;

			ArchiveFileInfo afiFile = default(ArchiveFileInfo);
			foreach (ArchiveFileInfo afiTmp in m_szeExtractor.ArchiveFileData)
				if (afiTmp.FileName.Equals(p_strPath, StringComparison.InvariantCultureIgnoreCase))
				{
					afiFile = afiTmp;
					break;
				}
				else if (afiTmp.FileName.StartsWith(strPathWithSep, StringComparison.InvariantCultureIgnoreCase) || afiTmp.FileName.StartsWith(strPathWithAltSep, StringComparison.InvariantCultureIgnoreCase))
					return true;
			return (afiFile == null) ? false : afiFile.IsDirectory;
		}

		/// <summary>
		/// Gets a list of directories that are in the specified directory in this archive.
		/// </summary>
		/// <param name="p_strDirectory">The directory in the archive whose descendents are to be returned.</param>
		/// <returns>A list of directories that are in the specified directory in this archive.</returns>
		public string[] GetDirectories(string p_strDirectory)
		{
			if (String.IsNullOrEmpty(p_strDirectory))
				return m_strFiles.ToArray();
			string strPrefix = p_strDirectory;
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
		/// Gets the contents of the specified file in the archive.
		/// </summary>
		/// <param name="p_strPath">The file whose contents are to be retrieved.</param>
		/// <returns>The contents of the specified file in the archive.</returns>
		public byte[] GetFileContents(string p_strPath)
		{
			string strPath = p_strPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			if (!m_dicFileIndex.ContainsKey(strPath))
				throw new FileNotFoundException("The requested file does not exist in the archive.", p_strPath);
			
			ArchiveFileInfo afiFile = m_szeExtractor.ArchiveFileData[m_dicFileIndex[strPath]];
			byte[] bteFile = new byte[afiFile.Size];
			using (MemoryStream msmFile = new MemoryStream())
			{
				m_szeExtractor.ExtractFile(p_strPath, msmFile);
				msmFile.Position = 0;
				for (Int32 intOffset = 0, intRead = 0; intOffset < bteFile.Length && ((intRead = msmFile.Read(bteFile, intOffset, bteFile.Length - intOffset)) >= 0); intOffset += intRead) ;
				msmFile.Close();
			}
			return bteFile;
		}
	}
}
