using System;
using System.Collections.Generic;
using System.Drawing;
using Fomm.Games.Fallout3.Tools.CriticalRecords;
using System.Text;
using Fomm.PackageManager.ModInstallLog;
using Fomm.PackageManager;
using System.IO;
using System.Windows.Forms;

namespace Fomm.Games.Fallout3.Tools
{
	/// <summary>
	/// Checks for conflicts with mod-author specified critical records.
	/// </summary>
	public class PluginConflictDetector
	{
		private const string EXTRA_INFO_CRITICAL_RECORDS = "CriticalRecords";
		
		private MainForm m_frmMainForm = null;
		private BackgroundWorkerProgressDialog m_bwdProgress = null;

		#region Properties

		/// <summary>
		/// Gets the main plugin management form.
		/// </summary>
		/// <value>The main plugin management form.</value>
		protected MainForm MainForm
		{
			get
			{
				return m_frmMainForm;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the obejct with the given values.
		/// </summary>
		/// <param name="p_frmMainForm">The main plugin management form.</param>
		public PluginConflictDetector(MainForm p_frmMainForm)
		{
			m_frmMainForm = p_frmMainForm;
		}

		#endregion

		/// <summary>
		/// Checks for conflicts with mod-author specified critical records.
		/// </summary>
		public void CheckForConflicts()
		{
			using (m_bwdProgress = new BackgroundWorkerProgressDialog(CheckForCriticalRecordConflicts))
			{
				m_bwdProgress.ShowItemProgress = false;
				m_bwdProgress.OverallProgressStep = 1;
				m_bwdProgress.OverallMessage = "Checking for conflicts...";
				if (m_bwdProgress.ShowDialog() == DialogResult.Cancel)
				{
					MainForm.ClearExtraInfo(EXTRA_INFO_CRITICAL_RECORDS);
					foreach (ListViewItem lviItem in MainForm.PluginsListViewItems)
						lviItem.BackColor = Color.Transparent;
				}
			}
		}

		/// <summary>
		/// Launches the conflict detector.
		/// </summary>
		/// <remarks>
		///  This method is called by a <see cref="BackgroundWorkerProgressDialog"/>.
		/// </remarks>
		private void CheckForCriticalRecordConflicts()
		{
			MainForm.ClearExtraInfo(EXTRA_INFO_CRITICAL_RECORDS);

			List<ListViewItem> lstItems = new List<ListViewItem>();
			foreach (ListViewItem lviItem in MainForm.PluginsListViewItems)
			{
				lstItems.Add(lviItem);
				lviItem.BackColor = Color.Transparent;
			}
			lstItems.Sort((a, b) =>
			{
				Int32 intIndexA = 0;
				Int32 intIndexB = 0;
				if (a == null)
				{
					if (b == null)
						return 0;
					return -1;
				}
				if (Int32.TryParse(a.SubItems[1].Text, System.Globalization.NumberStyles.HexNumber, null, out intIndexA))
				{
					if ((b != null) && Int32.TryParse(b.SubItems[1].Text, System.Globalization.NumberStyles.HexNumber, null, out intIndexB))
						return intIndexA.CompareTo(intIndexB);
					return 1;
				}
				if (!Int32.TryParse(b.SubItems[1].Text, System.Globalization.NumberStyles.HexNumber, null, out intIndexB))
					return 0;
				return -1;
			});

			Int32 intIndex = 0;
			for (Int32 i = lstItems.Count - 1; i >= 0; i--)
				if (!Int32.TryParse(lstItems[i].SubItems[1].Text, System.Globalization.NumberStyles.HexNumber, null, out intIndex))
					lstItems.RemoveAt(i);

			List<string> lstPlugins = new List<string>();
			foreach (ListViewItem lviItem in lstItems)
				lstPlugins.Add(lviItem.Text);

			m_bwdProgress.OverallProgressMaximum = lstPlugins.Count;
			ConflictDetector cdrDetector = new ConflictDetector();
			cdrDetector.ConflictDetected += new EventHandler<ConflictDetectedEventArgs>(cdrDetector_ConflictDetected);
			cdrDetector.PluginProcessed += new EventHandler<PluginProcessedEventArgs>(cdrDetector_PluginProcessed);
			cdrDetector.DetectConflicts(lstPlugins);
		}

		/// <summary>
		/// Called when the conflict detector has processed a plugin.
		/// </summary>
		/// <remarks>
		/// This updates the detector progress bar.
		/// </remarks>
		/// <param name="sender">The object that trigger the event.</param>
		/// <param name="e">A <see cref="PluginProcessedEventArgs"/> describing the event arguments.</param>
		private void cdrDetector_PluginProcessed(object sender, PluginProcessedEventArgs e)
		{
			m_bwdProgress.StepOverallProgress();
			e.Cancel = m_bwdProgress.Cancelled();
		}

		/// <summary>
		/// Called when the conflict detector has found a conflict.
		/// </summary>
		/// <remarks>
		/// This adds a message to the appropriate plugin's info box, and changes the plugin's
		/// node's background colour as appropriate.
		/// </remarks>
		/// <param name="sender">The object that trigger the event.</param>
		/// <param name="e">A <see cref="ConflictDetectedEventArgs"/> describing the event arguments.</param>
		private void cdrDetector_ConflictDetected(object sender, ConflictDetectedEventArgs e)
		{
			StringBuilder stbMessage = new StringBuilder();
			List<Color> lstBackgroundColours = new List<Color> { Color.LightSkyBlue, Color.Yellow, Color.Red };
			Int32 intColourIndex = 0;
			switch (e.ConflictInfo.Severity)
			{
				case CriticalRecordInfo.ConflictSeverity.Conflict:
					stbMessage.Append(@"\b \cf1 CONFLICT\cf0 :\b0  ");
					intColourIndex = 2;
					break;
				case CriticalRecordInfo.ConflictSeverity.Warning:
					stbMessage.Append(@"\b \cf2 WARNING\cf0 :\b0  ");
					intColourIndex = 1;
					break;
				case CriticalRecordInfo.ConflictSeverity.Info:
					stbMessage.Append(@"\b \cf3 INFO\cf0 :\b0  ");
					intColourIndex = 0;
					break;
			}
			foreach (ListViewItem lviItem in MainForm.PluginsListViewItems)
			{
				if (lviItem.Text.Equals(e.ConflictedPlugin.Name))
				{
					if (lstBackgroundColours.IndexOf(lviItem.BackColor) < intColourIndex)
						lviItem.BackColor = lstBackgroundColours[intColourIndex];
					break;
				}
			}

			if (InstallLog.Current.GetCurrentFileOwnerName(e.ConflictingPlugin.Name) == null)
				stbMessage.AppendFormat("Form Id \\b {0:x8}\\b0  is overridden by \\b {1}\\b0 .\\par \\pard\\li720\\sl240\\slmult1 {2}\\par \\pard\\sl240\\slmult1 ", e.FormId, e.ConflictingPlugin.Name, e.ConflictInfo.Reason);
			else
			{
				fomod fomodMod = new fomod(Path.Combine(Program.GameMode.ModDirectory, InstallLog.Current.GetCurrentFileOwnerName(e.ConflictingPlugin.Name) + ".fomod"));
				stbMessage.AppendFormat("Form Id \\b {0:x8}\\b0  is overridden by \\b {1}\\b0  in \\b {2}\\b0 .\\par \\pard\\li720\\sl240\\slmult1 {3}\\par \\pard\\sl240\\slmult1 ", e.FormId, e.ConflictingPlugin.Name, fomodMod.ModName, e.ConflictInfo.Reason);
			}
			MainForm.AddExtraInfo(EXTRA_INFO_CRITICAL_RECORDS, e.ConflictedPlugin.Name, stbMessage.ToString());
		}
	}
}
