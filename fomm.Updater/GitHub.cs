using System;
using System.Collections.Generic;
using System.Net;
using MiniJSON;

namespace Fomm.Updater
{
	/// <summary>
	/// Description of GitHub.
	/// </summary>
  public class GitHub : IReleaseProvider
	{
    private HttpWebRequest request;
    const string repo = "niveuseverto/fomm";

		public GitHub()
		{
      request = (HttpWebRequest)WebRequest.Create(String.Format("https://api.github.com/repos/{0}/releases", repo));
      request.KeepAlive = false;
      request.Accept = "*/*";
      request.UserAgent = String.Format("{0} Updater ({1})", Fomm.ProductInfo.ShortName, Fomm.ProductInfo.Version);
      request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us");
      request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip");
      request.UseDefaultCredentials = true;
      request.PreAuthenticate = true;
      request.AllowAutoRedirect = false;
		}

    private bool IsUpdate(string contentType)
    {
      return contentType.Equals("application/x-zip-compressed");
    }

    public bool GetLatest(ref Release release)
    {
      string response = (new System.IO.StreamReader(new System.IO.Compression.GZipStream(request.GetResponse().GetResponseStream(), System.IO.Compression.CompressionMode.Decompress))).ReadToEnd();
      List<object> releases = (List<object>)Json.Deserialize(response);
      Dictionary<string, object> latestRelease = (Dictionary<string, object>)releases[0];
      string tag = latestRelease["tag_name"].ToString();
      string strVersion = tag.ToLower();
      if (strVersion[0] == 'v')
      {
        strVersion = strVersion.Substring(1, strVersion.Length - 1);
      }
      Version version = null;
      if (Version.TryParse(strVersion, out version))
      {
        release.Name = latestRelease["name"].ToString();
        release.Notes = latestRelease["body"].ToString();
        release.Version = version;

        List<object> assets = (List<object>)latestRelease["assets"];
        foreach (Dictionary<string, object> asset in assets)
        {
          string name = asset["name"].ToString();
          string contentType = asset["content_type"].ToString();
          Uri url = new Uri(String.Format("https://github.com/{0}/releases/download/{1}/{2}", repo, tag, name));

          release.Files.Add(new Release.File(url, name, contentType, IsUpdate(contentType)));
        }

        return true;
      }
      return false;
    }
	}
}
