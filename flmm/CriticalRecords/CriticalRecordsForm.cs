using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Fomm.TESsnip;

namespace Fomm.CriticalRecords
{
	/// <summary>
	/// The form that allows selection of critical records in a mod.
	/// </summary>
	/// <seealso cref="fomod.CriticalRecords"/>
	public partial class CriticalRecordsForm : Form
	{
		private bool m_booPopulatingForm = false;

		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		public CriticalRecordsForm()
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

			cbxSeverity.DataSource = Enum.GetValues(typeof(CriticalRecordInfo.ConflictSeverity));
			cbxSeverity.SelectedItem = CriticalRecordInfo.ConflictSeverity.Conflict;

			Settings.GetWindowPosition("CREditor", this);
		}

		/// <summary>
		/// A simple constructor that initializes the form with the given values.
		/// </summary>
		/// <param name="p_fomodMod">The mod whose plugins are going to have records marked as critical.</param>
		public CriticalRecordsForm(string[] p_strPlugins)
			: this()
		{			
			foreach (string strFile in p_strPlugins)
				if (strFile.ToLowerInvariant().EndsWith(".esm") || strFile.ToLowerInvariant().EndsWith(".esp"))
					LoadPlugin(strFile);
		}

		#endregion

		#region Record Loading

		/// <summary>
		/// Loads the specified plugin.
		/// </summary>
		/// <param name="p_strPlugin">The plugin whose records are to be loaded.</param>
		private void LoadPlugin(string p_strPlugin)
		{
			byte[] bteData = File.ReadAllBytes(p_strPlugin);
			CriticalRecordPlugin crpPlugin = new CriticalRecordPlugin(bteData, p_strPlugin);
			TreeNode tndPluginRoot = new TreeNode(p_strPlugin);
			tvwRecords.BeginUpdate();
			CreatePluginTree(crpPlugin, tndPluginRoot);
			tvwRecords.Nodes.Add(tndPluginRoot);
			tvwRecords.EndUpdate();
		}

		/// <summary>
		/// Reloads the plugin of the given node.
		/// </summary>
		/// <param name="p_tndPluginNode">The node whose plugin records are to be reloaded.</param>
		private void ReloadPlugin(TreeNode p_tndPluginNode)
		{
			TreeNode tndPluginRoot = p_tndPluginNode;
			while (!(tndPluginRoot.Tag is Plugin))
				tndPluginRoot = tndPluginRoot.Parent;

			CriticalRecordPlugin crpPlugin = (CriticalRecordPlugin)tndPluginRoot.Tag;
			tvwRecords.BeginUpdate();
			tndPluginRoot.Nodes.Clear();
			CreatePluginTree(crpPlugin, tndPluginRoot);
			tvwRecords.EndUpdate();
		}

		/// <summary>
		/// Creates the tree of records for the given plugin using the given node.
		/// </summary>
		/// <param name="p_plgPlugin">The <see cref="Plugin"/> for which to build a record tree.</param>
		/// <param name="p_tndNode">The <see cref="TreeNode"/> at which to root the record tree.</param>
		private void CreatePluginTree(CriticalRecordPlugin p_crpPlugin, TreeNode p_tndNode)
		{
			p_tndNode.Tag = p_crpPlugin;
			foreach (Rec recRecord in p_crpPlugin.Records)
				WalkPluginTree(p_crpPlugin, recRecord, p_tndNode);
		}

		/// <summary>
		/// Recursively builds the tree of records for the given record using the given node.
		/// </summary>
		/// <param name="p_plgPlugin">The <see cref="Plugin"/> for which to build a record tree.</param>
		/// <param name="p_recRecord">The <see cref="Rec"/> for which to build a record tree.</param>
		/// <param name="p_tndNode">The <see cref="TreeNode"/> at which to build the record tree.</param>
		private void WalkPluginTree(CriticalRecordPlugin p_crpPlugin, Rec p_recRecord, TreeNode p_tndNode)
		{
			if (p_recRecord.Name.Equals("TES4"))
				return;
			TreeNode tndSubNode = null;
			if (p_recRecord is Record)
			{
				tndSubNode = new TreeNode(String.Format("{0:x8}: {1}", ((Record)p_recRecord).FormID, p_recRecord.DescriptiveName));
				if (p_crpPlugin.IsRecordCritical(((Record)p_recRecord).FormID))
					tndSubNode.BackColor = Color.Red;
			}
			else
				tndSubNode = new TreeNode(p_recRecord.DescriptiveName);
			tndSubNode.Tag = p_recRecord;
			if (p_recRecord is GroupRecord)
				foreach (Rec recSubRecord in ((GroupRecord)p_recRecord).Records)
					WalkPluginTree(p_crpPlugin, recSubRecord, tndSubNode);
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
			CriticalRecordPlugin crpPlugin = tndRoot.Tag as CriticalRecordPlugin;

			Record recRecord = e.Node.Tag as Record;
			if (recRecord == null)
			{
				splitContainer1.Panel2Collapsed = true;
				return;
			}

			splitContainer1.Panel2Collapsed = false;
			m_booPopulatingForm = true;
			ckbIsCritical.Checked = crpPlugin.IsRecordCritical(recRecord.FormID);
			if (ckbIsCritical.Checked)
			{
				tbxReason.Text = crpPlugin.GetCriticalRecordInfo(recRecord.FormID).Reason;
				cbxSeverity.SelectedItem = crpPlugin.GetCriticalRecordInfo(recRecord.FormID).Severity;
			}
			else
				tbxReason.Text = null;
			m_booPopulatingForm = false;
		}

		/// <summary>
		/// Handles the events of the edit panel.
		/// </summary>
		/// <remarks>
		/// This persists the changes made to the criticality of the selected record.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void criticalInfoChanged(object sender, EventArgs e)
		{
			if (m_booPopulatingForm)
				return;

			TreeNode tndRoot = tvwRecords.SelectedNode;
			if (tndRoot == null)
				return;
			Record recRecord = tndRoot.Tag as Record;
			if (recRecord == null)
				return;
			while (tndRoot.Parent != null)
				tndRoot = tndRoot.Parent;
			CriticalRecordPlugin crpPlugin = tndRoot.Tag as CriticalRecordPlugin;
			if (ckbIsCritical.Checked)
				crpPlugin.SetCriticalRecord(recRecord.FormID, (CriticalRecordInfo.ConflictSeverity)cbxSeverity.SelectedItem, tbxReason.Text);
			else
				crpPlugin.UnsetCriticalRecord(recRecord.FormID);
			tvwRecords.SelectedNode.BackColor = ckbIsCritical.Checked ? Color.Red : Color.Transparent;
		}

		#region Menu Handling

		/// <summary>
		/// Handles the <see cref="ToolStripMenuItem.Click"/> event of the open menu item.
		/// </summary>
		/// <remarks>
		/// This opens a plugin for editing.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void openNewPluginToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (OpenModDialog.ShowDialog() == DialogResult.OK)
				foreach (string strPlugin in OpenModDialog.FileNames)
					LoadPlugin(strPlugin);
		}

		/// <summary>
		/// Handles the <see cref="ToolStripMenuItem.Click"/> event of the save menu item.
		/// </summary>
		/// <remarks>
		/// This save the current plugin.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (tvwRecords.SelectedNode == null)
			{
				MessageBox.Show("No plugin selected to save.", "Error");
				return;
			}
			TreeNode tndPlugin = tvwRecords.SelectedNode;
			while (!(tndPlugin.Tag is Plugin))
				tndPlugin = tndPlugin.Parent;
			CriticalRecordPlugin crpPlugin = (CriticalRecordPlugin)tndPlugin.Tag;
			crpPlugin.Save(tndPlugin.Text);
			crpPlugin.Name = tndPlugin.Text;
			ReloadPlugin(tndPlugin);
			splitContainer1.Panel2Collapsed = true;
		}

		/// <summary>
		/// Handles the <see cref="ToolStripMenuItem.Click"/> event of the close menu item.
		/// </summary>
		/// <remarks>
		/// This closes the current plugin without saving.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void closeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (tvwRecords.SelectedNode == null)
			{
				MessageBox.Show("No plugin selected to close.", "Error");
				return;
			}

			TreeNode tndPlugin = tvwRecords.SelectedNode;
			while (!(tndPlugin.Tag is Plugin))
				tndPlugin = tndPlugin.Parent;
			tndPlugin.Tag = null;
			tvwRecords.Nodes.Remove(tndPlugin);
		}

		/// <summary>
		/// Handles the <see cref="ToolStripMenuItem.Click"/> event of the close all menu item.
		/// </summary>
		/// <remarks>
		/// This closes all open plugins without saving.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show(this, "This will close all open plugins, and you will lose any unsaved changes.\n" +
				"Are you sure you wish to continue", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
			tvwRecords.Nodes.Clear();
			GC.Collect();
		}

		#endregion

		/// <summary>
		/// Raising the <see cref="Fomr.Closing"/> event of the form.
		/// </summary>
		/// <remarks>
		/// This cleans up the controls and save the current windows location.
		/// </remarks>
		/// <param name="e">The <see cref="CancelEventArgs"/> that will be passed to the event.</param>
		protected override void OnClosing(CancelEventArgs e)
		{
			tvwRecords.Nodes.Clear();
			Settings.SetWindowPosition("CREditor", this);
			base.OnClosing(e);
		}
	}
}
