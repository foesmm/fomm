using System;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
	/// <summary>
	/// The possible states of a mod file.
	/// </summary>
	public enum ModFileState
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
	/// A dependency that requires a specified file to be in a specified <see cref="ModFileState"/>.
	/// </summary>
	public class FileDependency : IDependency
	{
		private string m_strFile = null;
		private ModFileState m_mfsState = ModFileState.Active;
		private DependencyStateManager m_dsmStateManager = null;

		#region Properties

		/// <summary>
		/// Gets the path of the file that must be in the specified <see cref="State"/>.
		/// </summary>
		/// <value>The path of the file that must be in the specified <see cref="State"/>.</value>
		public string File
		{
			get
			{
				return m_strFile;
			}
		}

		/// <summary>
		/// Gets the <see cref="ModFileState"/> that the specified <see cref="File"/> must be in.
		/// </summary>
		/// <value>The <see cref="ModFileState"/> that the specified <see cref="File"/> must be in.</value>
		public ModFileState State
		{
			get
			{
				return m_mfsState;
			}
		}

		/// <summary>
		/// Gets whether or not the dependency is fufilled.
		/// </summary>
		/// <remarks>
		/// The dependency is fufilled if the specified <see cref="File"/> is in the
		/// specified <see cref="State"/>.
		/// </remarks>
		/// <value>Whether or not the dependency is fufilled.</value>
		/// <seealso cref="IDependency.IsFufilled"/>
		public bool IsFufilled
		{
			get
			{
				switch (m_mfsState)
				{
					case ModFileState.Active:
						return (m_dsmStateManager.InstalledPlugins.ContainsKey(m_strFile) && m_dsmStateManager.InstalledPlugins[m_strFile]);
					case ModFileState.Inactive:
						return (m_dsmStateManager.InstalledPlugins.ContainsKey(m_strFile) && !m_dsmStateManager.InstalledPlugins[m_strFile]);
					case ModFileState.Missing:
						return (!m_dsmStateManager.InstalledPlugins.ContainsKey(m_strFile));
				}
				return false;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with the given values.
		/// </summary>
		/// <param name="p_strFile">The file that must be is the specified state.</param>
		/// <param name="p_mfsState">The state in which the specified file must be.</param>
		/// <param name="p_dsmStateManager">The manager that reports the currect install state.</param>
		public FileDependency(string p_strFile, ModFileState p_mfsState, DependencyStateManager p_dsmStateManager)
		{
			m_mfsState = p_mfsState;
			m_strFile = p_strFile;
			m_dsmStateManager = p_dsmStateManager;
		}

		#endregion

		/// <summary>
		/// Generates a text representation of the dependency.
		/// </summary>
		/// <returns>A text representation of the dependency.</returns>
		public override string ToString()
		{
			return m_strFile + " (" + m_mfsState.ToString() + ") : " + IsFufilled;
		}
	}
}
