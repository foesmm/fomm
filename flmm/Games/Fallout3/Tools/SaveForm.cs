using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using Fomm.Properties;

namespace Fomm.Games.Fallout3.Tools
{
  internal partial class SaveForm : Form
  {
    private struct SaveFile
    {
      internal DateTime saved;
      internal string FileName;
      internal string Player;
      internal string Karma;
      internal int Level;
      internal string Location;
      internal string Playtime;

      //internal byte[] face;
      //internal int FaceOffset;

      internal byte[] ImageData;
      internal int ImageWidth;
      internal int ImageHeight;
      internal string[] plugins;
      private Bitmap image;

      internal Bitmap Image
      {
        get
        {
          if (image != null)
          {
            return image;
          }
          image = new Bitmap(ImageWidth, ImageHeight, PixelFormat.Format24bppRgb);
          var bd = image.LockBits(new Rectangle(0, 0, ImageWidth, ImageHeight), ImageLockMode.WriteOnly,
                                         PixelFormat.Format24bppRgb);
          Marshal.Copy(ImageData, 0, bd.Scan0, ImageData.Length);
          image.UnlockBits(bd);
          return image;
        }
      }
    }

    internal enum SaveSortOrder
    {
      Name,
      Player,
      Location,
      Date,
      FileSize
    }

    internal class SaveListSorter : IComparer
    {
      internal static SaveSortOrder order = SaveSortOrder.Name;

      public int Compare(object a, object b)
      {
        var sa = (SaveFile) ((ListViewItem) a).Tag;
        var sb = (SaveFile) ((ListViewItem) b).Tag;
        switch (order)
        {
          case SaveSortOrder.Name:
            return string.Compare(sa.FileName, sb.FileName);
          case SaveSortOrder.Player:
            return string.Compare(sa.Player, sb.Player);
          case SaveSortOrder.Location:
            return string.Compare(sa.Location, sb.Location);
          case SaveSortOrder.Date:
            return DateTime.Compare(sa.saved, sb.saved);
          case SaveSortOrder.FileSize:
            var sizea = (new FileInfo(Program.GameMode.SavesPath + sa.FileName)).Length;
            var sizeb = (new FileInfo(Program.GameMode.SavesPath + sb.FileName)).Length;
            if (sizea == sizeb)
            {
              return 0;
            }
            if (sizea > sizeb)
            {
              return -1;
            }
            return 1;
          default:
            return 0;
        }
      }
    }

    private readonly string[] aPlugins;
    private readonly string[] iPlugins;
    private readonly List<SaveFile> saves = new List<SaveFile>();

    internal SaveForm(string[] ActivePlugins, string[] InactivePlugins)
    {
      InitializeComponent();
      Array.Sort(ActivePlugins);
      Array.Sort(InactivePlugins);
      aPlugins = ActivePlugins;
      iPlugins = InactivePlugins;
      SaveImageList.Images.AddRange(new Image[]
      {
        Resources.GreenSquare,
        Resources.YellowSquare, Resources.YellowSquare
      });
      Icon = Resources.fomm02;
      cmbSort.SelectedIndex = 3;
      lvSaves.ListViewItemSorter = new SaveListSorter();
      foreach (var file in Directory.GetFiles(Program.GameMode.SavesPath))
      {
        BinaryReader br;
        SaveFile sf;
        try
        {
          br = new BinaryReader(File.OpenRead(file));
        }
        catch
        {
          continue;
        }
        try
        {
          if (br.BaseStream.Length < 12)
          {
            br.Close();
            continue;
          }
          var str = "";
          for (var i = 0; i < 11; i++)
          {
            str += (char) br.ReadByte();
          }
          if (str != "FO3SAVEGAME")
          {
            br.Close();
            continue;
          }
          sf = new SaveFile();
          sf.saved = (new FileInfo(file)).LastWriteTime;
          sf.FileName = Path.GetFileName(file);
          br.BaseStream.Position += 9;

          sf.ImageWidth = br.ReadInt32();
          br.ReadByte();
          sf.ImageHeight = br.ReadInt32();
          br.ReadByte();
          br.ReadInt32();
          br.ReadByte();
          var s = br.ReadInt16();
          br.ReadByte();
          for (var i = 0; i < s; i++)
          {
            sf.Player += (char) br.ReadByte();
          }
          br.ReadByte();
          s = br.ReadInt16();
          br.ReadByte();
          for (var i = 0; i < s; i++)
          {
            sf.Karma += (char) br.ReadByte();
          }
          br.ReadByte();
          sf.Level = br.ReadInt32();
          br.ReadByte();
          s = br.ReadInt16();
          br.ReadByte();
          for (var i = 0; i < s; i++)
          {
            sf.Location += (char) br.ReadByte();
          }
          br.ReadInt32(); //|<short>|
          for (var i = 0; i < 3; i++)
          {
            sf.Playtime += (char) br.ReadByte();
          }
          sf.Playtime += " hours, ";
          br.ReadByte();
          for (var i = 0; i < 2; i++)
          {
            sf.Playtime += (char) br.ReadByte();
          }
          sf.Playtime += " minutes and ";
          br.ReadByte();
          for (var i = 0; i < 2; i++)
          {
            sf.Playtime += (char) br.ReadByte();
          }
          sf.Playtime += " seconds";
          br.ReadByte();

          sf.ImageData = new byte[sf.ImageHeight*sf.ImageWidth*3];
          br.Read(sf.ImageData, 0, sf.ImageData.Length);
          //Flip the blue and red channels
          for (var i = 0; i < sf.ImageWidth*sf.ImageHeight; i++)
          {
            var temp = sf.ImageData[i*3];
            sf.ImageData[i*3] = sf.ImageData[i*3 + 2];
            sf.ImageData[i*3 + 2] = temp;
          }
          br.ReadByte();
          br.ReadInt32();
          sf.plugins = new string[br.ReadByte()];
          for (var i = 0; i < sf.plugins.Length; i++)
          {
            br.ReadByte();
            s = br.ReadInt16();
            br.ReadByte();
            sf.plugins[i] = "";
            for (var j = 0; j < s; j++)
            {
              sf.plugins[i] += (char) br.ReadByte();
            }
          }
        }
        catch (Exception)
        {
          continue;
        }
        finally
        {
          br.Close();
        }
        saves.Add(sf);
      }
      UpdateSaveList();
    }

    private void UpdateSaveList()
    {
      lvSaves.BeginUpdate();
      lvSaves.Clear();
      foreach (var sf in saves)
      {
        var lvi = new ListViewItem(sf.FileName);
        lvi.ToolTipText = "Player: " + sf.Player + "\nLevel: " + sf.Level + " (" + sf.Karma + ")\nLocation: " +
                          sf.Location + "\nPlay time: " + sf.Playtime +
                          "\nDate saved: " + sf.saved + "\nNumber of plugins: " +
                          sf.plugins.Length;
        lvi.Tag = sf;
        var worst = 0;
        foreach (var s in sf.plugins)
        {
          if (Array.BinarySearch(aPlugins, s) < 0)
          {
            if (Array.BinarySearch(iPlugins, s) < 0)
            {
              worst = 2;
              break;
            }
            worst = 1;
          }
        }
        lvi.ImageIndex = worst;
        lvSaves.Items.Add(lvi);
      }
      lvSaves.EndUpdate();
    }

    private void UpdatePluginList(string[] plugins)
    {
      lvPlugins.SuspendLayout();
      lvPlugins.Items.Clear();
      foreach (var s in plugins)
      {
        var lvi = new ListViewItem(s);
        if (Array.BinarySearch(aPlugins, s) >= 0)
        {
          lvi.ImageIndex = 0;
        }
        else if (Array.BinarySearch(iPlugins, s) >= 0)
        {
          lvi.ImageIndex = 1;
        }
        else
        {
          lvi.ImageIndex = 2;
        }
        lvPlugins.Items.Add(lvi);
      }
      lvPlugins.ResumeLayout();
    }

    private void lvSaves_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (lvSaves.SelectedItems.Count != 1)
      {
        return;
      }
      var sf = (SaveFile) lvSaves.SelectedItems[0].Tag;
      lName.Text = "Name: " + sf.Player + " (" + sf.Level + ": " + sf.Karma + ")";
      lLocation.Text = "Location: " + sf.Location;
      lDate.Text = "Date saved: " + sf.saved + " (" + sf.Playtime + ")";
      UpdatePluginList(sf.plugins);
      pictureBox1.Image = sf.Image;
    }

    private void cmbSort_KeyPress(object sender, KeyPressEventArgs e)
    {
      e.Handled = true;
    }

    private void cmbSort_SelectedIndexChanged(object sender, EventArgs e)
    {
      SaveListSorter.order = (SaveSortOrder) cmbSort.SelectedIndex;
      lvSaves.Sort();
    }

    private void bExport_Click(object sender, EventArgs e)
    {
      if (lvSaves.SelectedItems.Count != 1)
      {
        return;
      }
      var sf = (SaveFile) lvSaves.SelectedItems[0].Tag;
      var ofd = new SaveFileDialog();
      ofd.Filter = "Text file (*.txt)|*.txt";
      ofd.AddExtension = true;
      ofd.RestoreDirectory = true;
      if (ofd.ShowDialog() != DialogResult.OK)
      {
        return;
      }
      var sw = new StreamWriter(ofd.FileName);
      foreach (var plugin in sf.plugins)
      {
        sw.WriteLine("[X] " + plugin);
      }
      sw.Close();
    }
  }
}