using System;
using System.Collections.Generic;

namespace WebsiteAPIs.API
{
	/// <summary>
	/// Description of NexusMods.
	/// </summary>
	public class NexusMods : BaseAPI
	{
		private readonly string[] m_strWebsiteURIPatterns = {
			@"nexus\.com/downloads/file\.php\?id=(?<id>\d+)",
			@"nexusmods\.com/mods/(?<id>\d+)",
			@"nexusmods\.com/newvegas/mods/(?<id>\d+)"
		};
		public override string[] WebsiteURIPatterns {
			get {
				return m_strWebsiteURIPatterns;
			}
		}
		
		public NexusMods()
		{
		}
	}
}
