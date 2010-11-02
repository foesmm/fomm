using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Fomm.PackageManager;
using Fomm.Games.Fallout3.Tools.TESsnip;

namespace Fomm.Games.Fallout3.Tools.CriticalRecords
{
	/// <summary>
	/// Describes the arguments for the <see cref="ConflictDetector.PluginProcessed"/> event.
	/// </summary>
	public class PluginProcessedEventArgs : EventArgs
	{
		#region Properties

		/// <summary>
		/// Gets or sets whether the conflict detection should be cancelled.
		/// </summary>
		/// <value>Whether the conflict detection should be cancelled.</value>
		public bool Cancel { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		public PluginProcessedEventArgs()
		{
			Cancel = false;
		}

		#endregion
	}

	/// <summary>
	/// Describes the arguments for the <see cref="ConflictDetector.ConflictDetected"/> event.
	/// </summary>
	public class ConflictDetectedEventArgs : EventArgs
	{
		#region Properties

		/// <summary>
		/// Gets the conflicted plugin.
		/// </summary>
		/// <value>The conflicted plugin.</value>
		public Plugin ConflictedPlugin { get; protected set; }

		/// <summary>
		/// Gets the conflicting plugin.
		/// </summary>
		/// <value>The conflicting plugin.</value>
		public Plugin ConflictingPlugin { get; protected set; }

		/// <summary>
		/// Gets the overridden form id.
		/// </summary>
		/// <value>The overridden form id.</value>
		public UInt32 FormId { get; protected set; }

		/// <summary>
		/// Gets the conflict info.
		/// </summary>
		/// <value>The conflict info.</value>
		public CriticalRecordInfo ConflictInfo { get; protected set; }

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with the given values.
		/// </summary>
		/// <param name="p_plgConflictedPlugin">The plugin that is conflicted.</param>
		/// <param name="p_plgConflictingPlugin">The plugin that is conflicting.</param>
		/// <param name="p_uintFormId">The form id that is overridden.</param>
		/// <param name="p_criInfo">The <see cref="CriticalRecordInfo"/> describing the conflict.</param>
		public ConflictDetectedEventArgs(Plugin p_plgConflictedPlugin, Plugin p_plgConflictingPlugin, UInt32 p_uintFormId, CriticalRecordInfo p_criInfo)
		{
			ConflictedPlugin = p_plgConflictedPlugin;
			ConflictingPlugin = p_plgConflictingPlugin;
			FormId = p_uintFormId;
			ConflictInfo = p_criInfo;
		}

		#endregion
	}

	/// <summary>
	/// Detects critical record conflicts.
	/// </summary>
	public class ConflictDetector
	{
		/// <summary>
		/// The list of plugins not to process.
		/// </summary>
		protected List<string> SKIP_PLUGINS = new List<string> { "fallout3.esm",
																"anchorage.esm",
																"thepitt.esm",
																"brokensteel.esm",
																"zeta.esm",
																"pointlookout.esm" };
		#region Events

		/// <summary>
		/// Raised after a plugin has been examined for conflicts.
		/// </summary>
		public event EventHandler<PluginProcessedEventArgs> PluginProcessed;

		/// <summary>
		/// Raised when a conflict is detected.
		/// </summary>
		public event EventHandler<ConflictDetectedEventArgs> ConflictDetected;

		/// <summary>
		/// Raises the <see cref="PluginProcessed"/> event.
		/// </summary>
		protected void OnPluginProcessed()
		{
			if (PluginProcessed != null)
			{
				PluginProcessedEventArgs ppaArgs = new PluginProcessedEventArgs();
				PluginProcessed(this, ppaArgs);
				m_booCancelled |= ppaArgs.Cancel;
			}
		}

		/// <summary>
		/// Raises the <see cref="ConflictDetected"/> event.
		/// </summary>
		/// <param name="p_plgConflictedPlugin">The plugin that is conflicted.</param>
		/// <param name="p_plgConflictingPlugin">The plugin that is conflicting.</param>
		/// <param name="p_uintFormId">The form id that is overridden.</param>
		/// <param name="p_criInfo">The <see cref="CriticalRecordInfo"/> describing the conflict.</param>
		protected void OnConflictDetected(Plugin p_plgConflictedPlugin, Plugin p_plgConflictingPlugin, UInt32 p_uintFormId, CriticalRecordInfo p_criInfo)
		{
			if (ConflictDetected != null)
			{
				ConflictDetectedEventArgs cdaArgs = new ConflictDetectedEventArgs(p_plgConflictedPlugin, p_plgConflictingPlugin, p_uintFormId, p_criInfo);
				ConflictDetected(this, cdaArgs);
			}
		}

		#endregion

		private bool m_booCancelled = false;

		/// <summary>
		/// Checks for conflicts with mod-author specified critical records. Used by background worker dialog.
		/// </summary>
		public void DetectConflicts(IList<string> p_lstOrderedPlugins)
		{
			m_booCancelled = false;
			Plugin plgPlugin = null;
			CriticalRecordPlugin crpBasePlugin = null;
			string strMasterPlugin = null;
			string strPlugin = null;
			UInt32 uintAdjustedFormId = 0;
			string strBasePlugin = null;
			for (Int32 intIndex = 0; intIndex < p_lstOrderedPlugins.Count; intIndex++)
			{
				strBasePlugin = p_lstOrderedPlugins[intIndex];
				if (m_booCancelled)
					return;

				OnPluginProcessed();

				if (SKIP_PLUGINS.Contains(strBasePlugin.ToLowerInvariant()))
					continue;

				crpBasePlugin = new CriticalRecordPlugin(Path.Combine(Program.GameMode.PluginsPath, strBasePlugin), false);
				if (!crpBasePlugin.HasCriticalRecordData)
					continue;
				for (Int32 i = intIndex + 1; i < p_lstOrderedPlugins.Count; i++)
				{
					strPlugin = p_lstOrderedPlugins[i];
					plgPlugin = new Plugin(Path.Combine(Program.GameMode.PluginsPath, strPlugin), false);
					foreach (UInt32 uintFormId in crpBasePlugin.CriticalRecordFormIds)
					{
						strMasterPlugin = crpBasePlugin.GetMaster((Int32)uintFormId >> 24) ?? strBasePlugin;
						if (plgPlugin.GetMasterIndex(strMasterPlugin) < 0)
							continue;
						uintAdjustedFormId = ((UInt32)plgPlugin.GetMasterIndex(strMasterPlugin) << 24);
						uintAdjustedFormId = uintAdjustedFormId + (uintFormId & 0x00ffffff);
						if (plgPlugin.ContainsFormId(uintAdjustedFormId))
							OnConflictDetected(crpBasePlugin, plgPlugin, uintFormId, crpBasePlugin.GetCriticalRecordInfo(uintFormId));
					}
				}
			}
		}
	}
}
