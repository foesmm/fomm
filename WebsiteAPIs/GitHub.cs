
using System;
using CsQuery;

namespace WebsiteAPIs
{
	/// <summary>
	/// Description of GitHub.
	/// </summary>
	public static class GitHub
	{
		public static string APIURI
		{
			get
			{
				return "https://api.github.com/";
			}
		}
		
		public static string FommRepository
		{
			get
			{
				return "niveuseverto/fomm";
			}
		}
		
		public static string ProjectPathURI
		{
			get
			{
				return "https://github.com/niveuseverto/fomm";
			}
		}
		
		public static string LatestReleaseURI
		{
			get
			{
				return string.Format("{0}/releases/latest", ProjectPathURI);
			}
		}
		
		public static Version GetLatestReleaseVersion()
		{
			string strVersionPage = null;
			using (System.Net.WebClient wclGetter = new System.Net.WebClient())
			{
				strVersionPage = wclGetter.DownloadString(WebsiteAPIs.GitHub.LatestReleaseURI);
			}
			
			if (!String.IsNullOrEmpty(strVersionPage)) {
				CQ dom = strVersionPage;
				
				string strVersion = dom["h1.release-title > a"].Text().Replace("FOMM v", "");
				
				Version ver = null;
				if (Version.TryParse(strVersion, out ver))
					return ver;
			}
			
			return null;
		}
	}
}
