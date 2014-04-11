using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GeMod.Interface;

namespace Fomm.ModSites.Provider
{
	/// <summary>
	/// Description of NexusMods.
	/// </summary>
	public class NexusMods : IBaseAPI
	{
		private readonly string[] _strWebsiteUriPatterns = {
			@"nexus\.com/downloads/file\.php\?id=(?<id>\d+)",
			@"nexusmods\.com/mods/(?<id>\d+)",
			@"nexusmods\.com/newvegas/mods/(?<id>\d+)"
		};

    public bool IsSupported(IFomodInfo modInfo)
    {
      return true;
    }

    public ModVersion GetLatestVersion(IFomodInfo modInfo)
    {
      foreach (string uriPattern in _strWebsiteUriPatterns)
      {
        Match match = Regex.Match(modInfo.Website, uriPattern);
        var i = 0;
      }

      return null;
    }
	}
}
