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
        foreach (IDependency dpnDependency in m_lstDependencies)
        {
          bool booThisFufilled = dpnDependency.IsFufilled;
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

    /// <summary>
    /// Gets a message describing whether or not the dependency is fufilled.
    /// </summary>
    /// <remarks>
    /// If the dependency is fufilled the message is "Passed." If the dependency is not fufilled the
    /// message is a list of the sub-dependecies' messages.
    /// </remarks>
    /// <value>A message describing whether or not the dependency is fufilled.</value>
    /// <seealso cref="IDependency.Message"/>
    public string Message
    {
      get
      {
        StringBuilder stbMessage = new StringBuilder();
        if (m_dopOperator == DependencyOperator.Or)
        {
          stbMessage.Append("(");
        }

        bool booAllFufilled = (m_dopOperator == DependencyOperator.And) ? true : false;
        for (Int32 i = 0; i < m_lstDependencies.Count; i++)
        {
          IDependency dpnDependency = m_lstDependencies[i];
          bool booThisFufilled = dpnDependency.IsFufilled;
          if (!booThisFufilled)
          {
            stbMessage.Append(dpnDependency.Message);
          }
          switch (m_dopOperator)
          {
            case DependencyOperator.And:
              if (i < m_lstDependencies.Count - 1)
              {
                stbMessage.AppendLine();
              }
              booAllFufilled &= booThisFufilled;
              break;
            case DependencyOperator.Or:
              if (i < m_lstDependencies.Count - 1)
              {
                stbMessage.AppendLine(" OR");
              }
              booAllFufilled |= booThisFufilled;
              break;
          }
        }
        if (m_dopOperator == DependencyOperator.Or)
        {
          stbMessage.Append(")");
        }
        return booAllFufilled ? "Passed" : stbMessage.ToString();
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

    /// <summary>
    /// Generates a text representation of the dependency.
    /// </summary>
    /// <returns>A text representation of the dependency.</returns>
    public override string ToString()
    {
      StringBuilder stbString = new StringBuilder("(");
      for (Int32 i = 0; i < m_lstDependencies.Count; i++)
      {
        IDependency dpdDependency = m_lstDependencies[i];
        stbString.Append(dpdDependency);
        if (i < m_lstDependencies.Count - 1)
        {
          stbString.Append(" ").AppendLine(m_dopOperator.ToString());
        }
      }
      stbString.Append(") => ").Append(IsFufilled);
      return stbString.ToString();
    }
  }
}