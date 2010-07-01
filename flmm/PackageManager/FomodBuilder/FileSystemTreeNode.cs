using System;
using Fomm.Util;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using SevenZip;

namespace Fomm.PackageManager.FomodBuilder
{
	/// <summary>
	/// A tree node that encapsulates a file system item.
	/// </summary>
	/// <remarks>
	/// This tracks the sources of and item.
	/// </remarks>
	public class FileSystemTreeNode : TreeNode
	{
		public const string NEW_PREFIX = "new:";

		private Set<string> m_lstSources = new Set<string>();

		public new string FullPath
		{
			get
			{
				if (TreeView != null)
					return base.FullPath;
				Stack<string> stkPath = new Stack<string>();
				TreeNode tndParent = this;
				do
				{
					stkPath.Push(tndParent.Text);
					tndParent = tndParent.Parent;
				} while (tndParent != null);
				StringBuilder stbPath = new StringBuilder();
				while (stkPath.Count > 0)
				{
					stbPath.Append(stkPath.Pop());
					if (stkPath.Count > 0)
						stbPath.Append(Path.DirectorySeparatorChar);
				}
				return stbPath.ToString();
			}
		}

		public bool IsDirectory
		{
			get
			{
				if ((m_lstSources.Count == 0) || m_lstSources[0].StartsWith(NEW_PREFIX))
					return true;
				if (m_lstSources[0].StartsWith(Archive.ARCHIVE_PREFIX))
				{
					KeyValuePair<string, string> kvpArchive = Archive.ParseArchivePath(m_lstSources[0]);
					Archive arcArchive = new Archive(kvpArchive.Key);
					return arcArchive.IsDirectory(kvpArchive.Value);
				}
				return Directory.Exists(m_lstSources[0]);
			}
		}

		public bool IsArchive
		{
			get
			{
				if (m_lstSources.Count == 0)
					return false;
				if (m_lstSources[0].StartsWith(Archive.ARCHIVE_PREFIX) || m_lstSources[0].StartsWith(NEW_PREFIX))
					return false;
				SevenZipExtractor szeExtractor = null;
				try
				{
					szeExtractor = new SevenZipExtractor(m_lstSources[0]);
					object g = szeExtractor.ArchiveFileNames;
				}
				catch (Exception e)
				{
					return false;
				}
				return true;
			}
		}

		public new FileSystemTreeNode Parent
		{
			get
			{
				return (FileSystemTreeNode)base.Parent;
			}
		}

		public IList<string> Sources
		{
			get
			{
				return m_lstSources;
			}
		}

		public FileSystemTreeNode(FileSystemTreeNode p_tndCopy)
			: base(p_tndCopy.Text)
		{
			this.Name = p_tndCopy.Name;
			this.m_lstSources = new Set<string>(p_tndCopy.m_lstSources);
		}

		public FileSystemTreeNode(string p_strName, string p_strPath)
			: base(p_strName)
		{
			if (!p_strPath.StartsWith(Archive.ARCHIVE_PREFIX) && !p_strPath.StartsWith(NEW_PREFIX) && !Directory.Exists(p_strPath) && !File.Exists(p_strPath))
				throw new FileNotFoundException("The given path is not valid.", p_strPath);
			m_lstSources.Add(p_strPath);
		}

		public void AddSource(string p_strSource)
		{
			if (IsDirectory)
			{
				m_lstSources.Remove(p_strSource);
				m_lstSources.Add(p_strSource);
			}
			else
				m_lstSources[0] = p_strSource;
		}
	}
}
