using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Fomm.SharpZipLib.Zip.Compression;

namespace Fomm.Games.Fallout3.Tools.InstallTweaker
{
  internal static class BsaTrimmer
  {
    private static int shrunkcount;

    private static void Commit(BinaryWriter bw, long offset, byte[] data, long offset2, int add, bool parse)
    {
      var newsize = 0;
      IntPtr pdata;
      if (parse)
      {
        pdata = NativeMethods.ddsShrink(data, data.Length, out newsize);
      }
      else
      {
        pdata = IntPtr.Zero;
      }
      byte[] newdata;
      if (pdata == IntPtr.Zero)
      {
        newdata = data;
      }
      else
      {
        shrunkcount++;
        newdata = new byte[newsize];
        Marshal.Copy(pdata, newdata, 0, newsize);
      }
      bw.Write(newdata);
      bw.BaseStream.Position = offset;
      bw.Write(newdata.Length + add);
      bw.Write((int) offset2);
      bw.BaseStream.Position = bw.BaseStream.Length;
    }

    public static void Trim(IntPtr hwnd, string In, string Out, ReportProgressDelegate del)
    {
      NativeMethods.ddsInit(hwnd);
      var br = new BinaryReader(File.OpenRead(In), Encoding.Default);
      var bw = new BinaryWriter(File.Create(Out), Encoding.Default);
      var sb = new StringBuilder(64);
      var inf = new Inflater();
      bool Compressed, SkipName;

      if (br.ReadInt32() != 0x00415342)
      {
        throw new Exception("Invalid bsa");
      }
      var version = br.ReadUInt32();
      bw.Write((int) 0x00415342);
      bw.Write(version);
      bw.Write(br.ReadInt32());
      var flags = br.ReadUInt32();
      if ((flags & 0x004) > 0)
      {
        Compressed = true;
        flags ^= 0x4;
      }
      else
      {
        Compressed = false;
      }
      if ((flags & 0x100) > 0 && version == 0x68)
      {
        SkipName = true;
      }
      else
      {
        SkipName = false;
      }
      flags ^= 0x2;
      var FolderCount = br.ReadInt32();
      var FileCount = br.ReadInt32();
      bw.Write(flags);

      bw.Write(FolderCount);
      bw.Write(FileCount);
      bw.Write(br.ReadInt32());
      bw.Write(br.ReadInt32());
      bw.Write(br.ReadInt32());

      var folderFileCount = new int[FolderCount];
      for (var i = 0; i < FolderCount; i++)
      {
        bw.Write(br.ReadInt64());
        folderFileCount[i] = br.ReadInt32();
        bw.Write(folderFileCount[i]);
        bw.Write(br.ReadInt32());
      }
      var fileLengths = new int[FileCount];
      var offsetOffsets = new long[FileCount];
      var fileOffsets = new uint[FileCount];
      var parsefiles = new bool[FileCount];
      var file = 0;
      for (var i = 0; i < FolderCount; i++)
      {
        var len = br.ReadByte();
        bw.Write(len);
        sb.Length = 0;
        while (--len > 0)
        {
          var c = br.ReadChar();
          sb.Append(c);
          bw.Write(c);
        }
        br.ReadByte();
        bw.Write((byte) 0);
        var parse = true;
        if (sb.ToString().StartsWith("textures\\interface\\"))
        {
          parse = false;
        }

        for (var j = 0; j < folderFileCount[i]; j++)
        {
          bw.Write(br.ReadUInt64());
          offsetOffsets[file] = br.BaseStream.Position;
          fileLengths[file] = br.ReadInt32();
          bw.Write(fileLengths[file]);
          fileOffsets[file] = br.ReadUInt32();
          bw.Write(fileOffsets[file]);
          parsefiles[file] = parse;
          file++;
        }
      }

      for (var i = 0; i < FileCount; i++)
      {
        sb.Length = 0;
        while (true)
        {
          var c = (char) br.ReadByte();
          //bw.Write(c);
          if (c == '\0')
          {
            break;
          }
          sb.Append(c);
        }
        if (!sb.ToString().EndsWith(".dds", StringComparison.OrdinalIgnoreCase))
        {
          parsefiles[i] = false;
        }
      }

      var count = 0;
      for (var i = 0; i < FileCount; i++)
      {
        if ((i%100) == 0)
        {
          del("Processing file " + i + " of " + FileCount);
        }
        br.BaseStream.Position = fileOffsets[i];
        var offset = bw.BaseStream.Position;
        var add = 0;
        if (SkipName)
        {
          var len = br.ReadByte();
          bw.Write(len);
          bw.Write(br.ReadBytes(len + 1));
          add = len + 2;
        }
        var compressed2 = Compressed;
        if ((fileLengths[i] & (1 << 30)) != 0)
        {
          compressed2 = !compressed2;
          fileLengths[i] ^= (1 << 30);
        }
        if (!compressed2)
        {
          var bytes = new byte[fileLengths[i]];
          br.Read(bytes, 0, fileLengths[i]);
          Commit(bw, offsetOffsets[i], bytes, offset, add, parsefiles[i]);
        }
        else
        {
          count++;
          var uncompressed = new byte[br.ReadUInt32()];
          var compressed = new byte[fileLengths[i] - 4];
          br.Read(compressed, 0, fileLengths[i] - 4);
          inf.Reset();
          inf.SetInput(compressed);
          inf.Inflate(uncompressed);
          Commit(bw, offsetOffsets[i], uncompressed, offset, add, parsefiles[i]);
        }
      }

      br.Close();
      bw.Close();
      NativeMethods.ddsClose();
    }
  }
}