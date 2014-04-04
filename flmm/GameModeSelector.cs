using System;
using System.Windows.Forms;
using Fomm.Games;
using Fomm.Properties;

namespace Fomm
{
  /// <summary>
  ///   Selects the game for which mods will be managed.
  /// </summary>
  public partial class GameModeSelector : Form
  {
    #region Properties

    /// <summary>
    ///   Gets the selected game mode.
    /// </summary>
    /// <value>The selected game mode.</value>
    public SupportedGameModes SelectedGameMode
    {
      get
      {
        if (radFallout3.Checked)
        {
          return SupportedGameModes.Fallout3;
        }
        if (radFalloutNV.Checked)
        {
          return SupportedGameModes.FalloutNV;
        }
        throw new Exception("Unrecognized game selection.");
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    ///   The default constructor.
    /// </summary>
    public GameModeSelector()
    {
      InitializeComponent();
      Icon = Resources.fomm02;
      switch (Settings.Default.rememberedGameMode)
      {
        case SupportedGameModes.Fallout3:
          radFallout3.Checked = true;
          break;
        case SupportedGameModes.FalloutNV:
          radFalloutNV.Checked = true;
          break;
        default:
          throw new Exception("Unrecozied value for SupportedGameModes");
      }
    }

    #endregion

    /// <summary>
    ///   Hanldes the <see cref="Control.Click" /> event of the OK button.
    /// </summary>
    /// <remarks>
    ///   This makes the mod manager remember the selected game, if requested.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs" /> describing the event arguments.</param>
    private void butOK_Click(object sender, EventArgs e)
    {
      Settings.Default.rememberGameMode = cbxRemember.Checked;
      Settings.Default.rememberedGameMode = SelectedGameMode;
      Settings.Default.Save();
      DialogResult = DialogResult.OK;
    }
  }
}