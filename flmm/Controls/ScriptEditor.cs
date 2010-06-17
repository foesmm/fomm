using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Fomm.Controls
{
	/// <summary>
	/// Text editor for the script of a fomod.
	/// </summary>
	public partial class ScriptEditor : UserControl
	{
		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		public ScriptEditor()
		{
			InitializeComponent();
		}

		#endregion

		/// <summary>
		/// Checks the syntax of the script.
		/// </summary>
		protected void CheckSyntax()
		{
			string stdout;
			string errors = Fomm.PackageManager.ScriptCompiler.CheckSyntax(cedEditor.Text, out stdout);
			if (errors != null)
			{
				MessageBox.Show(errors);
			}
			else
			{
				MessageBox.Show("No errors found");
			}
		}

		/// <summary>
		/// Handles the <see cref="Control.Click"/> event of the check syntax button.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event argument.</param>
		private void tsbCheckSyntax_Click(object sender, EventArgs e)
		{
			CheckSyntax();
		}
	}
}
