using System;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Diagnostics;
using GeMod.Interface;
using CsQuery;

namespace WebsiteAPIs
{
	#region Supporting Data Structures

	/// <summary>
	/// The file categories.
	/// </summary>
	public enum FileCategory
	{
		/// <summary>
		/// The main file category.
		/// </summary>
		MainFile = 1,

		/// <summary>
		/// The update file category.
		/// </summary>
		UpdateFile,

		/// <summary>
		/// The optional file category.
		/// </summary>
		OptionalFile,

		/// <summary>
		/// The old version file category.
		/// </summary>
		OldVersion,

		/// <summary>
		/// The miscellaneous file category.
		/// </summary>
		Miscellaneous
	}

	/// <summary>
	/// The Nexus sites.
	/// </summary>
	public enum NexusSite
	{
		/// <summary>
		/// The Fallout 3 Nexus site.
		/// </summary>
		Fallout3,

		/// <summary>
		/// The Fallout: New Vegas Nexus site.
		/// </summary>
		FalloutNV,
	}

	/// <summary>
	/// A delegate for a method that takes 2 parameters and returns void.
	/// </summary>
	/// <remarks>
	/// This duplicates the functionality of the delegate with the same signature
	/// found in .NET v3.5 and later. It is duplicated here to support pre-3.5 installs.
	/// </remarks>
	/// <typeparam name="T1">The type of the first parameter.</typeparam>
	/// <typeparam name="T2">The type of the second parameter.</typeparam>
	/// <param name="p_t1Value">The first parameter.</param>
	/// <param name="p_t2Value">The second parameter.</param>
	public delegate void Action<T1, T2>(T1 p_t1Value, T2 p_t2Value);

	/// <summary>
	/// The event arguments passed to objects subscribed to the <see cref="NexusAPI.UploadProgress"/>
	/// event.
	/// </summary>
	/// <remarks>
	/// These event arguments report the current upload progress, and allows cancellation.
	/// </remarks>
	public class ProgressEventArgs : EventArgs
	{
		#region Properties

		/// <summary>
		/// Gets or sets the percentage of the file that has been uploaded.
		/// </summary>
		/// <value>The percentage of the file that has been uploaded.</value>
		public Int32 PrecentComplete { get; protected set; }

		/// <summary>
		/// Gets or sets whether to cancel the upload.
		/// </summary>
		/// <value>Whether to cancel the upload.</value>
		public bool Cancel { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// A simple construtor that initializes the object with the given values.
		/// </summary>
		/// <param name="p_intPercentComplete">The percentage of the file that has been uploaded.</param>
		public ProgressEventArgs(Int32 p_intPercentComplete)
		{
			PrecentComplete = p_intPercentComplete;
			Cancel = false;
		}

		#endregion
	}

	#endregion

	/// <summary>
	/// An API to the Nexus sites.
	/// </summary>
	public class NexusAPI
	{
		#region Events

		/// <summary>
		/// Reports the upload progress of a file.
		/// </summary>
		public event EventHandler<ProgressEventArgs> UploadProgress;

		/// <summary>
		/// Raises the <see cref="UploadProgress"/> event.
		/// </summary>
		/// <param name="p_intPercentComplete">The percentage of the file that has been uploaded.</param>
		protected void OnUploadProgress(Int32 p_intPercentComplete)
		{
			if (UploadProgress != null)
				UploadProgress(this, new ProgressEventArgs(p_intPercentComplete));
		}

		#endregion

		private static Regex m_rgxUploadProgressKey = new Regex("name=\"APC_UPLOAD_PROGRESS\".*?value=\"(.*?)\"");
		private static Regex m_rgxVersion = new Regex("Version</div>.*?<div [^>]+>([^<]+)<", RegexOptions.Singleline);
		private static Regex m_rgxName = new Regex("<h2>(.*?)</h2>\\s+<h3>", RegexOptions.Singleline);
		private static Regex m_rgxAuthor = new Regex("Author</div>.*?<div [^>]+>([^<]+)<", RegexOptions.Singleline);
		private static Regex m_rgxScreenshotUrl = new Regex("<div class=\"file_title\">Images</div>.*?<img src=\"(.*?)\"", RegexOptions.Singleline);

		private CookieContainer m_ckcCookies = new CookieContainer();
		private NexusSite m_nxsSite = NexusSite.Fallout3;
		private string m_strSite = null;
		private string m_strUsername = null;
		private string m_strPassword = null;
		private bool m_booLoggedIn = false;

		#region Properties

		/// <summary>
		/// Gets the login key used by the site to track logins.
		/// </summary>
		/// <value>The login key used by the site to track logins.</value>
		public string LoginKey
		{
			get
			{
				AssertLoggedIn();
				CookieCollection cclCookies = m_ckcCookies.GetCookies(new Uri("http://" + m_strSite));
				return cclCookies["sid"].Value;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initilaizes the object.
		/// </summary>
		/// <param name="p_nxsSite">The nexus site with which to interect.</param>
		public NexusAPI(NexusSite p_nxsSite)
		{
			m_nxsSite = p_nxsSite;
			m_strSite = GetWebsite(m_nxsSite);
		}

		/// <summary>
		/// A simple constructor that initilaizes the object.
		/// </summary>
		/// <param name="p_nxsSite">The nexus site with which to interect.</param>
		/// <param name="p_strUsername">The username with which to log into the site.</param>
		/// <param name="p_strPassword">The password with which to log into the site.</param>
		public NexusAPI(NexusSite p_nxsSite, string p_strUsername, string p_strPassword)
			: this(p_nxsSite)
		{
			m_strUsername = p_strUsername;
			m_strPassword = p_strPassword;
		}

		/// <summary>
		/// A simple constructor that initilaizes the object.
		/// </summary>
		/// <param name="p_nxsSite">The nexus site with which to interect.</param>
		/// <param name="p_strLoginKey">The login key used by the site to track logins.</param>
		public NexusAPI(NexusSite p_nxsSite, string p_strLoginKey)
			: this(p_nxsSite)
		{
			Cookie ckeLoginCookie = null;
			ckeLoginCookie = new Cookie("sid", p_strLoginKey, "/", "www.nexusmods.com");
			m_ckcCookies.Add(ckeLoginCookie);
			m_booLoggedIn = true;
		}

		#endregion

		/// <summary>
		/// Gets the website for the given Nexus site.
		/// </summary>
		/// <param name="p_nstSite">The site for which the retrieve the website.</param>
		/// <returns>The website for the given Nexus site.</returns>
		public static string GetWebsite(NexusSite p_nstSite)
		{
			switch (p_nstSite)
			{
				case NexusSite.Fallout3:
					return "www.nexusmods.com/fallout3/";
				case NexusSite.FalloutNV:
					return "www.nexusmods.com/newvegas/";
				default:
					throw new Exception("Unrecognized value for NexusSite.");
			}
		}

		/// <summary>
		/// Tries to get the files id from the given filename.
		/// </summary>
		/// <param name="p_strFileName">The filename from which to parse the file id.</param>
		/// <param name="p_intFileId">The out parameter that will contain the file id, if found.</param>
		/// <returns><lang cref="true"/> if the file id was found;
		/// <lang cref="false"/> otherwise.</returns>
		protected bool TryParseFileId(string p_strFileName, out Int32 p_intFileId)
		{
			Regex rgxFileId = new Regex(@"-(\d\d\d\d\d+)");
			if (rgxFileId.IsMatch(p_strFileName))
			{
				foreach (Match mchFileId in rgxFileId.Matches(p_strFileName))
				{
					Int32 intId = Int32.Parse(mchFileId.Groups[1].Value);
					if (GetModExists(intId))
					{
						//strPackedFomodPath = strPackedFomodPath.Remove(mchFileId.Index, mchFileId.Length) + Path.GetExtension(strSource);
						p_intFileId = intId;
						return true;
					}
				}
			}
			p_intFileId = -1;
			return false;
		}

		/// <summary>
		/// Ensures that the API is logged in to the site.
		/// </summary>
		/// <exception cref="SiteLoginException">Thrown if the API can't login to the site.</exception>
		protected void AssertLoggedIn()
		{
			if (!m_booLoggedIn && !Login(m_strUsername, m_strPassword))
				throw new InvalidOperationException("You must be logged in to call this method; the given credentials are invalid.");
		}

		/// <summary>
		/// Logs into the site with the credentials passed to the constructor.
		/// </summary>
		/// <returns><lang cref="true"/> if the login succeeded; <lang cref="false"/> otherwise.</returns>
		/// <exception cref="HttpException">Thrown if the login page can't be accessed.</exception>
		public bool Login()
		{
			return Login(m_strUsername, m_strPassword);
		}

		/// <summary>
		/// Logs into the site.
		/// </summary>
		/// <param name="p_strUsername">The username with which to log into the site.</param>
		/// <param name="p_strPassword">The password with which to log into the site.</param>
		/// <returns><lang cref="true"/> if the login succeeded; <lang cref="false"/> otherwise.</returns>
		/// <exception cref="HttpException">Thrown if the login page can't be accessed.</exception>
		public bool Login(string p_strUsername, string p_strPassword)
		{
			HttpWebRequest hwrLogin = (HttpWebRequest)WebRequest.Create(String.Format("http://www.nexusmods.com/games/sessions/?Login&uri={0}", m_strSite));
			hwrLogin.CookieContainer = m_ckcCookies;
			hwrLogin.Method = "POST";
			hwrLogin.ContentType = "application/x-www-form-urlencoded";

			string strFields = String.Format("username={0}&password={1}", p_strUsername, p_strPassword);
			byte[] bteFields = System.Text.Encoding.UTF8.GetBytes(strFields);
			hwrLogin.ContentLength = bteFields.Length;
			hwrLogin.GetRequestStream().Write(bteFields, 0, bteFields.Length);

			string strLoginResultPage = null;
			using (WebResponse wrpLoginResultPage = hwrLogin.GetResponse())
			{
				if (((HttpWebResponse)wrpLoginResultPage).StatusCode != HttpStatusCode.OK)
					throw new HttpException("Request to the login page failed with HTTP error: " + ((HttpWebResponse)wrpLoginResultPage).StatusCode);

				Stream stmLoginResultPage = wrpLoginResultPage.GetResponseStream();
				using (StreamReader srdLoginResultPage = new StreamReader(stmLoginResultPage))
				{
					strLoginResultPage = srdLoginResultPage.ReadToEnd();
					srdLoginResultPage.Close();
				}
				wrpLoginResultPage.Close();
			}
			m_booLoggedIn = !strLoginResultPage.Contains("error.php?");
			return m_booLoggedIn;
		}

		#region Mod File Management

		/// <summary>
		/// Gets the file upload page of the specified mod.
		/// </summary>
		/// <returns>The file upload page of the specified mod.</returns>
		/// <param name="p_intModKey">The key of the mod to manage.</param>
		/// <exception cref="HttpException">Thrown if the upload page can't be accessed.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the API can't log in to the site.</exception>
		protected string GetUploadPage(Int32 p_intModKey)
		{
			return null; // @fixme
			AssertLoggedIn();
			HttpWebRequest hwrFileUploadPage = (HttpWebRequest)WebRequest.Create(String.Format("http://{0}/members/addfile.php?id={1}", m_strSite, p_intModKey));
			hwrFileUploadPage.CookieContainer = m_ckcCookies;
			hwrFileUploadPage.Method = "GET";

			string strFileUploadPage = null;
			using (WebResponse wrpFileUploadPage = hwrFileUploadPage.GetResponse())
			{
				if (((HttpWebResponse)wrpFileUploadPage).StatusCode != HttpStatusCode.OK)
					throw new HttpException("Request to the file upload page failed with HTTP error: " + ((HttpWebResponse)wrpFileUploadPage).StatusCode);

				Stream stmFileUploadPage = wrpFileUploadPage.GetResponseStream();
				using (StreamReader srdFileUploadPage = new StreamReader(stmFileUploadPage))
				{
					strFileUploadPage = srdFileUploadPage.ReadToEnd();
					srdFileUploadPage.Close();
				}
				wrpFileUploadPage.Close();
			}
			return strFileUploadPage;
		}

		/// <summary>
		/// Gets an upload progress key for the specified mod.
		/// </summary>
		/// <returns>An upload progress key for the specified mod.</returns>
		/// <param name="p_intModKey">The key of the mod to manage.</param>
		protected string GetUploadProgressKey(Int32 p_intModKey)
		{
			string strFileUploadPage = GetUploadPage(p_intModKey);
			string strKey = m_rgxUploadProgressKey.Match(strFileUploadPage).Groups[1].Value;
			return strKey;
		}

		/// <summary>
		/// Gets the key for the specified file, in the specified mod.
		/// </summary>
		/// <returns>The key for the specified file, in the specified mod. <lang cref="null"/> is
		/// returned if a file with the given title does not exist.</returns>
		/// <param name="p_intModKey">The key of the mod to manage.</param>
		/// <param name="p_strFileTitle">The title of the files whose key is to be retrieved.</param>
		protected string GetFileKey(Int32 p_intModKey, string p_strFileTitle)
		{
			string strFileUploadPage = GetUploadPage(p_intModKey);
			Regex rgxFileKey = new Regex(">\\s+" + p_strFileTitle + "</a>.*?do_deletesinglefile.php\\?id=(\\d+)", RegexOptions.Singleline);
			if (!rgxFileKey.IsMatch(strFileUploadPage))
				return null;
			string strKey = rgxFileKey.Match(strFileUploadPage).Groups[1].Value;
			return strKey;
		}

		/// <summary>
		/// Deletes the specified file from the specified mod.
		/// </summary>
		/// <param name="p_intModKey">The key of the mod to manage.</param>
		/// <param name="p_strFileTitle">The title of the file to delete.</param>
		/// <exception cref="HttpException">Thrown if the delete page can't be accessed.</exception>
		protected void DeleteFile(Int32 p_intModKey, string p_strFileTitle)
		{
			return; // @fixme
			string strFileKey = GetFileKey(p_intModKey, p_strFileTitle);
			if (strFileKey == null)
				return;

			HttpWebRequest hwrFileDeletePage = (HttpWebRequest)WebRequest.Create(String.Format("http://{0}/members/do_deletesinglefile.php?id={1}", m_strSite, strFileKey));
			hwrFileDeletePage.CookieContainer = m_ckcCookies;
			hwrFileDeletePage.Method = "GET";

			string strFileDeletePage = null;
			using (WebResponse wrpFileDeletedPage = hwrFileDeletePage.GetResponse())
			{
				if (((HttpWebResponse)wrpFileDeletedPage).StatusCode != HttpStatusCode.OK)
					throw new HttpException("Request to the file delete page failed with HTTP error: " + ((HttpWebResponse)wrpFileDeletedPage).StatusCode);

				Stream stmFileUploadPage = wrpFileDeletedPage.GetResponseStream();
				using (StreamReader srdFileUploadPage = new StreamReader(stmFileUploadPage))
				{
					strFileDeletePage = srdFileUploadPage.ReadToEnd();
					srdFileUploadPage.Close();
				}
				wrpFileDeletedPage.Close();
			}
		}

		/// <summary>
		/// Checks if the given file title is valid, according to the Nexus's rules.
		/// </summary>
		/// <remarks>
		/// This makes sure the given string contains only letters, numbers, spaces, underscores, and dashes.
		/// </remarks>
		/// <param name="p_strName">The file title to check.</param>
		/// <returns><lang cref="true"/> if the given title is valid; <lang cref="false"/> otherwise.</returns>
		protected bool isFileTitleValid(string p_strName)
		{
			if (String.IsNullOrEmpty(p_strName))
				return false;
			Regex rgxName = new Regex("^[a-zA-Z0-9 _-]+$");
			return rgxName.IsMatch(p_strName);
		}

		/// <summary>
		/// Edits a file's info.
		/// </summary>
		/// <param name="p_intModKey">The key of the mod to manage.</param>
		/// <param name="p_strOldFileTitle">The old title of the file.</param>
		/// <param name="p_strNewFileTitle">The new title of the file.</param>
		/// <param name="p_fctCategory">The category to which to upload the file.</param>
		/// <param name="p_strDescription">The file description.</param>
		/// <returns><lang cref="true"/> if the file info was edited; <lang cref="false"/> otherwise.</returns>
		/// <exception cref="FileNotFoundException">If the specified file does not exist on hte Nexus.</exception>
		public bool EditFile(Int32 p_intModKey, string p_strOldFileTitle, string p_strNewFileTitle, FileCategory p_fctCategory, string p_strDescription)
		{
			return false; // @fixme
			AssertLoggedIn();
			if (!isFileTitleValid(p_strNewFileTitle))
				throw new ArgumentException("The file title can only contain letters, numbers, spaces, hypens and underscores.", "p_strNewFileTitle");

			string strFileKey = GetFileKey(p_intModKey, p_strOldFileTitle);
			if (strFileKey == null)
				throw new FileNotFoundException("The file does not exist.");

			NameValueCollection nvc = new NameValueCollection();
			nvc.Add("filename", p_strNewFileTitle);
			nvc.Add("category", ((Int32)p_fctCategory).ToString());
			nvc.Add("desc", p_strDescription);

			string strBoundary = Guid.NewGuid().ToString().Replace("-", "");
			HttpWebRequest hwrFileEditPage = (HttpWebRequest)WebRequest.Create(String.Format("http://{0}/members/do_editfilename.php?id={1}", m_strSite, strFileKey));
			hwrFileEditPage.ContentType = "multipart/form-data; boundary=" + strBoundary;
			hwrFileEditPage.Method = "POST";
			hwrFileEditPage.KeepAlive = true;
			hwrFileEditPage.Credentials = System.Net.CredentialCache.DefaultCredentials;
			hwrFileEditPage.CookieContainer = m_ckcCookies;
			hwrFileEditPage.Timeout = System.Threading.Timeout.Infinite;
			hwrFileEditPage.ReadWriteTimeout = System.Threading.Timeout.Infinite;

			byte[] bteBoundary = System.Text.Encoding.ASCII.GetBytes("\r\n--" + strBoundary + "\r\n");
			string strFormDataTemplate = "\r\n--" + strBoundary + "\r\nContent-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
			using (Stream stmRequestContent = new MemoryStream())
			{
				foreach (string strFormField in nvc.Keys)
				{
					string strFormItem = String.Format(strFormDataTemplate, strFormField, nvc[strFormField]);
					byte[] bteFormItem = System.Text.Encoding.UTF8.GetBytes(strFormItem);
					stmRequestContent.Write(bteFormItem, 0, bteFormItem.Length);
				}

				stmRequestContent.Write(bteBoundary, 0, bteBoundary.Length);
				hwrFileEditPage.ContentLength = stmRequestContent.Length;

				try
				{
					stmRequestContent.Position = 0;
					Int32 intTotalBytesRead = 0;
					Int64 intRequestContentLength = stmRequestContent.Length;
					using (Stream stmRequest = hwrFileEditPage.GetRequestStream())
					{
						byte[] bteBuffer = new byte[1024];
						Int32 intBytesRead = 0;
						while ((intBytesRead = stmRequestContent.Read(bteBuffer, 0, bteBuffer.Length)) != 0)
						{
							stmRequest.Write(bteBuffer, 0, intBytesRead);
							intTotalBytesRead += intBytesRead;
							OnUploadProgress((Int32)(intTotalBytesRead * 100 / intRequestContentLength));
						}
						stmRequest.Close();
					}
				}
				catch (Exception e)
				{
					throw e;
				}
				stmRequestContent.Close();
			}

			string strReponse = null;
			using (WebResponse wrpUpload = hwrFileEditPage.GetResponse())
			{
				using (Stream stmResponse = wrpUpload.GetResponseStream())
				{
					using (StreamReader srdResponse = new StreamReader(stmResponse))
					{
						strReponse = srdResponse.ReadToEnd();
						srdResponse.Close();
					}
					stmResponse.Close();
				}
				wrpUpload.Close();
			}
			return strReponse.Contains("File details changed");
		}

		/// <summary>
		/// Uploads a file to the specified mod.
		/// </summary>
		/// <param name="p_intModKey">The key of the mod to manage.</param>
		/// <param name="p_strFileTitle">The title of the file.</param>
		/// <param name="p_fctCategory">The category to which to upload the file.</param>
		/// <param name="p_strDescription">The file description.</param>
		/// <param name="p_strFilePath">The path of the file to upload.</param>
		/// <param name="p_booOverwrite">Whether or not to overwrite any existing file with the given title.</param>
		/// <returns><lang cref="true"/> if the file uploaded; <lang cref="false"/> otherwise.</returns>
		/// <exception cref="FileNotFoundException">If the specified file does not exist.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the API can't log in to the site.</exception>
		public bool UploadFile(Int32 p_intModKey, string p_strFileTitle, FileCategory p_fctCategory, string p_strDescription, string p_strFilePath, bool p_booOverwrite)
		{
			return false; // @fixme
			AssertLoggedIn();
			if (!isFileTitleValid(p_strFileTitle))
				throw new ArgumentException("The file title can only contain letters, numbers, spaces, hypens and underscores.", "p_strFileTitle");

			if (!File.Exists(p_strFilePath))
				throw new FileNotFoundException("The file to upload does not exist.", p_strFilePath);

			if (p_booOverwrite)
				DeleteFile(p_intModKey, p_strFileTitle);

			NameValueCollection nvc = new NameValueCollection();
			nvc.Add("filename", p_strFileTitle);
			nvc.Add("category", ((Int32)p_fctCategory).ToString());
			nvc.Add("desc", p_strDescription);

			string strKey = GetUploadProgressKey(p_intModKey);
			string strURL = String.Format("http://{0}/members/do_addfile.php?fid={1}&prog_key={2}", m_strSite, p_intModKey, strKey);
			nvc.Add("APC_UPLOAD_PROGRESS", strKey);

			string strBoundary = Guid.NewGuid().ToString().Replace("-", "");

			HttpWebRequest hwrUpload = (HttpWebRequest)WebRequest.Create(strURL);
			hwrUpload.ContentType = "multipart/form-data; boundary=" + strBoundary;
			hwrUpload.Method = "POST";
			hwrUpload.KeepAlive = true;
			hwrUpload.Credentials = System.Net.CredentialCache.DefaultCredentials;
			hwrUpload.CookieContainer = m_ckcCookies;
			hwrUpload.Timeout = System.Threading.Timeout.Infinite;
			hwrUpload.ReadWriteTimeout = System.Threading.Timeout.Infinite;

			byte[] bteBoundary = System.Text.Encoding.ASCII.GetBytes("\r\n--" + strBoundary + "\r\n");
			string strFormDataTemplate = "\r\n--" + strBoundary + "\r\nContent-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
			using (Stream stmRequestContent = new MemoryStream())
			{
				foreach (string strFormField in nvc.Keys)
				{
					string strFormItem = String.Format(strFormDataTemplate, strFormField, nvc[strFormField]);
					byte[] bteFormItem = System.Text.Encoding.UTF8.GetBytes(strFormItem);
					stmRequestContent.Write(bteFormItem, 0, bteFormItem.Length);
				}

				stmRequestContent.Write(bteBoundary, 0, bteBoundary.Length);
				string strFileHeaderTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n";
				string strHeader = string.Format(strFileHeaderTemplate, "fileupload", Path.GetFileName(p_strFilePath));
				byte[] bteHeader = System.Text.Encoding.UTF8.GetBytes(strHeader);
				stmRequestContent.Write(bteHeader, 0, bteHeader.Length);

				using (FileStream fstFile = new FileStream(p_strFilePath, FileMode.Open, FileAccess.Read))
				{
					byte[] bteBuffer = new byte[1024];
					Int32 intBytesRead = 0;
					while ((intBytesRead = fstFile.Read(bteBuffer, 0, bteBuffer.Length)) != 0)
						stmRequestContent.Write(bteBuffer, 0, intBytesRead);
					stmRequestContent.Write(bteBoundary, 0, bteBoundary.Length);
					fstFile.Close();
				}
				hwrUpload.ContentLength = stmRequestContent.Length;

				try
				{
					stmRequestContent.Position = 0;
					Int32 intTotalBytesRead = 0;
					using (Stream stmRequest = hwrUpload.GetRequestStream())
					{
						byte[] bteBuffer = new byte[1024];
						Int32 intBytesRead = 0;
						while ((intBytesRead = stmRequestContent.Read(bteBuffer, 0, bteBuffer.Length)) != 0)
						{
							stmRequest.Write(bteBuffer, 0, intBytesRead);
							intTotalBytesRead += intBytesRead;
							OnUploadProgress((Int32)(intTotalBytesRead * 100 / stmRequestContent.Length));
						}
						stmRequest.Close();
					}
				}
				catch (Exception e)
				{
					throw e;
				}
				stmRequestContent.Close();
			}

			string strReponse = null;
			using (WebResponse wrpUpload = hwrUpload.GetResponse())
			{
				using (Stream stmResponse = wrpUpload.GetResponseStream())
				{
					using (StreamReader srdResponse = new StreamReader(stmResponse))
					{
						strReponse = srdResponse.ReadToEnd();
						srdResponse.Close();
					}
					stmResponse.Close();
				}
				wrpUpload.Close();
			}
			return strReponse.Contains("File added successfully to");
		}

		#endregion

		/// <summary>
		/// Determines if the specified file exists.
		/// </summary>
		/// <param name="p_intFileId">The id of the file whose existence is to be determined.</param>
		/// <returns><lang cref="true"/> if the file exists;
		/// <lang cref="false"/> otherwise.</returns>
		public bool GetModExists(Int32 p_intModKey)
		{
			string strURL = String.Format("http://{0}mods/{1}/", m_strSite, p_intModKey);

			HttpWebRequest hwrFilePage = (HttpWebRequest)WebRequest.Create(strURL);
			hwrFilePage.CookieContainer = m_ckcCookies;
			hwrFilePage.Method = "GET";

			string strFilePage = null;
			using (WebResponse wrpFilePage = hwrFilePage.GetResponse())
			{
				if (((HttpWebResponse)wrpFilePage).StatusCode == HttpStatusCode.Found)
					return true;
				if (((HttpWebResponse)wrpFilePage).StatusCode != HttpStatusCode.OK)
					return false;

				Stream stmFilePage = wrpFilePage.GetResponseStream();
				using (StreamReader srdFilePage = new StreamReader(stmFilePage))
				{
					strFilePage = srdFilePage.ReadToEnd();
					srdFilePage.Close();
				}
				wrpFilePage.Close();
			}
			return !strFilePage.Contains("error=file_exist");
		}

		#region File Info

		#region Info Page Parsing

		/// <summary>
		/// Parse the mod version out of the mod's info page.
		/// </summary>
		/// <param name="p_strInfoPage">The info page of the mod whose version is being parsed.</param>
		/// <returns>The version string from the given mod info page.</returns>
		private string ParseModVersion(string p_strInfoPage)
		{
			CQ dom = p_strInfoPage;
			string strWebVersion = dom["p.file-version > strong"].Text();
			
			return strWebVersion;
		}

		/// <summary>
		/// Parse the mod name out of the mod's info page.
		/// </summary>
		/// <param name="p_strInfoPage">The info page of the mod whose author is being parsed.</param>
		/// <returns>The name from the given mod info page.</returns>
		private string ParseModName(string p_strInfoPage)
		{
			CQ dom = p_strInfoPage;
			
			string strName = dom["span.header-name"].Text();

			return strName;
		}

		/// <summary>
		/// Parse the mod author out of the mod's info page.
		/// </summary>
		/// <param name="p_strInfoPage">The info page of the mod whose author is being parsed.</param>
		/// <returns>The author string from the given mod info page.</returns>
		private string ParseModAuthor(string p_strInfoPage)
		{
			CQ dom = p_strInfoPage;
			
			string strAuthor = dom["div.file-stats > div.uploader > a"].Text();
			
			return strAuthor;
		}

		/// <summary>
		/// Parse the mod screenshot URL out of the mod's info page.
		/// </summary>
		/// <param name="p_strInfoPage">The info page of the mod whose screenshot url is being parsed.</param>
		/// <returns>The screenshot url from the given mod info page.</returns>
		private Uri ParseScreenshotUrl(string p_strInfoPage)
		{
			CQ dom = p_strInfoPage;
			
			string strURL = dom["#gallery-ul-0 > li:first > a"].Attr("href");
			
			Uri uriScreenshot = new Uri(strURL);
			
			return uriScreenshot;
		}

		/// <summary>
		/// Parse the mod info out of the mod's info page.
		/// </summary>
		/// <param name="p_strInfoPage">The info page of the mod whose information is being parsed.</param>
		/// <param name="p_booIncludeScreenshot">Whether or not to retrieve the mod's screenshot.</param>
		/// <returns>A <see cref="ModInfo"/> describing the mod's information.</returns>
		protected ModInfo ParseModInfo(Uri p_uriModUrl, string p_strInfoPage, bool p_booIncludeScreenshot)
		{
			string strName = ParseModName(p_strInfoPage);
			string strAuthor = ParseModAuthor(p_strInfoPage);
			string strVersion = ParseModVersion(p_strInfoPage);
			Screenshot sstScreenshot = null;
			if (p_booIncludeScreenshot)
			{
				Uri uriScreenshotUrl = ParseScreenshotUrl(p_strInfoPage);
				if (uriScreenshotUrl != null)
					sstScreenshot = new Screenshot(uriScreenshotUrl.ToString(), DownloadData(true, uriScreenshotUrl));
			}
			return new ModInfo(strName, strAuthor, strVersion, p_uriModUrl, sstScreenshot);
		}

		#endregion

		//private Int32 ParseMonth(

		/// <summary>
		/// Gets the mod info for the specified file, and tries to guess the version of
		/// the given file.
		/// </summary>
		/// <remarks>
		/// This method tries to guess the version of the given file. Note that this is a best guess,
		/// and could be very wrong.
		/// </remarks>
		/// <param name="p_intFileId">The id of the file whose version is to be retrieved.</param>
		/// <param name="p_booIncludeScreenshot">Whether or not to retrieve the mod's screenshot.</param>
		/// <returns>The current verison of the specified file.</returns>
		/// <exception cref="InvalidOperationException">Thrown if the API can't log in to the site.</exception>
		public ModInfo GetFileInfoGuessVersion(string p_strFileName, bool p_booIncludeScreenshot)
		{
			Int32 intFileId = -1;
			if (!TryParseFileId(p_strFileName, out intFileId))
				return null;
			string strURL = String.Format("http://{0}mods/{1}/", m_strSite, intFileId);
			string strInfoPage = GetWebPage(true, strURL);
			ModInfo mifLastestInfo = ParseModInfo(new Uri(strURL), strInfoPage, p_booIncludeScreenshot);

//			if (File.Exists(p_strFileName))
//			{
//				FileInfo fifInfo = new FileInfo(p_strFileName);
//				string strFilename = Path.GetFileName(p_strFileName);
//				strFilename = strFilename.Remove(strFilename.IndexOf("-" + intFileId));
//
//				string strFilesPage = GetWebPage(true, String.Format("http://{0}ajax/modfiles/?id={1}", m_strSite, intFileId));
//				
//				CQ dom = strFilesPage;
//				
//				
//				Regex rgxFiles = new Regex("\\s+(.*?)</a></h2>");
//				foreach (Match mchFile in rgxFiles.Matches(strFilesPage))
//				{
//					if (mchFile.Groups[1].Value.Replace(' ', '_').StartsWith(strFilename, StringComparison.InvariantCultureIgnoreCase))
//					{
//						Regex rgxFileVersion = new Regex(@"lightbulb.png""\s+/>\s+Version\s+(.*?)</div>");
//						Regex rgxFileDate = new Regex(@"calendar.png""\s+/>\s+(.*?)</div>");
//						Match mchFileDate = rgxFileDate.Match(strFilesPage, mchFile.Groups[1].Index);
//						Match mchFileVersion = rgxFileVersion.Match(strFilesPage, mchFile.Groups[1].Index, mchFileDate.Groups[1].Index - mchFile.Groups[1].Index);
//						if (mchFileVersion.Success)
//						{
//							mifLastestInfo = new ModInfo(mifLastestInfo.ModName, mifLastestInfo.Author, mchFileVersion.Groups[1].Value, mifLastestInfo.URL, mifLastestInfo.Screenshot);
//						}
//						else
//						{
//							DateTime dteFileDate = DateTime.Now;
//							DateTime.TryParse(mchFileDate.Groups[1].Value, out dteFileDate);
//							if (fifInfo.CreationTime <= dteFileDate)
//								mifLastestInfo = new ModInfo(mifLastestInfo.ModName, mifLastestInfo.Author, null, mifLastestInfo.URL, mifLastestInfo.Screenshot);
//						}
//						break;
//					}
//				}
//			}

			return mifLastestInfo;
		}

		/// <summary>
		/// Gets the current mod info of the specified file.
		/// </summary>
		/// <param name="p_intModKey">The key of the mod whose version is to be retrieved.</param>
		/// <param name="p_booIncludeScreenshot">Whether or not to retrieve the mod's screenshot.</param>
		/// <returns>The current verison of the specified file.</returns>
		/// <exception cref="InvalidOperationException">Thrown if the API can't log in to the site.</exception>
		public ModInfo GetModInfo(Int32 p_intModKey, bool p_booIncludeScreenshot)
		{
			string strURL = String.Format("http://{0}mods/{1}/", m_strSite, p_intModKey);
			string strInfoPage = GetWebPage(true, strURL);
			return ParseModInfo(new Uri(strURL), strInfoPage, p_booIncludeScreenshot);
		}

		/// <summary>
		/// Gets the current info of the specified mod in an asynchronous request.
		/// </summary>
		/// <remarks>
		/// The given <see cref="Action{object, ModInfo}"/> is called upon completion of the retrieval of the
		/// file version. The first parameter passed to the delegate is the user-supplied state. The second
		/// parameter passed to the delegate is the string representation of the file version.
		/// </remarks>
		/// <param name="p_intModKey">The id of the mod whose info is to be retrieved.</param>
		/// <param name="p_booIncludeScreenshot">Whether or not to retrieve the mod's screenshot.</param>
		/// <param name="p_actCallback">The method to call upon completion of the request.</param>
		/// <param name="p_objState">User supplied state.</param>
		/// <exception cref="InvalidOperationException">Thrown if the API can't log in to the site.</exception>
		public void GetModInfoAsync(Int32 p_intModKey, bool p_booIncludeScreenshot, Action<object, ModInfo> p_actCallback, object p_objState)
		{
			AssertLoggedIn();
			string strURL = String.Format("http://{0}mods/{1}/", m_strSite, p_intModKey);

			HttpWebRequest hwrFilePage = (HttpWebRequest)WebRequest.Create(strURL);
			hwrFilePage.CookieContainer = m_ckcCookies;
			hwrFilePage.Method = "GET";

			AsyncOperation aopOperation = AsyncOperationManager.CreateOperation(new KeyValuePair<Action<object, ModInfo>, object>(p_actCallback, p_objState));
			hwrFilePage.BeginGetResponse(GetFilePageCallback, new object[] { hwrFilePage, aopOperation, p_booIncludeScreenshot });
		}

		/// <summary>
		/// Handles the callback of the http request that retrieves the file page when
		/// getting a file version.
		/// </summary>
		/// <remarks>
		/// This method parses the version out of the requested page and passes it to a user
		/// supplied method.
		/// </remarks>
		/// <param name="p_asrResult">The asynchronous result of the request for the file page.</param>
		private void GetFilePageCallback(IAsyncResult p_asrResult)
		{
			object[] objState = (object[])p_asrResult.AsyncState;
			HttpWebRequest hwrFilePage = (HttpWebRequest)objState[0];
			AsyncOperation aopOperation = (AsyncOperation)objState[1];
			bool booIncludeScreenshot = (bool)objState[2];

			string strFilePage = null;
			try
			{
				strFilePage = RetrievePageResponse(hwrFilePage.EndGetResponse(p_asrResult));
			}
			catch (Exception e)
			{
				strFilePage = "Error";
#if TRACE
				Trace.WriteLine("Problem parsing the version HTTP response from " + hwrFilePage.Address);
				Trace.Indent();
				Trace.WriteLine("Exception: ");
				Trace.WriteLine(e.Message);
				Trace.WriteLine(e.ToString());
				if (e.InnerException != null)
				{
					Trace.WriteLine("Inner Exception: ");
					Trace.WriteLine(e.InnerException.Message);
					Trace.WriteLine(e.InnerException.ToString());
				}
				Trace.Unindent();
				Trace.Flush();
#endif
			}

			ModInfo mifInfo = null;
			if (!"Error".Equals(strFilePage))
				mifInfo = ParseModInfo(hwrFilePage.RequestUri, strFilePage, booIncludeScreenshot);
			else
				mifInfo = new ModInfo("Error", "Error", "Error", hwrFilePage.RequestUri, null);

			aopOperation.PostOperationCompleted((p_mifInfo) => { CallGetFileVersionAsyncCallback((KeyValuePair<Action<object, ModInfo>, object>)aopOperation.UserSuppliedState, (ModInfo)p_mifInfo); }, mifInfo);
		}

		/// <summary>
		/// Calls the callback method for <see cref="GetVersionAsync"/>.
		/// </summary>
		/// <param name="p_kvpCallback">An object whose key is the callback method to call and whose value is the user-supplied state info to pass to the callback method.</param>
		/// <param name="p_mifInfo">The info of the mod that was retrieved in the asynchronous call.</param>
		private void CallGetFileVersionAsyncCallback(KeyValuePair<Action<object, ModInfo>, object> p_kvpCallback, ModInfo p_mifInfo)
		{
			if (p_kvpCallback.Key != null)
				p_kvpCallback.Key(p_kvpCallback.Value, p_mifInfo);
		}

		#endregion

		/// <summary>
		/// Downloads the specified page.
		/// </summary>
		/// <param name="p_booRequiresLogin">Whether or not the page requires the client to be logged in.</param>
		/// <param name="p_strURL">The url of the page to download.</param>
		/// <returns>The requested webpage.</returns>
		protected string GetWebPage(bool p_booRequiresLogin, string p_strURL)
		{
			if (p_booRequiresLogin)
				AssertLoggedIn();

			HttpWebRequest hwrFilePage = (HttpWebRequest)WebRequest.Create(p_strURL);
			hwrFilePage.CookieContainer = m_ckcCookies;
			hwrFilePage.Method = "GET";

			string strPage = RetrievePageResponse(hwrFilePage.GetResponse());
			return strPage;
		}

		/// <summary>
		/// Downloads the specified data..
		/// </summary>
		/// <param name="p_booRequiresLogin">Whether or not the request requires the client to be logged in.</param>
		/// <param name="p_uriURL">The url of the data to download.</param>
		/// <returns>The requested data.</returns>
		protected byte[] DownloadData(bool p_booRequiresLogin, Uri p_uriURL)
		{
			if (p_booRequiresLogin)
				AssertLoggedIn();

			HttpWebRequest hwrFilePage = (HttpWebRequest)WebRequest.Create(p_uriURL);
			hwrFilePage.CookieContainer = m_ckcCookies;
			hwrFilePage.Method = "GET";

			byte[] bteData = null;
			using (WebResponse wrpFilePage = hwrFilePage.GetResponse())
			{
				if (((HttpWebResponse)wrpFilePage).StatusCode != HttpStatusCode.OK)
					throw new HttpException("Request to the page failed with HTTP error: " + ((HttpWebResponse)wrpFilePage).StatusCode);

				bteData = new byte[wrpFilePage.ContentLength];
				using (Stream stmFilePage = wrpFilePage.GetResponseStream())
				{
					Int32 intReadCount = 0;
					while (intReadCount < bteData.Length)
						intReadCount += stmFilePage.Read(bteData, intReadCount, bteData.Length - intReadCount);
				}
				wrpFilePage.Close();
			}
			return bteData;
		}

		/// <summary>
		/// Doanloads the web page from the given <see cref="WebResponse"/>.
		/// </summary>
		/// <param name="p_wrpInfoPage"></param>
		/// <returns></returns>
		private string RetrievePageResponse(WebResponse p_wrpInfoPage)
		{
			string strPage = null;
			using (WebResponse wrpFilePage = p_wrpInfoPage)
			{
				if (((HttpWebResponse)wrpFilePage).StatusCode != HttpStatusCode.OK)
					throw new HttpException("Request to the page failed with HTTP error: " + ((HttpWebResponse)wrpFilePage).StatusCode);

				Stream stmFilePage = wrpFilePage.GetResponseStream();
				using (StreamReader srdFilePage = new StreamReader(stmFilePage))
				{
					strPage = srdFilePage.ReadToEnd();
					srdFilePage.Close();
				}
				wrpFilePage.Close();
			}
			return strPage;
		}
		
		public Int32 IsNexusMod(string p_strWebsite)
		{
			string[] arPatterns = new string[]{
				@"nexus\.com/downloads/file\.php\?id=(?<id>\d+)",
				@"nexusmods\.com/downloads/file\.php\?id=(?<id>\d+)",
				@"nexusmods\.com/mods/(?<id>\d+)",
				@"nexusmods\.com/newvegas/mods/(?<id>\d+)"
			};
			
			Int32 intFileId;
			string strFileId = "-1";
				
			foreach (string pattern in arPatterns) {
				Match match = Regex.Match(p_strWebsite, pattern);
				if (match.Success) {
					strFileId = match.Groups["id"].Value.Trim();
					break;
				}
			}
			
			if (!Int32.TryParse(strFileId, out intFileId))
				intFileId = -1;
					
			return intFileId;
		}
	}
}
