using System;
using System.Net;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.Serialization;

namespace fomm.Updater
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		public MainForm()
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
                request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us");
                request.UserAgent =
                   "FOMM Updater (0.13.22)";
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip");
                request.UseDefaultCredentials = true;
                request.PreAuthenticate = true;
                request.AllowAutoRedirect = false;
            
                string response = (new System.IO.StreamReader(new System.IO.Compression.GZipStream(request.GetResponse().GetResponseStream(), System.IO.Compression.CompressionMode.Decompress))).ReadToEnd();
            MessageBox.Show(response);
		}
	}
}
