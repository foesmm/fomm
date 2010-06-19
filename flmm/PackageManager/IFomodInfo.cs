using System;

namespace Fomm.PackageManager
{
	/// <summary>
	/// A contract specifying an object that contains information about a fomod.
	/// </summary>
	public interface IFomodInfo
	{
		/// <summary>
		/// Gets or sets the name of the fomod.
		/// </summary>
		/// <value>The name of the fomod.</value>
		string Name { get; set; }

		/// <summary>
		/// Gets or sets the human readable form of the fomod's version.
		/// </summary>
		/// <value>The human readable form of the fomod's version.</value>
		string HumanReadableVersion { get; set; }

		/// <summary>
		/// Gets or sets the version of the fomod.
		/// </summary>
		/// <value>The version of the fomod.</value>
		Version MachineVersion { get; set; }

		/// <summary>
		/// Gets or sets the author of the fomod.
		/// </summary>
		/// <value>The author of the fomod.</value>
		string Author { get; set; }

		/// <summary>
		/// Gets or sets the description of the fomod.
		/// </summary>
		/// <value>The description of the fomod.</value>
		string Description { get; set; }

		/// <summary>
		/// Gets or sets the minimum version of FOMM required to load the fomod.
		/// </summary>
		/// <value>The minimum version of FOMM required to load the fomod.</value>
		Version MinFommVersion { get; set; }

		/// <summary>
		/// Gets or sets the contact email of the fomod.
		/// </summary>
		/// <value>The contact email of the fomod.</value>
		string Email { get; set; }

		/// <summary>
		/// Gets or sets the website of the fomod.
		/// </summary>
		/// <value>The website of the fomod.</value>
		string Website { get; set; }

		/// <summary>
		/// Gets or sets the FOMM groups to which the fomod belongs.
		/// </summary>
		/// <value>The FOMM groups to which the fomod belongs.</value>
		string[] Groups { get; set; }
	}
}
