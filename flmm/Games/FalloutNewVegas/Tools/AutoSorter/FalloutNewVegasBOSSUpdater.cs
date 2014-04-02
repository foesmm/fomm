using Fomm.Games.Fallout3.Tools.AutoSorter;

namespace Fomm.Games.FalloutNewVegas.Tools.AutoSorter
{
  /// <summary>
  /// Utility class that handles checking for, and retreiving, new
  /// version of the load order template.
  /// </summary>
  public class FalloutNewVegasBOSSUpdater : Fallout3BOSSUpdater
  {
    /// <summary>
    /// Gets the URL where the latest masterlist lives.
    /// </summary>
    /// <value>The URL where the latest masterlist lives.</value>
    protected override string MasterListURL
    {
      get
      {
        return Properties.Settings.Default.falloutNewVegasMasterListUpdateUrl;
      }
    }
  }
}