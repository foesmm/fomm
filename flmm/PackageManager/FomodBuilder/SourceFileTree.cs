using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using Fomm.Controls;

namespace Fomm.PackageManager.FomodBuilder
{
	/// <summary>
	/// A tree of source files for a fomod.
	/// </summary>
	public partial class SourceFileTree : UserControl
	{
		/// <summary>
		/// Class used to pass dragged file system objects to other controls.
		/// </summary>
		public class SourceFileSystemDragData
		{
			private string m_strPath;
			private bool m_booIsDirectory;

			#region Properties

			/// <summary>
			/// Gets the path of the source file system object being dragged.
			/// </summary>
			/// <value>The path of the source file system object being dragged.</value>
			public string Path
			{
				get
				{
					return m_strPath;
				}
			}

			/// <summary>
			/// Gets whether or not the <see cref="Path"/> is a directory.
			/// </summary>
			/// <value>Whether or not the <see cref="Path"/> is a directory.</value>
			public bool IsDirectory
			{
				get
				{
					return m_booIsDirectory;
				}
			}

			#endregion

			#region Constructors

			/// <summary>
			/// A simple constructor that initializes the object.
			/// </summary>
			/// <param name="p_strPath">The path of the source file system object being dragged.</param>
			public SourceFileSystemDragData(string p_strPath, bool p_booIsDirectory)
			{
				m_strPath = p_strPath;
				m_booIsDirectory = p_booIsDirectory;
			}

			#endregion
		}

		#region Properties

		/// <summary>
		/// Gets or sets the sources listed in the control.
		/// </summary>
		/// <value>The sources listed in the control.</value>
		public string[] Sources
		{
			get
			{
				List<string> lstSource = new List<string>();
				foreach (FileSystemTreeNode tndSource in tvwSource.Nodes)
					lstSource.Add(tndSource.LastSource);
				return lstSource.ToArray();
			}
			set
			{
				tvwSource.Nodes.Clear();
				AddSourceFiles(value);
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		public SourceFileTree()
		{
			InitializeComponent();
			tvwSource.TreeViewNodeSorter = new NodeComparer();
		}

		#endregion

		#region Source

		/// <summary>
		/// Handles the <see cref="Control.DragEnter"/> event of the source tree view.
		/// </summary>
		/// <remarks>
		/// This determines if the item being dragged can be dropped at the current location.
		/// </remarks>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">A <see cref="DragEventArgs"/> that describes the event arguments.</param>
		private void tvwSource_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] strFileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
				foreach (string strFile in strFileNames)
				{
					if (!tvwSource.Nodes.ContainsKey(strFile) && (Directory.Exists(strFile) || (File.Exists(strFile) && !".lnk".Equals(Path.GetExtension(strFile).ToLowerInvariant()))))
					{
						e.Effect = DragDropEffects.Copy;
						return;
					}
				}
			}
			e.Effect = DragDropEffects.None;
		}

		/// <summary>
		/// Addes the specified files to the source tree.
		/// </summary>
		/// <param name="p_strFileNames">The paths to add to the source tree.</param>
		protected void AddSourceFiles(string[] p_strFileNames)
		{
			Cursor crsOldCursor = Cursor;
			Cursor = Cursors.WaitCursor;
			foreach (string strFile in p_strFileNames)
				AddFileToSource(strFile);
			Cursor = crsOldCursor;
		}

		/// <summary>
		/// Handles the <see cref="Control.DragDrop"/> event of the source tree view.
		/// </summary>
		/// <remarks>
		/// This handles adding the dropped file/folder to the source tree.
		/// </remarks>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">A <see cref="DragEventArgs"/> that describes the event arguments.</param>
		private void tvwSource_DragDrop(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(DataFormats.FileDrop))
				return;
			string[] strFileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
			AddSourceFiles(strFileNames);
		}

		/// <summary>
		/// Adds the given file to the source tree.
		/// </summary>
		/// <param name="p_strFile">The file to add.</param>
		protected void AddFileToSource(string p_strFile)
		{
			Cursor crsOldCursor = Cursor;
			Cursor = Cursors.WaitCursor;
			if (Directory.Exists(p_strFile) || (File.Exists(p_strFile) && !".lnk".Equals(Path.GetExtension(p_strFile).ToLowerInvariant())))
				addSourceFile(null, p_strFile);
			Cursor = crsOldCursor;
		}

		/// <summary>
		/// This adds a file/folder to the source file structure.
		/// </summary>
		/// <param name="p_tndRoot">The node to which to add the file/folder.</param>
		/// <param name="p_strFile">The path to add to the source file structure.</param>
		private void addSourceFile(FileSystemTreeNode p_tndRoot, string p_strFile)
		{
			if (!p_strFile.StartsWith(Archive.ARCHIVE_PREFIX) && !p_strFile.StartsWith(FileSystemTreeNode.NEW_PREFIX))
			{
				FileSystemInfo fsiInfo = null;
				if (Directory.Exists(p_strFile))
					fsiInfo = new DirectoryInfo(p_strFile);
				else if (File.Exists(p_strFile))
					fsiInfo = new FileInfo(p_strFile);
				else
					return;
				if ((fsiInfo.Attributes & FileAttributes.System) > 0)
					return;
			}

			FileSystemTreeNode tndFile = null;
			TreeNodeCollection tncSiblings = (p_tndRoot == null) ? tvwSource.Nodes : p_tndRoot.Nodes;
			if (tncSiblings.ContainsKey(p_strFile))
				tndFile = (FileSystemTreeNode)tncSiblings[p_strFile];
			else
			{
				tndFile = new FileSystemTreeNode(Path.GetFileName(p_strFile), p_strFile);
				tndFile.Name = p_strFile;
				tncSiblings.Add(tndFile);
			}
			if (tndFile.IsDirectory)
			{
				tndFile.ImageKey = "folder";
				tndFile.SelectedImageKey = "folder";
				if ((p_tndRoot == null) || (p_tndRoot.IsExpanded))
				{
					tndFile.Sources[p_strFile].IsLoaded = true;
					if (p_strFile.StartsWith(Archive.ARCHIVE_PREFIX))
					{
						KeyValuePair<string, string> kvpPath = Archive.ParseArchivePath(p_strFile);
						Archive arcArchive = new Archive(kvpPath.Key);
						string[] strFolders = arcArchive.GetDirectories(kvpPath.Value);
						for (Int32 i = 0; i < strFolders.Length; i++)
							addSourceFile(tndFile, Archive.GenerateArchivePath(kvpPath.Key, strFolders[i]));
						string[] strFiles = arcArchive.GetFiles(kvpPath.Value);
						for (Int32 i = 0; i < strFiles.Length; i++)
							addSourceFile(tndFile, Archive.GenerateArchivePath(kvpPath.Key, strFiles[i]));
					}
					else if (!p_strFile.StartsWith(FileSystemTreeNode.NEW_PREFIX))
					{
						string[] strFolders = Directory.GetDirectories(p_strFile);
						for (Int32 i = 0; i < strFolders.Length; i++)
							addSourceFile(tndFile, strFolders[i]);
						string[] strFiles = Directory.GetFiles(p_strFile);
						for (Int32 i = 0; i < strFiles.Length; i++)
							addSourceFile(tndFile, strFiles[i]);
					}
				}
			}
			else
			{
				tndFile.Sources[p_strFile].IsLoaded = true;
				string strExtension = Path.GetExtension(p_strFile).ToLowerInvariant();
				if (!imlIcons.Images.ContainsKey(strExtension))
				{
					string strIconPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()) + strExtension;
					File.CreateText(strIconPath).Close();
					imlIcons.Images.Add(strExtension, System.Drawing.Icon.ExtractAssociatedIcon(strIconPath));
					File.Delete(strIconPath);
				}
				tndFile.ImageKey = strExtension;
				tndFile.SelectedImageKey = strExtension;
				if (tndFile.IsArchive)
				{
					Archive arcArchive = new Archive(p_strFile);
					string[] strFolders = arcArchive.GetDirectories("/");
					for (Int32 i = 0; i < strFolders.Length; i++)
						addSourceFile(tndFile, Archive.GenerateArchivePath(p_strFile, strFolders[i]));
					string[] strFiles = arcArchive.GetFiles("/");
					for (Int32 i = 0; i < strFiles.Length; i++)
						addSourceFile(tndFile, Archive.GenerateArchivePath(p_strFile, strFiles[i]));
				}
			}
		}

		/// <summary>
		/// Handles the <see cref="TreeView.ItemDrag"/> event of the source tree view.
		/// </summary>
		/// <remarks>
		/// This starts the drag operation of item in the source tree view.
		/// </remarks>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">A <see cref="ItemDragEventArgs"/> that describes the event arguments.</param>
		private void tvwSource_ItemDrag(object sender, ItemDragEventArgs e)
		{
			List<SourceFileSystemDragData> lstData = new List<SourceFileSystemDragData>();
			if (tvwSource.SelectedNodes.Contains((FileSystemTreeNode)e.Item))
				foreach (FileSystemTreeNode tndNode in tvwSource.SelectedNodes)
					lstData.Add(new SourceFileSystemDragData(tndNode.Name, tndNode.IsDirectory));
			else
			{
				FileSystemTreeNode tndNode = (FileSystemTreeNode)e.Item;
				lstData.Add(new SourceFileSystemDragData(tndNode.Name, tndNode.IsDirectory));
			}
			tvwSource.DoDragDrop(lstData, DragDropEffects.Copy);
		}

		/// <summary>
		/// Handles the <see cref="TreeView.QueryContinueDrag"/> event of the source tree view.
		/// </summary>
		/// <remarks>
		/// This aborts the drag operation of an item from the source tree view if the action is iterrup6ted
		/// or stopped over something other than the fomod tree view.
		/// </remarks>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">A <see cref="ItemDragEventArgs"/> that describes the event arguments.</param>
		private void tvwSource_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
		{
			if ((e.Action != DragAction.Drop) && ((Control.MouseButtons & MouseButtons.Left) != MouseButtons.Left))
				e.Action = DragAction.Cancel;
		}

		/// <summary>
		/// Handles the <see cref="TreeView.BeforeExpand"/> event of the source tree view.
		/// </summary>
		/// <remarks>
		/// This handles retrieving the sub-files and sub-folders to display in the tree view.
		/// </remarks>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">A <see cref="TreeViewCancelEventArgs"/> that describes the event arguments.</param>
		private void tvwSource_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			Cursor crsOldCursor = Cursor;
			Cursor = Cursors.WaitCursor;
			string strPath = null;
			foreach (FileSystemTreeNode tndFolder in e.Node.Nodes)
			{
				if (tndFolder.LastSource.IsLoaded || !tndFolder.IsDirectory)
					continue;
				tndFolder.LastSource.IsLoaded = true;
				strPath = tndFolder.LastSource;
				if (strPath.StartsWith(Archive.ARCHIVE_PREFIX))
				{
					KeyValuePair<string, string> kvpPath = Archive.ParseArchivePath(strPath);
					Archive arcArchive = new Archive(kvpPath.Key);
					string[] strFolders = arcArchive.GetDirectories(kvpPath.Value);
					for (Int32 i = 0; i < strFolders.Length; i++)
						addSourceFile(tndFolder, Archive.GenerateArchivePath(kvpPath.Key, strFolders[i]));
					string[] strFiles = arcArchive.GetFiles(kvpPath.Value);
					for (Int32 i = 0; i < strFiles.Length; i++)
						addSourceFile(tndFolder, Archive.GenerateArchivePath(kvpPath.Key, strFiles[i]));
				}
				else if (!strPath.StartsWith(FileSystemTreeNode.NEW_PREFIX))
				{
					string[] strFolders = Directory.GetDirectories(strPath);
					for (Int32 i = 0; i < strFolders.Length; i++)
						addSourceFile(tndFolder, strFolders[i]);
					string[] strFiles = Directory.GetFiles(strPath);
					for (Int32 i = 0; i < strFiles.Length; i++)
						addSourceFile(tndFolder, strFiles[i]);
				}
			}
			Cursor = crsOldCursor;
		}

		#endregion

		/// <summary>
		/// Handles the <see cref="Control.Click"/> event of the add files button.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void butAddFiles_Click(object sender, EventArgs e)
		{
			if (ofdFileChooser.ShowDialog(this) == DialogResult.OK)
				foreach (string strFile in ofdFileChooser.FileNames)
					AddFileToSource(strFile);
		}

		/// <summary>
		/// Handles the <see cref="Control.Click"/> event of the add older button.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void butAddFolder_Click(object sender, EventArgs e)
		{
			if (fbdFolderChooser.ShowDialog(this) == DialogResult.OK)
				AddFileToSource(fbdFolderChooser.SelectedPath);
		}
	}
}
