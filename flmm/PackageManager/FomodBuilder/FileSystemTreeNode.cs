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
	public class FileSystemTreeNode : TreeNode, IComparable<FileSystemTreeNode>
	{
		/// <summary>
		/// The prefix used to indicate the node was created by the user.
		/// </summary>
		public const string NEW_PREFIX = "new:";

		private static Dictionary<string, Archive> m_dicArchiveCache = new Dictionary<string, Archive>(StringComparer.InvariantCultureIgnoreCase);

		private Set<string> m_lstSources = new Set<string>();
		private bool? m_booIsAchive = null;
		private bool? m_booIsDirectory = null;

		#region Properties

		/// <summary>
		/// Gets the path to the node in the current tree.
		/// </summary>
		/// <value>The path to the node in the current tree.</value>
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

		/// <summary>
		/// Gets whether or not the node represents a directory.
		/// </summary>
		/// <value>Whether or not the node represents a directory.</value>
		public bool IsDirectory
		{
			get
			{
				if (m_booIsDirectory.HasValue)
					return m_booIsDirectory.Value;


				if ((m_lstSources.Count == 0) || LastSource.StartsWith(NEW_PREFIX))
					m_booIsDirectory = true;
				else if (LastSource.StartsWith(Archive.ARCHIVE_PREFIX))
				{
					KeyValuePair<string, string> kvpArchive = Archive.ParseArchivePath(LastSource);
					Archive arcArchive = null;
					lock (m_dicArchiveCache)
					{
						if (!m_dicArchiveCache.ContainsKey(kvpArchive.Key))
							m_dicArchiveCache[kvpArchive.Key] = new Archive(kvpArchive.Key);
						arcArchive = m_dicArchiveCache[kvpArchive.Key];
					}
					m_booIsDirectory = arcArchive.IsDirectory(kvpArchive.Value);
				}
				else
					m_booIsDirectory = Directory.Exists(LastSource);
				return m_booIsDirectory.Value;
			}
		}

		/// <summary>
		/// Gets whether or not the node represents an archive.
		/// </summary>
		/// <value>Whether or not the node represents an archive.</value>
		public bool IsArchive
		{
			get
			{
				if (m_booIsAchive.HasValue)
					return m_booIsAchive.Value;

				if ((m_lstSources.Count == 0) || LastSource.StartsWith(Archive.ARCHIVE_PREFIX) || LastSource.StartsWith(NEW_PREFIX))
					m_booIsAchive = false;
				else
				{
					SevenZipExtractor szeExtractor = null;
					m_booIsAchive = true;
					try
					{
						szeExtractor = new SevenZipExtractor(LastSource);
						UInt32 g = szeExtractor.FilesCount;
					}
					catch (Exception e)
					{
						m_booIsAchive = false;
					}
				}
				return m_booIsAchive.Value;
			}
		}

		/// <summary>
		/// Gets the node's parent.
		/// </summary>
		/// <remarks>
		/// This casts the parent as a <see cref="FileSystemTreeNode"/>.
		/// </remarks>
		/// <value>The node's parent.</value>
		public new FileSystemTreeNode Parent
		{
			get
			{
				return (FileSystemTreeNode)base.Parent;
			}
		}

		/// <summary>
		/// Gets the sources for the node.
		/// </summary>
		/// <value>The sources for the node.</value>
		public IList<string> Sources
		{
			get
			{
				return m_lstSources;
			}
		}

		/// <summary>
		/// Gets the last source for the node.
		/// </summary>
		/// <remarks>
		/// The last source is the source that will overwrite the other sources. It is the source
		/// last added.
		/// </remarks>
		/// <value>The last source for the node.</value>
		public string LastSource
		{
			get
			{
				if (m_lstSources.Count > 0)
					return m_lstSources[m_lstSources.Count - 1];
				return null;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// The copy constructor.
		/// </summary>
		/// <param name="p_tndCopy">The node to copy.</param>
		public FileSystemTreeNode(FileSystemTreeNode p_tndCopy)
			: base(p_tndCopy.Text)
		{
			this.Name = p_tndCopy.Name;
			this.m_lstSources = new Set<string>(p_tndCopy.m_lstSources);
		}

		/// <summary>
		/// A simple constructor that initializes the object with the given values.
		/// </summary>
		/// <param name="p_strName">The name of the node.</param>
		/// <param name="p_strPath">The path of the file system item being represented by the node.</param>
		public FileSystemTreeNode(string p_strName, string p_strPath)
			: base(p_strName)
		{
			if (!p_strPath.StartsWith(Archive.ARCHIVE_PREFIX) && !p_strPath.StartsWith(NEW_PREFIX) && !Directory.Exists(p_strPath) && !File.Exists(p_strPath))
				throw new FileNotFoundException("The given path is not valid.", p_strPath);
			m_lstSources.Add(p_strPath);
		}

		#endregion

		/// <summary>
		/// Adds the specified path as a source for the node.
		/// </summary>
		/// <param name="p_strSource">The path to add as a source for the node.</param>
		public void AddSource(string p_strSource)
		{
			m_lstSources.Remove(p_strSource);
			m_lstSources.Add(p_strSource);
		}

		#region IComparable<FileSystemTreeNode> Members

		/// <summary>
		/// Compares this node to another.
		/// </summary>
		/// <remarks>
		/// A directory is less than a file. If the nodes being compared are
		/// both directories, or both not directories, their display text
		/// is compared.
		/// </remarks> 
		/// <param name="other">The <see cref="FileSystemTreeNode"/> to which to compare this node.</param>
		/// <returns>A value less than 0 if this node is less than the other.
		/// 0 if this node is equal to the other.
		/// A value greater than 0 if this node is greater than the other.</returns>
		public int CompareTo(FileSystemTreeNode other)
		{
			Int32 intResult = other.IsDirectory.CompareTo(this.IsDirectory);
			if (intResult == 0)
				intResult = this.Text.CompareTo(other.Text);
			return intResult;
		}

		#endregion
	}
}
