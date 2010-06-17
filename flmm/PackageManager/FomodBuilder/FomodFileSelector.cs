using System;
using System.Windows.Forms;
using System.Collections.Generic;
using SevenZip;
using System.Collections;
using System.Text;
using System.IO;
using System.Drawing;
using Fomm.Controls;
using System.Text.RegularExpressions;

namespace Fomm.PackageManager.FomodBuilder
{
	/// <summary>
	/// Enables the selection and ordering of files for the creation of a fomod.
	/// </summary>
	public partial class FomodFileSelector : UserControl, IStatusProviderAware
	{
		/// <summary>
		/// The rich-text formated content of the help box.
		/// </summary>
		private const string HELP_STRING = @"{\rtf1\ansi\ansicpg1252\deff0\deflang4105{\fonttbl{\f0\fnil\fcharset0 Arial;}{\f1\fnil\fcharset2 Symbol;}}
{\*\generator Msftedit 5.41.21.2509;}\viewkind4\uc1\pard{\pntext\f0 1.\tab}{\*\pn\pnlvlbody\pnf0\pnindent0\pnstart1\pndec{\pntxta.}}
\fi-360\li720\sl240\slmult1\lang9\fs18 Add files and/or folders to the \b Source Files\b0  box. You can either drag and drop files and folders, or use the buttons.\par
{\pntext\f0 2.\tab}Browse the Source Files tree and drag the files and folder you want to include in your FOMOD into the \b FOMOD Files\b0  box.\par
\pard\sl240\slmult1\par
Remeber, you can customize the FOMOD file structure by doing any of the following:\par
\pard{\pntext\f1\'B7\tab}{\*\pn\pnlvlblt\pnf1\pnindent0{\pntxtb\'B7}}\fi-360\li720\sl240\slmult1 You can rename any folder in the \b FOMOD Files\b0  box.\par
{\pntext\f1\'B7\tab}You can create new folders on the \b FOMOD Files\b0  box.\par
{\pntext\f1\'B7\tab}You can remove folders and file from the \b FOMOD Files\b0  box. Doing so will not delete the file from your computer.\par
}
 ";
		#region Properties

		/// <summary>
		/// Gets or sets the sources listed in the control.
		/// </summary>
		/// <value>The sources listed in the control.</value>
		public string[] Sources
		{
			get
			{
				return sftSources.Sources;
			}
			set
			{
				sftSources.Sources = value;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		public FomodFileSelector()
		{
			InitializeComponent();
			rtbHelp.Rtf = HELP_STRING;

			rtbHelp.Visible = false;
		}

		#endregion

		#region Path Mapping

		/// <summary>
		/// Gets a list of path mappings, from source to destination, required to
		/// create the specified fomod file structure.
		/// </summary>
		/// <returns>A list of path mappings, from source to destination, required to
		/// create the specified fomod file structure.</returns>
		public IList<KeyValuePair<string, string>> GetCopyPaths()
		{
			List<FileSystemTreeNode> lstPathNodes = new List<FileSystemTreeNode>();
			foreach (FileSystemTreeNode tndNode in tvwFomod.Nodes)
				lstPathNodes.Add(CopyTree(tndNode));
			FileSystemTreeNode tndPathNode = null;
			for (Int32 i = lstPathNodes.Count - 1; i >= 0; i--)
			{
				tndPathNode = lstPathNodes[i];
				if (tndPathNode.IsDirectory && (tndPathNode.Nodes.Count == 0))
					lstPathNodes.RemoveAt(i);
				else
					ProcessTree(tndPathNode);
			}
			List<KeyValuePair<string, string>> lstPaths = new List<KeyValuePair<string, string>>();
			GetCopyPaths(lstPaths, lstPathNodes);
			return lstPaths;
		}

		/// <summary>
		/// Walks the given tree to generate a list of path mappings, from source to destination, required to
		/// create the specified fomod file structure.
		/// </summary>
		/// <param name="p_lstPaths">The list of mappings.</param>
		/// <param name="p_tncNodes">The tree to use to generate the mappings.</param>
		private void GetCopyPaths(List<KeyValuePair<string, string>> p_lstPaths, IList p_tncNodes)
		{
			foreach (FileSystemTreeNode tndNode in p_tncNodes)
			{
				foreach (string strSource in tndNode.Sources)
					p_lstPaths.Add(new KeyValuePair<string, string>(strSource, tndNode.FullPath));
				GetCopyPaths(p_lstPaths, tndNode.Nodes);
			}
		}

		#region Tree Copy

		/// <summary>
		/// Copies the tree rooted at the given node.
		/// </summary>
		/// <param name="p_tndSource">The root of the tree to copy.</param>
		/// <returns>The root of the copied tree.</returns>
		private FileSystemTreeNode CopyTree(FileSystemTreeNode p_tndSource)
		{
			FileSystemTreeNode tndDest = new FileSystemTreeNode(p_tndSource);
			CopyTree(p_tndSource, tndDest);
			return tndDest;
		}

		/// <summary>
		/// Copies the tree rooted at the given source node to the tree rooted
		/// at the given destination node.
		/// </summary>
		/// <param name="p_tndSource">The root of the tree to copy.</param>
		/// <param name="p_tndDest">The root of the tree to which to copy.</param>
		private void CopyTree(FileSystemTreeNode p_tndSource, FileSystemTreeNode p_tndDest)
		{
			FileSystemTreeNode tndCopy = null;
			foreach (FileSystemTreeNode tndSourceNode in p_tndSource.Nodes)
			{
				tndCopy = new FileSystemTreeNode(tndSourceNode);
				p_tndDest.Nodes.Add(tndCopy);
				CopyTree(tndSourceNode, tndCopy);
			}
		}

		#endregion

		/// <summary>
		/// Processes the tree rooted at the given node to romve any superfluous nodes and sources.
		/// </summary>
		/// <remarks>
		/// This method cleans up the given tree so that the most efficient set of mappings
		/// needed to create the fomod file structure can be generated.
		/// </remarks>
		/// <param name="p_tndNode">The node at which the fomod file structure tree is rooted.</param>
		private void ProcessTree(FileSystemTreeNode p_tndNode)
		{
			if (p_tndNode.Nodes.Count == 0)
			{
				for (Int32 j = p_tndNode.Sources.Count - 1; j >= 0; j--)
					if (p_tndNode.Sources[j].StartsWith(FileSystemTreeNode.NEW_PREFIX))
						p_tndNode.Sources.RemoveAt(j);
				return;
			}
			foreach (FileSystemTreeNode tndNode in p_tndNode.Nodes)
				ProcessTree(tndNode);
			List<string> lstSubPaths = new List<string>();
			string strSource = null;
			for (Int32 j = p_tndNode.Sources.Count - 1; j >= 0; j--)
			{
				strSource = p_tndNode.Sources[j];
				lstSubPaths.Clear();
				if (strSource.StartsWith(Archive.ARCHIVE_PREFIX))
				{
					KeyValuePair<string, string> kvpPath = Archive.ParseArchive(strSource);
					Archive arcArchive = new Archive(kvpPath.Key);
					foreach (string strPath in arcArchive.GetDirectories(kvpPath.Value))
						lstSubPaths.Add(String.Format("{0}{1}//{2}", Archive.ARCHIVE_PREFIX, kvpPath.Key, strPath));
					foreach (string strPath in arcArchive.GetFiles(kvpPath.Value))
						lstSubPaths.Add(String.Format("{0}{1}//{2}", Archive.ARCHIVE_PREFIX, kvpPath.Key, strPath));
				}
				else if (strSource.StartsWith(FileSystemTreeNode.NEW_PREFIX))
				{
					p_tndNode.Sources.RemoveAt(j);
					continue;
				}
				else
				{
					lstSubPaths.AddRange(Directory.GetDirectories(strSource));
					lstSubPaths.AddRange(Directory.GetFiles(strSource));
				}
				Int32 intFoundCount = 0;
				foreach (string strSubPath in lstSubPaths)
				{
					foreach (FileSystemTreeNode tndNode in p_tndNode.Nodes)
					{
						if (tndNode.Sources.Contains(strSubPath) && (tndNode.Nodes.Count == 0))
						{
							intFoundCount++;
							break;
						}
					}
				}
				if (intFoundCount == lstSubPaths.Count)
				{
					FileSystemTreeNode tndNode = null;
					foreach (string strSubPath in lstSubPaths)
					{
						for (Int32 i = p_tndNode.Nodes.Count - 1; i >= 0; i--)
						{
							tndNode = (FileSystemTreeNode)p_tndNode.Nodes[i];
							if (tndNode.Sources.Contains(strSubPath))
							{
								if (tndNode.Sources.Count > 1)
									tndNode.Sources.Remove(strSubPath);
								else
									p_tndNode.Nodes.RemoveAt(i);
								break;
							}
						}
					}
				}
				else
					p_tndNode.Sources.RemoveAt(j);
			}
		}

		#endregion

		#region Fomod

		/// <summary>
		/// Handles the <see cref="Control.DragOver"/> event of the fomod tree view.
		/// </summary>
		/// <remarks>
		/// This determines if the item being dragged can be dropped at the current location.
		/// </remarks>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">A <see cref="DragEventArgs"/> that describes the event arguments.</param>
		private void tvwFomod_DragOver(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(typeof(SourceFileTree.SourceFileSystemDragData)))
				return;
			e.Effect = DragDropEffects.Copy;
			FileSystemTreeNode tndFolder = (FileSystemTreeNode)tvwFomod.GetNodeAt(tvwFomod.PointToClient(new Point(e.X, e.Y)));
			if ((tndFolder != null) && tndFolder.IsDirectory)
				tvwFomod.SelectedNode = tndFolder;
			else
				tvwFomod.SelectedNode = null;
		}

		/// <summary>
		/// Handles the <see cref="Control.DragDrop"/> event of the fomod tree view.
		/// </summary>
		/// <remarks>
		/// This handles adding the dropped file/folder to the fomod tree.
		/// </remarks>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">A <see cref="DragEventArgs"/> that describes the event arguments.</param>
		private void tvwFomod_DragDrop(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(typeof(SourceFileTree.SourceFileSystemDragData)))
				return;
			Cursor crsOldCursor = Cursor;
			Cursor = Cursors.WaitCursor;
			FileSystemTreeNode tndFolder = (FileSystemTreeNode)tvwFomod.GetNodeAt(tvwFomod.PointToClient(new Point(e.X, e.Y)));
			string strPath = ((SourceFileTree.SourceFileSystemDragData)e.Data.GetData(typeof(SourceFileTree.SourceFileSystemDragData))).Path;
			if (tndFolder != null)
			{
				if (!tndFolder.IsDirectory)
					tndFolder = tndFolder.Parent;
				if (tndFolder != null)
				{
					addFomodFile(tndFolder, strPath);
					tndFolder.Expand();
				}
				else
					addFomodFile(null, strPath);
			}
			else
				addFomodFile(null, strPath);
			Cursor = crsOldCursor;
		}

		/// <summary>
		/// This adds a file/folder to the fomod file structure.
		/// </summary>
		/// <param name="p_tndRoot">The node to which to add the file/folder.</param>
		/// <param name="p_strFile">The path to add to the fomod file structure.</param>
		/// <returns>The node that was added for the specified file/folder. <lang cref="null"/>
		/// is returned if the given path is invalid.</returns>
		private FileSystemTreeNode addFomodFile(TreeNode p_tndRoot, string p_strFile)
		{
			if (!p_strFile.StartsWith(Archive.ARCHIVE_PREFIX) && !p_strFile.StartsWith(FileSystemTreeNode.NEW_PREFIX))
			{
				FileSystemInfo fsiInfo = null;
				if (Directory.Exists(p_strFile))
					fsiInfo = new DirectoryInfo(p_strFile);
				else if (File.Exists(p_strFile))
					fsiInfo = new FileInfo(p_strFile);
				else
					return null;
				if ((fsiInfo.Attributes & FileAttributes.System) > 0)
					return null;
			}

			string strFileName = Path.GetFileName(p_strFile);
			FileSystemTreeNode tndFile = null;
			TreeNodeCollection tncSiblings = (p_tndRoot == null) ? tvwFomod.Nodes : p_tndRoot.Nodes;
			if (tncSiblings.ContainsKey(strFileName.ToLowerInvariant()))
			{
				tndFile = (FileSystemTreeNode)tncSiblings[strFileName.ToLowerInvariant()];
				tndFile.AddSource(p_strFile);
			}
			else
			{
				tndFile = new FileSystemTreeNode(strFileName, p_strFile);
				tndFile.ContextMenuStrip = cmsFomodNode;
				tndFile.Name = strFileName.ToLowerInvariant();
				tncSiblings.Add(tndFile);
			}
			if (tndFile.IsDirectory)
			{
				tndFile.ImageKey = "folder";
				tndFile.SelectedImageKey = "folder";
				if ((p_tndRoot == null) || (p_tndRoot.IsExpanded))
				{
					if (p_strFile.StartsWith(Archive.ARCHIVE_PREFIX))
					{
						KeyValuePair<string, string> kvpPath = Archive.ParseArchive(p_strFile);
						Archive arcArchive = new Archive(kvpPath.Key);
						string[] strFolders = arcArchive.GetDirectories(kvpPath.Value);
						foreach (string strSubFolder in strFolders)
							addFomodFile(tndFile, String.Format("{0}{1}//{2}", Archive.ARCHIVE_PREFIX, kvpPath.Key, strSubFolder));
						string[] strFiles = arcArchive.GetFiles(kvpPath.Value);
						foreach (string strfile in strFiles)
							addFomodFile(tndFile, String.Format("{0}{1}//{2}", Archive.ARCHIVE_PREFIX, kvpPath.Key, strfile));
					}
					else if (!p_strFile.StartsWith(FileSystemTreeNode.NEW_PREFIX))
					{
						string[] strFolders = Directory.GetDirectories(p_strFile);
						foreach (string strSubFolder in strFolders)
							addFomodFile(tndFile, strSubFolder);
						string[] strFiles = Directory.GetFiles(p_strFile);
						foreach (string strfile in strFiles)
							addFomodFile(tndFile, strfile);
					}
				}
			}
			else
			{
				string strExtension = Path.GetExtension(p_strFile).ToLowerInvariant();
				string strIconPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()) + strExtension;
				File.CreateText(strIconPath).Close();
				imlIcons.Images.Add(strExtension, System.Drawing.Icon.ExtractAssociatedIcon(strIconPath));
				File.Delete(strIconPath);
				tndFile.ImageKey = strExtension;
				tndFile.SelectedImageKey = strExtension;
			}
			return tndFile;
		}

		/// <summary>
		/// Handles the <see cref="TreeView.BeforeExpand"/> event of the fomod tree view.
		/// </summary>
		/// <remarks>
		/// This handles retrieving the sub-files and sub-folders to display in the tree view.
		/// </remarks>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">A <see cref="TreeViewCancelEventArgs"/> that describes the event arguments.</param>
		private void tvwFomod_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			Cursor crsOldCursor = Cursor;
			Cursor = Cursors.WaitCursor;
			List<string> lstFolders = new List<string>();
			List<string> lstFiles = new List<string>();
			foreach (FileSystemTreeNode tndFolder in e.Node.Nodes)
			{
				if ((tndFolder.Nodes.Count > 0) || !tndFolder.IsDirectory)
					continue;
				lstFolders.Clear();
				lstFiles.Clear();
				foreach (string strSource in tndFolder.Sources)
				{
					if (strSource.StartsWith(Archive.ARCHIVE_PREFIX))
					{
						KeyValuePair<string, string> kvpPath = Archive.ParseArchive(strSource);
						Archive arcArchive = new Archive(kvpPath.Key);
						string[] strFolders = arcArchive.GetDirectories(kvpPath.Value);
						foreach (string strSubFolder in strFolders)
							lstFolders.Add(String.Format("{0}{1}//{2}", Archive.ARCHIVE_PREFIX, kvpPath.Key, strSubFolder));
						string[] strFiles = arcArchive.GetFiles(kvpPath.Value);
						foreach (string strfile in strFiles)
							lstFiles.Add(String.Format("{0}{1}//{2}", Archive.ARCHIVE_PREFIX, kvpPath.Key, strfile));
					}
					else if (!strSource.StartsWith(FileSystemTreeNode.NEW_PREFIX))
					{
						string[] strFolders = Directory.GetDirectories(strSource);
						lstFolders.AddRange(strFolders);
						string[] strFiles = Directory.GetFiles(strSource);
						lstFiles.AddRange(strFiles);
					}
				}
				foreach (string strSubFolder in lstFolders)
					addFomodFile(tndFolder, strSubFolder);
				foreach (string strfile in lstFiles)
					addFomodFile(tndFolder, strfile);
			}
			Cursor = crsOldCursor;
		}

		/// <summary>
		/// Handles the <see cref="TreeView.AfterLabelEdit"/> event of the fomod tree view.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">A <see cref="TreeViewCancelEventArgs"/> that describes the event arguments.</param>
		private void tvwFomod_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			e.Node.Name = e.Label.ToLowerInvariant();
		}

		#endregion

		#region Fomod Context Menu

		/// <summary>
		/// Handles the <see cref="Control.Click"/> event of the rename context menu item.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> that describes the event arguments.</param>
		private void renameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			tvwFomod.SelectedNode.BeginEdit();
		}

		/// <summary>
		/// Handles the <see cref="Control.Click"/> event of the delete context menu item.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> that describes the event arguments.</param>
		private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			TreeNode tndNode = tvwFomod.SelectedNode;
			if (tndNode.Parent == null)
				tvwFomod.Nodes.Remove(tndNode);
			else
				tndNode.Parent.Nodes.Remove(tndNode);
		}

		/// <summary>
		/// Handles the <see cref="Control.KeyDown"/> event of the fomod tree view.
		/// </summary>
		/// <remarks>
		/// This delegates the key press to the fomod tree nodes' context menu.
		/// </remarks>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">A <see cref="KeyEventArgs"/> that describes the event arguments.</param>
		private void tvwFomod_KeyDown(object sender, KeyEventArgs e)
		{
			foreach (ToolStripItem item in cmsFomodNode.Items)
				if ((item is ToolStripMenuItem) && (e.KeyData == ((ToolStripMenuItem)item).ShortcutKeys))
					item.PerformClick();
		}

		/// <summary>
		/// Handles the <see cref="Control.Click"/> event of the new folder context menu item.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> that describes the event arguments.</param>
		private void newFolderToolStripMenuItem_Click(object sender, EventArgs e)
		{
			TreeNode tndNode = tvwFomod.SelectedNode;
			FileSystemTreeNode tndNewNode = addFomodFile(tndNode, FileSystemTreeNode.NEW_PREFIX + "//New Folder");
			if (tndNode != null)
				tndNode.Expand();
			tndNewNode.BeginEdit();
		}

		/// <summary>
		/// Handles the <see cref="Control.MouseDown"/> event of the fomod tree view.
		/// </summary>
		/// <remarks>
		/// This selects the node under the cursor when the user right-clicks.
		/// </remarks>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">A <see cref="MouseEventArgs"/> that describes the event arguments.</param>
		private void tvwFomod_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				FileSystemTreeNode tndFolder = (FileSystemTreeNode)tvwFomod.GetNodeAt(e.Location);
				tvwFomod.SelectedNode = tndFolder;
			}
		}

		#endregion

		/// <summary>
		/// Handles the <see cref="Control.Click"/> event of the help link.
		/// </summary>
		/// <remarks>
		/// This shows/hides the help box as appropriate.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="LinkLabelLinkClickedEventArgs"/> describing the event arguments.</param>
		private void lnkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			rtbHelp.Visible = !rtbHelp.Visible;
			lnkHelp.Text = rtbHelp.Visible ? "Close Help" : "Open Help";
		}

		#region Find Fomod Files

		/// <summary>
		/// Finds all files in the fomod file structure matching the given pattern.
		/// </summary>
		/// <param name="p_strPattern">The pattern of the files to find.</param>
		/// <returns>Returns pairs of values representing the found files. The key of the pair is the fomod file path,
		/// and the value is the source path for the file.</returns>
		public List<KeyValuePair<string, string>> FindFomodFiles(string p_strPattern)
		{
			string[] strPatterns = p_strPattern.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);
			Queue<string> queDirectories = new Queue<string>();
			for (Int32 i = 0; i < strPatterns.Length - 1; i++)
				queDirectories.Enqueue(strPatterns[i]);
			string strFileNamePattern = (strPatterns.Length > 0) ? strPatterns[strPatterns.Length - 1] : "*";
			strFileNamePattern = strFileNamePattern.Replace(".", @"\.").Replace("*", @"\.*");
			Regex rgxFileNamePattern = new Regex(strFileNamePattern, RegexOptions.IgnoreCase);
			List<KeyValuePair<string, string>> lstMatches = new List<KeyValuePair<string, string>>();
			foreach (FileSystemTreeNode tndFolder in tvwFomod.Nodes)
				lstMatches.AddRange(FindFomodFiles(tndFolder, queDirectories, rgxFileNamePattern));
			return lstMatches;
		}

		/// <summary>
		/// The recursive method that searches the fomod file structure for files in the specified directory
		/// matching the given pattern.
		/// </summary>
		/// <param name="p_tndRoot">The node from which to being searching.</param>
		/// <param name="p_queDirectories">The path to the directory in which to search.</param>
		/// <param name="p_rgxFileNamePattern">The pattern of the files to find.</param>
		/// <returns>Returns pairs of values representing the found files. The key of the pair is the fomod file path,
		/// and the value is the source path for the file.</returns>
		private List<KeyValuePair<string, string>> FindFomodFiles(FileSystemTreeNode p_tndRoot, Queue<string> p_queDirectories, Regex p_rgxFileNamePattern)
		{
			List<KeyValuePair<string, string>> lstMatches = new List<KeyValuePair<string, string>>();
			List<string> lstFolders = new List<string>();
			List<string> lstFiles = new List<string>();
			if (p_tndRoot.IsDirectory && p_tndRoot.Name.Equals(p_queDirectories.Peek()))
			{
				p_queDirectories.Dequeue();
				if (p_tndRoot.Nodes.Count == 0)
				{
					foreach (string strSource in p_tndRoot.Sources)
					{
						if (strSource.StartsWith(Archive.ARCHIVE_PREFIX))
						{
							KeyValuePair<string, string> kvpPath = Archive.ParseArchive(strSource);
							Archive arcArchive = new Archive(kvpPath.Key);
							string[] strFolders = arcArchive.GetDirectories(kvpPath.Value);
							foreach (string strSubFolder in strFolders)
								lstFolders.Add(String.Format("{0}{1}//{2}", Archive.ARCHIVE_PREFIX, kvpPath.Key, strSubFolder));
							string[] strFiles = arcArchive.GetFiles(kvpPath.Value);
							foreach (string strFile in strFiles)
								lstFiles.Add(String.Format("{0}{1}//{2}", Archive.ARCHIVE_PREFIX, kvpPath.Key, strFile));
						}
						else if (!strSource.StartsWith(FileSystemTreeNode.NEW_PREFIX))
						{
							string[] strFolders = Directory.GetDirectories(strSource);
							lstFolders.AddRange(strFolders);
							string[] strFiles = Directory.GetFiles(strSource);
							foreach (string strFile in strFiles)
								lstFiles.Add(strFile);
						}
					}
					foreach (string strSubFolder in lstFolders)
						addFomodFile(p_tndRoot, strSubFolder);
					foreach (string strfile in lstFiles)
						addFomodFile(p_tndRoot, strfile);
				}
				foreach (FileSystemTreeNode tndNode in p_tndRoot.Nodes)
					lstMatches.AddRange(FindFomodFiles(tndNode, p_queDirectories, p_rgxFileNamePattern));
			}
			else if ((p_queDirectories.Count == 0) && p_rgxFileNamePattern.IsMatch(p_tndRoot.Name))
				lstMatches.Add(new KeyValuePair<string, string>(p_tndRoot.FullPath, p_tndRoot.Sources[0]));
			return lstMatches;
		}

		#endregion

		#region IStatusProviderAware Members

		/// <summary>
		/// Gets the label upon which to display status message from <see cref="SiteStatusProvider"/>s.
		/// </summary>
		/// <value>The label upon which to display status message from <see cref="SiteStatusProvider"/>s.</value>
		public Control StatusProviderSite
		{
			get
			{
				return lblFomodFiles;
			}
		}

		#endregion
	}
}
