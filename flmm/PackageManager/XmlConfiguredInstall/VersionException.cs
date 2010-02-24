using System;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
	/// <summary>
	/// An exception class that represents a version mismatch.
	/// </summary>
	public class VersionException : ApplicationException
	{
		private Version m_verRequiredVersion = null;
		private Version m_verInstalledVersion = null;
		private String m_strName = null;

		#region Properties

		/// <summary>
		/// Gets or sets the minimum version required.
		/// </summary>
		/// <value>The minimum version required.</value>
		public Version RequiredVersion
		{
			get
			{
				return m_verRequiredVersion;
			}
			set
			{
				m_verRequiredVersion = value;
			}
		}

		/// <summary>
		/// Gets or sets the installed version. 
		/// </summary>
		/// <remarks>
		/// This value is null if the versioned component is not found.
		/// </remarks>
		/// <value>The installed version.</value>
		public Version InstalledVersion
		{
			get
			{
				return m_verInstalledVersion;
			}
			set
			{
				m_verInstalledVersion = value;
			}
		}

		/// <summary>
		/// Gets or sets the name of the component whose version is mismatched.
		/// </summary>
		/// <value>The name of the component whose version is mismatched.</value>
		public String ComponentName
		{
			get
			{
				return m_strName;
			}
			set
			{
				m_strName = value;
			}
		}


		#endregion

		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		public VersionException()
		{
		}

		/// <summary>
		/// A simple constructor that initializes the object's properties.
		/// </summary>
		/// <param name="p_verRequired">The component's required version.</param>
		/// <param name="p_verInstalled">The component's installed version; null if not installed.</param>
		/// <param name="p_strComponentName">The component's name.</param>
		public VersionException(Version p_verRequired, Version p_verInstalled, String p_strComponentName)
		{
			RequiredVersion = p_verRequired;
			InstalledVersion = p_verInstalled;
			ComponentName = p_strComponentName;
		}

		#endregion
	}
}
