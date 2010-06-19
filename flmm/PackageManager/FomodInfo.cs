using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;

namespace Fomm.PackageManager
{
	/// <summary>
	/// Encapsulates the editing of FOMOD info.
	/// </summary>
	public partial class FomodInfo : UserControl
	{
		protected static readonly Regex m_rgxEmail = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?", RegexOptions.Singleline | RegexOptions.CultureInvariant);

		private fomod m_fomodMod = null;
		private byte[] m_bteScreenshot = null;
		private bool m_booIsValid = true;

		#region Properties

		/// <summary>
		/// Sets the fomod whose information is being edited.
		/// </summary>
		/// <value>The fomod whose information is being edited.</value>
		public fomod Fomod
		{
			set
			{
				m_fomodMod = value;
				tbName.Text = value.Name;
				tbAuthor.Text = value.Author;
				tbVersion.Text = value.HumanReadableVersion;
				tbMVersion.Text = value.HumanReadableVersion.ToString();
				tbDescription.Text = value.Description;
				tbWebsite.Text = value.Website;
				tbEmail.Text = value.Email;
				if (value.MinFommVersion == new Version(0, 0, 0, 0))
					tbMinFommVersion.Text = "";
				else tbMinFommVersion.Text = value.MinFommVersion.ToString();

				string[] strGroups = Settings.GetStringArray("fomodGroups");
				clbGroups.SuspendLayout();
				foreach (string strGroup in strGroups)
					clbGroups.Items.Add(strGroup, Array.IndexOf<string>(value.Groups, strGroup.ToLowerInvariant()) != -1);
				clbGroups.ResumeLayout();

				pbxScreenshot.Image = value.GetScreenshot();
			}
		}

		/// <summary>
		/// Gets or sets the name of the mod.
		/// </summary>
		/// <value>The name of the mod.</value>
		public string ModName
		{
			get
			{
				return tbName.Text;
			}
			set
			{
				tbName.Text = value;
			}
		}

		/// <summary>
		/// Gets or sets the author of the mod.
		/// </summary>
		/// <value>The author of the mod.</value>
		public string Author
		{
			get
			{
				return tbAuthor.Text;
			}
			set
			{
				tbAuthor.Text = value;
			}
		}

		/// <summary>
		/// Gets or sets the human readable Version of the mod.
		/// </summary>
		/// <value>The human readable Version of the mod.</value>
		public string Version
		{
			get
			{
				return tbVersion.Text;
			}
			set
			{
				tbVersion.Text = value;
			}
		}

		/// <summary>
		/// Gets or sets the machine Version of the mod.
		/// </summary>
		/// <value>The machine Version of the mod.</value>
		public string MachineVersion
		{
			get
			{
				return tbMVersion.Text;
			}
			set
			{
				tbMVersion.Text = value;
			}
		}

		/// <summary>
		/// Gets or sets the Description of the mod.
		/// </summary>
		/// <value>The Description of the mod.</value>
		public string Description
		{
			get
			{
				return tbDescription.Text;
			}
			set
			{
				tbDescription.Text = value;
			}
		}

		/// <summary>
		/// Gets or sets the Website of the mod.
		/// </summary>
		/// <value>The Website of the mod.</value>
		public string Website
		{
			get
			{
				return tbWebsite.Text;
			}
			set
			{
				tbWebsite.Text = value;
			}
		}

		/// <summary>
		/// Gets or sets the Email of the mod.
		/// </summary>
		/// <value>The Email of the mod.</value>
		public string Email
		{
			get
			{
				return tbEmail.Text;
			}
			set
			{
				tbEmail.Text = value;
			}
		}

		/// <summary>
		/// Gets or sets the required FOMM version of the mod.
		/// </summary>
		/// <value>The required FOMM version of the mod.</value>
		public string RequiredFOMMVersion
		{
			get
			{
				return tbMinFommVersion.Text;
			}
			set
			{
				tbMinFommVersion.Text = value;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		public FomodInfo()
		{
			InitializeComponent();
		}

		#endregion

		#region Screenshot

		/// <summary>
		/// Handles the <see cref="Control.CLick"/> event of the set screenshot button.
		/// </summary>
		/// <remarks>
		/// Sets the screenshot for the fomod.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void butSetScreenshot_Click(object sender, EventArgs e)
		{
			if (ofdScreenshot.ShowDialog() == DialogResult.OK)
			{
				m_bteScreenshot = File.ReadAllBytes(ofdScreenshot.FileName);
				pbxScreenshot.Image = Bitmap.FromFile(ofdScreenshot.FileName);
			}
		}

		/// <summary>
		/// Handles the <see cref="Control.CLick"/> event of the clear screenshot button.
		/// </summary>
		/// <remarks>
		/// Removes the screenshot from the fomod.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void butClearScreenshot_Click(object sender, EventArgs e)
		{
			pbxScreenshot.Image = null;
			m_bteScreenshot = null;
		}

		#endregion

		#region Validation

		/// <summary>
		/// Handles the <see cref="Control.Validating"/> event of the machine version textbox.
		/// </summary>
		/// <remarks>
		/// Ensures that the version is valid, if present.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="CancelEventArgs"/> describing the event arguments.</param>
		private void tbMVersion_Validating(object sender, CancelEventArgs e)
		{
			erpErrors.SetError(tbMVersion, null);
			if (!String.IsNullOrEmpty(tbMVersion.Text))
			{
				try
				{
					new Version(tbMVersion.Text);
				}
				catch
				{
					erpErrors.SetError(tbMVersion, "Invalid version. Must be in #.#.#.# format.");
					m_booIsValid = false;
				}
			}
		}

		/// <summary>
		/// Handles the <see cref="Control.Validating"/> event of the website textbox.
		/// </summary>
		/// <remarks>
		/// Ensures that the URL is valid, if present.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="CancelEventArgs"/> describing the event arguments.</param>
		private void tbWebsite_Validating(object sender, CancelEventArgs e)
		{
			erpErrors.SetError(tbWebsite, null);
			if (!String.IsNullOrEmpty(tbWebsite.Text))
			{
				Uri uri;
				if (!Uri.TryCreate(tbWebsite.Text, UriKind.Absolute, out uri) || uri.IsFile || (uri.Scheme != "http" && uri.Scheme != "https"))
				{
					erpErrors.SetError(tbWebsite, "Invalid web address specified.\nDid you miss the 'http://'?");
					m_booIsValid = false;
				}
			}
		}

		/// <summary>
		/// Handles the <see cref="Control.Validating"/> event of the minimum FOMM version textbox.
		/// </summary>
		/// <remarks>
		/// Ensures that the version is valid, if present.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="CancelEventArgs"/> describing the event arguments.</param>
		private void tbMinFommVersion_Validating(object sender, CancelEventArgs e)
		{
			erpErrors.SetError(tbMinFommVersion, null);
			if (!String.IsNullOrEmpty(tbMinFommVersion.Text))
			{
				Version verVersion = null;
				try
				{
					verVersion = new Version(tbMinFommVersion.Text);
				}
				catch
				{
					erpErrors.SetError(tbMinFommVersion, "Invalid version. Must be in #.#.#.# format.");
					m_booIsValid = false;
					return;
				}
				if (verVersion > Program.MVersion)
				{
					erpErrors.SetError(tbMinFommVersion, "Specified version is newer than this version of FOMM.");
					m_booIsValid = false;
				}
			}
		}

		/// <summary>
		/// Handles the <see cref="Control.Validating"/> event of the email textbox.
		/// </summary>
		/// <remarks>
		/// Ensures that the email is valid, if present.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="CancelEventArgs"/> describing the event arguments.</param>
		private void tbEmail_Validating(object sender, CancelEventArgs e)
		{
			erpErrors.SetError(tbEmail, null);
			if (!String.IsNullOrEmpty(tbEmail.Text) && !m_rgxEmail.IsMatch(tbEmail.Text))
			{
				erpErrors.SetError(tbEmail, "Invalid email address.");
				m_booIsValid = false;
			}
		}

		/// <summary>
		/// Validate the controls on this control.
		/// </summary>
		/// <returns><lang cref="true"/> if all controls passed validation; <lang cref="false"/> otherwise.</returns>
		public bool PerformValidation()
		{
			m_booIsValid=true;
			this.ValidateChildren();
			return m_booIsValid;
		}

		#endregion

		/// <summary>
		/// Saves the edited info to the fomod being edited.
		/// </summary>
		/// <returns><lang cref="false"/> if the info failed validation and was not saved;
		/// <lang cref="true"/> otherwise.</returns>
		/// <exception cref="InvalidOperationException">Thrown if the method is called before <see cref="Fomod"/> is set.</exception>
		public bool SaveInfo()
		{
			if (m_fomodMod == null)
				throw new InvalidOperationException("Save() cannot be called before setting the Fomod being edited.");

			if (!this.ValidateChildren())
				return false;

			if (!String.IsNullOrEmpty(tbVersion.Text) && String.IsNullOrEmpty(tbMVersion.Text))
			{
				erpErrors.SetError(tbMVersion, "Machine must be specified if Version is specified.");
				return false;
			}

			m_fomodMod.Name = tbName.Text;
			m_fomodMod.Author = tbAuthor.Text;
			m_fomodMod.HumanReadableVersion = tbVersion.Text;
			m_fomodMod.Description = tbDescription.Text;
			m_fomodMod.Website = tbWebsite.Text;
			m_fomodMod.Email = tbEmail.Text;
			if (!String.IsNullOrEmpty(tbMinFommVersion.Text))
				m_fomodMod.MinFommVersion = new Version(tbMinFommVersion.Text);
			if (!String.IsNullOrEmpty(tbMVersion.Text))
				m_fomodMod.MachineVersion = new Version(tbMVersion.Text);

			m_fomodMod.Groups = new string[clbGroups.CheckedItems.Count];
			for (Int32 i = 0; i < m_fomodMod.Groups.Length; i++)
				m_fomodMod.Groups[i] = ((string)clbGroups.CheckedItems[i]).ToLowerInvariant();

			m_fomodMod.CommitInfo(pbxScreenshot.Image != null, m_bteScreenshot);

			return true;
		}

		/// <summary>
		/// Raises the <see cref="Control.Resize"/> event.
		/// </summary>
		/// <remarks>
		/// This handle the resizing of the screenshot piture box, as anchoring it screws up the
		/// autoscrolling (don't know why).
		/// </remarks>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			pbxScreenshot.Height = this.ClientSize.Height - pbxScreenshot.Top - 3;
			pbxScreenshot.Width = this.ClientSize.Width - 6;
		}
	}
}
