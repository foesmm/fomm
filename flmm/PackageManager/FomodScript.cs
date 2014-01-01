using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Fomm.PackageManager
{
	/// <summary>
	/// The possible types for the fomod script.
	/// </summary>
	public enum FomodScriptType
	{
		/// <summary>
		/// The C# script type.
		/// </summary>
		CSharp,

		/// <summary>
		/// XML configuration file script type.
		/// </summary>
		XMLConfig
	}

	/// <summary>
	/// Describes the script of a fomod.
	/// </summary>
	public class FomodScript
	{
		/// <summary>
		/// The mapping of valid script names to their respective script formats.
		/// </summary>
		private static Dictionary<string, FomodScriptType> m_dicFormats = new Dictionary<string, FomodScriptType>(StringComparer.InvariantCultureIgnoreCase)
																		{
																			{"script.cs", FomodScriptType.CSharp},
																			{"ModuleConfig.xml", FomodScriptType.XMLConfig}
																		};

		/// <summary>
		/// Get the list of valid script names.
		/// </summary>
		/// <value>The list of valid script names.</value>
		public static string[] ScriptNames
		{
			get
			{
				return new List<string>(m_dicFormats.Keys).ToArray();
			}
		}

		private FomodScriptType m_fstType = FomodScriptType.CSharp;
		private string m_strText = null;

		#region Properties

		/// <summary>
		/// Gets or sets the type of the script.
		/// </summary>
		/// <value>The type of the script.</value>
		public string FileName
		{
			get
			{
				foreach (KeyValuePair<string, FomodScriptType> kvpFormat in m_dicFormats)
					if (kvpFormat.Value.Equals(m_fstType))
						return kvpFormat.Key;
				throw new Exception("Unexpected value for FomodScriptType enum.");
			}
			set
			{
				string strLoweredValue = (value ?? "").ToLowerInvariant();
				if (!m_dicFormats.ContainsKey(strLoweredValue))
					throw new ArgumentException("Unrecognized script name: " + value);
				m_fstType = m_dicFormats[strLoweredValue];
			}
		}

		/// <summary>
		/// Gets or sets the script type.
		/// </summary>
		/// <value>The script type.</value>
		public FomodScriptType Type
		{
			get
			{
				return m_fstType;
			}
			set
			{
				m_fstType = value;
			}
		}

		/// <summary>
		/// Gets or sets the script text.
		/// </summary>
		/// <value>The script text.</value>
		public string Text
		{
			get
			{
				return m_strText;
			}
			set
			{
				if (value == null)
					m_strText = null;
				else
					m_strText = value.Replace(": BaseScript", ": Fallout3BaseScript");
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with the given values.
		/// </summary>
		/// <param name="p_fstType">The script type.</param>
		/// <param name="p_strText">The script text.</param>
		public FomodScript(FomodScriptType p_fstType, string p_strText)
		{
			Type = p_fstType;
			Text = p_strText;
		}

		/// <summary>
		/// A simple constructor that initializes the object with the given values.
		/// </summary>
		/// <param name="p_strPath">The path of the script file. This is used to determine the <see cref="Type"/>.</param>
		/// <param name="p_strText">The script text.</param>
		public FomodScript(string p_strPath, string p_strText)
		{
			FileName = Path.GetFileName(p_strPath);
			Text = p_strText;
		}

		#endregion
	}
}
