using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Fomm.TESsnip;

namespace Fomm.PackageManager
{
	/// <summary>
	/// The form that allows selection of critical records in a mod.
	/// </summary>
	/// <seealso cref="fomod.CriticalRecords"/>
	public partial class CriticalRecordsForm : Form
	{
		private fomod m_fomodMod = null;
		private Dictionary<string, Dictionary<UInt32, string>> m_dicCriticalRecordsBackup = null;
		private Dictionary<string, string> m_dicInstalledNamesBackup = null;
		
		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the form with the given values.
		/// </summary>
		/// <param name="p_fomodMod">The mod whose plugins are going to have records marked as critical.</param>
		public CriticalRecordsForm(fomod p_fomodMod)
		{
			if (!RecordStructure.Loaded)
			{
				try
				{
					RecordStructure.Load();
				}
				catch (Exception ex)
				{
					MessageBox.Show("Could not parse RecordStructure.xml. Record-at-once editing will be unavailable.\n" + ex.Message, "Warning");
				}
			}
			InitializeComponent();
			m_fomodMod = p_fomodMod;
			List<string> lstFiles = m_fomodMod.GetFileList();
			foreach (string strFile in lstFiles)
			{
				if (strFile.ToLowerInvariant().EndsWith(".esm") || strFile.ToLowerInvariant().EndsWith(".esp"))
				{
					LoadPlugin(strFile);
				}
			}
			m_dicCriticalRecordsBackup = new Dictionary<string, Dictionary<uint, string>>(m_fomodMod.CriticalRecords);
			m_dicInstalledNamesBackup = new Dictionary<string, string>(m_fomodMod.CriticalRecordPluginInstalledNames);
		}

		#endregion

		#region Record Loading

		/// <summary>
		/// Loads the specified plugin.
		/// </summary>
		/// <param name="p_strPlugin">The plugin whose records are to be loaded.</param>
		private void LoadPlugin(string p_strPlugin)
		{
			byte[] bteData = m_fomodMod.GetFile(p_strPlugin);
			Plugin plgPlugin = new Plugin(bteData, p_strPlugin);
			TreeNode tndPluginRoot = new TreeNode(p_strPlugin);
			CreatePluginTree(plgPlugin, tndPluginRoot);
			tvwRecords.Nodes.Add(tndPluginRoot);
		}

		/// <summary>
		/// Creates the tree of records for the given plugin using the given node.
		/// </summary>
		/// <param name="p_plgPlugin">The <see cref="Plugin"/> for which to build a record tree.</param>
		/// <param name="p_tndNode">The <see cref="TreeNode"/> at which to root the record tree.</param>
		private void CreatePluginTree(Plugin p_plgPlugin, TreeNode p_tndNode)
		{
			p_tndNode.Tag = p_plgPlugin;
			foreach (Rec recRecord in p_plgPlugin.Records)
				WalkPluginTree(p_plgPlugin, recRecord, p_tndNode);
		}

		/// <summary>
		/// Recursively builds the tree of records for the given record using the given node.
		/// </summary>
		/// <param name="p_plgPlugin">The <see cref="Plugin"/> for which to build a record tree.</param>
		/// <param name="p_recRecord">The <see cref="Rec"/> for which to build a record tree.</param>
		/// <param name="p_tndNode">The <see cref="TreeNode"/> at which to build the record tree.</param>
		private void WalkPluginTree(Plugin p_plgPlugin, Rec p_recRecord, TreeNode p_tndNode)
		{
			if (p_recRecord.Name.Equals("TES4"))
				return;
			TreeNode tndSubNode = null;
			if (p_recRecord is Record)
			{
				tndSubNode = new TreeNode(String.Format("{0:x8}: {1}", ((Record)p_recRecord).FormID, p_recRecord.DescriptiveName));
				string strInstalledName = m_fomodMod.GetCriticalRecordPluginInstalledName(p_plgPlugin.Name);
				if ((strInstalledName != null) && m_fomodMod.IsRecordCritical(strInstalledName, ((Record)p_recRecord).FormID))
					tndSubNode.BackColor = Color.Red;
			}
			else
				tndSubNode = new TreeNode(p_recRecord.DescriptiveName);
			tndSubNode.Tag = p_recRecord;
			if (p_recRecord is GroupRecord)
				foreach (Rec recSubRecord in ((GroupRecord)p_recRecord).Records)
					WalkPluginTree(p_plgPlugin, recSubRecord, tndSubNode);
			p_tndNode.Nodes.Add(tndSubNode);
		}

		#endregion

		/// <summary>
		/// Handles the <see cref="TreeView.AfterSelect"/> event of the record tree view.
		/// </summary>
		/// <remarks>
		/// This sets up the edit section of the form if the selected node is a record
		/// that can be marked as critical.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="TreeViewEventArgs"/> describing the event arguments.</param>
		private void tvwRecords_AfterSelect(object sender, TreeViewEventArgs e)
		{
			TreeNode tndRoot = e.Node;
			while (tndRoot.Parent != null)
				tndRoot = tndRoot.Parent;
			Plugin plgPlugin = tndRoot.Tag as Plugin;

			tbxInstalledName.Tag = null;
			tbxInstalledName.Text = m_fomodMod.GetCriticalRecordPluginInstalledName(plgPlugin.Name) ?? plgPlugin.Name;
			tbxInstalledName.Tag = plgPlugin;

			Record recRecord = e.Node.Tag as Record;
			if (recRecord == null)
			{
				splitContainer1.Panel2Collapsed = true;
				return;
			}
			
			splitContainer1.Panel2Collapsed = false;
			ckbIsCritical.Checked = m_fomodMod.IsRecordCritical(tbxInstalledName.Text, recRecord.FormID);
			if (ckbIsCritical.Checked)
				tbxReason.Text = m_fomodMod.GetCriticalRecordReason(tbxInstalledName.Text, recRecord.FormID);
			else
				tbxReason.Text = null;
		}

		/// <summary>
		/// Handles the <see cref="Control.Leave"/> event of the edit panel.
		/// </summary>
		/// <remarks>
		/// This persists the changes made to the criticality of the selected record.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void splitContainer1_Panel2_Leave(object sender, EventArgs e)
		{
			TreeNode tndRoot = tvwRecords.SelectedNode;
			Record recRecord = tndRoot.Tag as Record;
			if (recRecord == null)
				return;
			while (tndRoot.Parent != null)
				tndRoot = tndRoot.Parent;
			Plugin plgPlugin = tndRoot.Tag as Plugin;
			if (ckbIsCritical.Checked)
			{
				m_fomodMod.SetCriticalRecordPluginInstalledName(plgPlugin.Name, tbxInstalledName.Text);
				m_fomodMod.SetCriticalRecord(tbxInstalledName.Text, recRecord.FormID, tbxReason.Text);
			}
			else
				m_fomodMod.UnsetCriticalRecord(tbxInstalledName.Text, recRecord.FormID);
			tvwRecords.SelectedNode.BackColor = ckbIsCritical.Checked ? Color.Red : Color.Transparent;
		}

		/// <summary>
		/// Handles the <see cref="TextBox.TextChanged"/> event of the installed name textbox.
		/// </summary>
		/// <remarks>
		/// This persists the changes made to the installed name of a plugin.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void tbxInstalledName_TextChanged(object sender, EventArgs e)
		{
			Plugin plgPlugin = tbxInstalledName.Tag as Plugin;
			if (plgPlugin == null)
				return;
			m_fomodMod.SetCriticalRecordPluginInstalledName(plgPlugin.Name, tbxInstalledName.Text);
		}

		/// <summary>
		/// Handles the <see cref="Button.Click"/> event of the cancel button.
		/// </summary>
		/// <remarks>
		/// This undoes any changes we've made to the critical records.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void butCancel_Click(object sender, EventArgs e)
		{
			m_fomodMod.CriticalRecords = m_dicCriticalRecordsBackup;
			m_fomodMod.CriticalRecordPluginInstalledNames = m_dicInstalledNamesBackup;
		}

		/// <summary>
		/// Handles the <see cref="Button.Click"/> event of the ok button.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void butOK_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
		}
	}
}
