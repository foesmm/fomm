using System;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;

namespace Fomm.AutoSorter
{
	/// <summary>
	/// Utility class that handles checking for, and retreiving, new
	/// version of the load order template.
	/// </summary>
	public class BOSSUpdater
	{
		private const string MASTERLIST_VERSION_URL = "http://better-oblivion-sorting-software.googlecode.com/svn/FO3Masterlist/";
		private const string MASTERLIST_URL = "http://better-oblivion-sorting-software.googlecode.com/svn/FO3Masterlist/masterlist.txt";
		private static Regex m_rgxVersion = new Regex(@"Revision (\d+): /FO3Masterlist");
	
		/// <summary>
		/// Gets the current verison of the BOSS Fallout 3 Masterlist.
		/// </summary>
		/// <returns>The current verison of the BOSS Fallout 3 Masterlist.</returns>
		public static Int32 GetMasterlistVersion()
		{
			string strVersionPage = null;
			using (WebClient wclGetter = new WebClient())
			{
				strVersionPage = wclGetter.DownloadString(MASTERLIST_VERSION_URL);
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
				strMasterlist = wclGetter.DownloadString(MASTERLIST_URL).Substring(3);
			}
			File.WriteAllText(p_strPath, GetMasterlistVersion().ToString() + strMasterlist);
			LoadOrderSorter.LoadList();
		}
	}
}
