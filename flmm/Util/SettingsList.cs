using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing.Design;

namespace Fomm.Util
{
  /// <summary>
  ///   Extends the <see cref="StringCollection" /> to add implicit conversions to and from
  ///   similar data structures.
  /// </summary>
  [Editor("System.Windows.Forms.Design.StringCollectionEditor", typeof (UITypeEditor))]
  public class SettingsList : StringCollection, IEnumerable<string>
  {
    #region String List Conversions

    /// <summary>
    ///   This class decorates <see cref="StringEnumerator" /> to make it appear
    ///   as a IEnumerator{string} />.
    /// </summary>
    private class EnumeratorOfString : IEnumerator<string>
    {
      private StringEnumerator m_senEnumerator;

      #region Contructors

      /// <summary>
      ///   A simple contructor that initializes the object with the given values.
      /// </summary>
      /// <param name="p_senEnumerator">The <see cref="StringEnumerator" /> to decorate.</param>
      public EnumeratorOfString(StringEnumerator p_senEnumerator)
      {
        m_senEnumerator = p_senEnumerator;
      }

      #endregion

      #region IEnumerator<string> Members

      /// <summary>
      ///   Gets the current value in the enumeration.
      /// </summary>
      /// <value>The current value in the enumeration.</value>
      public string Current
      {
        get
        {
          return m_senEnumerator.Current;
        }
      }

      #endregion

      #region IDisposable Members

      /// <summary>
      ///   Disposes of the object.
      /// </summary>
      public void Dispose()
      {
        Reset();
      }

      #endregion

      #region IEnumerator Members

      /// <summary>
      ///   Gets the current value in the enumeration.
      /// </summary>
      /// <value>The current value in the enumeration.</value>
      object IEnumerator.Current
      {
        get
        {
          return m_senEnumerator.Current;
        }
      }

      /// <summary>
      ///   Moves to the next item in the enumeration.
      /// </summary>
      /// <returns><lang langref="true" /> if there is another item; <lang langref="false" /> otherwise.</returns>
      public bool MoveNext()
      {
        return m_senEnumerator.MoveNext();
      }

      /// <summary>
      ///   Resets the enumeration.
      /// </summary>
      public void Reset()
      {
        m_senEnumerator.Reset();
      }

      #endregion
    }

    /// <summary>
    ///   Implicitly converts <see cref="SettingsStringList" /> to a string array.
    /// </summary>
    /// <param name="arr">The <see cref="SettingsStringList" /> to convert to a string array.</param>
    /// <returns>A string array containing the strings in the given <see cref="SettingsStringList" />.</returns>
    public static implicit operator string[](SettingsList arr)
    {
      if (arr == null)
      {
        return null;
      }
      var lstValues = new List<string>(arr);
      return lstValues.ToArray();
    }

    /// <summary>
    ///   Implicitly converts a string array to a <see cref="SettingsStringList" />.
    /// </summary>
    /// <param name="values">The string array to convert to a <see cref="SettingsStringList" />.</param>
    /// <returns>A <see cref="SettingsStringList" /> containing the strings in the given string array.</returns>
    public static implicit operator SettingsList(string[] values)
    {
      if (values == null)
      {
        return null;
      }
      var sslValues = new SettingsList();
      sslValues.AddRange(values);
      return sslValues;
    }

    /// <summary>
    ///   Implicitly converts a List{string} to a SettingsStringList/>.
    /// </summary>
    /// <param name="values">The List{string} to convert to a SettingsStringList.</param>
    /// <returns>A SettingsStringList containing the strings in the given List{string}.</returns>
    public static implicit operator SettingsList(List<string> values)
    {
      if (values == null)
      {
        return null;
      }
      var sslValues = new SettingsList();
      sslValues.AddRange(values.ToArray());
      return sslValues;
    }

    #region IEnumerable<string> Members

    /// <summary>
    ///   Returns an enumerate
    /// </summary>
    /// <returns></returns>
    public new IEnumerator<string> GetEnumerator()
    {
      return new EnumeratorOfString(base.GetEnumerator());
    }

    #endregion

    #endregion

    #region Int32 List Conversions

    /// <summary>
    ///   Implicitly converts <see cref="SettingsStringList" /> to a string array.
    /// </summary>
    /// <param name="arr">The <see cref="SettingsStringList" /> to convert to a string array.</param>
    /// <returns>A string array containing the strings in the given <see cref="SettingsStringList" />.</returns>
    public static implicit operator Int32[](SettingsList arr)
    {
      if (arr == null)
      {
        return null;
      }
      var lstValues = new List<Int32>();
      foreach (var s in arr)
      {
        int intValue;
        Int32.TryParse(s, out intValue);
        lstValues.Add(intValue);
      }
      return lstValues.ToArray();
    }

    /// <summary>
    ///   Implicitly converts a string array to a <see cref="SettingsStringList" />.
    /// </summary>
    /// <param name="values">The string array to convert to a <see cref="SettingsStringList" />.</param>
    /// <returns>A <see cref="SettingsStringList" /> containing the strings in the given string array.</returns>
    public static implicit operator SettingsList(Int32[] values)
    {
      if (values == null)
      {
        return null;
      }
      var sslValues = new SettingsList();
      foreach (var intValue in values)
      {
        sslValues.Add(intValue.ToString());
      }
      return sslValues;
    }

    #endregion
  }
}