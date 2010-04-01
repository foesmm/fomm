using System;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
	/// <summary>
	/// Defines the interface for a dependency
	/// </summary>
	public interface IDependency
	{
		/// <summary>
		/// Gets whether or not the dependency is fufilled, base on the given state.
		/// </summary>
		/// <value>Whether or not the dependency is fufilled, base on the given state.</value>
		bool IsFufilled { get; }
	}
}
