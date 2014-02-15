using System;
using System.Net;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using MiniJSON;

namespace fomm.Updater
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class UpdateForm : Form
	{
		public UpdateForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
			
			HttpWebRequest request = (HttpWebRequest) WebRequest.Create("https://api.github.com/repos/niveuseverto/fomm/releases");
			request.KeepAlive = false;
            request.Accept = "*/*";
            request.UserAgent = "FOMM Updater (0.13.22)";
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us");
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip");
            request.UseDefaultCredentials = true;
            request.PreAuthenticate = true;
            request.AllowAutoRedirect = false;
        
            string response = (new System.IO.StreamReader(new System.IO.Compression.GZipStream(request.GetResponse().GetResponseStream(), System.IO.Compression.CompressionMode.Decompress))).ReadToEnd();
            List<object> releases = (List<object>)Json.Deserialize(response);
            Dictionary<string, object> release = (Dictionary<string, object>)releases[0];
            string release_name = release["name"].ToString();
        	string release_notes = release["body"].ToString();
            string tag_name = release["tag_name"].ToString();
            Version remoteVersion = null;
            if (Version.TryParse(tag_name, out remoteVersion))
            	Debug.WriteLine(String.Format("Newer version found {0}", remoteVersion.ToString()));
            
            List<object> assets = (List<object>)release["assets"];
            foreach (Dictionary<string, object> asset in assets) {
            	Debug.WriteLine(String.Format("{0} of {1}", asset["name"], asset["content_type"]));
            	string downloadURI = String.Format("https://github.com/{0}/releases/download/{1}/{2}", "niveuseverto/fomm", tag_name, asset["name"]);
            }
            string file_name = "";
            
            MessageBox.Show(String.Format("Is Elevated: {0}", UpdateHelper.IsProcessElevated));
            MessageBox.Show(String.Format("{0}: {1}", AppDomain.CurrentDomain.BaseDirectory, UpdateHelper.HasWriteAccessToFolder(AppDomain.CurrentDomain.BaseDirectory).ToString()));
//            string downloadURI = String.Format("https://github.com/{0}/releases/download/{1}/{2}", "niveuseverto/fomm", tag_name, file_name);
            MessageBox.Show(release_notes);
            
		}
	}
}
