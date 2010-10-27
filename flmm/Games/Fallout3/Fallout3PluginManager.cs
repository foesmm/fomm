using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Fomm.Games.Fallout3
{
	/// <summary>
	/// Activates/deactivates Fallout 3 plugins.
	/// </summary>
	public class Fallout3PluginManager : PluginManager
	{
		private List<string> m_lstPluginPaths = null;

		#region Plugin Activation/Deactivation

		/// <summary>
		/// Gets the list of active plugins.
		/// </summary>
		/// <value>The list of active plugins.</value>
		public override string[] ActivePluginList
		{
			get
			{
				string strPluginsFilePath = ((Fallout3GameMode)Program.GameMode).PluginsFilePath;

				List<string> lstActivePlugins = null;
				if (File.Exists(strPluginsFilePath))
				{
					string[] strPlugins = File.ReadAllLines(strPluginsFilePath);
					char[] strInvalidChars = Path.GetInvalidFileNameChars();
					for (int i = 0; i < strPlugins.Length; i++)
					{
						strPlugins[i] = strPlugins[i].Trim();
						if (strPlugins[i].Length == 0 || strPlugins[i][0] == '#' || strPlugins[i].IndexOfAny(strInvalidChars) != -1)
							continue;
						string strPluginPath = Path.Combine(Program.GameMode.PluginsPath, strPlugins[i]);
						if (!File.Exists(strPluginPath))
							continue;
						lstActivePlugins.Add(strPluginPath);
					}
				}
				else
					lstActivePlugins = new List<string>();
				return lstActivePlugins.ToArray();
			}
		}

		/// <summary>
		/// Activates the specified plugin.
		/// </summary>
		/// <param name="p_strPath">The path to the plugin to activate.</param>
		public override void ActivatePlugin(string p_strPath)
		{
			string strPluginsFilePath = ((Fallout3GameMode)Program.GameMode).PluginsFilePath;
			List<string> lstActivePlugins = null;
			if (File.Exists(strPluginsFilePath))
				lstActivePlugins = new List<string>(File.ReadAllLines(strPluginsFilePath));
			else
				lstActivePlugins = new List<string>();

			string strPluginFilename = Path.GetFileName(p_strPath);
			if (!lstActivePlugins.Contains(strPluginsFilePath))
				lstActivePlugins.Add(strPluginFilename);

			File.WriteAllLines(strPluginsFilePath, lstActivePlugins.ToArray(), System.Text.Encoding.Default);
		}

		/// <summary>
		/// Deactivates the specified plugin.
		/// </summary>
		/// <param name="p_strPath">The path to the plugin to deactivate.</param>
		public override void DeactivatePlugin(string p_strPath)
		{
			string strPluginsFilePath = ((Fallout3GameMode)Program.GameMode).PluginsFilePath;
			if (File.Exists(strPluginsFilePath))
			{
				List<string> lstActivePlugins = new List<string>(File.ReadAllLines(strPluginsFilePath));
				string strPluginFilename = Path.GetFileName(p_strPath);
				lstActivePlugins.Remove(strPluginFilename);

				File.WriteAllLines(strPluginsFilePath, lstActivePlugins.ToArray(), System.Text.Encoding.Default);
			}
		}

		#endregion

		#region Plugin Ordering

		/// <summary>
		/// Gets an ordered list of plugins.
		/// </summary>
		/// <value>An ordered list of plugins.</value>
		public override string[] OrderedPluginList
		{
			get
			{
				return m_lstPluginPaths.ToArray();
			}
		}

		/// <summary>
		/// Sorts the list of plugins paths.
		/// </summary>
		/// <remarks>
		/// This sorts the plugin paths based on the load order of the plugins the paths represent.
		/// </remarks>
		/// <param name="p_strPlugins">The list of plugin paths to sort.</param>
		/// <returns>The sorted list of plugin paths.</returns>
		public override string[] SortPluginList(string[] p_strPlugins)
		{
			List<FileInfo> lstPlugins = new List<FileInfo>();
			string strPlugin = null;
			for (Int32 i = 0; i < p_strPlugins.Length; i++)
			{
				strPlugin = p_strPlugins[i];
				if (!strPlugin.StartsWith(Program.GameMode.PluginsPath, StringComparison.InvariantCultureIgnoreCase) && !File.Exists(strPlugin))
					strPlugin = Path.Combine(Program.GameMode.PluginsPath, strPlugin);
				if (File.Exists(strPlugin))
					lstPlugins.Add(new FileInfo(strPlugin));
			}
			lstPlugins.Sort(delegate(FileInfo a, FileInfo b)
			{
				if (Tools.TESsnip.Plugin.GetIsEsm(a.FullName) == Tools.TESsnip.Plugin.GetIsEsm(b.FullName))
					return a.LastWriteTime.CompareTo(b.LastWriteTime);
				return Tools.TESsnip.Plugin.GetIsEsm(a.FullName) ? -1 : 1;
			});
			List<string> lstPluginPaths = new List<string>();
			foreach (FileInfo fifPlugin in lstPlugins)
				lstPluginPaths.Add(fifPlugin.FullName);
			return lstPluginPaths.ToArray();
		}

		public void LoadPluginList()
		{
			m_lstPluginPaths = new List<string>();

			DirectoryInfo difPluginsDirectory = new DirectoryInfo(Program.GameMode.PluginsPath);
			List<FileInfo> lstPlugins = new List<FileInfo>(Program.GetFiles(difPluginsDirectory, "*.esp"));
			lstPlugins.AddRange(Program.GetFiles(difPluginsDirectory, "*.esm"));

			lstPlugins.Sort(delegate(FileInfo a, FileInfo b)
			{
				if (Tools.TESsnip.Plugin.GetIsEsm(a.FullName) == Tools.TESsnip.Plugin.GetIsEsm(b.FullName))
					return a.LastWriteTime.CompareTo(b.LastWriteTime);
				return Tools.TESsnip.Plugin.GetIsEsm(a.FullName) ? -1 : 1;
			});

			List<string> lstPluginPaths = new List<string>();
			for (Int32 i = 0; i < lstPlugins.Count; i++)
			{
				FileInfo fifPlugin = lstPlugins[i];
				m_lstPluginPaths.Add(fifPlugin.FullName);

				if ((fifPlugin.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				{
					if (MessageBox.Show(null, String.Format("'{0}' is read-only, so its load order cannot be changed. Would you like to make it not read-only?", fifPlugin.Name), "Read Only", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
						fifPlugin.Attributes &= ~FileAttributes.ReadOnly;
				}
			}
		}

		/// <summary>
		/// Sets the load order of the specifid plugin.
		/// </summary>
		/// <param name="p_strPluginPath">The full path to the plugin file whose load order is to be set.</param>
		/// <param name="p_intPluginLoadOrderIndex">The new load order index of the plugin.</param>
		public override void SetLoadOrder(string p_strPluginPath, Int32 p_intOrderIndex)
		{
			DateTime dteTimestamp = new DateTime(2008, 1, 1);
			TimeSpan tspTwoMins = TimeSpan.FromMinutes(2 + p_intOrderIndex);
			dteTimestamp += tspTwoMins;
			File.SetLastWriteTime(p_strPluginPath, dteTimestamp);
		}

		#endregion

		#region Plugin Activation

		public override void CommitActivePlugins(List<string> p_strActivePlugins)
		{
			if (p_strActivePlugins == null)
				return;
			File.WriteAllLines(((Fallout3GameMode)Program.GameMode).PluginsFilePath, p_strActivePlugins.ToArray());
		}

		#endregion
	}
}
