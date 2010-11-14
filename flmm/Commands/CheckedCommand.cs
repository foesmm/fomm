using System;
using System.ComponentModel;

namespace Fomm.Commands
{
	/// <summary>
	/// A command that can be in an on or off state.
	/// </summary>
	/// <typeparam name="T">The type of the command argument.</typeparam>
	public class CheckedCommand<T> : Command<T>
	{
		private bool m_booIsChecked;

		#region Properties

		/// <summary>
		/// Gets or sets whether the command is checked.
		/// </summary>
		/// <value>Whether the command is checked.</value>
		public bool IsChecked
		{
			get
			{
				return this.m_booIsChecked;
			}
			set
			{
				if (this.m_booIsChecked != value)
				{
					this.m_booIsChecked = value;
					OnPropertyChanged(new PropertyChangedEventArgs("IsChecked"));
				}
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with the given values.
		/// </summary>
		/// <param name="p_strName">The name of the command.</param>
		/// <param name="p_strDescription">The description of the command.</param>
		/// <param name="p_eehExecute">An <see cref="EventHandler<ExecutedEventArgs<T>>"/> that will be
		/// perform the command work.</param>
		public CheckedCommand(string p_strName, string p_strDescription, EventHandler<ExecutedEventArgs<T>> p_eehExecute)
			: base(p_strName, p_strDescription, p_eehExecute)
		{
		}

		/// <summary>
		/// A simple constructor that initializes the object with the given values.
		/// </summary>
		/// <param name="p_strName">The name of the command.</param>
		/// <param name="p_strDescription">The description of the command.</param>
		/// <param name="p_booIsChecked">Whether the command is checked.</param>
		/// <param name="p_eehExecute">An <see cref="EventHandler<ExecutedEventArgs<T>>"/> that will be
		/// perform the command work.</param>
		public CheckedCommand(string p_strName, string p_strDescription, bool p_booIsChecked, EventHandler<ExecutedEventArgs<T>> p_eehExecute)
			: base(p_strName, p_strDescription, p_eehExecute)
		{
			IsChecked = p_booIsChecked;
		}

		#endregion
	}
}
