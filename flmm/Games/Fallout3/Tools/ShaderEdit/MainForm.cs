using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using Fomm.Properties;
using Encoding = System.Text.Encoding;

namespace Fomm.Games.Fallout3.Tools.ShaderEdit
{
  partial class MainForm : Form
  {
    private class Shader
    {
      internal string name;
      internal char[] name2;
      internal byte[] data;
    }

    private uint unknown;
    private readonly List<Shader> shaders = new List<Shader>();
    //private bool ChangedShader=false;
    private bool ChangedFile;
    private int Editing = -1;
    private string FileName = "";
    private HLSLImporter HLSLImporterForm = new HLSLImporter();

    public MainForm()
    {
      InitializeComponent();
      Icon = Resources.fomm02;
    }

    public MainForm(string path) : this()
    {
      Open(path);
    }

    private void cmbShaderSelect_KeyPress(object sender, KeyPressEventArgs e)
    {
      e.Handled = true;
    }

    private void Open(string path)
    {
      FileName = Path.GetFileName(path);
      Text = "SDP Editor (" + FileName + ")";
      var br = new BinaryReader(File.OpenRead(path), Encoding.Default);
      unknown = br.ReadUInt32();
      var num = br.ReadInt32();
      br.ReadInt32();
      for (var i = 0; i < num; i++)
      {
        var s = new Shader();
        var name = br.ReadChars(0x100);
        s.name = "";
        s.name2 = name;
        for (var i2 = 0; i2 < 100; i2++)
        {
          if (name[i2] == '\0')
          {
            break;
          }
          s.name += name[i2];
        }
        var size = br.ReadInt32();
        s.data = br.ReadBytes(size);
        shaders.Add(s);
        cmbShaderSelect.Items.Add(s.name);
      }
      br.Close();
      bOpen.Enabled = false;
      bClose.Enabled = true;
      cmbShaderSelect.Enabled = true;
      bSave.Enabled = true;
    }

    private void bOpen_Click(object sender, EventArgs e)
    {
      openFileDialog1.Filter = "Fallout 3 shader package (*.sdp)|*.sdp";
      openFileDialog1.Title = "Select Shader package to edit";
      if (openFileDialog1.ShowDialog() != DialogResult.OK)
      {
        return;
      }
      Open(openFileDialog1.FileName);
    }

    private unsafe void cmbShaderSelect_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (cmbShaderSelect.SelectedIndex == Editing)
      {
        return;
      }
      if (tbEdit.Modified)
      {
        if (!Compile())
        {
          if (MessageBox.Show("The current shader could not be compiled. Do you want to discard your changes?",
                              "Question",
                              MessageBoxButtons.YesNo) != DialogResult.Yes)
          {
            return;
          }
        }
      }
      Editing = cmbShaderSelect.SelectedIndex;
      var ptr = NativeMethods.Disasm(shaders[Editing].data, shaders[Editing].data.Length, 0);
      var text = new string(ptr);
      text = text.Replace("" + (char) 10, Environment.NewLine);
      tbEdit.Text = text;
      bCompile.Enabled = true;
      tbEdit.Enabled = true;
      bImport.Enabled = true;
    }

    private bool Save()
    {
      saveFileDialog1.Title = "Select file name to save as";
      saveFileDialog1.Filter = "Fallout 3 shader package (*.sdp)|*.sdp";
      if (tbEdit.Modified)
      {
        if (!Compile())
        {
          if (
            MessageBox.Show("One of your shaders did not compile. Do you want to save anyway?", "Question",
                            MessageBoxButtons.YesNo) !=
            DialogResult.Yes)
          {
            return false;
          }
        }
      }
      if (saveFileDialog1.ShowDialog() != DialogResult.OK)
      {
        return false;
      }
      var bw = new BinaryWriter(File.Create(saveFileDialog1.FileName), Encoding.Default);
      bw.Write(unknown);
      bw.Write(shaders.Count);
      var tsize = 0;
      foreach (var s in shaders)
      {
        tsize += 0x100 + 4 + s.data.Length;
      }
      bw.Write(tsize);
      foreach (var s in shaders)
      {
        bw.Write(s.name2);
        bw.Write(s.data.Length);
        bw.Write(s.data);
      }
      bw.Close();
      ChangedFile = false;
      return true;
    }

    private void bSave_Click(object sender, EventArgs e)
    {
      Save();
    }

    private unsafe bool Compile()
    {
      if (Editing == -1)
      {
        return true;
      }
      var b = new byte[tbEdit.Text.Length];
      for (var i = 0; i < tbEdit.Text.Length; i++)
      {
        b[i] = (byte) tbEdit.Text[i];
      }
      var data = NativeMethods.Asm(b, b.Length);
      var size = (data[3] << 24) + (data[2] << 16) + (data[1] << 8) + data[0];
      if (size == 0)
      {
        var error = "";
        for (var i = 4; i < 0x10000; i++)
        {
          if (data[i] == '\0')
          {
            break;
          }
          error += (char) data[i];
        }
        MessageBox.Show("Shader assembly failed: " + Environment.NewLine +
                        error.Replace("" + (char) 10, Environment.NewLine));
        return false;
      }

      shaders[Editing].data = new byte[size];
      var newdata = new byte[size];
      for (var i = 0; i < size; i++)
      {
        newdata[i] = data[i + 4];
      }
      Array.Copy(newdata, 0, shaders[Editing].data, 0, size);
      ChangedFile = true;
      tbEdit.Modified = false;
      Text = "SDP Editor (" + FileName + ")";
      return true;
    }

    private void bCompile_Click(object sender, EventArgs e)
    {
      Compile();
    }

    private void bClose_Click(object sender, EventArgs e)
    {
      if (tbEdit.Modified || ChangedFile)
      {
        switch (MessageBox.Show("Do you want to save your changes?", "Question", MessageBoxButtons.YesNoCancel))
        {
          case DialogResult.Yes:
            if (!Save())
            {
              return;
            }
            break;
          case DialogResult.No:
            break;
          default:
            return;
        }
      }
      unknown = 0;
      shaders.Clear();
      cmbShaderSelect.Enabled = false;
      bOpen.Enabled = true;
      bClose.Enabled = false;
      bSave.Enabled = false;
      bCompile.Enabled = false;
      ChangedFile = false;
      Editing = -1;
      cmbShaderSelect.Items.Clear();
      cmbShaderSelect.Text = "";
      tbEdit.Text = "";
      tbEdit.Enabled = false;
      bImport.Enabled = false;
    }

    private void bImport_Click(object sender, EventArgs e)
    {
      ImportMenu.Show(PointToScreen(bImport.Location));
    }

    private unsafe void importHLSLToolStripMenuItem_Click(object sender, EventArgs e)
    {
      openFileDialog1.Filter = "HLSL text file|*.*";
      openFileDialog1.Title = "Select HLSL file to import";
      if (openFileDialog1.ShowDialog() != DialogResult.OK)
      {
        return;
      }
      if (HLSLImporterForm.ShowDialog() != DialogResult.OK)
      {
        return;
      }
      var text = File.ReadAllText(openFileDialog1.FileName, Encoding.Default);
      var data = NativeMethods.Compile(text, text.Length, HLSLImporterForm.EntryPoint, HLSLImporterForm.Profile,
                                         HLSLImporterForm.Debug);
      var size = (data[3] << 24) + (data[2] << 16) + (data[1] << 8) + data[0];
      if (size == 0)
      {
        var error = "";
        for (var i = 4; i < 0x10000; i++)
        {
          if (data[i] == '\0')
          {
            break;
          }
          error += (char) data[i];
        }
        MessageBox.Show("Shader compilation failed: " + Environment.NewLine +
                        error.Replace("" + (char) 10, Environment.NewLine));
      }
      else
      {
        shaders[Editing].data = new byte[size];
        var newdata = new byte[size];
        for (var i = 0; i < size; i++)
        {
          newdata[i] = data[i + 4];
        }
        Array.Copy(newdata, 0, shaders[Editing].data, 0, size);
        ChangedFile = true;
        Text = "SDP Editor (" + FileName + ")";
        Editing = -1;
        cmbShaderSelect_SelectedIndexChanged(null, null);
      }
    }

    private unsafe void importBinaryToolStripMenuItem_Click(object sender, EventArgs e)
    {
      openFileDialog1.Filter = "Compiled shader (*.vso,*.pso)|*.vso;*.pso";
      openFileDialog1.Title = "Select HLSL file to import";
      if (openFileDialog1.ShowDialog() != DialogResult.OK)
      {
        return;
      }
      var b = File.ReadAllBytes(openFileDialog1.FileName);
      var result = NativeMethods.Disasm(b, b.Length, 0);
      if (new IntPtr(result) == IntPtr.Zero)
      {
        MessageBox.Show("An error occured during shader disassembly", "Error");
      }
      else
      {
        shaders[Editing].data = b;
        var text = new string(result);
        tbEdit.Text = text.Replace("" + (char) 10, Environment.NewLine);
      }
    }

    private void exportBinaryToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (tbEdit.Modified && !Compile())
      {
        MessageBox.Show("Could not assemble shader", "Error");
        return;
      }
      saveFileDialog1.Title = "Select file name to save as";
      saveFileDialog1.Filter = "Compiled shader (*.vso,*.pso)|*.vso;*.pso";
      if (saveFileDialog1.ShowDialog() != DialogResult.OK)
      {
        return;
      }
      File.WriteAllBytes(saveFileDialog1.FileName, shaders[Editing].data);
    }

    private void tbEdit_ModifiedChanged(object sender, EventArgs e)
    {
      if (tbEdit.Modified)
      {
        Text = "SDP Editor (" + FileName + " *)";
      }
      else
      {
        Text = "SDP Editor (" + FileName + ")";
      }
    }

    private void exportBinaryAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (tbEdit.Modified && !Compile())
      {
        MessageBox.Show("Could not assemble shader", "Error");
        return;
      }
      if (folderBrowserDialog1.ShowDialog() != DialogResult.OK)
      {
        return;
      }
      foreach (Shader shader in shaders)
      {
        var path = Path.Combine(folderBrowserDialog1.SelectedPath, shader.name);
        if (File.Exists(path))
        {
          MessageBox.Show("File " + path + " already exists, skipping.", "Error");
        }
        File.WriteAllBytes(path, shader.data);
      }
    }

    private void tbEdit_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
    {
      if (e.KeyCode == Keys.A && e.Control)
      {
        tbEdit.SelectAll();
      }
    }
  }
}