using System;
using System.Windows.Forms;

namespace Fomm.Controls
{
	/// <summary>
	/// A list view that has <see cref="Control.DoubleBuffered"/> set to <lang cref="true"/>.
	/// </summary>
	public class DoubleBufferedListView : ListView
	{
		/// <summary>
		/// The default constructor.
		/// </summary>
		public DoubleBufferedListView()
		{
			this.DoubleBuffered = true;
		}
	}
}
