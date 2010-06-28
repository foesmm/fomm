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
		}

		#endregion

		private void butOK_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}
	}
}
