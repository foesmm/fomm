using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Fomm.PackageManager
{
	/// <summary>
	/// A form that gathers login credentials.
	/// </summary>
	public partial class LoginForm : Form
	{
		#region  Properties

		/// <summary>
		/// Gets or sets the username on the form.
		/// </summary>
		/// <value>The username on the form.</value>
		public string Username
		{
			get
			{
				return tbxUsername.Text;
			}
			set
			{
				tbxUsername.Text = value;
			}
		}

		/// <summary>
		/// Gets the password on the form.
		/// </summary>
		/// <value>The password on the form.</value>
		public string Password
		{
			get
			{
				return tbxPassword.Text;
			}
		}

		/// <summary>
		/// Gets or sets whether the user has selected to stay logged in.
		/// </summary>
		/// <value>Whether the user has selected to stay logged in.</value>
		public bool StayLoggedIn
		{
			get
			{
				return ckbStayLoggedIn.Checked;
			}
			set
			{
				ckbStayLoggedIn.Checked = value;
			}
		}

		/// <summary>
		/// Sets the error message to display.
		/// </summary>
		/// <remarks>
		/// If the given value is <lang cref="null"/> or empty,
		/// then no message is displayed.
		/// </remarks>
		/// <value>The error message to display.</value>
		public string ErrorMessage
		{
			set
			{
				lblError.Text = value;
				lblError.Visible = !String.IsNullOrEmpty(value);
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor the initializes the object with the given values.
		/// </summary>
		/// <param name="p_strMessage">The prompt message to use.</param>
		public LoginForm(string p_strMessage)
		{
			InitializeComponent();
			lblPrompt.Text = p_strMessage;		
		}

		/// <summary>
		/// A simple constructor the initializes the object with the given values.
		/// </summary>
		/// <param name="p_strMessage">The prompt message to use.</param>
		/// <param name="p_strUsername">The username on the form.</param>
		public LoginForm(string p_strMessage, string p_strUsername)
			: this(p_strMessage)
		{
			Username = p_strUsername;
		}

		#endregion

		/// <summary>
		/// Handles the <see cref="Button.Click"/> event of the login button.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void butLogin_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}
	}
}
