using System;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;

namespace Fomm.Games.Fallout3.Tools.AutoSorter
{
	/// <summary>
	/// Utility class that handles checking for, and retreiving, new
	/// version of the load order template.
	/// </summary>
	public class BOSSUpdater
	{
		private const string MASTERLIST_URL = "http://better-oblivion-sorting-software.googlecode.com/svn/data/boss-fallout/masterlist.txt";
		private static Regex m_rgxVersion = new Regex(@"Revision (\d+): ");

		public static string MasterListUrl
		{
			get
			{
				string strMasterListUrl = Settings.GetString("MasterListUpdateUrl");
				if (String.IsNullOrEmpty(strMasterListUrl))
					strMasterListUrl = MASTERLIST_URL;
				return strMasterListUrl;
			}
		}

		/// <summary>
		/// Gets the current verison of the BOSS Fallout 3 Masterlist.
		/// </summary>
		/// <returns>The current verison of the BOSS Fallout 3 Masterlist.</returns>
		public static Int32 GetMasterlistVersion()
		{
			string strVersionPage = null;
			using (WebClient wclGetter = new WebClient())
			{
				string strMasterListUrl = MasterListUrl;
				Int32 intLastDividerPos = strMasterListUrl.LastIndexOfAny(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
				string strVersionUrl = strMasterListUrl.Substring(0, intLastDividerPos);
				strVersionPage = wclGetter.DownloadString(strVersionUrl);
			}

			string strWebVersion = m_rgxVersion.Match(strVersionPage).Groups[1].Value.Trim();
			return Int32.Parse(strWebVersion);
		}

		/// <summary>
		/// Updates the BOSS Fallout 3 Masterlist used by FOMM.
		/// </summary>
		public static void UpdateMasterlist(string p_strPath)
		{
			string strMasterlist = null;
			using (WebClient wclGetter = new WebClient())
			{
				//the substring is to remove the 3byte EFBBBF Byte Order Mark (BOM)
				strMasterlist = wclGetter.DownloadString(MasterListUrl).Substring(3);
			}
			File.WriteAllText(p_strPath, GetMasterlistVersion().ToString() + Environment.NewLine + strMasterlist);
			LoadOrderSorter.LoadList();
		}
	}
}
