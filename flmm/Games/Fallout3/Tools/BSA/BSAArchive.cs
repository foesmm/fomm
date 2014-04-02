using System;
using System.Text;
using Fomm.Properties;
using Fomm.SharpZipLib.Checksums;
using Fomm.SharpZipLib.Zip.Compression;
using StringList = System.Collections.Generic.List<string>;
using HashTable = System.Collections.Generic.Dictionary<ulong, Fomm.Games.Fallout3.Tools.BSA.BSAArchive.BSAFileInfo>;
using System.IO;

namespace Fomm.Games.Fallout3.Tools.BSA
{
  internal class BSAArchive
  {
    internal class BSALoadException : Exception
    {
    }

    [Flags]
    private enum FileFlags
    {
      [UsedImplicitly]
      Meshes = 1,
      [UsedImplicitly]
      Textures = 2
    }

    internal struct BSAFileInfo
    {
      internal readonly BSAArchive bsa;
      internal readonly int offset;
      internal readonly int size;
      internal readonly bool compressed;

      internal BSAFileInfo(BSAArchive _bsa, int _offset, int _size)
      {
        bsa = _bsa;
        offset = _offset;
        size = _size;

        if ((size & (1 << 30)) != 0)
        {
          size ^= 1 << 30;
          compressed = !bsa.defaultCompressed;
        }
        else
        {
          compressed = bsa.defaultCompressed;
        }
      }

      internal byte[] GetRawData()
      {
        bsa.br.BaseStream.Seek(offset, SeekOrigin.Begin);
        if (bsa.SkipNames)
        {
          bsa.br.BaseStream.Position += bsa.br.ReadByte() + 1;
        }
        if (compressed)
        {
          var b = new byte[size - 4];
          var output = new byte[bsa.br.ReadUInt32()];
          bsa.br.Read(b, 0, size - 4);

          var inf = new Inflater();
          inf.SetInput(b, 0, b.Length);
          inf.Inflate(output);

          return output;
        }
        return bsa.br.ReadBytes(size);
      }
    }

    private struct BSAFileInfo4
    {
      internal string path;
      internal readonly ulong hash;
      internal readonly int size;
      internal readonly uint offset;

      internal BSAFileInfo4(BinaryReader br)
      {
        path = null;

        hash = br.ReadUInt64();
        size = br.ReadInt32();
        offset = br.ReadUInt32();
      }
    }

    private struct BSAFolderInfo4
    {
      internal string path;
      internal readonly ulong hash;
      internal readonly int count;
      internal int offset;

      internal BSAFolderInfo4(BinaryReader br)
      {
        path = null;
        offset = 0;

        hash = br.ReadUInt64();
        count = br.ReadInt32();
        //offset=br.ReadInt32();
        br.BaseStream.Position += 4; //Don't need the offset here
      }
    }

    private struct BSAHeader4
    {
      internal readonly uint bsaVersion;
      internal readonly int directorySize;
      internal readonly int archiveFlags;
      internal readonly int folderCount;
      internal readonly int fileCount;
      internal readonly int totalFolderNameLength;
      internal readonly int totalFileNameLength;
      internal readonly FileFlags fileFlags;

      internal BSAHeader4(BinaryReader br)
      {
        br.BaseStream.Position += 4;
        bsaVersion = br.ReadUInt32();
        directorySize = br.ReadInt32();
        archiveFlags = br.ReadInt32();
        folderCount = br.ReadInt32();
        fileCount = br.ReadInt32();
        totalFolderNameLength = br.ReadInt32();
        totalFileNameLength = br.ReadInt32();
        fileFlags = (FileFlags) br.ReadInt32();
      }
    }

    private BinaryReader br;
    private bool defaultCompressed;
    private bool SkipNames;
    private HashTable files;
    private string[] fileNames;

    public string[] FileNames
    {
      get
      {
        return fileNames;
      }
    }

    internal BSAArchive(string path)
    {
      br = new BinaryReader(File.OpenRead(path), Encoding.Default);
      BSAHeader4 header = new BSAHeader4(br);
      if (header.bsaVersion != 0x68 && header.bsaVersion != 0x67)
      {
        throw new BSALoadException();
      }
      defaultCompressed = (header.archiveFlags & 4) > 0;
      SkipNames = (header.archiveFlags & 0x100) > 0 && header.bsaVersion == 0x68;
      files = new HashTable();

      //Read folder info
      var folderInfo = new BSAFolderInfo4[header.folderCount];
      var fileInfo = new BSAFileInfo4[header.fileCount];
      fileNames = new string[header.fileCount];
      for (var i = 0; i < header.folderCount; i++)
      {
        folderInfo[i] = new BSAFolderInfo4(br);
      }
      var count = 0;
      for (uint i = 0; i < header.folderCount; i++)
      {
        folderInfo[i].path = new string(br.ReadChars(br.ReadByte() - 1));
        br.BaseStream.Position++;
        folderInfo[i].offset = count;
        for (var j = 0; j < folderInfo[i].count; j++)
        {
          fileInfo[count + j] = new BSAFileInfo4(br);
        }
        count += folderInfo[i].count;
      }
      for (uint i = 0; i < header.fileCount; i++)
      {
        fileInfo[i].path = "";
        char c;
        while ((c = br.ReadChar()) != '\0')
        {
          fileInfo[i].path += c;
        }
      }

      for (var i = 0; i < header.folderCount; i++)
      {
        for (var j = 0; j < folderInfo[i].count; j++)
        {
          var fi4 = fileInfo[folderInfo[i].offset + j];
          var ext = Path.GetExtension(fi4.path);
          var fi = new BSAFileInfo(this, (int) fi4.offset, fi4.size);
          var fpath = Path.Combine(folderInfo[i].path, Path.GetFileNameWithoutExtension(fi4.path));
          var hash = GenHash(fpath, ext);
          files[hash] = fi;
          fileNames[folderInfo[i].offset + j] = fpath + ext;
        }
      }

      Array.Sort(fileNames);
    }

    private static ulong GenHash(string file)
    {
      file = file.ToLowerInvariant().Replace('/', '\\');
      return GenHash(Path.ChangeExtension(file, null), Path.GetExtension(file));
    }

    private static ulong GenHash(string file, string ext)
    {
      file = file.ToLower();
      ext = ext.ToLower();
      ulong hash = 0;
      if (file.Length > 0)
      {
        hash = (ulong) (
          (((byte) file[file.Length - 1])*0x1) +
          ((file.Length > 2 ? (byte) file[file.Length - 2] : 0)*0x100) +
          (file.Length*0x10000) +
          (((byte) file[0])*0x1000000)
          );
      }
      if (file.Length > 3)
      {
        hash += (ulong) (GenHash2(file.Substring(1, file.Length - 3))*0x100000000);
      }
      if (ext.Length > 0)
      {
        hash += (ulong) (GenHash2(ext)*0x100000000);
        byte i = 0;
        switch (ext)
        {
          case ".nif":
            i = 1;
            break;
            //case ".kf": i=2; break;
          case ".dds":
            i = 3;
            break;
            //case ".wav": i=4; break;
        }
        if (i != 0)
        {
          var a = (byte) (((i & 0xfc) << 5) + (byte) ((hash & 0xff000000) >> 24));
          var b = (byte) (((i & 0xfe) << 6) + (byte) (hash & 0xff));
          var c = (byte) ((i << 7) + (byte) ((hash & 0xff00) >> 8));
          hash -= hash & 0xFF00FFFF;
          hash += (uint) ((a << 24) + b + (c << 8));
        }
      }
      return hash;
    }

    private static uint GenHash2(string s)
    {
      uint hash = 0;
      for (var i = 0; i < s.Length; i++)
      {
        hash *= 0x1003f;
        hash += (byte) s[i];
      }
      return hash;
    }

    internal void Dispose()
    {
      if (files != null)
      {
        files.Clear();
      }
      if (br != null)
      {
        br.Close();
        br = null;
      }
    }

    internal byte[] GetFile(string path)
    {
      var hash = GenHash(path);
      if (!files.ContainsKey(hash))
      {
        return null;
      }
      return files[hash].GetRawData();
    }
  }

  internal static class SDPArchives
  {
    private static string GetPath(int package)
    {
      return "data\\shaders\\shaderpackage" + package.ToString().PadLeft(3, '0') + ".sdp";
    }

    private static bool ReplaceShader(string file, string shader, byte[] newdata, out byte[] OldData, uint crc)
    {
      var tempshader = Path.Combine(Program.tmpPath, "tempshader");

      var timeStamp = File.GetLastWriteTime(file);
      File.Delete(tempshader);
      File.Move(file, tempshader);
      var br = new BinaryReader(File.OpenRead(tempshader), Encoding.Default);
      var bw = new BinaryWriter(File.Create(file), Encoding.Default);
      bw.Write(br.ReadInt32());
      var num = br.ReadInt32();
      bw.Write(num);
      var sizeoffset = br.BaseStream.Position;
      bw.Write(br.ReadInt32());
      var found = false;
      OldData = null;
      for (var i = 0; i < num; i++)
      {
        var name = br.ReadChars(0x100);
        var size = br.ReadInt32();
        var data = br.ReadBytes(size);

        bw.Write(name);
        var sname = "";
        for (var i2 = 0; i2 < 100; i2++)
        {
          if (name[i2] == '\0')
          {
            break;
          }
          sname += name[i2];
        }
        if (!found && sname == shader)
        {
          var ccrc = new Crc32();
          ccrc.Update(data);
          if (crc == 0 || ccrc.Value == crc)
          {
            bw.Write(newdata.Length);
            bw.Write(newdata);
            found = true;
            OldData = data;
          }
          else
          {
            bw.Write(size);
            bw.Write(data);
          }
        }
        else
        {
          bw.Write(size);
          bw.Write(data);
        }
      }
      bw.BaseStream.Position = sizeoffset;
      bw.Write((int) (bw.BaseStream.Length - 12));
      br.Close();
      bw.Close();
      File.Delete(tempshader);
      File.SetLastWriteTime(file, timeStamp);
      return found;
    }

    internal static bool EditShader(int package, string name, byte[] newData, out byte[] oldData)
    {
      var path = GetPath(package);
      if (!File.Exists(path))
      {
        oldData = null;
        return false;
      }
      return ReplaceShader(path, name, newData, out oldData, 0);
    }

    internal static bool RestoreShader(int package, string name, byte[] data, uint crc)
    {
      byte[] unused;
      var path = GetPath(package);
      if (!File.Exists(path))
      {
        return false;
      }
      return ReplaceShader(path, name, data, out unused, crc);
    }

    internal static byte[] GetShader(int package, string shader)
    {
      var file = GetPath(package);
      if (!File.Exists(file))
      {
        return null;
      }

      var br = new BinaryReader(File.OpenRead(file), Encoding.Default);
      br.ReadInt32();
      var num = br.ReadInt32();
      br.ReadInt32();
      var found = false;
      byte[] OldData = null;
      for (var i = 0; i < num; i++)
      {
        var name = br.ReadChars(0x100);
        var size = br.ReadInt32();
        var data = br.ReadBytes(size);

        var sname = "";
        for (var i2 = 0; i2 < 100; i2++)
        {
          if (name[i2] == '\0')
          {
            break;
          }
          sname += name[i2];
        }
        if (!found && sname == shader)
        {
          found = true;
          OldData = data;
        }
      }
      br.Close();
      return OldData;
    }
  }
}