using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BatchShaderProcessor
{
  public partial class Form1 : Form
  {
    private class Shader
    {
      internal string name;
      internal char[] name2;
      internal byte[] data;
    }

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi)]
    public static extern unsafe sbyte* Disasm(byte[] data, int len, byte Color);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi)]
    public static extern unsafe byte* Asm(byte[] data, int len);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi)]
    public static extern unsafe byte* Compile(string data, int len, string EntryPoint, string Profile, byte Debug);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi), SuppressUnmanagedCodeSecurity]
    public static extern void ddsInit(IntPtr hwnd);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi)]
    public static extern IntPtr ddsShrink(byte[] data, int len, out int oSize);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi)]
    public static extern void ddsClose();

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi), SuppressUnmanagedCodeSecurity]
    public static extern IntPtr ddsLoad(byte[] data, int len);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi), SuppressUnmanagedCodeSecurity]
    public static extern IntPtr ddsCreate(int width, int height);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi), SuppressUnmanagedCodeSecurity]
    public static extern void ddsBlt(IntPtr source, int sL, int sT, int sW, int sH, IntPtr dest, int dL, int dT, int dW,
                                     int dH);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi), SuppressUnmanagedCodeSecurity]
    public static extern IntPtr ddsSave(IntPtr ptr, int format, int mipmaps, out int length);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi), SuppressUnmanagedCodeSecurity]
    public static extern void ddsRelease(IntPtr tex);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi), SuppressUnmanagedCodeSecurity]
    public static extern void ddsGetSize(IntPtr tex, out int width, out int height);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi), SuppressUnmanagedCodeSecurity]
    public static extern IntPtr ddsLock(IntPtr tex, out int length, out int pitch);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi), SuppressUnmanagedCodeSecurity]
    public static extern void ddsUnlock(IntPtr tex);

    [DllImport("ShaderDisasm", CharSet = CharSet.Ansi), SuppressUnmanagedCodeSecurity]
    public static extern void ddsSetData(IntPtr tex, byte[] data, int len);

    public Form1()
    {
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      openFileDialog1.Filter = "Fallout 3 shader package (*.sdp)|*.sdp";
      openFileDialog1.Title = "Select Shader package to edit";
      if (openFileDialog1.ShowDialog() != DialogResult.OK || openFileDialog1.FileName == null)
      {
        return;
      }
      Open(openFileDialog1.FileName);
    }

    private void button2_Click(object sender, EventArgs e)
    {

    }

    private unsafe void Open(string filename)
    {
      if (folderBrowserDialog1.ShowDialog() != DialogResult.OK)
      {
        return;
      }
      var path = Path.GetFullPath(filename);
      var br = new BinaryReader(File.OpenRead(path), Encoding.Default);
      var unknown = br.ReadUInt32();
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

        var ptr = Disasm(s.data, s.data.Length, 0);
        var text = new string(ptr);
        text = text.Replace("" + (char)10, Environment.NewLine);

        var outfile = Path.Combine(folderBrowserDialog1.SelectedPath, s.name);
        File.WriteAllText(outfile, text);
      }
    }
  }
}
