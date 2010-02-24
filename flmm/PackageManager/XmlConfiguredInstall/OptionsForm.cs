using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Drawing;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
	public partial class OptionsForm : Form
	{
		private XmlConfiguredScript m_xcsScript = null;

		public OptionsForm(XmlConfiguredScript p_xcsScript, Dictionary<string, bool> p_dicPlugins, XmlDocument p_xmlConfig)
		{
			m_xcsScript = p_xcsScript;
			InitializeComponent();
			loadConfig(p_xmlConfig, p_dicPlugins);
			if (lvwPlugins.Items.Count > 0)
				lvwPlugins.Items[0].Selected = true;
		}

		#region Form Members

		/// <summary>
		/// The possible plugin group types.
		/// </summary>
		enum GroupType
		{
			/// <summary>
			/// At least one plugin in the group must be selected.
			/// </summary>
			SelectAtLeastOne,

			/// <summary>
			/// At most one plugin in the group must be selected.
			/// </summary>
			SelectAtMostOne,

			/// <summary>
			/// Exactly one plugin in the group must be selected.
			/// </summary>
			SelectExactlyOne,

			/// <summary>
			/// All plugins in the group must be selected.
			/// </summary>
			SelectAll,

			/// <summary>
			/// Any number of plugins in the group may be selected.
			/// </summary>
			SelectAny,

			/// <summary>
			/// This state should not be used.
			/// </summary>
			Inavlid
		}

		/// <summary>
		/// The possible plugin types.
		/// </summary>
		enum PluginType
		{
			/// <summary>
			/// Indicates the plugin must be installed.
			/// </summary>
			Required,

			/// <summary>
			/// Indicates the plugin is optional.
			/// </summary>
			Optional,

			/// <summary>
			/// Indicates the plugin is recommended for stability.
			/// </summary>
			Recommended,

			/// <summary>
			/// Indicates that using the plugin could result in instability (i.e., a prerequisite plugin is missing).
			/// </summary>
			NotUsable,

			/// <summary>
			/// Indicates that using the plugin could result in instability if loaded
			/// with the currently active plugins (i.e., a prerequisite plugin is missing),
			/// but that the prerequisite plugin is installed, just not activated.
			/// </summary>
			CouldBeUsable,

			/// <summary>
			/// This state should not be used.
			/// </summary>
			Invalid
		}

		/// <summary>
		/// The possible states of a mod file.
		/// </summary>
		enum ModFileState
		{
			/// <summary>
			/// Indicates the mod file is not installed.
			/// </summary>
			Missing,

			/// <summary>
			/// Indicates the mod file is installed, but not active.
			/// </summary>
			Inactive,

			/// <summary>
			/// Indicates the mod file is installed and active.
			/// </summary>
			Active
		}

		/// <summary>
		/// The possible relations of dependancies.
		/// </summary>
		enum DependancyOperator
		{
			/// <summary>
			/// Indicates all contained dependancies must be satisfied in order for this dependancy to be satisfied.
			/// </summary>
			And,

			/// <summary>
			/// Indicates at least one listed dependancy must be satisfied in order for this dependancy to be satisfied.
			/// </summary>
			Or
		}

		/// <summary>
		/// A plugin file or folder.
		/// </summary>
		/// <remarks>
		/// This class describes the location of the file/folder in the FOMOD, as well as where the
		/// file/folder should be installed.
		/// </remarks>
		public class PluginFile : IComparable<PluginFile>
		{
			private string m_strSource = null;
			private string m_strDest = null;
			private bool m_booIsFolder = false;
			private bool m_booAlwaysInstall = false;
			private bool m_booInstallIfUsable = false;
			private Int32 m_intPriority = 0;

			#region Properties

			/// <summary>
			/// Gets the file's/folder's location in the FOMOD.
			/// </summary>
			/// <value>The file's/folder's location in the FOMOD.</value>
			public string Source
			{
				get
				{
					return m_strSource;
				}
			}

			/// <summary>
			/// Gets where the file/folder should be installed.
			/// </summary>
			/// <value>Where the file/folder should be installed.</value>
			public string Destination
			{
				get
				{
					return m_strDest;
				}
			}

			/// <summary>
			/// Gets whether this item is a folder.
			/// </summary>
			/// <value>Whether this item is a folder.</value>
			public bool IsFolder
			{
				get
				{
					return m_booIsFolder;
				}
			}

			/// <summary>
			/// Gets whether this item should always be installed, regardless of whether or not the plugin is selected.
			/// </summary>
			/// <value>Whether this item should always be installed, regardless of whether or not the plugin is selected.</value>
			public bool AlwaysInstall
			{
				get
				{
					return m_booAlwaysInstall;
				}
			}

			/// <summary>
			/// Gets whether this item should be installed if the plugins is usable, regardless of whether or not the plugin is selected.
			/// </summary>
			/// <value>Whether this item should be installed if the plugins is usable, regardless of whether or not the plugin is selected.</value>
			public bool InstallIfUsable
			{
				get
				{
					return m_booInstallIfUsable;
				}
			}

			/// <summary>
			/// Gets the priority of this item.
			/// </summary>
			/// <remarks>
			/// A higher number indicates the file or folder should be installed after the
			/// items with lower numbers. This value does not have to be unique.
			/// </remarks>
			/// <value>The priority of this item.</value>
			public Int32 Priority
			{
				get
				{
					return m_intPriority;
				}
			}

			#endregion

			#region Constructors

			/// <summary>
			/// A simple constructor that initializes the values of the object.
			/// </summary>
			/// <param name="p_strSource">The file's/folder's location in the FOMOD.</param>
			/// <param name="p_strDest">Where the file/folder should be installed.</param>
			/// <param name="p_booIsFolder">Whether this item is a folder.</param>
			/// <param name="p_intPriority">The priority of the item.</param>
			/// <param name="p_booAlwaysInstall">Whether this item should always be installed, regardless of whether or not the plugin is selected.</param>
			/// <param name="p_booInstallIfUsable">Whether this item should be installed when the plugin is not <see cref="PluginType.NotUsable"/>, regardless of whether or not the plugin is selected.</param>
			public PluginFile(string p_strSource, string p_strDest, bool p_booIsFolder, Int32 p_intPriority, bool p_booAlwaysInstall, bool p_booInstallIfUsable)
			{
				m_strSource = p_strSource;
				m_strDest = p_strDest;
				m_booIsFolder = p_booIsFolder;
				m_booAlwaysInstall = p_booAlwaysInstall;
				m_booInstallIfUsable = p_booInstallIfUsable;
				m_intPriority = p_intPriority;
			}

			#endregion

			#region IComparable<PluginFile> Members

			/// <summary>
			/// Determines whether this PluginFile is less than, equal to,
			/// or greater than the given PluginFile.
			/// </summary>
			/// <param name="other">The PluginFile to which to compare this PluginFile.</param>
			/// <returns>A value less than 0 if this PluginFile is less than the given PluginFile,
			/// or 0 if this PluginFile is equal to the given PluginFile,
			///or a value greater than 0 if this PluginFile is greater than the given PluginFile.</returns>
			public int CompareTo(PluginFile other)
			{
				Int32 intResult = m_intPriority.CompareTo(other.Priority);
				if (intResult == 0)
				{
					intResult = m_booIsFolder.CompareTo(other.IsFolder);
					if (intResult == 0)
					{
						intResult = m_strSource.CompareTo(other.Source);
						if (intResult == 0)
							intResult = m_strDest.CompareTo(other.Destination);
					}
				}
				return intResult;
			}

			#endregion
		}

		/// <summary>
		/// Describes a plugin.
		/// </summary>
		/// <remarks>
		/// This class tracks the name, description, type, and files/folders associated with a plugin.
		/// </remarks>
		class PluginInfo
		{
			private string m_strName = null;
			private string m_strDesc = null;
			private Image m_imgImage = null;
			private PluginType m_ptpType = PluginType.Invalid;
			private List<PluginFile> m_lstFiles = null;

			#region Properties

			/// <summary>
			/// Gets the name of the plugin.
			/// </summary>
			/// <value>The name of the plugin.</value>
			public string Name
			{
				get
				{
					return m_strName;
				}
			}

			/// <summary>
			/// Gets the description of the plugin.
			/// </summary>
			/// <value>The description of the plugin.</value>
			public string Description
			{
				get
				{
					return m_strDesc;
				}
			}

			/// <summary>
			/// Gets the plugin image.
			/// </summary>
			/// <value>The plugin image</value>
			public Image Image
			{
				get
				{
					return m_imgImage;
				}
			}

			/// <summary>
			/// Gets the <see cref="PluginType"/> of the plugin.
			/// </summary>
			/// <value>The <see cref="PluginType"/> of the plugin.</value>
			public PluginType Type
			{
				get
				{
					return m_ptpType;
				}
			}

			/// <summary>
			/// Gets the list of files and folders associated with the plugin.
			/// </summary>
			/// <value>The list of files and folders associated with the plugin.</value>
			public List<PluginFile> Files
			{
				get
				{
					return m_lstFiles;
				}
			}

			#endregion

			#region Constructors

			/// <summary>
			/// A simple constructor that initializes the values of the object.
			/// </summary>
			/// <param name="p_strName">The name of the plugin.</param>
			/// <param name="p_strDesc">The description of the plugin.</param>
			/// <param name="p_imgImage">The plugin image.</param>
			/// <param name="p_ptpType">The <see cref="PluginType"/> of the plugin.</param>
			public PluginInfo(string p_strName, string p_strDesc, Image p_imgImage, PluginType p_ptpType)
			{
				m_strName = p_strName;
				m_lstFiles = new List<PluginFile>();
				m_ptpType = p_ptpType;
				m_strDesc = p_strDesc;
				m_imgImage = p_imgImage;
			}

			#endregion
		}

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
		/// Loads the configuration file data into the form.
		/// </summary>
		/// <param name="p_xmlConfig">The configuration file.</param>
		/// <param name="p_dicUserPlugins">The list of plugins installed on the user's system, and
		/// whether each plugin is active.</param>
		private void loadConfig(XmlDocument p_xmlConfig, Dictionary<string, bool> p_dicUserPlugins)
		{
			lblTitle.Text = p_xmlConfig.SelectSingleNode("/config/moduleName").InnerText;
			adjustListViewColumnWidth();
			XmlNodeList xnlGroups = p_xmlConfig.SelectNodes("/config/optionalFileGroups/*");
			foreach (XmlNode xndGroup in xnlGroups)
			{
				ListViewGroup lvgGroup = addGroup(xndGroup);
				XmlNodeList xnlPlugins = xndGroup.FirstChild.ChildNodes;
				foreach (XmlNode xndPlugin in xnlPlugins)
				{
					PluginInfo pifPlugin = readPlugin(xndPlugin, p_dicUserPlugins);
					addPlugin(lvgGroup, pifPlugin);
				}
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
		/// Adds a group to the list of plugins.
		/// </summary>
		/// <param name="p_xndGroup">The configuration file node corresponding to the group to add.</param>
		/// <returns>The added group.</returns>
		private ListViewGroup addGroup(XmlNode p_xndGroup)
		{
			string strName = p_xndGroup.Attributes["name"].InnerText;
			GroupType gtpType = (GroupType)Enum.Parse(typeof(GroupType), p_xndGroup.Attributes["type"].InnerText);

			ListViewGroup lvgGroup = null;
			foreach (ListViewGroup lvgExistingGroup in lvwPlugins.Groups)
				if (lvgExistingGroup.Name.Equals(strName))
				{
					lvgGroup = lvgExistingGroup;
					break;
				}
			if (lvgGroup == null)
			{
				lvgGroup = new ListViewGroup();
				lvwPlugins.Groups.Add(lvgGroup);
			}
			lvgGroup.Name = strName;
			lvgGroup.Tag = gtpType;
			switch (gtpType)
			{
				case GroupType.SelectAll:
					lvgGroup.Header = strName + " (All Required)";
					break;
				case GroupType.SelectAtLeastOne:
					lvgGroup.Header = strName + " (One Required)";
					break;
				case GroupType.SelectAtMostOne:
					lvgGroup.Header = strName + " (Select Only One)";
					break;
				case GroupType.SelectExactlyOne:
					lvgGroup.Header = strName + " (Select One)";
					break;
				case GroupType.SelectAny:
					lvgGroup.Header = strName;
					break;
			}
			return lvgGroup;
		}

		/// <summary>
		/// Reads the file info from the given XML nodes.
		/// </summary>
		/// <param name="p_xnlFiles">The list of XML nodes containing the file info to read.</param>
		/// <returns>An ordered list of <see cref="PluginFile"/>s representing the data in the given list.</returns>
		public static List<PluginFile> readFileInfo(XmlNodeList p_xnlFiles)
		{
			List<PluginFile> lstFiles = new List<PluginFile>();
			foreach (XmlNode xndFile in p_xnlFiles)
			{
				string strSource = xndFile.Attributes["source"].InnerText;
				string strDest = (xndFile.Attributes["destination"] == null) ? strSource : xndFile.Attributes["destination"].InnerText;
				bool booAlwaysInstall = Boolean.Parse(xndFile.Attributes["alwaysInstall"].InnerText);
				bool booInstallIfUsable = Boolean.Parse(xndFile.Attributes["installIfUsable"].InnerText);
				Int32 intPriority = Int32.Parse(xndFile.Attributes["priority"].InnerText);
				switch (xndFile.Name)
				{
					case "file":
						lstFiles.Add(new PluginFile(strSource, strDest, false, intPriority, booAlwaysInstall, booInstallIfUsable));
						break;
					case "folder":
						lstFiles.Add(new PluginFile(strSource, strDest, true, intPriority, booAlwaysInstall, booInstallIfUsable));
						break;
				}
			}
			lstFiles.Sort();
			return lstFiles;
		}

		/// <summary>
		/// Determines if the given composite dependancy if fufilled.
		/// </summary>
		/// <remarks>
		/// A composite dependancy is fufilled if and only if all of its contained dependancies
		/// are fufilled.</remarks>
		/// <param name="p_xndCompositeDependancy">The composite dependancy for which it is to be determined whether or not it is fufilled.</param>
		/// <param name="p_dicUserPlugins">The list of plugins installed on the user's system, and
		/// whether each plugin is active.</param>
		/// <returns>true if the composite dependancy is fufilled; false otherwise.</returns>
		private bool isDependancyFufilled(XmlNode p_xndCompositeDependancy, Dictionary<string, bool> p_dicUserPlugins)
		{
			DependancyOperator dopOperator = (DependancyOperator)Enum.Parse(typeof(DependancyOperator), p_xndCompositeDependancy.Attributes["operator"].InnerText);
			XmlNodeList xnlDependancies = p_xndCompositeDependancy.ChildNodes;
			bool booAllFufilled = (dopOperator == DependancyOperator.And) ? true : false;
			bool booThisFufilled = true;
			foreach (XmlNode xndDependancy in xnlDependancies)
			{
				switch (xndDependancy.Name)
				{
					case "dependancies":
						booThisFufilled = isDependancyFufilled(xndDependancy, p_dicUserPlugins);
						break;
					case "dependancy":
						string strDependancy = xndDependancy.Attributes["file"].InnerText.ToLower();
						ModFileState mfsModState = (ModFileState)Enum.Parse(typeof(ModFileState), xndDependancy.Attributes["state"].InnerText);
						switch (mfsModState)
						{
							case ModFileState.Active:
								booThisFufilled = (p_dicUserPlugins.ContainsKey(strDependancy) && p_dicUserPlugins[strDependancy]);
								break;
							case ModFileState.Inactive:
								booThisFufilled = (p_dicUserPlugins.ContainsKey(strDependancy) && !p_dicUserPlugins[strDependancy]);
								break;
							case ModFileState.Missing:
								booThisFufilled = (!p_dicUserPlugins.ContainsKey(strDependancy));
								break;
						}
						break;
				}
				switch (dopOperator)
				{
					case DependancyOperator.And:
						booAllFufilled &= booThisFufilled;
						break;
					case DependancyOperator.Or:
						booAllFufilled |= booThisFufilled;
						break;
				}
			}
			return booAllFufilled;
		}

		/// <summary>
		/// Reads a plugin's information from the configuration file.
		/// </summary>
		/// <param name="p_xndPlugin">The configuration file node corresponding to the plugin to read.</param>
		/// <param name="p_dicUserPlugins">The list of plugins installed on the user's system, and
		/// whether each plugin is active.</param>
		/// <returns>The plugin information.</returns>
		private PluginInfo readPlugin(XmlNode p_xndPlugin, Dictionary<string, bool> p_dicUserPlugins)
		{
			string strName = p_xndPlugin.Attributes["name"].InnerText;
			string strDesc = p_xndPlugin.SelectSingleNode("description").InnerText.Trim();
			PluginType ptpType = PluginType.Invalid;
			XmlNode xndTypeDescriptor = p_xndPlugin.SelectSingleNode("typeDescriptor").FirstChild;
			switch (xndTypeDescriptor.Name)
			{
				case "type":
					ptpType = (PluginType)Enum.Parse(typeof(PluginType), xndTypeDescriptor.Attributes["name"].InnerText);
					break;
				case "dependancyType":
					ptpType = (PluginType)Enum.Parse(typeof(PluginType), xndTypeDescriptor.SelectSingleNode("defaultType").Attributes["name"].InnerText);
					XmlNodeList xnlPatterns = xndTypeDescriptor.SelectNodes("patterns/*");
					foreach (XmlNode xndPattern in xnlPatterns)
					{
						XmlNode xndCompositeDependancy = xndPattern.SelectSingleNode("dependancies");
						if (isDependancyFufilled(xndCompositeDependancy, p_dicUserPlugins))
						{
							ptpType = (PluginType)Enum.Parse(typeof(PluginType), xndPattern.SelectSingleNode("type").Attributes["name"].InnerText);
							break;
						}
					}
					break;
			}
			XmlNode xndImage = p_xndPlugin.SelectSingleNode("image");
			Image imgImage = null;
			if (xndImage != null)
			{
				string strImageFilePath = xndImage.Attributes["path"].InnerText;
				imgImage = m_xcsScript.GetImageFromFomod(strImageFilePath);
			}
			PluginInfo pifPlugin = new PluginInfo(strName, strDesc, imgImage, ptpType);

			XmlNodeList xnlPluginFiles = p_xndPlugin.SelectNodes("files/*");
			pifPlugin.Files.AddRange(readFileInfo(xnlPluginFiles));
			return pifPlugin;
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
		/// Sizes the column of the list view of plugins to fill the control.
		/// </summary>
		private void adjustListViewColumnWidth()
		{
			lvwPlugins.Columns[0].Width = lvwPlugins.Width - SystemInformation.VerticalScrollBarWidth - 6;
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
		}

		/// <summary>
		/// Handles the Click event of the cancel button.
		/// </summary>
		/// <remarks>
		/// This cancels the dialog.
		/// </remarks>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void butCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		/// <summary>
		/// Handles the Click event of the OK button.
		/// </summary>
		/// <remarks>
		/// This OKs the dialog.
		/// </remarks>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void butOK_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		#endregion
	}
}
