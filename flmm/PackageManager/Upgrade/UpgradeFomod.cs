using System;
using System.Collections.Generic;
using System.Text;

namespace Fomm.PackageManager.Upgrade
{
	/// <summary>
	/// A specilised fomod that allows the overriding of the base name.
	/// </summary>
	/// <remarks>
	/// This is used when upgrading a fomod to a fomod that has a different base name.
	/// </remarks>
	internal class UpgradeFomod : fomod
	{
		private string m_strBaseName;

		#region Properties

		/// <summary>
		/// Gets the current base name of the fomod.
		/// </summary>
		/// <value>The current base name of the fomod.</value>
		internal override string BaseName
		{
			get
			{
				return m_strBaseName;
			}
		}

		/// <summary>
		/// Gets the original base name of the fomod.
		/// </summary>
		/// <remarks>
		/// This always returns the value that the <see cref="Fomod.BaseName"/> property
		/// would be expected to return.
		/// </remarks>
		/// <value>The original base name of the fomod.</value>
		internal string OriginalBaseName
		{
			get
			{
				return base.BaseName;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object.
		/// </summary>
		/// <param name="path">The path to the fomod file.</param>
		internal UpgradeFomod(string path)
			: base(path)
		{
		}

		#endregion

		/// <summary>
		/// Sets the base name of the fomod.
		/// </summary>
		/// <param name="p_strBaseName">The new base name of the fomod.</param>
		internal void SetBaseName(string p_strBaseName)
		{
			m_strBaseName = p_strBaseName;
		}
	}
}
