using System;
using Fomm.PackageManager.XmlConfiguredInstall;
using Fomm.PackageManager;

namespace Fomm.Games.Fallout3.Script.XmlConfiguredInstall
{
	/// <summary>
	/// This class manages the state of the installation.
	/// </summary>
	public class Fallout3DependencyStateManager : DependencyStateManager
	{
		#region Properties

		/// <summary>
		/// Gets the installed version of FOSE.
		/// </summary>
		/// <remarks>
		/// <lang cref="null"/> is returned if FOSE is not installed.
		/// </remarks>
		/// <value>The installed version of FOSE.</value>
		public Version FoseVersion
		{
			get
			{
				return ((Fallout3ModInstallScript)Script).GetFoseVersion();
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with the given values.
		/// </summary>
		/// <param name="p_misInstallScript">The install script.</param>
		public Fallout3DependencyStateManager(ModInstallScript p_misInstallScript)
			: base(p_misInstallScript)
		{
		}

		#endregion
	}
}
