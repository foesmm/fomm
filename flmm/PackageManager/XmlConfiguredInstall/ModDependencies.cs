using System;
using System.Collections.Generic;
using System.Text;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
	/// <summary>
	/// Encapsulates the mod level dependencies of a mod.
	/// </summary>
	public class ModDependencies
	{
		#region Properties

		/// <summary>
		/// Gets or sets the programme versions that the mod depends on.
		/// </summary>
		/// <value>The programme versions that the mod depends on.</value>
		public IDictionary<string, Version> ProgrammeDependencies { get; protected set; }

		/// <summary>
		/// Gets or sets the files that the mod depends on.
		/// </summary>
		/// <value>The files that the mod depends on.</value>
		public IList<string> FileDependencies { get; protected set; }

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with the given values.
		/// </summary>
		/// <param name="p_dicProgrameDependencies">The programme versions that the mod depends on.</param>
		/// <param name="p_lstFileDependencies">The files that the mod depends on.</param>
		public ModDependencies(IDictionary<string, Version> p_dicProgrameDependencies, IList<string> p_lstFileDependencies)
		{
			ProgrammeDependencies = p_dicProgrameDependencies;
			FileDependencies = p_lstFileDependencies;
		}

		#endregion
	}
}
