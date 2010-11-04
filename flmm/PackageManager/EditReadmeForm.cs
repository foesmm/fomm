using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Fomm.PackageManager
{
	public partial class EditReadmeForm : Form
	{
		#region Properties

		/// <summary>
		/// Gets or sets the <see cref="Readme"/> being edited.
		/// </summary>
		/// <value>The <see cref="Readme"/> being edited.</value>
		public Readme Readme
		{
			get
			{
				return redReadmeEditor.Readme;
			}
			set
			{
				redReadmeEditor.Readme = value;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		public EditReadmeForm()
		{
			InitializeComponent();

			Icon = Fomm.Properties.Resources.fomm02;
			Properties.Settings.Default.windowPositions.GetWindowPosition("EditReadmeForm", this);
		}

		#endregion

		/// <summary>
		/// Raises the <see cref="Form.Closing"/> event.
		/// </summary>
		/// <remarks>
		/// Saves the window's position.
		/// </remarks>
		/// <param name="e">A <see cref="CancelEventArgs"/> describing the event arguments.</param>
		protected override void OnClosing(CancelEventArgs e)
		{
			Properties.Settings.Default.windowPositions.SetWindowPosition("EditReadmeForm", this);
			Properties.Settings.Default.Save();
			base.OnClosing(e);
		}

		/// <summary>
		/// Handles the <see cref="Control.Click"/> event of the OK button.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void butOK_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}
	}
}
