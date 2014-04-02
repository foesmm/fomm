using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using GeMod.Interface;

namespace Fomm.PackageManager
{
  /// <summary>
  /// Encapsulates the editing of FOMod info.
  /// </summary>
  public partial class FomodInfoControl : UserControl, IFomodInfo
  {
    private Screenshot m_shtScreenshot = null;

    #region Properties

    /// <summary>
    /// Gets or sets the screenshot used by the fomod.
    /// </summary>
    /// <value>The screenshot used by the fomod.</value>
    public Screenshot Screenshot
    {
      get
      {
        return m_shtScreenshot;
      }
      set
      {
        m_shtScreenshot = value;
        pbxScreenshot.Image = (m_shtScreenshot != null) ? m_shtScreenshot.Image : null;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// The default constructor.
    /// </summary>
    public FomodInfoControl()
    {
      InitializeComponent();

      //this try...catch is required because design time viewing on the control doesn't recognize
      // the implicit conversion of Properties.Settings.Default.pluginGroups to an array of string.
      // no idea why
      try
      {
        string[] strGroups = Properties.Settings.Default.pluginGroups;
        if (strGroups != null)
        {
          clbGroups.SuspendLayout();
          foreach (string strGroup in strGroups)
          {
            clbGroups.Items.Add(strGroup, 0);
          }
          clbGroups.ResumeLayout();
        }
      }
      catch
      {
      }
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
        m_shtScreenshot = new Screenshot(ofdScreenshot.FileName, File.ReadAllBytes(ofdScreenshot.FileName));
        pbxScreenshot.Image = m_shtScreenshot.Image;
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
      m_shtScreenshot = null;
    }

    #endregion

    #region Validation

    /// <summary>
    /// Ensures that the version is valid, if present.
    /// </summary>
    /// <returns><lang cref="true"/> if the version is valid or not present;
    /// <lang cref="false"/> otherwise.</returns>
    protected bool ValidateMachineVersion()
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
          return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Handles the <see cref="Control.Validating"/> event of the machine version textbox.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">A <see cref="CancelEventArgs"/> describing the event arguments.</param>
    private void tbMVersion_Validating(object sender, CancelEventArgs e)
    {
      ValidateMachineVersion();
    }

    /// <summary>
    /// Ensures that the URL is valid, if present.
    /// </summary>
    /// <returns><lang cref="true"/> if the website is valid or not present;
    /// <lang cref="false"/> otherwise.</returns>
    protected bool ValidateWebsite()
    {
      erpErrors.SetError(tbWebsite, null);
      if (!String.IsNullOrEmpty(tbWebsite.Text))
      {
        Uri uri;
        if (!Uri.TryCreate(tbWebsite.Text, UriKind.Absolute, out uri) || uri.IsFile ||
            (uri.Scheme != "http" && uri.Scheme != "https"))
        {
          erpErrors.SetError(tbWebsite, "Invalid web address specified.\nDid you miss the 'http://'?");
          return false;
        }
      }
      return true;
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
      ValidateWebsite();
    }

    /// <summary>
    /// Ensures that the version is valid, if present.
    /// </summary>
    /// <returns><lang cref="true"/> if the version is valid or not present;
    /// <lang cref="false"/> otherwise.</returns>
    protected bool ValidateMinFommVersion()
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
          return false;
        }
        if (verVersion > Program.MVersion)
        {
          erpErrors.SetError(tbMinFommVersion, "Specified version is newer than this version of FOMM.");
          return false;
        }
      }
      return true;
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
      ValidateMinFommVersion();
    }

    /// <summary>
    /// Validate the controls on this control.
    /// </summary>
    /// <returns><lang cref="true"/> if all controls passed validation; <lang cref="false"/> otherwise.</returns>
    public bool PerformValidation()
    {
      bool booIsValid = ValidateMachineVersion();
      booIsValid &= ValidateMinFommVersion();
      booIsValid &= ValidateWebsite();
      return booIsValid;
    }

    #endregion

    /// <summary>
    /// Loads the info from the given <see cref="fomod"/> into the edit form.
    /// </summary>
    /// <param name="p_fomodMod">The <see cref="fomod"/> whose info is to be edited.</param>
    public void LoadFomod(fomod p_fomodMod)
    {
      ModName = p_fomodMod.ModName;
      Author = p_fomodMod.Author;
      HumanReadableVersion = String.IsNullOrEmpty(p_fomodMod.HumanReadableVersion)
        ? p_fomodMod.MachineVersion.ToString()
        : p_fomodMod.HumanReadableVersion;
      MachineVersion = p_fomodMod.MachineVersion;
      Description = p_fomodMod.Description;
      Website = p_fomodMod.Website;
      Email = p_fomodMod.Email;
      MinFommVersion = p_fomodMod.MinFommVersion;
      Groups = p_fomodMod.Groups;
      Screenshot = p_fomodMod.GetScreenshot();
    }

    /// <summary>
    /// Saves the edited info to the given fomod.
    /// </summary>
    /// <param name="p_fomodMod">The <see cref="fomod"/> to which to save the info.</param>
    /// <returns><lang cref="false"/> if the info failed validation and was not saved;
    /// <lang cref="true"/> otherwise.</returns>
    public bool SaveFomod(fomod p_fomodMod)
    {
      if (!this.ValidateChildren())
      {
        return false;
      }

      if (!String.IsNullOrEmpty(tbVersion.Text) && String.IsNullOrEmpty(tbMVersion.Text))
      {
        erpErrors.SetError(tbMVersion, "Machine must be specified if Version is specified.");
        return false;
      }

      p_fomodMod.ModName = ModName;
      p_fomodMod.Author = Author;
      p_fomodMod.HumanReadableVersion = String.IsNullOrEmpty(HumanReadableVersion)
        ? MachineVersion.ToString()
        : HumanReadableVersion;
      p_fomodMod.MachineVersion = MachineVersion;
      p_fomodMod.Description = Description;
      p_fomodMod.Website = Website;
      p_fomodMod.Email = Email;
      p_fomodMod.MinFommVersion = MinFommVersion;
      p_fomodMod.Groups = Groups;
      p_fomodMod.CommitInfo(true, Screenshot);

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

    #region IFomodInfo Members

    /// <summary>
    /// Gets or sets the human readable Version of the mod.
    /// </summary>
    /// <value>The human readable Version of the mod.</value>
    public string HumanReadableVersion
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
    public Version MachineVersion
    {
      get
      {
        if (String.IsNullOrEmpty(tbMVersion.Text) || !ValidateMachineVersion())
        {
          return fomod.DefaultVersion;
        }
        return new Version(tbMVersion.Text);
      }
      set
      {
        tbMVersion.Text = value.ToString();
      }
    }

    /// <summary>
    /// Gets or sets the required FOMM version of the mod.
    /// </summary>
    /// <value>The required FOMM version of the mod.</value>
    public Version MinFommVersion
    {
      get
      {
        if (String.IsNullOrEmpty(tbMinFommVersion.Text) || !ValidateMinFommVersion())
        {
          return fomod.DefaultMinFommVersion;
        }
        return new Version(tbMinFommVersion.Text);
      }
      set
      {
        tbMinFommVersion.Text = value.ToString();
      }
    }

    /// <summary>
    /// Gets or sets the FOMM groups to which the fomod belongs.
    /// </summary>
    /// <value>The FOMM groups to which the fomod belongs.</value>
    public string[] Groups
    {
      get
      {
        string[] strGroups = new string[clbGroups.CheckedItems.Count];
        for (Int32 i = 0; i < strGroups.Length; i++)
        {
          strGroups[i] = ((string) clbGroups.CheckedItems[i]).ToLowerInvariant();
        }
        return strGroups;
      }
      set
      {
        clbGroups.SuspendLayout();
        for (Int32 i = 0; i < clbGroups.Items.Count; i++)
        {
          clbGroups.SetItemChecked(i,
                                   Array.IndexOf<string>(value, ((string) clbGroups.Items[i]).ToLowerInvariant()) != -1);
        }
        clbGroups.ResumeLayout();
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

    #endregion
  }
}