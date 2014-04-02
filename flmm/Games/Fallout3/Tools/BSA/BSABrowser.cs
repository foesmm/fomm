using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Fomm.Properties;
using Fomm.SharpZipLib.Zip.Compression;

namespace Fomm.Games.Fallout3.Tools.BSA
{
  internal partial class BSABrowser : Form
  {
    internal BSABrowser()
    {
      InitializeComponent();
      Icon = Resources.fomm02;
      var path = Properties.Settings.Default.fallout3LastBSAUnpackPath;
      if (!String.IsNullOrEmpty(path))
      {
        SaveAllDialog.SelectedPath = path;
      }
      OpenBSA.InitialDirectory = Program.GameMode.PluginsPath;

      Properties.Settings.Default.windowPositions.GetWindowPosition("BSABrowser", this);
    }

    private void BSABrowser_Load(object sender, EventArgs e)
    {
      var tmp = Properties.Settings.Default.fallout3BSABrowserPanelSplit;
      splitContainer1.SplitterDistance = Math.Max(splitContainer1.Panel1MinSize + 1,
                                                  Math.Min(splitContainer1.Width - (splitContainer1.Panel2MinSize + 1),
                                                           tmp));
    }

    internal BSABrowser(string BSAPath)
      : this()
    {
      OpenArchive(BSAPath);
    }

    private class BSAFileEntry
    {
      private static readonly Inflater inf =
        new Inflater();

      internal readonly bool Compressed;
      private string fileName;
      private string lowername;

      internal string FileName
      {
        get
        {
          return fileName;
        }
        set
        {
          if (value == null)
          {
            return;
          }
          fileName = value;
          //lowername=Folder.ToLower()+"\\"+fileName.ToLower();
          lowername = Path.Combine(Folder.ToLower(), fileName.ToLower());
        }
      }

      internal string LowerName
      {
        get
        {
          return lowername;
        }
      }

      internal readonly string Folder;
      internal readonly uint Offset;
      internal readonly uint Size;
      internal readonly uint RealSize;

      internal BSAFileEntry(bool compressed, string folder, uint offset, uint size)
      {
        Compressed = compressed;
        Folder = folder;
        Offset = offset;
        Size = size;
      }

      internal BSAFileEntry(string path, uint offset, uint size)
      {
        Folder = Path.GetDirectoryName(path);
        FileName = Path.GetFileName(path);
        Offset = offset;
        Size = size;
      }

      internal BSAFileEntry(string path, uint offset, uint size, uint realSize)
      {
        Folder = Path.GetDirectoryName(path);
        FileName = Path.GetFileName(path);
        Offset = offset;
        Size = size;
        RealSize = realSize;
        Compressed = realSize != 0;
      }

      internal void Extract(string path, bool UseFolderName, BinaryReader br, bool SkipName)
      {
        if (UseFolderName)
        {
          path += "\\" + Folder + "\\" + FileName;
        }
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
          Directory.CreateDirectory(Path.GetDirectoryName(path));
        }
        var fs = File.Create(path);
        br.BaseStream.Position = Offset;
        if (SkipName)
        {
          br.BaseStream.Position += br.ReadByte() + 1;
        }
        if (!Compressed)
        {
          var bytes = new byte[Size];
          br.Read(bytes, 0, (int) Size);
          fs.Write(bytes, 0, (int) Size);
        }
        else
        {
          byte[] uncompressed = RealSize == 0 ? new byte[br.ReadUInt32()] : new byte[RealSize];
          var compressed = new byte[Size - 4];
          br.Read(compressed, 0, (int) (Size - 4));
          inf.Reset();
          inf.SetInput(compressed);
          inf.Inflate(uncompressed);
          fs.Write(uncompressed, 0, uncompressed.Length);
        }
        fs.Close();
      }
    }

    private bool ArchiveOpen;
    private BinaryReader br;
    private bool Compressed;
    private bool ContainsFileNameBlobs;
    private BSAFileEntry[] Files;
    private ListViewItem[] lvItems;
    private ListViewItem[] lvAllItems;

    private enum BSASortOrder
    {
      FolderName,
      FileName,
      FileSize,
      Offset,
      FileType
    }

    private class BSASorter : IComparer
    {
      internal static BSASortOrder order = 0;

      public int Compare(object a, object b)
      {
        var fa = (BSAFileEntry) ((ListViewItem) a).Tag;
        var fb = (BSAFileEntry) ((ListViewItem) b).Tag;
        switch (order)
        {
          case BSASortOrder.FolderName:
            return string.Compare(fa.LowerName, fb.LowerName);
          case BSASortOrder.FileName:
            return string.Compare(fa.FileName, fb.FileName);
          case BSASortOrder.FileSize:
            return fa.Size.CompareTo(fb.Size);
          case BSASortOrder.Offset:
            return fa.Offset.CompareTo(fb.Offset);
          case BSASortOrder.FileType:
            return string.Compare(Path.GetExtension(fa.FileName), Path.GetExtension(fb.FileName));
          default:
            return 0;
        }
      }
    }

    private void CloseArchive()
    {
      lvAllItems = null;
      tvFolders.Nodes.Clear();
      ArchiveOpen = false;
      bOpen.Text = "Open";
      lvFiles.Items.Clear();
      Files = null;
      lvItems = null;
      bExtract.Enabled = false;
      bExtractAll.Enabled = false;
      bPreview.Enabled = false;
      if (br != null)
      {
        br.Close();
      }
      br = null;
    }

    private void OpenArchive(string path)
    {
      try
      {
        br = new BinaryReader(File.OpenRead(path), Encoding.Default);
        //if(Program.ReadCString(br)!="BSA") throw new fommException("File was not a valid BSA archive");
        var type = br.ReadUInt32();
        var sb = new StringBuilder(64);
        if (type != 0x00415342 && type != 0x00000100)
        {
          //Might be a fallout 2 dat
          br.BaseStream.Position = br.BaseStream.Length - 8;
          var TreeSize = br.ReadUInt32();
          var DataSize = br.ReadUInt32();
          if (DataSize != br.BaseStream.Length)
          {
            MessageBox.Show("File is not a valid bsa archive");
            br.Close();
            return;
          }
          br.BaseStream.Position = DataSize - TreeSize - 8;
          var FileCount = br.ReadInt32();
          Files = new BSAFileEntry[FileCount];
          for (var i = 0; i < FileCount; i++)
          {
            var fileLen = br.ReadInt32();
            for (var j = 0; j < fileLen; j++)
            {
              sb.Append(br.ReadChar());
            }
            var comp = br.ReadByte();
            var realSize = br.ReadUInt32();
            var compSize = br.ReadUInt32();
            var offset = br.ReadUInt32();
            if (sb[0] == '\\')
            {
              sb.Remove(0, 1);
            }
            Files[i] = new BSAFileEntry(sb.ToString(), offset, compSize, comp == 0 ? 0 : realSize);
            sb.Length = 0;
          }
        }
        else if (type == 0x0100)
        {
          var hashoffset = br.ReadUInt32();
          var FileCount = br.ReadUInt32();
          Files = new BSAFileEntry[FileCount];

          var dataoffset = 12 + hashoffset + FileCount*8;
          var fnameOffset1 = 12 + FileCount*8;
          var fnameOffset2 = 12 + FileCount*12;

          for (var i = 0; i < FileCount; i++)
          {
            br.BaseStream.Position = 12 + i*8;
            var size = br.ReadUInt32();
            var offset = br.ReadUInt32() + dataoffset;
            br.BaseStream.Position = fnameOffset1 + i*4;
            br.BaseStream.Position = br.ReadInt32() + fnameOffset2;

            sb.Length = 0;
            while (true)
            {
              var b = br.ReadChar();
              if (b == '\0')
              {
                break;
              }
              sb.Append(b);
            }
            Files[i] = new BSAFileEntry(sb.ToString(), offset, size);
          }
        }
        else
        {
          var version = br.ReadInt32();
          if (version != 0x67 && version != 0x68)
          {
            if (MessageBox.Show("This BSA archive has an unknown version number.\n" +
                                "Attempt to open anyway?", "Warning", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
              br.Close();
              return;
            }
          }
          br.BaseStream.Position += 4;
          var flags = br.ReadUInt32();
          Compressed = (flags & 0x004) > 0;
          if ((flags & 0x100) > 0 && version == 0x68)
          {
            ContainsFileNameBlobs = true;
          }
          else
          {
            ContainsFileNameBlobs = false;
          }
          var FolderCount = br.ReadInt32();
          var FileCount = br.ReadInt32();
          br.BaseStream.Position += 12;
          Files = new BSAFileEntry[FileCount];
          var numfiles = new int[FolderCount];
          br.BaseStream.Position += 8;
          for (var i = 0; i < FolderCount; i++)
          {
            numfiles[i] = br.ReadInt32();
            br.BaseStream.Position += 12;
          }
          br.BaseStream.Position -= 8;
          var filecount = 0;
          for (var i = 0; i < FolderCount; i++)
          {
            int k = br.ReadByte();
            while (--k > 0)
            {
              sb.Append(br.ReadChar());
            }
            br.BaseStream.Position++;
            var folder = sb.ToString();
            for (var j = 0; j < numfiles[i]; j++)
            {
              br.BaseStream.Position += 8;
              var size = br.ReadUInt32();
              var comp = Compressed;
              if ((size & (1 << 30)) != 0)
              {
                comp = !comp;
                size ^= 1 << 30;
              }
              Files[filecount++] = new BSAFileEntry(comp, folder, br.ReadUInt32(), size);
            }
            sb.Length = 0;
          }
          for (var i = 0; i < FileCount; i++)
          {
            while (true)
            {
              var c = br.ReadChar();
              if (c == '\0')
              {
                break;
              }
              sb.Append(c);
            }
            Files[i].FileName = sb.ToString();
            sb.Length = 0;
          }
        }
      }
      catch (Exception ex)
      {
        if (br != null)
        {
          br.Close();
        }
        br = null;
        MessageBox.Show("An error occured trying to open the archive.\n" + ex.Message);
        return;
      }

      tvFolders.Nodes.Add(Path.GetFileNameWithoutExtension(path));
      tvFolders.Nodes[0].Nodes.Add("empty");
      if (tvFolders.Nodes[0].IsExpanded)
      {
        tvFolders.Nodes[0].Collapse();
      }
      tbSearch.Text = "";
      UpdateFileList();
      bOpen.Text = "Close";
      bExtract.Enabled = true;
      ArchiveOpen = true;
      bExtractAll.Enabled = true;
      bPreview.Enabled = true;
    }

    private void UpdateFileList()
    {
      lvFiles.BeginUpdate();
      lvItems = new ListViewItem[Files.Length];
      for (var i = 0; i < Files.Length; i++)
      {
        //ListViewItem lvi=new ListViewItem(Files[i].Folder+"\\"+Files[i].FileName);
        var lvi = new ListViewItem(Path.Combine(Files[i].Folder, Files[i].FileName));
        lvi.Tag = Files[i];
        lvi.ToolTipText = "File size: " + Files[i].Size + " bytes\nFile offset: " + Files[i].Offset + " bytes\n" +
                          (Files[i].Compressed ? "Compressed" : "Uncompressed");
        lvItems[i] = lvi;
      }
      lvFiles.Items.AddRange(lvItems);
      lvFiles.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
      lvFiles.EndUpdate();
    }

    private void bOpen_Click(object sender, EventArgs e)
    {
      if (ArchiveOpen)
      {
        CloseArchive();
      }
      else
      {
        if (OpenBSA.ShowDialog() == DialogResult.OK)
        {
          OpenArchive(OpenBSA.FileName);
          OpenBSA.InitialDirectory = Path.GetDirectoryName(OpenBSA.FileName);
        }
      }
    }

    private void bExtract_Click(object sender, EventArgs e)
    {
      if (lvFiles.SelectedItems.Count == 0)
      {
        return;
      }
      if (lvFiles.SelectedItems.Count == 1)
      {
        var fe = (BSAFileEntry) lvFiles.SelectedItems[0].Tag;
        SaveSingleDialog.FileName = fe.FileName;
        if (SaveSingleDialog.ShowDialog() == DialogResult.OK)
        {
          fe.Extract(SaveSingleDialog.FileName, false, br, ContainsFileNameBlobs);
          SaveSingleDialog.InitialDirectory = Path.GetDirectoryName(SaveSingleDialog.FileName);
        }
      }
      else
      {
        if (SaveAllDialog.ShowDialog() == DialogResult.OK)
        {
          var pf = new ProgressForm("Unpacking archive", false);
          pf.EnableCancel();
          pf.SetProgressRange(lvFiles.SelectedItems.Count);
          pf.Show();
          var count = 0;
          try
          {
            foreach (ListViewItem lvi in lvFiles.SelectedItems)
            {
              var fe = (BSAFileEntry) lvi.Tag;
              fe.Extract(SaveAllDialog.SelectedPath, true, br, ContainsFileNameBlobs);
              pf.UpdateProgress(count++);
              Application.DoEvents();
            }
          }
          catch (fommException)
          {
            MessageBox.Show("Operation cancelled", "Message");
          }
          catch (Exception ex)
          {
            MessageBox.Show(ex.Message, "Error");
          }
          pf.Unblock();
          pf.Close();
        }
      }
    }

    private void bExtractAll_Click(object sender, EventArgs e)
    {
      if (SaveAllDialog.ShowDialog() == DialogResult.OK)
      {
        var pf = new ProgressForm("Unpacking archive", false);
        pf.EnableCancel();
        pf.SetProgressRange(Files.Length);
        pf.Show();
        var count = 0;
        try
        {
          foreach (var fe in Files)
          {
            fe.Extract(SaveAllDialog.SelectedPath, true, br, ContainsFileNameBlobs);
            pf.UpdateProgress(count++);
            Application.DoEvents();
          }
        }
        catch (fommCancelException)
        {
          MessageBox.Show("Operation cancelled", "Message");
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.Message, "Error");
        }
        pf.Unblock();
        pf.Close();
      }
    }

    private void cmbSortOrder_SelectedIndexChanged(object sender, EventArgs e)
    {
      BSASorter.order = (BSASortOrder) cmbSortOrder.SelectedIndex;
    }

    private void cmbSortOrder_KeyPress(object sender, KeyPressEventArgs e)
    {
      e.Handled = true;
    }

    private void bSort_Click(object sender, EventArgs e)
    {
      lvFiles.ListViewItemSorter = new BSASorter();
      lvFiles.ListViewItemSorter = null;
    }

    private void BSABrowser_FormClosing(object sender, FormClosingEventArgs e)
    {
      if (ArchiveOpen)
      {
        CloseArchive();
      }
      Properties.Settings.Default.windowPositions.SetWindowPosition("BSABrowser", this);
      Properties.Settings.Default.fallout3LastBSAUnpackPath = SaveAllDialog.SelectedPath;
      Properties.Settings.Default.fallout3BSABrowserPanelSplit = splitContainer1.SplitterDistance;
      Properties.Settings.Default.Save();
    }

    private void bPreview_Click(object sender, EventArgs e)
    {
      if (lvFiles.SelectedItems.Count == 0)
      {
        return;
      }
      if (lvFiles.SelectedItems.Count == 1)
      {
        var fe = (BSAFileEntry) lvFiles.SelectedItems[0].Tag;
        switch (Path.GetExtension(fe.LowerName))
        {
            /*case ".nif":
            MessageBox.Show("Viewing of nif's disabled as their format differs from oblivion");
            return;
          case ".dds":
          case ".tga":
          case ".bmp":
          case ".jpg":
            System.Diagnostics.Process.Start("obmm\\NifViewer.exe", fe.LowerName);
            break;*/
          case ".lst":
          case ".txt":
          case ".xml":
            var path = Program.CreateTempDirectory();
            fe.Extract(Path.Combine(path, fe.FileName), false, br, ContainsFileNameBlobs);
            Process.Start(Path.Combine(path, fe.FileName));
            break;
          default:
            MessageBox.Show("Filetype not supported.\n" +
                            "Currently only txt or xml files can be previewed", "Error");
            break;
        }
      }
      else
      {
        MessageBox.Show("Can only preview one file at a time", "Error");
      }
    }

    private void lvFiles_ItemDrag(object sender, ItemDragEventArgs e)
    {
      if (lvFiles.SelectedItems.Count != 1)
      {
        return;
      }
      var fe = (BSAFileEntry) lvFiles.SelectedItems[0].Tag;
      var path = Path.Combine(Program.CreateTempDirectory(), fe.FileName);
      fe.Extract(path, false, br, ContainsFileNameBlobs);

      var obj = new DataObject();
      var sc = new StringCollection();
      sc.Add(path);
      obj.SetFileDropList(sc);
      lvFiles.DoDragDrop(obj, DragDropEffects.Move);
    }

    private void tbSearch_TextChanged(object sender, EventArgs e)
    {
      if (!ArchiveOpen)
      {
        return;
      }
      var str = tbSearch.Text;
      if (cbRegex.Checked && str.Length > 0)
      {
        Regex regex;
        try
        {
          regex = new Regex(str, RegexOptions.Singleline);
        }
        catch
        {
          return;
        }
        lvFiles.BeginUpdate();
        lvFiles.Items.Clear();
        var lvis =
          new List<ListViewItem>(Files.Length);
        for (var i = 0; i < lvItems.Length; i++)
        {
          if (regex.IsMatch(lvItems[i].Text))
          {
            lvis.Add(lvItems[i]);
          }
        }
        lvFiles.Items.AddRange(lvis.ToArray());
        lvFiles.EndUpdate();
      }
      else
      {
        str = str.ToLowerInvariant();
        lvFiles.BeginUpdate();
        lvFiles.Items.Clear();
        if (str.Length == 0)
        {
          lvFiles.Items.AddRange(lvItems);
        }
        else
        {
          var lvis =
            new List<ListViewItem>(Files.Length);
          for (var i = 0; i < lvItems.Length; i++)
          {
            if (lvItems[i].Text.Contains(str))
            {
              lvis.Add(lvItems[i]);
            }
          }
          lvFiles.Items.AddRange(lvis.ToArray());
        }
        lvFiles.EndUpdate();
      }
    }

    private void tvFolders_BeforeExpand(object sender, TreeViewCancelEventArgs e)
    {
      if (!ArchiveOpen || lvAllItems != null)
      {
        return;
      }
      tvFolders.Nodes[0].Nodes.Clear();
      var nodes =
        new Dictionary<string, TreeNode>();
      lvAllItems = (ListViewItem[]) lvItems.Clone();
      foreach (var lvi in lvAllItems)
      {
        var path = Path.GetDirectoryName(lvi.Text);
        if (path == string.Empty || nodes.ContainsKey(path))
        {
          continue;
        }
        var dirs = path.Split('\\');
        for (var i = 0; i < dirs.Length; i++)
        {
          var newpath = string.Join("\\", dirs, 0, i + 1);
          if (!nodes.ContainsKey(newpath))
          {
            var tn = new TreeNode(dirs[i]);
            tn.Tag = newpath;
            if (i == 0)
            {
              tvFolders.Nodes[0].Nodes.Add(tn);
            }
            else
            {
              nodes[path].Nodes.Add(tn);
            }
            nodes.Add(newpath, tn);
          }
          path = newpath;
        }
      }
    }

    private void tvFolders_AfterSelect(object sender, TreeViewEventArgs e)
    {
      if (lvAllItems == null)
      {
        return;
      }
      var s = e.Node.Tag as string;
      if (s == null)
      {
        lvItems = lvAllItems;
      }
      else
      {
        var lvis =
          new List<ListViewItem>(lvAllItems.Length);
        foreach (var lvi in lvAllItems)
        {
          if (lvi.Text.StartsWith(s))
          {
            lvis.Add(lvi);
          }
        }
        lvItems = lvis.ToArray();
      }
      tbSearch_TextChanged(null, null);
    }
  }
}