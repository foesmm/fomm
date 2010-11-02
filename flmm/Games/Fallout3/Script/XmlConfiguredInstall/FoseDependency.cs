using System;
using Fomm.PackageManager.XmlConfiguredInstall;

namespace Fomm.Games.Fallout3.Script.XmlConfiguredInstall
{
	/// <summary>
	/// A dependency that requires a minimum version of FOSE to be installed.
	/// </summary>
	public class FoseDependency : IDependency
	{
		private Fallout3DependencyStateManager m_dsmStateManager = null;
		private Version m_verMinVersion = null;

		#region IDependency Members

		/// <summary>
		/// Gets whether or not the dependency is fufilled.
		/// </summary>
		/// <remarks>
		/// The dependency is fufilled if the specified minimum version of
		/// FOSE is installed.
		/// </remarks>
		/// <value>Whether or not the dependency is fufilled.</value>
		/// <seealso cref="IDependency.IsFufilled"/>
		public bool IsFufilled
		{
			get
			{
				Version verInstalledVersion = m_dsmStateManager.FoseVersion;
				return ((verInstalledVersion != null) && (verInstalledVersion >= m_verMinVersion));
			}
		}

		/// <summary>
		/// Gets a message describing whether or not the dependency is fufilled.
		/// </summary>
		/// <remarks>
		/// If the dependency is fufilled the message is "Passed." If the dependency is not fufilled the
		/// message informs the user of the installed version and gives the URL from whence to obtain
		/// an update.
		/// </remarks>
		/// <value>A message describing whether or not the dependency is fufilled.</value>
		/// <seealso cref="IDependency.Message"/>
		public string Message
		{
			get
			{
				Version verInstalledVersion = m_dsmStateManager.FoseVersion;
				if (verInstalledVersion == null)
					return String.Format("This mod requires FOSE v{0} or higher. Please download from http://silverlock.org", m_verMinVersion);
				else if (verInstalledVersion < m_verMinVersion)
					return String.Format("This mod requires FOSE v{0} or higher. You have {1}. Please update from http://silverlock.org", m_verMinVersion, verInstalledVersion);
				else
					return "Passed";
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with the given values.
		/// </summary>
		/// <param name="p_dsmStateManager">The manager that reports the currect install state.</param>
		/// <param name="p_verVersion">The minimum required version of FOSE.</param>
		public FoseDependency(Fallout3DependencyStateManager p_dsmStateManager, Version p_verVersion)
		{
			m_dsmStateManager = p_dsmStateManager;
			m_verMinVersion = p_verVersion;
		}

		#endregion

		/// <summary>
		/// Generates a text representation of the dependency.
		/// </summary>
		/// <returns>A text representation of the dependency.</returns>
		public override string ToString()
		{
			return "FOSE: " + m_verMinVersion + " =/= " + m_dsmStateManager.FoseVersion;
		}
	}
}
