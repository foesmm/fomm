using System;
using System.Windows.Forms;
using System.Drawing;

namespace Fomm.Controls
{
	/// <summary>
	/// A page that is injected into the <see cref="SettingsForm"/>.
	/// </summary>
	public class SettingsPage : UserControl
	{
		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		public SettingsPage()
		{
			Text = "Settings";
			BackColor = Color.FromKnownColor(KnownColor.Transparent);
		}

		#endregion

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
		/// <returns><lang cref="true"/> if the changed settings require a programme restart;
		/// <lang cref="false"/> otherwise.</returns>
		public virtual bool SaveSettings()
		{
			return false;
		}

		#endregion
	}
}
