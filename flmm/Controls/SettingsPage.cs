using System;
using System.Windows.Forms;

namespace Fomm.Controls
{
	public abstract class SettingsPage : UserControl
	{
		public abstract void Load();
		public abstract void Save();
	}
}
