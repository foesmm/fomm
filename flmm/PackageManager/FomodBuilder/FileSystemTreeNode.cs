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
		/// <summary>
		/// The prefix used to indicate the node was created by the user.
		/// </summary>
		public const string NEW_PREFIX = "new:";

		private Set<string> m_lstSources = new Set<string>();

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

		/// <summary>
		/// Gets whether or not the node represents an archive.
		/// </summary>
		/// <value>Whether or not the node represents an archive.</value>
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
					UInt32 g = szeExtractor.FilesCount;
				}
				catch (Exception e)
				{
					return false;
				}
				return true;
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
