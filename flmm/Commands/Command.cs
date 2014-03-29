using System;
using System.ComponentModel;

namespace Fomm.Commands
{
  /// <summary>
  /// Describes the arguments passed to an event that is executing a command.
  /// </summary>
  /// <typeparam name="T">The type of the command argument.</typeparam>
  public class ExecutedEventArgs<T> : EventArgs
  {
    /// <summary>
    /// Gets or sets the command argument.
    /// </summary>
    /// <value>The command argument.</value>
    public T Argument { get; protected set; }

    /// <summary>
    /// A simple contructor that initializes the object with the given values.
    /// </summary>
    /// <param name="p_tArgument">The command argument.</param>
    public ExecutedEventArgs(T p_tArgument)
    {
      Argument = p_tArgument;
    }
  }

  /// <summary>
  /// The base class for commands.
  /// </summary>
  /// <typeparam name="T">The type of the command argument.</typeparam>
  public class Command<T> : INotifyPropertyChanged
  {
    /// <summary>
    /// Raised when a property changes value.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged = delegate { };

    /// <summary>
    /// Raised when the command has been executed.
    /// </summary>
    public event EventHandler<ExecutedEventArgs<T>> Executed = delegate { };

    private bool m_booCanExecute = true;

    #region Properties

    /// <summary>
    /// Gets the name of the command.
    /// </summary>
    /// <value>The name of the command.</value>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the description of the command.
    /// </summary>
    /// <value>The description of the command.</value>
    public string Description { get; private set; }

    /// <summary>
    /// Gets or sets whether the command can be executed.
    /// </summary>
    /// <value>Whether the command can be executed.</value>
    public bool CanExecute
    {
      get
      {
        return m_booCanExecute;
      }
      set
      {
        if (m_booCanExecute != value)
        {
          m_booCanExecute = value;
          OnPropertyChanged(new PropertyChangedEventArgs("CanExecute"));
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
    public Command(string p_strName, string p_strDescription, EventHandler<ExecutedEventArgs<T>> p_eehExecute)
    {
      Name = p_strName;
      Description = p_strDescription;
      Executed += p_eehExecute;
    }

    #endregion

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="p_tArgument">The command argument.</param>
    public void Execute(T p_tArgument)
    {
      Executed(this, new ExecutedEventArgs<T>(p_tArgument));
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="e">A <see cref="PropertyChangedEventArgs"/> describing the event properties.</param>
    protected void OnPropertyChanged(PropertyChangedEventArgs e)
    {
      PropertyChanged(this, e);
    }
  }
}
