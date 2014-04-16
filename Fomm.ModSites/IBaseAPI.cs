using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Fomm.Shared;

namespace Fomm.ModSites
{
  /// <summary>
  /// Description of BaseAPI.
  /// </summary>
  public interface IBaseAPI
  {
    bool IsSupported(IFomodInfo p_modInfo);
    ModVersion GetLatestVersion(IFomodInfo p_modInfo);
  }
}
