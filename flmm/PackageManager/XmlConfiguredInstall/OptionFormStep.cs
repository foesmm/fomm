using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
  /// <summary>
  /// Displays the optional plugin groups for a specific step in a mod's install.
  /// </summary>
  public partial class OptionFormStep : UserControl
  {
    /// <summary>
    /// Raised when an option is checked.
    /// </summary>
    public event EventHandler ItemChecked = delegate {};

    private DependencyStateManager m_dsmStateManager = null;

    #region Constructors

    /// <summary>
    /// A simple constructor that initializes the object with the given values.
    /// </summary>
    /// <param name="p_dsmStateManager">The install state manager.</param>
    /// <param name="p_lstGroups">The plugin groups to display.</param>
    public OptionFormStep(DependencyStateManager p_dsmStateManager, IList<PluginGroup> p_lstGroups)
    {
      m_dsmStateManager = p_dsmStateManager;

      InitializeComponent();

      loadPlugins(p_lstGroups);
      if (lvwPlugins.Items.Count > 0)
        lvwPlugins.Items[0].Selected = true;
    }

    #endregion

    #region Control Members

    /// <summary>
    /// Gets the list of files and folders that need to be installed.
    /// </summary>
    /// <remarks>
    /// The list returned is base upon the plugins that the user selected.
    /// </remarks>
    /// <value>The list of files and folders that need to be installed.</value>
    public List<PluginFile> FilesToInstall
    {
      get
      {
        List<PluginFile> lstInstall = new List<PluginFile>();
        foreach (ListViewItem lviItem in lvwPlugins.Items)
        {
          PluginInfo pifPlugin = (PluginInfo)lviItem.Tag;
          PluginType ptpPluginType = pifPlugin.Type;
          GroupType gtpGroupType = (GroupType)lviItem.Group.Tag;
          if (lviItem.Checked)
            lstInstall.AddRange(pifPlugin.Files);
          else
            foreach (PluginFile pflFile in pifPlugin.Files)
              if (pflFile.AlwaysInstall || (pflFile.InstallIfUsable && (pifPlugin.Type != PluginType.NotUsable)))
                lstInstall.Add(pflFile);
        }
        lstInstall.Sort();
        return lstInstall;
      }
    }

    /// <summary>
    /// Gets the list of files, and folders that may contain files, that need to be activated.
    /// </summary>
    /// <remarks>
    /// The list returned is base upon the plugins that the user selected.
    /// </remarks>
    /// <value>The list of files, and folders that may contain files, that need to be activated.</value>
    public List<PluginFile> PluginsToActivate
    {
      get
      {
        List<PluginFile> lstActivate = new List<PluginFile>();
        foreach (ListViewItem lviItem in lvwPlugins.Items)
        {
          PluginInfo pifPlugin = (PluginInfo)lviItem.Tag;
          if (lviItem.Checked)
            foreach (PluginFile pflFile in pifPlugin.Files)
            {
              if (pflFile.IsFolder)
              {
                if (pflFile.Destination.Length == 0)
                  lstActivate.Add(pflFile);
              }
              else if (String.IsNullOrEmpty(pflFile.Destination))
              {
                if (pflFile.Source.ToLower().EndsWith(".esm") || pflFile.Source.ToLower().EndsWith(".esp"))
                  lstActivate.Add(pflFile);
              }
              else if (pflFile.Destination.ToLower().EndsWith(".esm") || pflFile.Destination.ToLower().EndsWith(".esp"))
                lstActivate.Add(pflFile);
            }
        }
        lstActivate.Sort();
        return lstActivate;
      }
    }

    /// <summary>
    /// Loads the plugins into the form.
    /// </summary>
    /// <param name="p_lstGroups">The list of grouped plugins.</param>
    private void loadPlugins(IList<PluginGroup> p_lstGroups)
    {
      adjustListViewColumnWidth();
      foreach (PluginGroup pgpGroup in p_lstGroups)
      {
        ListViewGroup lvgGroup = addGroup(pgpGroup);
        foreach (PluginInfo pifPlugin in pgpGroup.Plugins)
          addPlugin(lvgGroup, pifPlugin);
      }
      checkDefaults();
    }

    /// <summary>
    /// Checks the plugins that should be checked by default.
    /// </summary>
    private void checkDefaults()
    {
      ListViewItem lviRequired = null;
      ListViewItem lviRecommended = null;
      PluginInfo pifPlugin = null;
      foreach (ListViewGroup lvgGroup in lvwPlugins.Groups)
      {
        switch ((GroupType)lvgGroup.Tag)
        {
          case GroupType.SelectAll:
            foreach (ListViewItem lviPlugin in lvgGroup.Items)
              lviPlugin.Checked = true;
            break;
          case GroupType.SelectExactlyOne:
            lviRequired = null;
            lviRecommended = null;
            foreach (ListViewItem lviPlugin in lvgGroup.Items)
            {
              pifPlugin = (PluginInfo)lviPlugin.Tag;
              switch (pifPlugin.Type)
              {
                case PluginType.Recommended:
                  lviRecommended = lviPlugin;
                  break;
                case PluginType.Required:
                  lviRequired = lviPlugin;
                  break;
              }
            }
            if (lviRequired != null)
              lviRequired.Checked = true;
            else if (lviRecommended != null)
              lviRecommended.Checked = true;
            else if (lvgGroup.Items.Count > 0)
              lvgGroup.Items[0].Checked = true;
            break;
          case GroupType.SelectAtLeastOne:
          default:
            bool booOneSelected = false;
            foreach (ListViewItem lviPlugin in lvgGroup.Items)
            {
              pifPlugin = (PluginInfo)lviPlugin.Tag;
              switch (pifPlugin.Type)
              {
                case PluginType.Recommended:
                case PluginType.Required:
                  lviPlugin.Checked = true;
                  booOneSelected = true;
                  break;
              }
            }
            if ((GroupType.SelectAtLeastOne == (GroupType)lvgGroup.Tag) && !booOneSelected && (lvgGroup.Items.Count > 0))
              lvgGroup.Items[0].Checked = true;
            break;
        }
      }
    }

    /// <summary>
    /// Sizes the column of the list view of plugins to fill the control.
    /// </summary>
    private void adjustListViewColumnWidth()
    {
      lvwPlugins.Columns[0].Width = lvwPlugins.Width - SystemInformation.VerticalScrollBarWidth - 6;
    }

    /// <summary>
    /// Adds a group to the list of plugins.
    /// </summary>
    /// <param name="p_pgpGroup">The plugin group to add.</param>
    /// <returns>The new <see cref="ListViewGroup"/> representing the group.</returns>
    private ListViewGroup addGroup(PluginGroup p_pgpGroup)
    {
      ListViewGroup lvgGroup = null;
      foreach (ListViewGroup lvgExistingGroup in lvwPlugins.Groups)
        if (lvgExistingGroup.Name.Equals(p_pgpGroup.Name))
        {
          lvgGroup = lvgExistingGroup;
          break;
        }
      if (lvgGroup == null)
      {
        lvgGroup = new ListViewGroup();
        lvwPlugins.Groups.Add(lvgGroup);
      }
      lvgGroup.Name = p_pgpGroup.Name;
      lvgGroup.Tag = p_pgpGroup.Type;
      switch (p_pgpGroup.Type)
      {
        case GroupType.SelectAll:
          lvgGroup.Header = p_pgpGroup.Name + " (All Required)";
          break;
        case GroupType.SelectAtLeastOne:
          lvgGroup.Header = p_pgpGroup.Name + " (One Required)";
          break;
        case GroupType.SelectAtMostOne:
          lvgGroup.Header = p_pgpGroup.Name + " (Select Only One)";
          break;
        case GroupType.SelectExactlyOne:
          lvgGroup.Header = p_pgpGroup.Name + " (Select One)";
          break;
        case GroupType.SelectAny:
          lvgGroup.Header = p_pgpGroup.Name;
          break;
      }
      return lvgGroup;
    }

    /// <summary>
    /// Adds a plugin to the list of plugins.
    /// </summary>
    /// <param name="p_lvgGroup">The group to which to add the plugin.</param>
    /// <param name="p_pifPlugin">The plugin to add.</param>
    private void addPlugin(ListViewGroup p_lvgGroup, PluginInfo p_pifPlugin)
    {
      string strName = p_pifPlugin.Name;
      ListViewItem lviPlugin = null;
      foreach (ListViewItem lviExistingPlugin in p_lvgGroup.Items)
        if (lviExistingPlugin.Text.Equals(strName))
        {
          lviPlugin = lviExistingPlugin;
          break;
        }
      if (lviPlugin == null)
      {
        lviPlugin = new ListViewItem();
        lvwPlugins.Items.Add(lviPlugin);
      }

      lviPlugin.Text = strName;
      lviPlugin.Tag = p_pifPlugin;
      lviPlugin.Group = p_lvgGroup;
      lviPlugin.Checked = false;
    }

    /// <summary>
    /// Handles the SizeChanged event of the list view of plugins.
    /// </summary>
    /// <remarks>
    /// This ensures that the column of the list view of plugins fills the control.
    /// </remarks>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    private void lvwPlugins_SizeChanged(object sender, EventArgs e)
    {
      adjustListViewColumnWidth();
    }

    /// <summary>
    /// Handles the SelectedIndexChanged event of the list view of plugins.
    /// </summary>
    /// <remarks>
    /// This changes the displayed description to that of the selected plugin.
    /// </remarks>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    private void lvwPlugins_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (lvwPlugins.SelectedItems.Count > 0)
      {
        PluginInfo pifPlugin = (PluginInfo)lvwPlugins.SelectedItems[0].Tag;
        tbxDescription.Text = pifPlugin.Description;
        pbxImage.Image = pifPlugin.Image;
      }
      else
      {
        tbxDescription.Text = "";
        pbxImage.Image = null;
      }
      sptImage.Panel2Collapsed = (pbxImage.Image == null);
    }

    /// <summary>
    /// Handles the ItemCheck event of the list view of plugins.
    /// </summary>
    /// <remarks>
    /// This enforces any restrictions on the selection of plugins.
    /// </remarks>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    private void lvwPlugins_ItemCheck(object sender, ItemCheckEventArgs e)
    {
      PluginInfo pifPlugin = (PluginInfo)lvwPlugins.Items[e.Index].Tag;
      switch (pifPlugin.Type)
      {
        case PluginType.Required:
          if (e.NewValue != CheckState.Checked)
            MessageBox.Show(this, pifPlugin.Name + " is required. You cannot unselect it.");
          e.NewValue = CheckState.Checked;
          return;
        case PluginType.Recommended:
          if (e.NewValue != CheckState.Checked)
            if (MessageBox.Show(this, pifPlugin.Name + " is recommended. Disabling it may result in game instability. Are you sure you want to continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
              e.NewValue = CheckState.Checked;
              return;
            }
          break;
        case PluginType.NotUsable:
        case PluginType.CouldBeUsable:
          if (e.NewValue == CheckState.Checked)
            if (MessageBox.Show(this, pifPlugin.Name + " is not usable with your loaded mods. Enabling it may result in game instability. Are you sure you want to continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
              e.NewValue = CheckState.Unchecked;
              return;
            }
          break;
      }
      ListViewGroup lvgGroup = lvwPlugins.Items[e.Index].Group;
      switch ((GroupType)lvgGroup.Tag)
      {
        case GroupType.SelectAll:
          if (e.NewValue != CheckState.Checked)
            MessageBox.Show(this, pifPlugin.Name + " is required. You cannot unselect it.");
          e.NewValue = CheckState.Checked;
          break;
        case GroupType.SelectAtLeastOne:
          if (e.NewValue != CheckState.Checked)
          {
            bool booOtherChecked = false;
            foreach (ListViewItem lviGroupItem in lvgGroup.Items)
              if ((lviGroupItem.Index != e.Index) && (lviGroupItem.Checked))
              {
                booOtherChecked = true;
                break;
              }
            if (!booOtherChecked)
            {
              MessageBox.Show(this, "You must select at least one plugin in this group.");
              e.NewValue = CheckState.Checked;
            }
          }
          break;
        case GroupType.SelectExactlyOne:
          if (e.NewValue != CheckState.Checked)
          {
            bool booOtherChecked = false;
            foreach (ListViewItem lviGroupItem in lvgGroup.Items)
              if ((lviGroupItem.Index != e.Index) && (lviGroupItem.Checked))
              {
                booOtherChecked = true;
                break;
              }
            if (!booOtherChecked)
            {
              MessageBox.Show(this, "You must select one plugin in this group.");
              e.NewValue = CheckState.Checked;
            }
          }
          break;
      }
    }

    /// <summary>
    /// Handles the ItemChecked event of the list view of plugins.
    /// </summary>
    /// <remarks>
    /// This enforces any restrictions on the selection of plugins.
    /// </remarks>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    private void lvwPlugins_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
      ListViewItem lviItem = e.Item;
      ListViewGroup lvgGroup = lviItem.Group;
      switch ((GroupType)lvgGroup.Tag)
      {
        case GroupType.SelectAtMostOne:
        case GroupType.SelectExactlyOne:
          if (lviItem.Checked)
            foreach (ListViewItem lviGroupItem in lvgGroup.Items)
              if ((lviGroupItem != lviItem) && (lviGroupItem.Index > -1))
                lviGroupItem.Checked = false;
          break;
      }
      PluginInfo pifPlugin = (PluginInfo)e.Item.Tag;
      if (lviItem.Checked)
      {
        foreach (ConditionalFlag cfgFlag in pifPlugin.Flags)
          m_dsmStateManager.SetFlagValue(cfgFlag.Name, cfgFlag.ConditionalValue, pifPlugin);
      }
      else
        m_dsmStateManager.RemoveFlags(pifPlugin);
      ItemChecked(this, new EventArgs());
    }

    #endregion
  }
}
