using System;
using System.Collections.Generic;

namespace WebsiteAPIs.API
{
	/// <summary>
	/// Description of TaleOfTwoWastelands.
	/// </summary>
	public class TaleOfTwoWastelands : BaseAPI
	{
		private readonly string[] m_strWebsiteURIPatterns = {
			@"taleoftwowastelands\.com"
		};
		public override string[] WebsiteURIPatterns {
			get {
				return m_strWebsiteURIPatterns;
			}
		}
		
		public TaleOfTwoWastelands()
		{
		}
	}
}
