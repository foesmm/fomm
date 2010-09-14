using System;

namespace Fomm.PackageManager.ModInstallLog
{
	/// <summary>
	/// A summary of an installed fomod's info.
	/// </summary>
	public class FomodInfo : IComparable<FomodInfo>
	{
		#region Properties

		/// <summary>
		/// Gets or sets the base name of the fomod.
		/// </summary>
		/// <value>The base name of the fomod.</value>
		public string BaseName { get; protected set; }

		/// <summary>
		/// Gets or sets the human-readable version of the fomod.
		/// </summary>
		/// <value>The human-readable version of the fomod.</value>
		public string Version { get; protected set; }

		/// <summary>
		/// Gets or set the machine-readable version of the fomod.
		/// </summary>
		/// <value>The machine-readable version of the fomod.</value>
		public Version MachineVersion { get; protected set; }

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with the specified values.
		/// </summary>
		/// <param name="p_strBaseName">The base name of the fomod.</param>
		/// <param name="p_strVersion">The human-readable version of the fomod.</param>
		/// <param name="p_verMachineVersion">The machine-readable version of the fomod.</param>
		public FomodInfo(string p_strBaseName, string p_strVersion, Version p_verMachineVersion)
		{
			BaseName = p_strBaseName;
			Version = p_strVersion;
			MachineVersion = p_verMachineVersion;
		}

		#endregion

		#region IComparable<FomodInfo> Members

		/// <summary>
		/// Compares this fomod info to the given fomod info.
		/// </summary>
		/// <param name="other">The fomod info to which to compare this fomod info.</param>
		/// <returns>A value less than 0 if this fomod info is less than the given fomod info;
		/// or, a value of 0 if this fomod info is equal to the given fomod info;
		/// or, a value greater than 0 if this fomod is greater then the given fomod info.</returns>
		public int CompareTo(FomodInfo other)
		{
			Int32 intResult = BaseName.CompareTo(other.BaseName);
			if (intResult == 0)
				intResult = MachineVersion.CompareTo(other.MachineVersion);
			return intResult;
		}

		#endregion
	}
}
