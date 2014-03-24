using System;
using System.Collections.Generic;
using System.Drawing;
using Fomm.Games.Fallout3.Tools.CriticalRecords;
using System.Text;
using Fomm.PackageManager.ModInstallLog;
using Fomm.PackageManager;
using System.IO;
using System.Windows.Forms;
using Fomm.Games.Fallout3.PluginFormatProviders;

namespace Fomm.Games.Fallout3.Tools
{
  /// <summary>
  /// Checks for conflicts with mod-author specified critical records.
  /// </summary>
  public class PluginConflictDetector
  {
    private CriticalRecordPluginFormatProvider m_pfpFormatProvider = null;
    private BackgroundWorkerProgressDialog m_bwdProgress = null;

    #region Constructors

    /// <summary>
    /// A simple constructor that initializes the obejct with the given values.
    /// </summary>
    /// <param name="p_frmMainForm">The main plugin management form.</param>
    public PluginConflictDetector(CriticalRecordPluginFormatProvider p_pfpFormatProvider)
    {
      m_pfpFormatProvider = p_pfpFormatProvider;
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
          m_pfpFormatProvider.Clear();
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
      m_pfpFormatProvider.Clear();
      List<string> lstPlugins = new List<string>(Program.GameMode.PluginManager.SortPluginList(Program.GameMode.PluginManager.ActivePluginList));

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
      Color clrHighlight = lstBackgroundColours[intColourIndex];
      if (m_pfpFormatProvider.HasFormat(e.ConflictedPlugin.Name))
      {
        PluginFormat pftFormat = m_pfpFormatProvider.GetFormat(e.ConflictedPlugin.Name);
        if (pftFormat.Highlight.HasValue && (lstBackgroundColours.IndexOf(pftFormat.Highlight.Value) > intColourIndex))
          clrHighlight = pftFormat.Highlight.Value;
      }

      if (InstallLog.Current.GetCurrentFileOwnerName(e.ConflictingPlugin.Name) == null)
        stbMessage.AppendFormat("Form Id \\b {0:x8}\\b0  is overridden by \\b {1}\\b0 .\\par \\pard\\li720\\sl240\\slmult1 {2}\\par \\pard\\sl240\\slmult1 ", e.FormId, e.ConflictingPlugin.Name, e.ConflictInfo.Reason);
      else
      {
        fomod fomodMod = new fomod(Path.Combine(Program.GameMode.ModDirectory, InstallLog.Current.GetCurrentFileOwnerName(e.ConflictingPlugin.Name) + ".fomod"));
        stbMessage.AppendFormat("Form Id \\b {0:x8}\\b0  is overridden by \\b {1}\\b0  in \\b {2}\\b0 .\\par \\pard\\li720\\sl240\\slmult1 {3}\\par \\pard\\sl240\\slmult1 ", e.FormId, e.ConflictingPlugin.Name, fomodMod.ModName, e.ConflictInfo.Reason);
      }
      m_pfpFormatProvider.AddFormat(e.ConflictedPlugin.Name, clrHighlight, stbMessage.ToString());
    }
  }
}
