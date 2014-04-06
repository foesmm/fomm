using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fomm.Updater
{
  public class Release
  {
    public class File
    {
      public Uri URL { get; set; }
      public string FileName { get; set; }
      public string ContentType { get; set; }
      public bool IsUpdate { get; set; }

      public File(Uri url, string fileName, string contentType, bool isUpdate)
      {
        URL = url;
        FileName = fileName;
        ContentType = contentType;
        IsUpdate = isUpdate;
      }
    }

    public string Name { get; set; }
    public Version Version { get; set; }
    public string Notes { get; set; }
    public List<File> Files { get; set; }

    private Release()
    {
      Files = new List<File>();
    }

    public static Release GetLatest(IReleaseProvider provider)
    {
      Release release = new Release(); 
      
      provider.GetLatest(ref release);
      
      return release;
    }

    public bool IsNewer()
    {
      Version fommVersion = Version.Parse(Fomm.ProductInfo.Version);
      return (Version.CompareTo(fommVersion) > 0);
    }

    public File GetUpdateFile()
    {
      return (from file in Files where file.IsUpdate select file).SingleOrDefault();
    }
  }
}
