using System;
using System.Collections.Generic;

namespace WebsiteAPIs
{
	/// <summary>
	/// Description of APIManager.
	/// </summary>
	public class APIManager
	{
		private List<BaseAPI> m_listAPI = new List<BaseAPI>();
		
		public APIManager()
		{
			m_listAPI.Add(new API.NexusMods());
			m_listAPI.Add(new API.TaleOfTwoWastelands());
		}
		
		public BaseAPI APIForWebsite(string p_strWebsite)
		{
			foreach (BaseAPI api in m_listAPI) {
				if (api.SupportingWebsiteURI(p_strWebsite))
					return api;
			}
			
			return null;
		}
	}
}
