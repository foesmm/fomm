using System.Windows.Forms;

namespace Fomm.Controls
{
  /// <summary>
  /// A page that is injected into the <see cref="SettingsForm"/>.
  /// </summary>
  public class SettingsPage : UserControl
  {
    #region Settings Management

    /// <summary>
    /// Loads the settings into the page's controls.
    /// </summary>
    public virtual void LoadSettings()
    {
    }

    /// <summary>
    /// Persists the settings from the page's controls.
    /// </summary>
    /// <returns><lang langref="true"/> if ettings were saved;
    /// <lang langref="false"/> otherwise.</returns>
    public virtual bool SaveSettings()
    {
      return false;
    }

    #endregion
  }
}