using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Fomm
{
	/// <summary>
	/// A message box that a remeber last selection checkbox. 
	/// </summary>
	public partial class RememberSelectionMessageBox : Form
	{
		#region Show Methods

		/// <summary>
		/// SHows the message box.
		/// </summary>
		/// <param name="p_ctlParent">The parent of the message box.</param>
		/// <param name="p_strMessage">The message to display.</param>
		/// <param name="p_strCaption">The windows title.</param>
		/// <param name="p_mbbButtons">The buttons to display.</param>
		/// <param name="p_mbiIcon">The icon to display.</param>
		/// <param name="p_booRemember">Indicates whether the selected button should be remembered.</param>
		public static DialogResult Show(Control p_ctlParent, string p_strMessage, string p_strCaption, MessageBoxButtons p_mbbButtons, MessageBoxIcon p_mbiIcon, out bool p_booRemember)
		{
			RememberSelectionMessageBox mbxBox = new RememberSelectionMessageBox();
			mbxBox.Init(p_strMessage, p_strCaption, p_mbbButtons, p_mbiIcon);
			DialogResult drsResult = DialogResult.OK;
			if (p_ctlParent == null)
			{
				mbxBox.StartPosition = FormStartPosition.CenterScreen;
				drsResult = mbxBox.ShowDialog();
			}
			else
				drsResult = mbxBox.ShowDialog(p_ctlParent);
			p_booRemember = mbxBox.RememberSelection;
			return drsResult;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets whether the remember selection checkbox is checked.
		/// </summary>
		/// <value>Whether the remember selection checkbox is checked.</value>
		public bool RememberSelection
		{
			get
			{
				return cbxRemember.Checked;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		protected RememberSelectionMessageBox()
		{
			InitializeComponent();
		}

		#endregion

		/// <summary>
		/// Sets up the form.
		/// </summary>
		/// <param name="p_strMessage">The message to display.</param>
		/// <param name="p_strCaption">The windows title.</param>
		/// <param name="p_mbbButtons">The buttons to display.</param>
		/// <param name="p_mbiIcon">The icon to display.</param>
		protected void Init(string p_strMessage, string p_strCaption, MessageBoxButtons p_mbbButtons, MessageBoxIcon p_mbiIcon)
		{
			bool booShowIcon = true;
			switch (p_mbiIcon)
			{
				case MessageBoxIcon.Information:
					pbxIcon.Image = Image.FromFile(@"C:\Program Files (x86)\Microsoft Visual Studio 9.0\Common7\VS2008ImageLibrary\1033\VS2008ImageLibrary\Objects\png_format\WinVista\info.png");
					break;
				case MessageBoxIcon.Error:
					pbxIcon.Image = Image.FromFile(@"C:\Program Files (x86)\Microsoft Visual Studio 9.0\Common7\VS2008ImageLibrary\1033\VS2008ImageLibrary\Objects\png_format\WinVista\error.png");
					break;
				case MessageBoxIcon.Warning:
					pbxIcon.Image = Image.FromFile(@"C:\Program Files (x86)\Microsoft Visual Studio 9.0\Common7\VS2008ImageLibrary\1033\VS2008ImageLibrary\Objects\png_format\WinVista\warning.png");
					break;
				case MessageBoxIcon.Question:
					pbxIcon.Image = Image.FromFile(@"C:\Program Files (x86)\Microsoft Visual Studio 9.0\Common7\VS2008ImageLibrary\1033\VS2008ImageLibrary\Objects\png_format\WinVista\help.png");
					break;
				case MessageBoxIcon.None:
					booShowIcon = false;
					break;
			}
			if (booShowIcon)
			{
				pbxIcon.MinimumSize = new Size(pbxIcon.Padding.Left + pbxIcon.Padding.Right + pbxIcon.Image.Width, pbxIcon.Padding.Top + pbxIcon.Padding.Bottom + pbxIcon.Image.Height);
				pnlMessage.MinimumSize = new Size(0, pbxIcon.MinimumSize.Height);
			}
			pbxIcon.Visible = booShowIcon;

			Text = p_strCaption;

			Graphics gphGraphics = Graphics.FromHwnd(this.Handle);
			albPrompt.Text = p_strMessage;
			SizeF szeTextSize = gphGraphics.MeasureString(albPrompt.Text, albPrompt.Font);

			Int32 intWindowWidth = (Int32)szeTextSize.Width + (booShowIcon ? pbxIcon.MinimumSize.Width : 0);
			if (intWindowWidth > 460)
				intWindowWidth = 460;
			MinimumSize = new Size(intWindowWidth, 0);
			MaximumSize = new Size(intWindowWidth, Int32.MaxValue);

			if (booShowIcon)
			{
				szeTextSize = gphGraphics.MeasureString(albPrompt.Text, albPrompt.Font, intWindowWidth - pbxIcon.MinimumSize.Width);
				Int32 intLabelPadding = (pbxIcon.MinimumSize.Height - (Int32)szeTextSize.Height) / 2;
				if (intLabelPadding > pnlLabel.Padding.Top)
					pnlLabel.Padding = new Padding(pnlLabel.Padding.Left, intLabelPadding, pnlLabel.Padding.Right, 0);
			}

			Int32 intLastButtonLeft = pnlButtons.Right - 6;
			//cancel button
			switch (p_mbbButtons)
			{
				case MessageBoxButtons.OKCancel:
				case MessageBoxButtons.RetryCancel:
				case MessageBoxButtons.YesNoCancel:
					Button butCancel = new Button();
					butCancel.Text = "Cancel";
					butCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
					butCancel.Location = new Point(intLastButtonLeft - butCancel.Width - 6, 12);
					butCancel.Click += new EventHandler(Button_Click);
					butCancel.Tag = DialogResult.Cancel;
					butCancel.TabIndex = 6;
					pnlButtons.Controls.Add(butCancel);
					intLastButtonLeft = butCancel.Left;
					this.CancelButton = butCancel;
					break;
			}

			//no button
			switch (p_mbbButtons)
			{
				case MessageBoxButtons.YesNo:
				case MessageBoxButtons.YesNoCancel:
					Button butNo = new Button();
					butNo.Text = "No";
					butNo.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
					butNo.Location = new Point(intLastButtonLeft - butNo.Width - 6, 12);
					butNo.Click += new EventHandler(Button_Click);
					butNo.Tag = DialogResult.No;
					butNo.TabIndex = 5;
					intLastButtonLeft = butNo.Left;
					pnlButtons.Controls.Add(butNo);
					if (p_mbbButtons == MessageBoxButtons.YesNo)
						this.CancelButton = butNo;
					break;
			}

			//yes button
			switch (p_mbbButtons)
			{
				case MessageBoxButtons.YesNo:
				case MessageBoxButtons.YesNoCancel:
					Button butYes = new Button();
					butYes.Text = "Yes";
					butYes.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
					butYes.Location = new Point(intLastButtonLeft - butYes.Width - 6, 12);
					butYes.Click += new EventHandler(Button_Click);
					butYes.Tag = DialogResult.Yes;
					butYes.TabIndex = 4;
					intLastButtonLeft = butYes.Left;
					pnlButtons.Controls.Add(butYes);
					this.AcceptButton = butYes;
					break;
			}

			//ok button
			switch (p_mbbButtons)
			{
				case MessageBoxButtons.OK:
				case MessageBoxButtons.OKCancel:
					Button butOk = new Button();
					butOk.Text = "OK";
					butOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
					butOk.Location = new Point(intLastButtonLeft - butOk.Width - 6, 12);
					butOk.Click += new EventHandler(Button_Click);
					butOk.Tag = DialogResult.OK;
					butOk.TabIndex = 3;
					intLastButtonLeft = butOk.Left;
					pnlButtons.Controls.Add(butOk);
					this.AcceptButton = butOk;
					break;
			}

			//ignore button
			switch (p_mbbButtons)
			{
				case MessageBoxButtons.AbortRetryIgnore:
					Button butIgnore = new Button();
					butIgnore.Text = "Ignore";
					butIgnore.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
					butIgnore.Location = new Point(intLastButtonLeft - butIgnore.Width - 6, 12);
					butIgnore.Click += new EventHandler(Button_Click);
					butIgnore.Tag = DialogResult.Ignore;
					butIgnore.TabIndex = 2;
					intLastButtonLeft = butIgnore.Left;
					pnlButtons.Controls.Add(butIgnore);
					this.CancelButton = butIgnore;
					break;
			}

			//retry button
			switch (p_mbbButtons)
			{
				case MessageBoxButtons.AbortRetryIgnore:
				case MessageBoxButtons.RetryCancel:
					Button butRetry = new Button();
					butRetry.Text = "Retry";
					butRetry.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
					butRetry.Location = new Point(intLastButtonLeft - butRetry.Width - 6, 12);
					butRetry.Click += new EventHandler(Button_Click);
					butRetry.Tag = DialogResult.Retry;
					butRetry.TabIndex = 1;
					intLastButtonLeft = butRetry.Left;
					pnlButtons.Controls.Add(butRetry);
					this.AcceptButton = butRetry;
					break;
			}

			//abort button
			switch (p_mbbButtons)
			{
				case MessageBoxButtons.AbortRetryIgnore:
					Button butAbort = new Button();
					butAbort.Text = "Abort";
					butAbort.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
					butAbort.Location = new Point(intLastButtonLeft - butAbort.Width - 6, 12);
					butAbort.Click += new EventHandler(Button_Click);
					butAbort.Tag = DialogResult.Abort;
					butAbort.TabIndex = 0;
					intLastButtonLeft = butAbort.Left;
					pnlButtons.Controls.Add(butAbort);
					this.AcceptButton = butAbort;
					break;
			}
		}

		/// <summary>
		/// Handles the <see cref="Control.Click"/> event of the message box's buttons.
		/// </summary>
		/// <remarks>
		/// This set the appropriate <see cref="DialogResult"/>.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event properties.</param>
		private void Button_Click(object sender, EventArgs e)
		{
			DialogResult = (DialogResult)((Button)sender).Tag;
		}
	}
}
