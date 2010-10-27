using System;
using System.Collections.Generic;
using System.Text;

namespace Fomm.Games
{
	/// <summary>
	/// Describes a tool for a specific game.
	/// </summary>
	/// <remarks>
	/// This class includes a reference to a method to launch the tool.
	/// </remarks>
	public class GameTool
	{
		/// <summary>
		/// The delegate for the method that launches the tool.
		/// </summary>
		/// <param name="p_frmMainForm">The main mod management form.</param>
		public delegate void LaunchToolMethod(MainForm p_frmMainForm);

		private string m_strName = null;
		private string m_strDescription = null;
		private LaunchToolMethod m_dlgCommand = null;
		
		#region Properties

		/// <summary>
		/// Gets the name of the Tool.
		/// </summary>
		/// <value>The name of the Tool.</value>
		public string Name
		{
			get
			{
				return m_strName;
			}
		}

		/// <summary>
		/// Gets the description of the Tool.
		/// </summary>
		/// <value>The description of the Tool.</value>
		public string Description
		{
			get
			{
				return m_strDescription;
			}
		}

		/// <summary>
		/// Gets the method that launches the Tool.
		/// </summary>
		/// <value>The method that launches the Tool.</value>
		public LaunchToolMethod Command
		{
			get
			{
				return m_dlgCommand;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with the given values.
		/// </summary>
		/// <param name="p_strName">The name of the Tool.</param>
		/// <param name="p_strDescription">The description of the Tool.</param>
		protected GameTool(string p_strName, string p_strDescription)
		{
			m_strName = p_strName;
			m_strDescription = p_strDescription;
		}

		/// <summary>
		/// A simple constructor that initializes the object with the given values.
		/// </summary>
		/// <param name="p_strName">The name of the Tool.</param>
		/// <param name="p_strDescription">The description of the Tool.</param>
		/// <param name="p_dlgCommand">The method that launches the Tool.</param>
		public GameTool(string p_strName, string p_strDescription, LaunchToolMethod p_dlgCommand)
			: this(p_strName, p_strDescription)
		{
			m_dlgCommand = p_dlgCommand;
		}

		#endregion

	}
}
