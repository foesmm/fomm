using System;
using System.Collections.Generic;
using GeMod.Interface;

namespace Fomm.ModSites
{
	/// <summary>
	/// Description of DataProviderManager.
	/// </summary>
	public class DataProviderManager
	{
		private readonly List<IBaseAPI> _providerList = new List<IBaseAPI>();
		
		public DataProviderManager()
		{
			_providerList.Add(new Provider.NexusMods());
			_providerList.Add(new Provider.TaleOfTwoWastelands());
		}

    public IBaseAPI SelectProvider(IFomodInfo modInfo)
		{
      IBaseAPI result = _providerList[0];

			foreach (IBaseAPI api in _providerList) {
        if (api.IsSupported(modInfo))
        {
          result = api;
          break;
        }
			}
			
			return result;
		}

	  public ModInfo GetRemoteInfo(IFomodInfo fomodInfo)
	  {
	    IBaseAPI api = SelectProvider(fomodInfo);
	    ModVersion version = api.GetLatestVersion(fomodInfo);

	    return null;
	  }
	}
}
