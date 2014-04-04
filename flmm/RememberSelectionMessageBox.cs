using System;
using System.Drawing;
using System.Windows.Forms;
using Fomm.Properties;

namespace Fomm
{
  /// <summary>
  ///   A message box that a remeber last selection checkbox.
  /// </summary>
  public partial class RememberSelectionMessageBox : Form
  {
    #region Show Methods

    /// <summary>
    ///   SHows the message box.
    /// </summary>
    /// <param name="p_ctlParent">The parent of the message box.</param>
    /// <param name="p_strMessage">The message to display.</param>
    /// <param name="p_strCaption">The windows title.</param>
    /// <param name="p_mbbButtons">The buttons to display.</param>
    /// <param name="p_mbiIcon">The icon to display.</param>
    /// <param name="p_booRemember">Indicates whether the selected button should be remembered.</param>
    public static DialogResult Show(Control p_ctlParent, string p_strMessage, string p_strCaption,
                                    MessageBoxButtons p_mbbButtons, MessageBoxIcon p_mbiIcon, out bool p_booRemember)
    {
      var mbxBox = new RememberSelectionMessageBox();
      mbxBox.Init(p_strMessage, p_strCaption, p_mbbButtons, p_mbiIcon);
      DialogResult drsResult;
      if (p_ctlParent == null)
      {
        mbxBox.StartPosition = FormStartPosition.CenterScreen;
        drsResult = mbxBox.ShowDialog();
      }
      else
      {
        drsResult = mbxBox.ShowDialog(p_ctlParent);
      }
      p_booRemember = mbxBox.RememberSelection;
      return drsResult;
    }

    #endregion

    #region Properties

    /// <summary>
    ///   Gets whether the remember selection checkbox is checked.
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
    ///   The default constructor.
    /// </summary>
    protected RememberSelectionMessageBox()
    {
      InitializeComponent();
    }

    #endregion

    /// <summary>
    ///   Sets up the form.
    /// </summary>
    /// <param name="p_strMessage">The message to display.</param>
    /// <param name="p_strCaption">The windows title.</param>
    /// <param name="p_mbbButtons">The buttons to display.</param>
    /// <param name="p_mbiIcon">The icon to display.</param>
    protected void Init(string p_strMessage, string p_strCaption, MessageBoxButtons p_mbbButtons,
                        MessageBoxIcon p_mbiIcon)
    {
      var booShowIcon = true;
      switch (p_mbiIcon)
      {
        case MessageBoxIcon.Information:
          pbxIcon.Image = Resources.info;
          break;
        case MessageBoxIcon.Error:
          pbxIcon.Image = Resources.error;
          break;
        case MessageBoxIcon.Warning:
          pbxIcon.Image = Resources.Warning;
          break;
        case MessageBoxIcon.Question:
          pbxIcon.Image = Resources.help;
          break;
        case MessageBoxIcon.None:
          booShowIcon = false;
          break;
      }
      if (booShowIcon)
      {
        pbxIcon.MinimumSize = new Size(pbxIcon.Padding.Left + pbxIcon.Padding.Right + pbxIcon.Image.Width,
                                       pbxIcon.Padding.Top + pbxIcon.Padding.Bottom + pbxIcon.Image.Height);
        pnlMessage.MinimumSize = new Size(0, pbxIcon.MinimumSize.Height);
      }
      pbxIcon.Visible = booShowIcon;

      Text = p_strCaption;

      var gphGraphics = Graphics.FromHwnd(Handle);
      albPrompt.Text = p_strMessage;
      var szeTextSize = gphGraphics.MeasureString(albPrompt.Text, albPrompt.Font);

      var intWindowWidth = (Int32) szeTextSize.Width + (booShowIcon ? pbxIcon.MinimumSize.Width : 0);
      if (intWindowWidth > 460)
      {
        intWindowWidth = 460;
      }
      MinimumSize = new Size(intWindowWidth, 0);
      MaximumSize = new Size(intWindowWidth, Int32.MaxValue);

      if (booShowIcon)
      {
        szeTextSize = gphGraphics.MeasureString(albPrompt.Text, albPrompt.Font,
                                                intWindowWidth - pbxIcon.MinimumSize.Width);
        var intLabelPadding = (pbxIcon.MinimumSize.Height - (Int32) szeTextSize.Height)/2;
        if (intLabelPadding > pnlLabel.Padding.Top)
        {
          pnlLabel.Padding = new Padding(pnlLabel.Padding.Left, intLabelPadding, pnlLabel.Padding.Right, 0);
        }
      }

      var intLastButtonLeft = pnlButtons.Right - 6;
      //cancel button
      switch (p_mbbButtons)
      {
        case MessageBoxButtons.OKCancel:
        case MessageBoxButtons.RetryCancel:
        case MessageBoxButtons.YesNoCancel:
          var butCancel = new Button();
          butCancel.Text = "Cancel";
          butCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
          butCancel.Location = new Point(intLastButtonLeft - butCancel.Width - 6, 12);
          butCancel.Click += Button_Click;
          butCancel.Tag = DialogResult.Cancel;
          butCancel.TabIndex = 6;
          pnlButtons.Controls.Add(butCancel);
          intLastButtonLeft = butCancel.Left;
          CancelButton = butCancel;
          break;
      }

      //no button
      switch (p_mbbButtons)
      {
        case MessageBoxButtons.YesNo:
        case MessageBoxButtons.YesNoCancel:
          var butNo = new Button();
          butNo.Text = "No";
          butNo.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
          butNo.Location = new Point(intLastButtonLeft - butNo.Width - 6, 12);
          butNo.Click += Button_Click;
          butNo.Tag = DialogResult.No;
          butNo.TabIndex = 5;
          intLastButtonLeft = butNo.Left;
          pnlButtons.Controls.Add(butNo);
          if (p_mbbButtons == MessageBoxButtons.YesNo)
          {
            CancelButton = butNo;
          }
          break;
      }

      //yes button
      switch (p_mbbButtons)
      {
        case MessageBoxButtons.YesNo:
        case MessageBoxButtons.YesNoCancel:
          var butYes = new Button();
          butYes.Text = "Yes";
          butYes.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
          butYes.Location = new Point(intLastButtonLeft - butYes.Width - 6, 12);
          butYes.Click += Button_Click;
          butYes.Tag = DialogResult.Yes;
          butYes.TabIndex = 4;
          intLastButtonLeft = butYes.Left;
          pnlButtons.Controls.Add(butYes);
          AcceptButton = butYes;
          break;
      }

      //ok button
      switch (p_mbbButtons)
      {
        case MessageBoxButtons.OK:
        case MessageBoxButtons.OKCancel:
          var butOk = new Button();
          butOk.Text = "OK";
          butOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
          butOk.Location = new Point(intLastButtonLeft - butOk.Width - 6, 12);
          butOk.Click += Button_Click;
          butOk.Tag = DialogResult.OK;
          butOk.TabIndex = 3;
          intLastButtonLeft = butOk.Left;
          pnlButtons.Controls.Add(butOk);
          AcceptButton = butOk;
          break;
      }

      //ignore button
      switch (p_mbbButtons)
      {
        case MessageBoxButtons.AbortRetryIgnore:
          var butIgnore = new Button();
          butIgnore.Text = "Ignore";
          butIgnore.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
          butIgnore.Location = new Point(intLastButtonLeft - butIgnore.Width - 6, 12);
          butIgnore.Click += Button_Click;
          butIgnore.Tag = DialogResult.Ignore;
          butIgnore.TabIndex = 2;
          intLastButtonLeft = butIgnore.Left;
          pnlButtons.Controls.Add(butIgnore);
          CancelButton = butIgnore;
          break;
      }

      //retry button
      switch (p_mbbButtons)
      {
        case MessageBoxButtons.AbortRetryIgnore:
        case MessageBoxButtons.RetryCancel:
          var butRetry = new Button();
          butRetry.Text = "Retry";
          butRetry.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
          butRetry.Location = new Point(intLastButtonLeft - butRetry.Width - 6, 12);
          butRetry.Click += Button_Click;
          butRetry.Tag = DialogResult.Retry;
          butRetry.TabIndex = 1;
          intLastButtonLeft = butRetry.Left;
          pnlButtons.Controls.Add(butRetry);
          AcceptButton = butRetry;
          break;
      }

      //abort button
      switch (p_mbbButtons)
      {
        case MessageBoxButtons.AbortRetryIgnore:
          var butAbort = new Button();
          butAbort.Text = "Abort";
          butAbort.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
          butAbort.Location = new Point(intLastButtonLeft - butAbort.Width - 6, 12);
          butAbort.Click += Button_Click;
          butAbort.Tag = DialogResult.Abort;
          butAbort.TabIndex = 0;
          pnlButtons.Controls.Add(butAbort);
          AcceptButton = butAbort;
          break;
      }
    }

    /// <summary>
    ///   Handles the <see cref="Control.Click" /> event of the message box's buttons.
    /// </summary>
    /// <remarks>
    ///   This set the appropriate <see cref="DialogResult" />.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs" /> describing the event properties.</param>
    private void Button_Click(object sender, EventArgs e)
    {
      DialogResult = (DialogResult) ((Button) sender).Tag;
    }
  }
}