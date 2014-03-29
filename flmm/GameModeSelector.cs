using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Fomm.Games;

namespace Fomm
{
  /// <summary>
  /// Selects the game for which mods will be managed.
  /// </summary>
  public partial class GameModeSelector : Form
  {
    #region Properties

    /// <summary>
    /// Gets the selected game mode.
    /// </summary>
    /// <value>The selected game mode.</value>
    public SupportedGameModes SelectedGameMode
    {
      get
      {
        if (radFallout3.Checked)
          return SupportedGameModes.Fallout3;
        if (radFalloutNV.Checked)
          return SupportedGameModes.FalloutNV;
        throw new Exception("Unrecognized game selection.");
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// The default constructor.
    /// </summary>
    public GameModeSelector()
    {
      InitializeComponent();
      this.Icon = Properties.Resources.fomm02;
      switch (Properties.Settings.Default.rememberedGameMode)
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
    /// Hanldes the <see cref="Control.Click"/> event of the OK button.
    /// </summary>
    /// <remarks>
    /// This makes the mod manager remember the selected game, if requested.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    private void butOK_Click(object sender, EventArgs e)
    {
      Properties.Settings.Default.rememberGameMode = cbxRemember.Checked;
      Properties.Settings.Default.rememberedGameMode = SelectedGameMode;
      Properties.Settings.Default.Save();
      DialogResult = DialogResult.OK;
    }
  }
}
