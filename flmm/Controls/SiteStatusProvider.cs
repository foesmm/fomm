using System.Windows.Forms;

namespace Fomm.Controls
{
  /// <summary>
  ///   A provider that allows the display of a status at a site specified by the control
  ///   upon which the status is being set.
  /// </summary>
  public class SiteStatusProvider : ErrorProvider
  {
    /// <summary>
    ///   Sets the status on the given control.
    /// </summary>
    /// <remarks>
    ///   This method is a synonym for <see cref="SetError(Control, string)" />.
    /// </remarks>
    /// <param name="p_ctlControl">Control</param>
    /// <param name="p_strMessage">The status message to display for the control.</param>
    /// <seealso cref="SetError(Control, string)" />
    public void SetStatus(Control p_ctlControl, string p_strMessage)
    {
      var ctlSite = p_ctlControl;
      while (ctlSite is IStatusProviderAware)
      {
        ctlSite = ((IStatusProviderAware) ctlSite).StatusProviderSite;
      }
      base.SetError(ctlSite, p_strMessage);
    }

    /// <summary>
    ///   Sets the status on the given control.
    /// </summary>
    /// <remarks>
    ///   This method is a synonym for <see cref="SetStatus(Control, string)" />.
    /// </remarks>
    /// <param name="p_ctlControl">Control</param>
    /// <param name="p_strMessage">The status message to display for the control.</param>
    /// <seealso cref="SetStatus(Control, string)" />
    public new void SetError(Control p_ctlControl, string p_strMessage)
    {
      var ctlSite = p_ctlControl;
      while (ctlSite is IStatusProviderAware)
      {
        ctlSite = ((IStatusProviderAware) ctlSite).StatusProviderSite;
      }
      base.SetError(ctlSite, p_strMessage);
    }
  }
}