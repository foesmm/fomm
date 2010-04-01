using System;
using System.Collections.Generic;
using System.Text;

namespace Fomm.PackageManager.XmlConfiguredInstall
{
	/// <summary>
	/// The possible relations of dependencies.
	/// </summary>
	public enum DependencyOperator
	{
		/// <summary>
		/// Indicates all contained dependencies must be satisfied in order for this dependency to be satisfied.
		/// </summary>
		And,

		/// <summary>
		/// Indicates at least one listed dependency must be satisfied in order for this dependency to be satisfied.
		/// </summary>
		Or
	}

	/// <summary>
	/// A dependency that requires a combination of sub-dependencies to be fufilled.
	/// </summary>
	/// <remarks>
	/// The combination of sub-dependencies that must be fufilled is determined by an
	/// operator (e.g., and, or).
	/// </remarks>
	public class CompositeDependency : IDependency
	{
		private List<IDependency> m_lstDependencies = new List<IDependency>();
		private DependencyOperator m_dopOperator = DependencyOperator.And;

		#region Properties

		/// <summary>
		/// Gets the <see cref="DependencyOperator"/> specifying which of the sub-dependencies
		/// must be fufilled in order for this dependency to be fufilled.
		/// </summary>
		/// <value>The <see cref="DependencyOperator"/> specifying which of the sub-dependencies
		/// must be fufilled in order for this dependency to be fufilled.</value>
		public DependencyOperator Operator
		{
			get
			{
				return m_dopOperator;
			}
		}

		/// <summary>
		/// Gets the sub-dependencies.
		/// </summary>
		/// <value>The sub-dependencies.</value>
		public IList<IDependency> Dependencies
		{
			get
			{
				return m_lstDependencies;
			}
		}

		/// <summary>
		/// Determines if the given composite dependency if fufilled.
		/// </summary>
		/// <remarks>
		/// A composite dependency is fufilled if and only if its contained dependencies
		/// are fufilled in the combination specified by the <see cref="Operator"/>.</remarks>
		public bool IsFufilled
		{
			get
			{
				bool booAllFufilled = (m_dopOperator == DependencyOperator.And) ? true : false;
				bool booThisFufilled = true;
				foreach (IDependency dpnDependency in m_lstDependencies)
				{
					booThisFufilled = dpnDependency.IsFufilled;
					switch (m_dopOperator)
					{
						case DependencyOperator.And:
							booAllFufilled &= booThisFufilled;
							break;
						case DependencyOperator.Or:
							booAllFufilled |= booThisFufilled;
							break;
					}
				}
				return booAllFufilled;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with the given values.
		/// </summary>
		/// <param name="p_dopOperator">The operator that specifies what combination of sub-dependencies
		/// must be fufilled in order for this dependancy to be fufilled.</param>
		public CompositeDependency(DependencyOperator p_dopOperator)
		{
			m_dopOperator = p_dopOperator;
		}

		#endregion

		public override string ToString()
		{
			StringBuilder stbString = new StringBuilder("(");
			IDependency dpdDependency = null;
			for (Int32 i = 0; i < m_lstDependencies.Count; i++)
			{
				dpdDependency = m_lstDependencies[i];
				stbString.Append(dpdDependency);
				if (i < m_lstDependencies.Count - 1)
					stbString.Append(" ").AppendLine(m_dopOperator.ToString());
			}
			stbString.Append(") => ").Append(IsFufilled);
			return stbString.ToString();
		}
	}
}
