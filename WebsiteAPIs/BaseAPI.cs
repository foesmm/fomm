using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WebsiteAPIs
{
	/// <summary>
	/// Description of BaseAPI.
	/// </summary>
	abstract public class BaseAPI
	{
		public BaseAPI()
		{
		}
		
		abstract public string[] WebsiteURIPatterns
		{
			get;
		}
		
		public bool SupportingWebsiteURI(string p_strWebsite)
		{
			bool bFound = false;
			
			foreach (string pattern in WebsiteURIPatterns) {
				bFound |= Regex.Match(p_strWebsite, pattern).Success;
			}
			
			return bFound;
		}
	}
}
