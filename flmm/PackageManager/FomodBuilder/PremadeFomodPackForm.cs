using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace Fomm.PackageManager.FomodBuilder
{
	/// <summary>
	/// Gathers information required to make a FOMod from a Premade FOMod Pack (PFP).
	/// </summary>
	public partial class PremadeFomodPackForm : Form
	{
		#region Properties

		/// <summary>
		/// Gets or sets the path to the selected PFP.
		/// </summary>
		/// <value>The path to the selected PFP.</value>
		public string PFPPath
		{
			get
			{
				return tbxPFP.Text;
			}
			set
			{
				tbxPFP.Text = value;
			}
		}

		/// <summary>
		/// Gets or sets the path to the directory containing the source files
		/// required for the PFP.
		/// </summary>
		/// <value>The path to the directory containing the source files
		/// required for the PFP.</value>
		public string SourcesPath
		{
			get
			{
				return tbxSources.Text;
			}
			set
			{
				tbxSources.Text = value;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		public PremadeFomodPackForm()
		{
			InitializeComponent();

			Icon = Fomm.Properties.Resources.fomm02;
			ofdPFP.InitialDirectory = Settings.GetString("LastPFPPath");
			fbdSources.SelectedPath = Settings.GetString("LastPFPSourcesPath");
		}

		#endregion

		/// <summary>
		/// Hanldes the <see cref="Control.Click"/> event of the select PFP
		/// button.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void butPFP_Click(object sender, EventArgs e)
		{
			if (ofdPFP.ShowDialog(this) == DialogResult.OK)
				tbxPFP.Text = ofdPFP.FileName;
		}

		/// <summary>
		/// Hanldes the <see cref="Control.Click"/> event of the select source folder
		/// button.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void butSources_Click(object sender, EventArgs e)
		{
			if (fbdSources.ShowDialog(this) == DialogResult.OK)
				tbxSources.Text = fbdSources.SelectedPath;
		}

		/// <summary>
		/// Validates the selected paths.
		/// </summary>
		/// <remarks>
		/// This makes sure the selected PFP file is a valid PFP, and that all required sources are present.
		/// </remarks>
		/// <returns><lang cref="true"/> if the selected paths are valid; <lang cref="false"/> otherwise.</returns>
		protected bool ValidateFiles()
		{
			erpError.Clear();
			if (String.IsNullOrEmpty(tbxPFP.Text))
			{
				erpError.SetError(butPFP,"You must specify a PFP.");
				return false;
			}
			if (!File.Exists(tbxPFP.Text))
			{
				erpError.SetError(butPFP,"File does not exist.");
				return false;
			}

			string strError = PremadeFomodPack.ValidatePFP(tbxPFP.Text);			
			if (!String.IsNullOrEmpty(strError))
			{
				erpError.SetError(butPFP, "File is not a valid PFP: " + strError);
				return false;
			}
			PremadeFomodPack pfpPack = new PremadeFomodPack(tbxPFP.Text);
			List<KeyValuePair<string, string>> lstSources = pfpPack.GetSources();
			if (lstSources.Count > 0)
			{
				if (String.IsNullOrEmpty(tbxSources.Text))
				{
					erpError.SetError(butSources, "You must specify a folder containing the required source files.");
					return false;
				}
				if (!Directory.Exists(tbxSources.Text))
				{
					erpError.SetError(butSources, "Folder does not exist.");
					return false;
				}
				Dictionary<string, string> dicMissingSources = new Dictionary<string, string>();
				foreach (KeyValuePair<string, string> kvpSource in lstSources)
					if (!File.Exists(Path.Combine(tbxSources.Text, kvpSource.Key)))
						dicMissingSources[kvpSource.Key] = kvpSource.Value;
				if (dicMissingSources.Count > 0)
				{
					erpError.SetError(butSources, "Missing sources.");

					StringBuilder stbErrorHtml = new StringBuilder("<html><body bgcolor=\"");
					stbErrorHtml.AppendFormat("#{0:x6}", Color.FromKnownColor(KnownColor.Control).ToArgb() & 0x00ffffff);
					stbErrorHtml.Append("\">The following sources are missing:<ul>");
					foreach (KeyValuePair<string,string> kvpSource in dicMissingSources)
						stbErrorHtml.Append("<li><a href=\"").Append(kvpSource.Value).Append("\">").Append(kvpSource.Key).Append("</a></li>");
					stbErrorHtml.Append("</ul></body></html>");
					ShowHTML(stbErrorHtml.ToString());
					return false;
				}
			}			

			return true;
		}

		/// <summary>
		/// Shows an HTML page.
		/// </summary>
		protected void ShowHTML(string p_strHTML)
		{
			Form frmHTMLPreview = new Form();
			frmHTMLPreview.Icon = Fomm.Properties.Resources.fomm02;
			frmHTMLPreview.ShowInTaskbar = false;
			frmHTMLPreview.StartPosition = FormStartPosition.CenterParent;
			WebBrowser wbrBrowser = new WebBrowser();
			frmHTMLPreview.Controls.Add(wbrBrowser);
			wbrBrowser.Dock = DockStyle.Fill;
			frmHTMLPreview.Text = "Missing Sources";
			wbrBrowser.WebBrowserShortcutsEnabled = false;
			wbrBrowser.AllowWebBrowserDrop = false;
			wbrBrowser.DocumentText = p_strHTML;
			wbrBrowser.Navigating += ((s, e) =>
			{
				e.Cancel = true;
				System.Diagnostics.Process.Start(e.Url.ToString());
			});
			frmHTMLPreview.ShowDialog(this);
		}

		private void butOK_Click(object sender, EventArgs e)
		{
			Settings.SetString("LastPFPPath", Path.GetDirectoryName(tbxPFP.Text));
			Settings.SetString("LastPFPSourcesPath", Path.GetDirectoryName(tbxSources.Text));
			if (ValidateFiles())
				DialogResult = DialogResult.OK;
		}
	}
}
