using System;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.Specialized;

namespace WebsiteAPIs
{
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
		/// The TES Nexus site.
		/// </summary>
		TES,

		/// <summary>
		/// The Dragon Age Nexus site.
		/// </summary>
		DragonAge
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
				switch (m_nxsSite)
				{
					case NexusSite.DragonAge:
						return cclCookies["DANEX_Member"].Value;
					case NexusSite.Fallout3:
						return cclCookies["FO3Nexus_Member"].Value;
					case NexusSite.TES:
						return cclCookies["TESNEX_Member"].Value;
				}
				return null;
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
			switch (p_nxsSite)
			{
				case NexusSite.DragonAge:
					m_strSite = "www.dragonagenexus.com";
					break;
				case NexusSite.Fallout3:
					m_strSite = "www.fallout3nexus.com";
					break;
				case NexusSite.TES:
					m_strSite = "www.tesnexus.com";
					break;
			}
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
			switch (p_nxsSite)
			{
				case NexusSite.DragonAge:
					ckeLoginCookie = new Cookie("DANEX_Member", p_strLoginKey, "/", m_strSite);
					break;
				case NexusSite.Fallout3:
					ckeLoginCookie = new Cookie("FO3Nexus_Member", p_strLoginKey, "/", m_strSite);
					break;
				case NexusSite.TES:
					ckeLoginCookie = new Cookie("TESNEX_Member", p_strLoginKey, "/", m_strSite);
					break;
			}
			m_ckcCookies.Add(ckeLoginCookie);
			m_booLoggedIn = true;
		}

		#endregion

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
			HttpWebRequest hwrLogin = (HttpWebRequest)WebRequest.Create(String.Format("http://{0}/modules/login/do_login.php?server=&redirect=", m_strSite));
			hwrLogin.CookieContainer = m_ckcCookies;
			hwrLogin.Method = "POST";
			hwrLogin.ContentType = "application/x-www-form-urlencoded";

			string strFields = String.Format("user={0}&pass={1}&submit=Login", p_strUsername, p_strPassword);
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

		/// <summary>
		/// Gets the file upload page of the specified mod.
		/// </summary>
		/// <returns>The file upload page of the specified mod.</returns>
		/// <param name="p_intModKey">The key of the mod to manage.</param>
		/// <exception cref="HttpException">Thrown if the upload page can't be accessed.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the API can't log in to the site.</exception>
		protected string GetUploadPage(Int32 p_intModKey)
		{
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
			Regex rgxFileKey = new Regex("\\s+" + p_strFileTitle + "</a>.*?do_deletesinglefile.php\\?id=(\\d+)", RegexOptions.Singleline);
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

		/// <summary>
		/// Gets the current verison of the specified file.
		/// </summary>
		/// <param name="p_intFileId">The id of the file whose version is to be retrieved.</param>
		/// <returns>The current verison of the specified file.</returns>
		/// <exception cref="InvalidOperationException">Thrown if the API can't log in to the site.</exception>
		public string GetFileVersion(Int32 p_intFileId)
		{
			AssertLoggedIn();
			string strURL = String.Format("http://{0}/downloads/file.php?id={1}", m_strSite, p_intFileId);

			HttpWebRequest hwrFilePage = (HttpWebRequest)WebRequest.Create(strURL);
			hwrFilePage.CookieContainer = m_ckcCookies;
			hwrFilePage.Method = "GET";

			string strFilePage = null;
			using (WebResponse wrpFilePage = hwrFilePage.GetResponse())
			{
				if (((HttpWebResponse)wrpFilePage).StatusCode != HttpStatusCode.OK)
					throw new HttpException("Request to the file page failed with HTTP error: " + ((HttpWebResponse)wrpFilePage).StatusCode);

				Stream stmFilePage = wrpFilePage.GetResponseStream();
				using (StreamReader srdFilePage = new StreamReader(stmFilePage))
				{
					strFilePage = srdFilePage.ReadToEnd();
					srdFilePage.Close();
				}
				wrpFilePage.Close();
			}

			string strWebVersion = m_rgxVersion.Match(strFilePage).Groups[1].Value.Trim();
			if (strWebVersion.StartsWith("ver. "))
				strWebVersion = strWebVersion.Substring(5);
			else if (strWebVersion.StartsWith("ver."))
				strWebVersion = strWebVersion.Substring(4);
			else if (strWebVersion.StartsWith("v. "))
				strWebVersion = strWebVersion.Substring(3);
			else if (strWebVersion.StartsWith("v."))
				strWebVersion = strWebVersion.Substring(2);
			else if (strWebVersion.StartsWith("v"))
				strWebVersion = strWebVersion.Substring(1);

			return strWebVersion;
		}

		/// <summary>
		/// Gets the current verison of the specified file in an asynchronous request.
		/// </summary>
		/// <remarks>
		/// The given <see cref="Action{object, string}"/> is called upon completion of the retrieval of the
		/// file version. The first parameter passed to the delegat is the user-supplied state. The second
		/// parameter passed to the delegate is the string representation of the file version.
		/// </remarks>
		/// <param name="p_intFileId">The id of the file whose version is to be retrieved.</param>
		/// <param name="p_actCallback">The method to call upon completion of the request.</param>
		/// <param name="p_objState">User supplied state.</param>
		/// <exception cref="InvalidOperationException">Thrown if the API can't log in to the site.</exception>
		public void GetFileVersionAsync(Int32 p_intFileId, Action<object, string> p_actCallback, object p_objState)
		{
			AssertLoggedIn();
			string strURL = String.Format("http://{0}/downloads/file.php?id={1}", m_strSite, p_intFileId);

			HttpWebRequest hwrFilePage = (HttpWebRequest)WebRequest.Create(strURL);
			hwrFilePage.CookieContainer = m_ckcCookies;
			hwrFilePage.Method = "GET";

			AsyncOperation aopOperation = AsyncOperationManager.CreateOperation(new KeyValuePair<Action<object, string>, object>(p_actCallback, p_objState));
			hwrFilePage.BeginGetResponse(GetFilePageCallback, new object[] { hwrFilePage, aopOperation });
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

			string strFilePage = null;
			using (WebResponse wrpFilePage = hwrFilePage.EndGetResponse(p_asrResult))
			{
				if (((HttpWebResponse)wrpFilePage).StatusCode != HttpStatusCode.OK)
					throw new HttpException("Request to the file page failed with HTTP error: " + ((HttpWebResponse)wrpFilePage).StatusCode);

				Stream stmFilePage = wrpFilePage.GetResponseStream();
				using (StreamReader srdFilePage = new StreamReader(stmFilePage))
				{
					strFilePage = srdFilePage.ReadToEnd();
					srdFilePage.Close();
				}
				wrpFilePage.Close();
			}

			string strWebVersion = m_rgxVersion.Match(strFilePage).Groups[1].Value.Trim();
			if (strWebVersion.StartsWith("ver. "))
				strWebVersion = strWebVersion.Substring(5);
			else if (strWebVersion.StartsWith("ver."))
				strWebVersion = strWebVersion.Substring(4);
			else if (strWebVersion.StartsWith("v. "))
				strWebVersion = strWebVersion.Substring(3);
			else if (strWebVersion.StartsWith("v."))
				strWebVersion = strWebVersion.Substring(2);
			else if (strWebVersion.StartsWith("v"))
				strWebVersion = strWebVersion.Substring(1);

			aopOperation.PostOperationCompleted((p_WebVersion) => { CallGetFileVersionAsyncCallback((KeyValuePair<Action<object, string>, object>)aopOperation.UserSuppliedState, (string)p_WebVersion); }, strWebVersion);
		}

		/// <summary>
		/// Calls the callback method for <see cref="GetVersionAsync"/>.
		/// </summary>
		/// <param name="p_kvpCallback">An object whose key is the callback method to call and whose value is the user-supplied state info to pass to the callback method.</param>
		/// <param name="p_strWebVersion">The version of the file that was retrieved in the asynchronous call.</param>
		private void CallGetFileVersionAsyncCallback(KeyValuePair<Action<object, string>, object> p_kvpCallback, string p_strWebVersion)
		{
			if (p_kvpCallback.Key != null)
				p_kvpCallback.Key(p_kvpCallback.Value, p_strWebVersion);
		}
	}
}
