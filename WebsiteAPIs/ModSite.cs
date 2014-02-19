using System;
using System.Collections.Generic;
using Fomm.Games;

namespace WebsiteAPIs
{
	/// <summary>
	/// Description of BaseModSite.
	/// </summary>
	public class ModSite
	{
		private List<SupportedGameModes> m_listSupportedGames = null;
		
		public ModSite()
		{
			m_listSupportedGames.AddRange({SupportedGameModes.Fallout3, SupportedGameModes.FalloutNV});
		}
	}
}
