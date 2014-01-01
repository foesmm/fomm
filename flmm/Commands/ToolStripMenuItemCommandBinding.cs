using System;
using System.Windows.Forms;
using System.ComponentModel;

namespace Fomm.Commands
{
	/// <summary>
	/// A class that binds a command to a <see cref="ToolStripMenuItem"/>.
	/// </summary>
	/// <typeparam name="T">The type of the command argument.</typeparam>
	public class ToolStripMenuItemCommandBinding<T> : CommandBinding<T>
	{
		#region Properties

		/// <summary>
		/// Gets the <see cref="ToolStripMenuItem"/> that is bound to the command.
		/// </summary>
		/// <value>The <see cref="ToolStripMenuItem"/> that is bound to the command.</value>
		public ToolStripMenuItem ToolStripMenuItem
		{
			get
			{
				return (ToolStripMenuItem)Trigger;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that intializes the object with the given values.
		/// </summary>
		/// <param name="p_tsiMenuItem">The menu item to bind to the command.</param>
		/// <param name="p_cmdCommand">The command to bind to the trigger.</param>
		/// <param name="p_dlgGetArgument">The method that returns the command argument.</param>
		public ToolStripMenuItemCommandBinding(ToolStripMenuItem p_tsiMenuItem, Command<T> p_cmdCommand, GetCommandArgument p_dlgGetArgument)
			: base(p_tsiMenuItem, p_cmdCommand, p_dlgGetArgument)
		{
			p_tsiMenuItem.Text = p_cmdCommand.Name;
			p_tsiMenuItem.Enabled = Command.CanExecute;
			if (Command is CheckedCommand<T>)
				p_tsiMenuItem.Checked = ((CheckedCommand<T>)Command).IsChecked;
			p_tsiMenuItem.Click += new EventHandler(ToolStripMenuItem_Click);
		}

		#endregion

		/// <summary>
		/// Alters properties on the Trigger in response to property changes on the command.
		/// </summary>
		/// <param name="e">A <see cref="PropertyChangedEventArgs"/> describing the changed property.</param>
		protected override void OnCommandPropertyChanged(PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "CanExecute":
					ToolStripMenuItem.Enabled = Command.CanExecute;
					break;
				case "IsChecked":
					if (Command is CheckedCommand<T>)
					{
						ToolStripMenuItem.Checked = ((CheckedCommand<T>)Command).IsChecked;
					}
					break;
			}
		}

		/// <summary>
		/// Handles the <see cref="Control.Click"/> event of the menu item.
		/// </summary>
		/// <remarks>
		/// This executes the event.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Execute();
		}
	}
}
