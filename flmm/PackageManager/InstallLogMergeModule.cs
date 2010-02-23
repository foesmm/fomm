using System;
using System.Xml;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Fomm.PackageManager
{
	/// <summary>
	/// A mod's install log tracks which files were installed as a
	/// pasrt of a specific mod. This is used when the mod is uninstalled.
	/// </summary>
	class InstallLogMergeModule : InstallLogBase
	{
		/// <summary>
		/// Describes an edit to an Ini file.
		/// </summary>
		internal class IniEdit : IComparable<IniEdit>
		{
			private string m_strFile = null;
			private string m_strSection = null;
			private string m_strKey = null;
			private string m_strValue;

			#region Properties

			/// <summary>
			/// Gets the file that was edited.
			/// </summary>
			/// <value>The file that was edited.</value>
			public string File
			{
				get
				{
					return m_strFile;
				}
			}

			/// <summary>
			/// Gets the section in the file that was edited.
			/// </summary>
			/// <value>The section in the file that was edited.</value>
			public string Section
			{
				get
				{
					return m_strSection;
				}
			}

			/// <summary>
			/// Gets the key in the file that was edited.
			/// </summary>
			/// <value>The key in the file that was edited.</value>
			public string Key
			{
				get
				{
					return m_strKey;
				}
			}

			/// <summary>
			/// Gets or sets the value to which the key was set.
			/// </summary>
			/// <value>The value to which the key was set.</value>
			public string Value
			{
				get
				{
					return m_strValue;
				}
				set
				{
					m_strValue = value;
				}
			}

			#endregion

			#region Constructors

			/// <summary>
			/// A simple constructor that initializes the object with the given values.
			/// </summary>
			/// <param name="p_strFile">The Ini file that was edited.</param>
			/// <param name="p_strSection">The section in the Ini file that was edited.</param>
			/// <param name="p_strKey">The key in the Ini file that was edited.</param>
			public IniEdit(string p_strFile, string p_strSection, string p_strKey)
			{
				m_strFile = p_strFile;
				m_strSection = p_strSection;
				m_strKey = p_strKey;
			}

			#endregion

			#region IComparable<IniEdit> Members

			/// <summary>
			/// Compares this IniEdit to the given IniEdit.
			/// </summary>
			/// <remarks>
			/// Two IniEdit objects can be strictly ordered by
			/// the following properties in the following order:
			/// File, Section, Key
			/// </remarks>
			/// <param name="other">The IniEdit to which to compare this IniEdit.</param>
			/// <returns>A value less than zero if this instance is less than the given instance,
			/// or a value of zero  if this instance is equal to the given instance,
			/// or a value greater than zero if this instance is greater than the given
			/// instance.</returns>
			public int CompareTo(IniEdit other)
			{
				Int32 intResult = m_strFile.CompareTo(other.m_strFile);
				if (intResult == 0)
				{
					intResult = m_strSection.CompareTo(other.m_strSection);
					if (intResult == 0)
						intResult = m_strKey.CompareTo(other.m_strKey);
				}
				return intResult;
			}

			#endregion
		}

		/// <summary>
		/// Describes an edit to an Sdp.
		/// </summary>
		internal class SdpEdit : IComparable<SdpEdit>
		{
			private Int32 m_intPackage = -1;
			private string m_strShaderName = null;
			private byte[] m_bteData;

			#region Properties

			/// <summary>
			/// Gets the package that was edited.
			/// </summary>
			/// <value>The package that was edited.</value>
			public Int32 Package
			{
				get
				{
					return m_intPackage;
				}
			}

			/// <summary>
			/// Gets the name of the sahder that was edited.
			/// </summary>
			/// <value>The name of the sahder that was edited.</value>
			public string ShaderName
			{
				get
				{
					return m_strShaderName;
				}
			}

			/// <summary>
			/// Gets or sets the data to which the Sdp was set.
			/// </summary>
			/// <value>The data to which the Sdp was set.</value>
			public byte[] Data
			{
				get
				{
					return m_bteData;
				}
				set
				{
					m_bteData = value;
				}
			}

			#endregion

			#region Constructors

			/// <summary>
			/// A simple constructor that initializes the object with the given values.
			/// </summary>
			/// <param name="p_intPackage">The package that was edited.</param>
			/// <param name="p_strShaderName">The name of the shader that was edited.</param>
			public SdpEdit(Int32 p_intPackage, string p_strShaderName)
			{
				m_intPackage = p_intPackage;
				m_strShaderName = p_strShaderName;
			}

			#endregion

			#region IComparable<SdpEdit> Members

			/// <summary>
			/// Compares this SdpEdit to the given SdpEdit.
			/// </summary>
			/// <remarks>
			/// Two SdpEdit objects can be strictly ordered by
			/// the following properties in the following order:
			/// Package, ShaderName
			/// </remarks>
			/// <param name="other">The SdpEdit to which to compare this SdpEdit.</param>
			/// <returns>A value less than zero if this instance is less than the given instance,
			/// or a value of zero  if this instance is equal to the given instance,
			/// or a value greater than zero if this instance is greater than the given
			/// instance.</returns>
			public int CompareTo(SdpEdit other)
			{
				Int32 intResult = m_intPackage.CompareTo(other.m_intPackage);
				if (intResult == 0)
					intResult = m_strShaderName.CompareTo(other.m_strShaderName);
				return intResult;
			}

			#endregion
		}
	
		private List<string> m_lstDataFiles = null;
		private List<string> m_lstReplacedDataFiles = null;
		private List<IniEdit> m_lstIniEdits = null;
		private List<IniEdit> m_lstReplacedIniValues = null;
		private List<SdpEdit> m_lstSdpEdits = null;
		private List<SdpEdit> m_lstReplacedSdpData = null;

		#region Properties

		/// <summary>
		/// Gets the list of data files installed during an install.
		/// </summary>
		/// <value>The list of data files installed during an install.</value>
		internal List<string> DataFiles
		{
			get
			{
				return m_lstDataFiles;
			}
		}

		/// <summary>
		/// Gets the list of original data files that were overwritten.
		/// </summary>
		/// <value>The list of original  data files that were overwritten.</value>
		internal List<string> ReplacedOriginalDataFiles
		{
			get
			{
				return m_lstReplacedDataFiles;
			}
		}

		/// <summary>
		/// Gets the list of Ini edits performed during an install.
		/// </summary>
		/// <value>The list of Ini edits performed during an install.</value>
		internal List<IniEdit> IniEdits
		{
			get
			{
				return m_lstIniEdits;
			}
		}

		/// <summary>
		/// Gets the list of original Ini values that were overwritten.
		/// </summary>
		/// <value>The list of original Ini values that were overwritten.</value>
		internal List<IniEdit> ReplacedOriginalIniValues
		{
			get
			{
				return m_lstReplacedIniValues;
			}
		}

		/// <summary>
		/// Gets the list of Sdp edits performed during an install.
		/// </summary>
		/// <value>The list of Sdp edits performed during an install.</value>
		internal List<SdpEdit> SdpEdits
		{
			get
			{
				return m_lstSdpEdits;
			}
		}

		/// <summary>
		/// Gets the list of original shader data that were overwritten.
		/// </summary>
		/// <value>The list of original shader data that were overwritten.</value>
		internal List<SdpEdit> ReplacedOriginalSdpData
		{
			get
			{
				return m_lstReplacedSdpData;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object.
		/// </summary>
		public InstallLogMergeModule()
		{
			m_lstDataFiles = new List<string>();
			m_lstReplacedDataFiles = new List<string>();
			m_lstIniEdits = new List<IniEdit>();
			m_lstReplacedIniValues = new List<IniEdit>();
			m_lstSdpEdits = new List<SdpEdit>();
			m_lstReplacedSdpData = new List<SdpEdit>();
		}

		#endregion

		/// <summary>
		/// Performs a case insensitive search.
		/// </summary>
		/// <param name="p_lstValues">The list though which to search.</param>
		/// <param name="p_strSearchString">The value for which to search.</param>
		/// <returns><lang cref="true"/> if the value is found in the list;
		/// <lang cref="false"/> otherwise.</returns>
		private bool ListContains(List<string> p_lstValues, string p_strSearchString)
		{
			string strLoweredSearchString = p_strSearchString.ToLowerInvariant();
			for (Int32 i = p_lstValues.Count - 1; i >= 0; i--)
				if (p_lstValues[i].ToLowerInvariant().Equals(strLoweredSearchString))
					return true;
			return false;
		}

		#region File Management

		/// <summary>
		/// Determins if this merge module contains the specified file. 
		/// </summary>
		/// <param name="p_strDataPath">The file for whose presence in this merge module will be determined.</param>
		/// <returns><lang cref="true"/> if the specified file is in this merge module;
		/// <lang cref="false"/> otherwise.</returns>
		internal bool ContainsFile(string p_strDataPath)
		{
			string strNormalizedPath = NormalizePath(p_strDataPath);
			return ListContains(m_lstDataFiles, strNormalizedPath);
		}

		/// <summary>
		/// Adds the given file to the mod install log.
		/// </summary>
		/// <remarks>
		/// Adding a file to a mod's install log indicates that said file was installed
		/// as part of the mod.
		/// </remarks>
		/// <param name="p_strDataPath">The file that was installed for the mod.</param>
		internal void AddFile(string p_strDataPath)
		{
			string strNormalizedPath = NormalizePath(p_strDataPath);
			if (!ListContains(m_lstDataFiles, strNormalizedPath))
				m_lstDataFiles.Add(strNormalizedPath);
		}

		/// <summary>
		/// Adds the given original data file to the mod install log.
		/// </summary>
		/// <remarks>
		/// This backs up an original data file we are overwriting.
		/// </remarks>
		/// <param name="p_strDataPath">The file that was overwritten.</param>
		internal void BackupOriginalDataFile(string p_strDataPath)
		{
			if (!ListContains(m_lstReplacedDataFiles, p_strDataPath))
				m_lstReplacedDataFiles.Add(p_strDataPath);
		}

		#endregion

		#region Ini Management

		/// <summary>
		/// Adds the given Ini edit to the mod install log.
		/// </summary>
		/// <remarks>
		/// Adding an Ini edit to a mod's install log indicates that said edit was made
		/// as part of the mod.
		/// </remarks>
		/// <param name="p_strFile">The Ini file that was edited.</param>
		/// <param name="p_strSection">The section in the Ini file that was edited.</param>
		/// <param name="p_strKey">The key in the Ini file that was edited.</param>
		/// <param name="p_strValue">The value to which the key was set.</param>
		internal void AddIniEdit(string p_strFile, string p_strSection, string p_strKey, string p_strValue)
		{
			string strLoweredFile = p_strFile.ToLowerInvariant();
			string strLoweredSection = p_strSection.ToLowerInvariant();
			string strLoweredKey = p_strKey.ToLowerInvariant();
			IniEdit iniEdit = new IniEdit(strLoweredFile, strLoweredSection, strLoweredKey);
			Int32 intIndex = m_lstIniEdits.IndexOf(iniEdit);
			if (intIndex == -1)
				m_lstIniEdits.Add(iniEdit);
			else
				iniEdit = m_lstIniEdits[intIndex];
			iniEdit.Value = p_strValue;
		}

		/// <summary>
		/// Adds the given original Ini value to the mod install log.
		/// </summary>
		/// <remarks>
		/// This backs up an original Ini value we are overwriting.
		/// </remarks>
		/// <param name="p_strFile">The Ini file that was edited.</param>
		/// <param name="p_strSection">The section in the Ini file that was edited.</param>
		/// <param name="p_strKey">The key in the Ini file that was edited.</param>
		/// <param name="p_strValue">The original value of the edited key.</param>
		internal void BackupOriginalIniValue(string p_strFile, string p_strSection, string p_strKey, string p_strValue)
		{
			string strLoweredFile = p_strFile.ToLowerInvariant();
			string strLoweredSection = p_strSection.ToLowerInvariant();
			string strLoweredKey = p_strKey.ToLowerInvariant();
			IniEdit iniEdit = new IniEdit(strLoweredFile, strLoweredSection, strLoweredKey);
			Int32 intIndex = m_lstReplacedIniValues.IndexOf(iniEdit);
			if (intIndex == -1)
				m_lstReplacedIniValues.Add(iniEdit);
			else
				iniEdit = m_lstReplacedIniValues[intIndex];
			iniEdit.Value = p_strValue;
		}

		#endregion

		#region Shader Management

		/// <summary>
		/// Adds the given Shader edit to the mod install log.
		/// </summary>
		/// <remarks>
		/// Adding an Shader edit to a mod's install log indicates that said edit was made
		/// as part of the mod.
		/// </remarks>
		/// <param name="p_intPackage">The package containing the shader that was edited.</param>
		/// <param name="p_strShader">The shader that was edited.</param>
		/// <param name="p_bteData">The value to which the shader was set.</param>
		internal void AddSdpEdit(int p_intPackage, string p_strShader, byte[] p_bteData)
		{
			string strLoweredShader = p_strShader.ToLowerInvariant();
			SdpEdit spdEdit = new SdpEdit(p_intPackage, p_strShader);
			Int32 intIndex = m_lstSdpEdits.IndexOf(spdEdit);
			if (intIndex == -1)
				m_lstSdpEdits.Add(spdEdit);
			else
				spdEdit = m_lstSdpEdits[intIndex];
			spdEdit.Data = p_bteData;
		}

		/// <summary>
		/// Adds the given original shader to the mod install log.
		/// </summary>
		/// <remarks>
		/// This backs up an original shader we are overwriting.
		/// </remarks>
		/// <param name="p_intPackage">The package containing the shader that was edited.</param>
		/// <param name="p_strShader">The shader that was edited.</param>
		/// <param name="p_bteData">The original data of the edited shader.</param>
		internal void BackupOriginalSpd(int p_intPackage, string p_strShader, byte[] p_bteData)
		{
			string strLoweredShader = p_strShader.ToLowerInvariant();
			SdpEdit spdEdit = new SdpEdit(p_intPackage, p_strShader);
			Int32 intIndex = m_lstReplacedSdpData.IndexOf(spdEdit);
			if (intIndex == -1)
				m_lstReplacedSdpData.Add(spdEdit);
			else
				spdEdit = m_lstReplacedSdpData[intIndex];
			spdEdit.Data = p_bteData;
		}

		#endregion
	}
}
