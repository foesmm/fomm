using System;
using System.Collections.Generic;

namespace Fomm.Games
{
	/// <summary>
	/// The base class for game-specific plugin management.
	/// </summary>
	public abstract class PluginManager
	{
		#region Plugin Activation/Deactivation

		/// <summary>
		/// Gets the list of active plugins.
		/// </summary>
		/// <value>The list of active plugins.</value>
		public abstract string[] ActivePluginList
		{
			get;
		}

		/// <summary>
		/// Activates the specified plugin.
		/// </summary>
		/// <param name="p_strPath">The path to the plugin to activate.</param>
		public abstract void ActivatePlugin(string p_strPath);

		/// <summary>
		/// Deactivates the specified plugin.
		/// </summary>
		/// <param name="p_strPath">The path to the plugin to deactivate.</param>
		public abstract void DeactivatePlugin(string p_strPath);

		/// <summary>
		/// Activates the specified plugins.
		/// </summary>
		/// <param name="p_lstPaths">The paths to the plugins to activate.</param>
		public void ActivatePlugins(IList<string> p_lstPaths)
		{
			foreach (string strPath in p_lstPaths)
				ActivatePlugin(strPath);
		}

		/// <summary>
		/// Deactivates the specified plugins.
		/// </summary>
		/// <param name="p_lstPaths">The paths to the plugins to deactivate.</param>
		public void DeactivatePlugins(IList<string> p_lstPaths)
		{
			foreach (string strPath in p_lstPaths)
				DeactivatePlugin(strPath);
		}

		#endregion

		#region Plugin Ordering

		/// <summary>
		/// Gets an ordered list of plugins.
		/// </summary>
		/// <value>An ordered list of plugins.</value>
		public abstract string[] OrderedPluginList
		{
			get;
		}

		/// <summary>
		/// Sorts the list of plugins paths.
		/// </summary>
		/// <remarks>
		/// This sorts the plugin paths based on the load order of the plugins the paths represent.
		/// </remarks>
		/// <param name="p_strPlugins">The list of plugin paths to sort.</param>
		/// <returns>The sorted list of plugin paths.</returns>
		public abstract string[] SortPluginList(string[] p_strPlugins);

		/// <summary>
		/// Sets the load order of the specifid plugin.
		/// </summary>
		/// <param name="p_strPluginPath">The full path to the plugin file whose load order is to be set.</param>
		/// <param name="p_intPluginLoadOrderIndex">The new load order index of the plugin.</param>
		public abstract void SetLoadOrder(string p_strPluginPath, int p_intPluginLoadOrderIndex);

		#endregion

		/// <summary>
		/// Gets the plugin info for the specified plugin.
		/// </summary>
		/// <param name="p_strPluginPath">The full path to the plugin for which to get the info.</param>
		/// <returns>The plugin info for the specified plugin.</returns>
		/// <exception cref="FileNotFoundException">Thrown if the specified plug in does not exist.</exception>
		public abstract PluginInfo GetPluginInfo(string p_strPluginPath);

		/// <summary>
		/// Determines if the specified plugin is critical to the current game.
		/// </summary>
		/// <param name="p_strPluginPath">The full path to the plugin for which it is to be determined whether or not it is critical.</param>
		/// <returns><lang cref="true"/> if the specified pluing is critical;
		/// <lang cref="false"/> otherwise.</returns>
		public abstract bool IsCriticalPlugin(string p_strPluginPath);
	}
}
