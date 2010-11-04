using System;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace Fomm.Util
{
	/// <summary>
	/// Extends the <see cref="StringCollection"/> to add implicit conversions to and from
	/// similar data structures.
	/// </summary>
	[Editor("System.Windows.Forms.Design.StringCollectionEditor", typeof(System.Drawing.Design.UITypeEditor))]
	public class SettingsList : StringCollection, IEnumerable<string>
	{
		#region String List Conversions

		/// <summary>
		/// This class decorates <see cref="StringEnumerator"/> to make it appear
		/// as a <see cref="IEnumerator{string}"/>.
		/// </summary>
		private class EnumeratorOfString : IEnumerator<string>
		{
			private StringEnumerator m_senEnumerator = null;

			#region Contructors

			/// <summary>
			/// A simple contructor that initializes the object with the given values.
			/// </summary>
			/// <param name="p_senEnumerator">The <see cref="StringEnumerator"/> to decorate.</param>
			public EnumeratorOfString(StringEnumerator p_senEnumerator)
			{
				m_senEnumerator = p_senEnumerator;
			}

			#endregion

			#region IEnumerator<string> Members

			/// <summary>
			/// Gets the current value in the enumeration.
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
			/// Disposes of the object.
			/// </summary>
			public void Dispose()
			{
				Reset();
			}

			#endregion

			#region IEnumerator Members

			/// <summary>
			/// Gets the current value in the enumeration.
			/// </summary>
			/// <value>The current value in the enumeration.</value>
			object System.Collections.IEnumerator.Current
			{
				get
				{
					return m_senEnumerator.Current;
				}
			}

			/// <summary>
			/// Moves to the next item in the enumeration.
			/// </summary>
			/// <returns><lang cref="true"/> if there is another item; <lang cref="false"/> otherwise.</returns>
			public bool MoveNext()
			{
				return m_senEnumerator.MoveNext();
			}

			/// <summary>
			/// Resets the enumeration.
			/// </summary>
			public void Reset()
			{
				m_senEnumerator.Reset();
			}

			#endregion
		}

		/// <summary>
		/// Implicitly converts <see cref="SettingsStringList"/> to a string array.
		/// </summary>
		/// <param name="arr">The <see cref="SettingsStringList"/> to convert to a string array.</param>
		/// <returns>A string array containing the strings in the given <see cref="SettingsStringList"/>.</returns>
		public static implicit operator string[](SettingsList arr)
		{
			List<string> lstValues = new List<string>(arr);
			return lstValues.ToArray();
		}

		/// <summary>
		/// Implicitly converts a string array to a <see cref="SettingsStringList"/>.
		/// </summary>
		/// <param name="values">The string array to convert to a <see cref="SettingsStringList"/>.</param>
		/// <returns>A <see cref="SettingsStringList"/> containing the strings in the given string array.</returns>
		public static implicit operator SettingsList(string[] values)
		{
			SettingsList sslValues = new SettingsList();
			sslValues.AddRange(values);
			return sslValues;
		}

		/// <summary>
		/// Implicitly converts a <see cref="List{string}"/> to a <see cref="SettingsStringList"/>.
		/// </summary>
		/// <param name="values">The <see cref="List{string}"/> to convert to a <see cref="SettingsStringList"/>.</param>
		/// <returns>A <see cref="SettingsStringList"/> containing the strings in the given <see cref="List{string}"/>.</returns>
		public static implicit operator SettingsList(List<string> values)
		{
			SettingsList sslValues = new SettingsList();
			sslValues.AddRange(values.ToArray());
			return sslValues;
		}

		#region IEnumerable<string> Members

		/// <summary>
		/// Returns an enumerate
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
		/// Implicitly converts <see cref="SettingsStringList"/> to a string array.
		/// </summary>
		/// <param name="arr">The <see cref="SettingsStringList"/> to convert to a string array.</param>
		/// <returns>A string array containing the strings in the given <see cref="SettingsStringList"/>.</returns>
		public static implicit operator Int32[](SettingsList arr)
		{
			List<Int32> lstValues = new List<Int32>();
			Int32 intValue = 0;
			for (Int32 i = 0; i < arr.Count; i++)
			{
				intValue = 0;
				Int32.TryParse(arr[i], out intValue);
				lstValues.Add(intValue);
			}
			return lstValues.ToArray();
		}

		/// <summary>
		/// Implicitly converts a string array to a <see cref="SettingsStringList"/>.
		/// </summary>
		/// <param name="values">The string array to convert to a <see cref="SettingsStringList"/>.</param>
		/// <returns>A <see cref="SettingsStringList"/> containing the strings in the given string array.</returns>
		public static implicit operator SettingsList(Int32[] values)
		{
			SettingsList sslValues = new SettingsList();
			foreach (Int32 intValue in values)
				sslValues.Add(intValue.ToString());
			return sslValues;
		}

		#endregion
	}
}
