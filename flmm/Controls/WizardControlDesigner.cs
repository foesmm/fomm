using System.Drawing;

namespace Fomm.Controls
{
  /// <summary>
  /// The designer that controls how the <see cref="WizardControl"/> behaves
  /// and is designed.
  /// </summary>
  public class WizardControlDesigner : VerticalTabControlDesigner
  {
    /// <summary>
    /// Determines of the control should respond to a mouse click.
    /// </summary>
    /// <param name="point">The point where the mouse was clicked.</param>
    /// <returns><lang cref="true"/> if the designed control should process the mouse click;
    /// <lang cref="false"/> otherwise.</returns>
    protected override bool GetHitTest(Point point)
    {
      if (base.GetHitTest(point))
      {
        return true;
      }

      var wizWizardControl = (WizardControl) Control;
      if (wizWizardControl.PreviousButton.ClientRectangle.Contains(wizWizardControl.PreviousButton.PointToClient(point)))
      {
        return true;
      }
      if (wizWizardControl.NextButton.ClientRectangle.Contains(wizWizardControl.NextButton.PointToClient(point)))
      {
        return true;
      }
      return false;
    }
  }
}