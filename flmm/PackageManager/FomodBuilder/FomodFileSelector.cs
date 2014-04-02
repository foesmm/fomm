using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Drawing;
using Fomm.Controls;
using System.Text.RegularExpressions;
using System.ComponentModel;

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
    private const string HELP_STRING =
      @"{\rtf1\ansi\ansicpg1252\deff0\deflang4105{\fonttbl{\f0\fnil\fcharset0 Arial;}{\f1\fnil\fcharset2 Symbol;}}
{\*\generator Msftedit 5.41.21.2509;}\viewkind4\uc1\pard{\pntext\f0 1.\tab}{\*\pn\pnlvlbody\pnf0\pnindent0\pnstart1\pndec{\pntxta.}}
\fi-360\li720\sl240\slmult1\lang9\fs18 Add files and/or folders to the \b Source Files\b0  box. You can either drag and drop files and folders, or use the buttons.\par
{\pntext\f0 2.\tab}Browse the \b Source Files\b0  tree and drag the files and folders you want to include in your FOMod into the \b FOMod Files\b0  box. Archive files (like Zip and 7z files) in the \b Source Files\b0  box can browsed like directories.\par
\pard\sl240\slmult1\par
Remeber, you can customize the FOMod file structure by doing any of the following:\par
\pard{\pntext\f1\'B7\tab}{\*\pn\pnlvlblt\pnf1\pnindent0{\pntxtb\'B7}}\fi-360\li720\sl240\slmult1 You can rename any folder in the \b FOMod Files\b0  box.\par
{\pntext\f1\'B7\tab}You can create new folders on the \b FOMod Files\b0  box.\par
{\pntext\f1\'B7\tab}You can remove folders and file from the \b FOMod Files\b0  box. Doing so will not delete the file from your computer.\par
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
      tvwFomod.TreeViewNodeSorter = new NodeComparer();
      tvwFomod.Sorted = true;
    }

    #endregion

    #region Path Mapping

    /// <summary>
    /// Populates the file selector based on the given sources and copy instructions.
    /// </summary>
    /// <param name="p_lstSources"></param>
    public void SetCopyInstructions(IList<SourceFile> p_lstSources,
                                    IList<KeyValuePair<string, string>> p_lstInstructions)
    {
      var lstInstructions = new List<KeyValuePair<string, string>>();
      //we need to replace any instructions of the form:
      // "blaFolder => /"
      // with a set of instructions that explicitly copies the contents of blaFolder
      // to the root, like:
      // "blaFolder/subFolder => subFolder"
      // "blaFolder/file.txt => file.txt"
      // this needs to be done so we can populate the FOMod Files tree
      foreach (var kvpInstruction in p_lstInstructions)
      {
        var strParentDirectory = Path.GetDirectoryName(kvpInstruction.Value);
        if (strParentDirectory == null)
        {
          if (kvpInstruction.Key.StartsWith(Archive.ARCHIVE_PREFIX))
          {
            var kvpSource = Archive.ParseArchivePath(kvpInstruction.Key);
            var arcSource = new Archive(kvpSource.Key);
            if (!arcSource.IsDirectory(kvpSource.Value))
            {
              throw new Exception("Copy instruction is renaming a file to the root directory.");
            }
            foreach (var strDirectory in arcSource.GetDirectories(kvpSource.Value))
            {
              var strDestPath = strDirectory.Substring(kvpSource.Value.Length);
              lstInstructions.Add(
                new KeyValuePair<string, string>(Archive.GenerateArchivePath(kvpSource.Key, strDirectory), strDestPath));
            }
            foreach (var strFile in arcSource.GetFiles(kvpSource.Value))
            {
              var strDestPath = strFile.Substring(kvpSource.Value.Length);
              lstInstructions.Add(new KeyValuePair<string, string>(Archive.GenerateArchivePath(kvpSource.Key, strFile),
                                                                   strDestPath));
            }
          }
          else
          {
            if (!Directory.Exists(kvpInstruction.Key))
            {
              throw new Exception("Copy instruction is renaming a file to the root directory.");
            }
            foreach (var strDirectory in Directory.GetDirectories(kvpInstruction.Key))
            {
              var strDestPath = strDirectory.Substring(kvpInstruction.Key.Length);
              lstInstructions.Add(new KeyValuePair<string, string>(strDirectory, strDestPath));
            }
            foreach (var strFile in Directory.GetFiles(kvpInstruction.Key))
            {
              var strDestPath = strFile.Substring(kvpInstruction.Key.Length);
              lstInstructions.Add(new KeyValuePair<string, string>(strFile, strDestPath));
            }
          }
        }
        else
        {
          lstInstructions.Add(kvpInstruction);
        }
      }

      foreach (var kvpInstruction in lstInstructions)
      {
        var strParentDirectory = Path.GetDirectoryName(kvpInstruction.Value);
        strParentDirectory = strParentDirectory.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        var tndRoot = findNode(strParentDirectory);
        if ((tndRoot == null) || !tndRoot.FullPath.Equals(strParentDirectory))
        {
          var strFoundPathLength = (tndRoot == null) ? -1 : tndRoot.FullPath.Length;
          //we need to create some folders
          var strRemainingFolders = strParentDirectory.Substring(strFoundPathLength + 1).Split(new char[]
          {
            Path.DirectorySeparatorChar
          }, StringSplitOptions.RemoveEmptyEntries);
          foreach (var strFolder in strRemainingFolders)
          {
            tndRoot = addFomodFile(tndRoot, FileSystemTreeNode.NEW_PREFIX + "//" + strFolder);
          }
        }
        addFomodFile(tndRoot, kvpInstruction.Key).Text = Path.GetFileName(kvpInstruction.Value);
      }
      var lstSources = new List<string>(p_lstSources.Count);
      foreach (var sflSource in p_lstSources)
      {
        if (!sflSource.Hidden)
        {
          lstSources.Add(sflSource.Source);
        }
      }
      sftSources.Sources = lstSources.ToArray();
    }

    /// <summary>
    /// Finds the deepest nod in the fomod file system along the given path.
    /// </summary>
    /// <param name="p_strPath">The path for which to find the deepest node.</param>
    /// <returns>The deepest nod in the fomod file structure that is along the given path.</returns>
    protected FileSystemTreeNode findNode(string p_strPath)
    {
      if (String.IsNullOrEmpty(p_strPath))
      {
        return null;
      }
      var strPath = p_strPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
      strPath = strPath.Trim(new char[]
      {
        Path.DirectorySeparatorChar
      });
      var strPathNodes = strPath.Split(Path.DirectorySeparatorChar);
      Array.Reverse(strPathNodes);
      var stkPath = new Stack<string>(strPathNodes);
      var tncNodes = tvwFomod.Nodes;
      FileSystemTreeNode tndLastNode = null;
      var intPathCount = 0;
      while ((tncNodes.Count > 0) && (intPathCount != stkPath.Count))
      {
        intPathCount = stkPath.Count;
        foreach (FileSystemTreeNode tndNode in tncNodes)
        {
          if (tndNode.Name.Equals(stkPath.Peek(), StringComparison.InvariantCultureIgnoreCase))
          {
            stkPath.Pop();
            if (stkPath.Count == 0)
            {
              return tndNode;
            }
            PopulateNodeWithChildren(tndNode);
            tndLastNode = tndNode;
            tncNodes = tndNode.Nodes;
            break;
          }
        }
      }
      return tndLastNode;
    }

    /// <summary>
    /// Gets a list of path mappings, from source to destination, required to
    /// create the specified fomod file structure.
    /// </summary>
    /// <returns>A list of path mappings, from source to destination, required to
    /// create the specified fomod file structure.</returns>
    public IList<KeyValuePair<string, string>> GetCopyInstructions()
    {
      var lstPathNodes = new List<FileSystemTreeNode>();
      foreach (FileSystemTreeNode tndNode in tvwFomod.Nodes)
      {
        lstPathNodes.Add(CopyTree(tndNode));
      }
      for (var i = lstPathNodes.Count - 1; i >= 0; i--)
      {
        var tndPathNode = lstPathNodes[i];
        if (tndPathNode.IsDirectory && (tndPathNode.Nodes.Count == 0))
        {
          lstPathNodes.RemoveAt(i);
        }
        else
        {
          ProcessTree(tndPathNode);
        }
      }
      var lstPaths = new List<KeyValuePair<string, string>>();
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
        if (tndNode.IsDirectory)
        {
          foreach (string strSource in tndNode.Sources)
          {
            p_lstPaths.Add(new KeyValuePair<string, string>(strSource, tndNode.FullPath));
          }
        }
        else
        {
          p_lstPaths.Add(new KeyValuePair<string, string>(tndNode.LastSource, tndNode.FullPath));
        }
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
      var tndDest = new FileSystemTreeNode(p_tndSource);
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
      foreach (FileSystemTreeNode tndSourceNode in p_tndSource.Nodes)
      {
        var tndCopy = new FileSystemTreeNode(tndSourceNode);
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
        for (var j = p_tndNode.Sources.Count - 1; j >= 0; j--)
        {
          if (p_tndNode.Sources[j].Path.StartsWith(FileSystemTreeNode.NEW_PREFIX))
          {
            p_tndNode.Sources.RemoveAt(j);
          }
        }
        return;
      }
      foreach (FileSystemTreeNode tndNode in p_tndNode.Nodes)
      {
        ProcessTree(tndNode);
      }
      var lstSubPaths = new List<string>();
      for (var j = p_tndNode.Sources.Count - 1; j >= 0; j--)
      {
        var srcSource = p_tndNode.Sources[j];
        lstSubPaths.Clear();
        if (srcSource.Path.StartsWith(Archive.ARCHIVE_PREFIX))
        {
          var kvpPath = Archive.ParseArchivePath(srcSource.Path);
          var arcArchive = new Archive(kvpPath.Key);
          foreach (var strPath in arcArchive.GetDirectories(kvpPath.Value))
          {
            lstSubPaths.Add(Archive.GenerateArchivePath(kvpPath.Key, strPath));
          }
          foreach (var strPath in arcArchive.GetFiles(kvpPath.Value))
          {
            lstSubPaths.Add(Archive.GenerateArchivePath(kvpPath.Key, strPath));
          }
        }
        else if (srcSource.Path.StartsWith(FileSystemTreeNode.NEW_PREFIX))
        {
          p_tndNode.Sources.RemoveAt(j);
          continue;
        }
        else
        {
          lstSubPaths.AddRange(Directory.GetDirectories(srcSource.Path));
          foreach (var strPath in Directory.GetFiles(srcSource.Path))
          {
            if ((new FileInfo(strPath).Attributes & FileAttributes.System) > 0)
            {
              continue;
            }
            lstSubPaths.AddRange(Directory.GetFiles(srcSource.Path));
          }
        }
        //if the source hasn't been loaded, then we treat it as if all
        // subpaths are present and have already been removed from
        // the children nodes, so we don't have to do anything
        if (srcSource.IsLoaded)
        {
          //if we find all the current folder's subpaths, and each subpath
          // has no children in the same source tree, then we can just copy
          // the current folder instead of copying each child individually
          var intFoundCount = 0;

          //so, for each subpath of the current folder...
          foreach (var strSubPath in lstSubPaths)
          {
            //...look through all the children nodes for the subpath...
            foreach (FileSystemTreeNode tndChild in p_tndNode.Nodes)
            {
              //...if we find the subpath...
              if (tndChild.Sources.Contains(strSubPath))
              {
                //...and the node containing the subpath has no children
                // containing anything in the same source tree...
                var booFound = false;
                foreach (FileSystemTreeNode tndSubNode in tndChild.Nodes)
                {
                  foreach (string strSubSource in tndSubNode.Sources)
                  {
                    if (strSubSource.StartsWith(strSubPath))
                    {
                      booFound = true;
                      break;
                    }
                  }
                  if (booFound)
                  {
                    break;
                  }
                }
                //...then we found the subpath.
                // if the node containing the subpath had had children containing
                // something in the same source tree, that would imply we aren't
                // copying all the current folder's descendants, so we would have to
                // copy each descendent individually, instead of just copying this folder
                if (!booFound)
                {
                  intFoundCount++;
                }
                break;
              }
            }
          }
          //if we found all the subpaths...
          if (intFoundCount == lstSubPaths.Count)
          {
            //...then remove the subpaths, so we just copy the
            // current folder instead of copying each child individually
            foreach (var strSubPath in lstSubPaths)
            {
              for (var i = p_tndNode.Nodes.Count - 1; i >= 0; i--)
              {
                var tndNode = (FileSystemTreeNode) p_tndNode.Nodes[i];
                if (tndNode.Sources.Contains(strSubPath))
                {
                  //if we are removing the last source, and there are no
                  // children nodes (implying this node isn't needed in
                  // another source tree), then prune this node away...
                  if ((tndNode.Nodes.Count == 0) && (tndNode.Sources.Count <= 1))
                  {
                    p_tndNode.Nodes.RemoveAt(i);
                  }
                  else //...otherwise just remove the source
                  {
                    tndNode.Sources.Remove(strSubPath);
                  }
                  break;
                }
              }
            }
          }
          else
          {
            //...else if we only found some of the subpaths
            // then remove the current folder from the sources so
            // it doesn't get copied: the current folder will be
            // created when the subpaths are copied...
            //...else if we found no subpaths then we remove the current folder
            // to prune empty folders
            p_tndNode.Sources.RemoveAt(j);
          }
        }
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
      if (!e.Data.GetDataPresent(typeof (List<SourceFileTree.SourceFileSystemDragData>)))
      {
        return;
      }
      e.Effect = DragDropEffects.Copy;
      var tndFolder =
        (FileSystemTreeNode) tvwFomod.GetNodeAt(tvwFomod.PointToClient(new Point(e.X, e.Y)));
      if ((tndFolder != null) && tndFolder.IsDirectory)
      {
        tvwFomod.SelectedNode = tndFolder;
      }
      else
      {
        tvwFomod.SelectedNode = null;
      }
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
      if (!e.Data.GetDataPresent(typeof (List<SourceFileTree.SourceFileSystemDragData>)))
      {
        return;
      }
      var crsOldCursor = Cursor;
      Cursor = Cursors.WaitCursor;
      tvwFomod.BeginUpdate();
      var tndFolder =
        (FileSystemTreeNode) tvwFomod.GetNodeAt(tvwFomod.PointToClient(new Point(e.X, e.Y)));
      var lstPaths =
        ((List<SourceFileTree.SourceFileSystemDragData>)
          e.Data.GetData(typeof (List<SourceFileTree.SourceFileSystemDragData>)));
      if (tndFolder != null)
      {
        if (!tndFolder.IsDirectory)
        {
          tndFolder = tndFolder.Parent;
        }
        if (tndFolder != null)
        {
          for (var i = 0; i < lstPaths.Count; i++)
          {
            addFomodFile(tndFolder, lstPaths[i].Path);
          }
          tndFolder.Expand();
        }
        else
        {
          for (var i = 0; i < lstPaths.Count; i++)
          {
            addFomodFile(null, lstPaths[i].Path);
          }
        }
      }
      else
      {
        for (var i = 0; i < lstPaths.Count; i++)
        {
          addFomodFile(null, lstPaths[i].Path);
        }
      }
      tvwFomod.EndUpdate();
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
        {
          fsiInfo = new DirectoryInfo(p_strFile);
        }
        else if (File.Exists(p_strFile))
        {
          fsiInfo = new FileInfo(p_strFile);
        }
        else
        {
          return null;
        }
        if ((fsiInfo.Attributes & FileAttributes.System) > 0)
        {
          return null;
        }
      }

      var strFileName = Path.GetFileName(p_strFile);
      FileSystemTreeNode tndFile = null;
      var tncSiblings = (p_tndRoot == null) ? tvwFomod.Nodes : p_tndRoot.Nodes;
      if (tncSiblings.ContainsKey(strFileName.ToLowerInvariant()))
      {
        tndFile = (FileSystemTreeNode) tncSiblings[strFileName.ToLowerInvariant()];
        tndFile.AddSource(p_strFile, false);
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
          PopulateNodeWithChildren(tndFile);
        }
      }
      else
      {
        tndFile.Sources[p_strFile].IsLoaded = true;
        var strExtension = Path.GetExtension(p_strFile).ToLowerInvariant();
        if (!imlIcons.Images.ContainsKey(strExtension))
        {
          var strIconPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()) + strExtension;
          File.CreateText(strIconPath).Close();
          imlIcons.Images.Add(strExtension, Icon.ExtractAssociatedIcon(strIconPath));
          File.Delete(strIconPath);
        }
        tndFile.ImageKey = strExtension;
        tndFile.SelectedImageKey = strExtension;
      }
      return tndFile;
    }

    /// <summary>
    /// Populates the given node with its children.
    /// </summary>
    /// <param name="p_tndNode">The node to populate with children.</param>
    protected void PopulateNodeWithChildren(FileSystemTreeNode p_tndNode)
    {
      if (!p_tndNode.IsDirectory)
      {
        return;
      }
      foreach (var srcSource in p_tndNode.Sources)
      {
        if (srcSource.IsLoaded)
        {
          continue;
        }
        var strSource = srcSource.Path;
        srcSource.IsLoaded = true;
        if (strSource.StartsWith(Archive.ARCHIVE_PREFIX))
        {
          var kvpPath = Archive.ParseArchivePath(strSource);
          var arcArchive = new Archive(kvpPath.Key);
          var strFolders = arcArchive.GetDirectories(kvpPath.Value);
          for (var i = 0; i < strFolders.Length; i++)
          {
            addFomodFile(p_tndNode, Archive.GenerateArchivePath(kvpPath.Key, strFolders[i]));
          }
          var strFiles = arcArchive.GetFiles(kvpPath.Value);
          for (var i = 0; i < strFiles.Length; i++)
          {
            addFomodFile(p_tndNode, Archive.GenerateArchivePath(kvpPath.Key, strFiles[i]));
          }
        }
        else if (!strSource.StartsWith(FileSystemTreeNode.NEW_PREFIX))
        {
          var strFolders = Directory.GetDirectories(strSource);
          for (var i = 0; i < strFolders.Length; i++)
          {
            addFomodFile(p_tndNode, strFolders[i]);
          }
          var strFiles = Directory.GetFiles(strSource);
          for (var i = 0; i < strFiles.Length; i++)
          {
            addFomodFile(p_tndNode, strFiles[i]);
          }
        }
      }
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
      var crsOldCursor = Cursor;
      Cursor = Cursors.WaitCursor;
      foreach (FileSystemTreeNode tndFolder in e.Node.Nodes)
      {
        PopulateNodeWithChildren(tndFolder);
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
      if (e.Label == null)
      {
        e.CancelEdit = true;
      }
      else
      {
        e.Node.Name = e.Label.ToLowerInvariant();
        tvwFomod.BeginInvoke((MethodInvoker) (() =>
        {
          tvwFomod.Sort();
        }));
      }
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
      var tndNode = tvwFomod.SelectedNode;
      if (
        MessageBox.Show(this, "Are you sure you want to delete '" + tndNode.Text + "?'", "Confirm",
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
      {
        return;
      }
      if (tndNode.Parent == null)
      {
        tvwFomod.Nodes.Remove(tndNode);
      }
      else
      {
        tndNode.Parent.Nodes.Remove(tndNode);
      }
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
      {
        if ((item is ToolStripMenuItem) && (e.KeyData == ((ToolStripMenuItem) item).ShortcutKeys))
        {
          item.PerformClick();
        }
      }
    }

    /// <summary>
    /// Handles the <see cref="Control.Click"/> event of the new folder context menu item.
    /// </summary>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> that describes the event arguments.</param>
    private void newFolderToolStripMenuItem_Click(object sender, EventArgs e)
    {
      TreeNode tndNode = null;
      //sometimes the selected node is set even when the no-node-selected context
      // menu was display, so check which menu we are comming from in order to
      // determine if we should be creating the new folder as a child of an existing
      // nide
      if (sender == nodeNewFolderToolStripMenuItem)
      {
        tndNode = tvwFomod.SelectedNode;
      }
      var tndNewNode = addFomodFile(tndNode, FileSystemTreeNode.NEW_PREFIX + "//New Folder");
      if (tndNode != null)
      {
        tndNode.Expand();
      }
      //make sure the node being edited is the only one selected
      tvwFomod.SelectedNode = null;
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
        var tndFolder = (FileSystemTreeNode) tvwFomod.GetNodeAt(e.Location);
        tvwFomod.SelectedNode = tndFolder;
      }
    }

    /// <summary>
    /// Handles the <see cref="MenuStrip.Opening"/> event of the node context menu.
    /// </summary>
    /// <remarks>
    /// This enables/disables the new folder menu item dependent upon whether the clicked
    /// node is a folder.
    /// </remarks>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">A <see cref="CancelEventArgs"/> that describes the event arguments.</param>
    private void cmsFomodNode_Opening(object sender, CancelEventArgs e)
    {
      nodeNewFolderToolStripMenuItem.Enabled = ((FileSystemTreeNode) tvwFomod.SelectedNode).IsDirectory;
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
      var strPatterns =
        p_strPattern.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
                    .Split(Path.DirectorySeparatorChar);
      var queDirectories = new Queue<string>();
      for (var i = 0; i < strPatterns.Length - 1; i++)
      {
        queDirectories.Enqueue(strPatterns[i].ToLowerInvariant());
      }
      var strFileNamePattern = (strPatterns.Length > 0) ? strPatterns[strPatterns.Length - 1] : "*";
      strFileNamePattern = strFileNamePattern.Replace(".", @"\.").Replace("*", @".*");
      var rgxFileNamePattern = new Regex("^" + strFileNamePattern + "$", RegexOptions.IgnoreCase);
      var lstMatches = new List<KeyValuePair<string, string>>();
      var intOriginalDepth = queDirectories.Count;
      foreach (FileSystemTreeNode tndFolder in tvwFomod.Nodes)
      {
        lstMatches.AddRange(FindFomodFiles(tndFolder, queDirectories, rgxFileNamePattern));
        if (intOriginalDepth != queDirectories.Count)
        {
          break;
        }
      }
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
    private List<KeyValuePair<string, string>> FindFomodFiles(FileSystemTreeNode p_tndRoot,
                                                              Queue<string> p_queDirectories, Regex p_rgxFileNamePattern)
    {
      var lstMatches = new List<KeyValuePair<string, string>>();
      if (p_tndRoot.IsDirectory && ((p_queDirectories.Count > 0) && p_tndRoot.Name.Equals(p_queDirectories.Peek())))
      {
        p_queDirectories.Dequeue();
        PopulateNodeWithChildren(p_tndRoot);
        var intOriginalDepth = p_queDirectories.Count;
        foreach (FileSystemTreeNode tndNode in p_tndRoot.Nodes)
        {
          lstMatches.AddRange(FindFomodFiles(tndNode, p_queDirectories, p_rgxFileNamePattern));
          if (intOriginalDepth != p_queDirectories.Count)
          {
            break;
          }
        }
      }
      else if ((p_queDirectories.Count == 0) && p_rgxFileNamePattern.IsMatch(p_tndRoot.Name))
      {
        lstMatches.Add(new KeyValuePair<string, string>(p_tndRoot.FullPath, p_tndRoot.LastSource));
      }
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