using System;
using System.Collections.Generic;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
	/// <summary>
	/// A dependency that requires a specified flag to have a specific value.
	/// </summary>
	public class FlagDependency : IDependency
	{
		private DependencyStateManager m_dsmStateManager = null;

		#region Properties

		/// <summary>
		/// Gets or sets the name of the flag that must have a specific value.
		/// </summary>
		/// <value>The name of the flag that must have a specific value.</value>
		public string FlagName { get; protected set; }

		/// <summary>
		/// Gets or sets the value the flag that must have.
		/// </summary>
		/// <value>The value the flag that must have.</value>
		public string Value { get; protected set; }

		/// <summary>
		/// Gets whether or not the dependency is fufilled.
		/// </summary>
		/// <remarks>
		/// The dependency is fufilled if the specified flag has the specified value.
		/// </remarks>
		/// <value>Whether or not the dependency is fufilled.</value>
		/// <seealso cref="IDependency.IsFufilled"/>
		public bool IsFufilled
		{
			get
			{
				string strValue = null;
				m_dsmStateManager.FlagValues.TryGetValue(FlagName, out strValue);
				if (String.IsNullOrEmpty(Value))
					return String.IsNullOrEmpty(strValue);
				return Value.Equals(strValue);
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with the given values.
		/// </summary>
		/// <param name="p_strFile">The name of the falge that must have a specific value.</param>
		/// <param name="p_mfsState">The state in which the specified file must be.</param>
		/// <param name="p_dsmStateManager">The value the flag that must have.</param>
		public FlagDependency(string p_strFlagName, string p_strValue, DependencyStateManager p_dsmStateManager)
		{
			FlagName = p_strFlagName;
			Value = p_strValue;
			m_dsmStateManager = p_dsmStateManager;
		}

		#endregion

		/// <summary>
		/// Generates a text representation of the dependency.
		/// </summary>
		/// <returns>A text representation of the dependency.</returns>
		public override string ToString()
		{
			return FlagName + " = " + Value + " => " + IsFufilled;
		}
	}
}
