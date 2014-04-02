using System;
using System.ComponentModel;

namespace Fomm.Commands
{
  /// <summary>
  /// The base class for binding a command to a command trigger.
  /// </summary>
  /// <typeparam name="T">The type of the command argument.</typeparam>
  public abstract class CommandBinding<T>
  {
    /// <summary>
    /// The delegate for the method that returns the command argument.
    /// </summary>
    /// <returns>The command argument.</returns>
    public delegate T GetCommandArgument();

    private GetCommandArgument m_dlgGetArgument;
    private readonly object m_objTrigger;
    private readonly Command<T> m_cmdCommand;

    #region Properties

    /// <summary>
    /// Gets the object that can trigger the command.
    /// </summary>
    /// <value>The object that can trigger the command.</value>
    public object Trigger
    {
      get
      {
        return m_objTrigger;
      }
    }

    /// <summary>
    /// Gets the command that can be triggered.
    /// </summary>
    /// <value>The command that can be triggered.</value>
    public Command<T> Command
    {
      get
      {
        return m_cmdCommand;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// A simple constructor that initializes the object with the given values.
    /// </summary>
    /// <param name="p_objTrigger">The object that can trigger the command.</param>
    /// <param name="p_cmdCommand">The command that can be triggered.</param>
    /// <param name="p_dlgGetArgument">The method that returns the command argument.</param>
    protected CommandBinding(object p_objTrigger, Command<T> p_cmdCommand, GetCommandArgument p_dlgGetArgument)
    {
      m_dlgGetArgument = p_dlgGetArgument;
      if (p_objTrigger == null)
      {
        throw new ArgumentNullException("p_objTrigger");
      }
      if (p_cmdCommand == null)
      {
        throw new ArgumentNullException("p_cmdCommand");
      }
      m_objTrigger = p_objTrigger;
      m_cmdCommand = p_cmdCommand;
      m_cmdCommand.PropertyChanged += new PropertyChangedEventHandler(CommandPropertyChanged);
    }

    #endregion

    /// <summary>
    /// Executes the command.
    /// </summary>
    public void Execute()
    {
      Command.Execute((m_dlgGetArgument == null) ? default(T) : m_dlgGetArgument());
    }

    /// <summary>
    /// Alters properties on the Trigger in response to property changes on the command.
    /// </summary>
    /// <param name="e">A <see cref="PropertyChangedEventArgs"/> describing the changed property.</param>
    protected virtual void OnCommandPropertyChanged(PropertyChangedEventArgs e)
    {
    }

    /// <summary>
    /// Handles the <see cref="Comand{T}.PropertyChanged"/> event of the command.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">A <see cref="PropertyChangedEventArgs"/> describing the event arguments.</param>
    private void CommandPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      OnCommandPropertyChanged(e);
    }
  }
}