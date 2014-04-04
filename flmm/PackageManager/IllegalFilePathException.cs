using System;

namespace Fomm.PackageManager
{
  /// <summary>
  ///   The exception that is thrown when the path being accessed is illegal.
  /// </summary>
  public class IllegalFilePathException : Exception
  {
    /// <summary>
    ///   Gets the illegal path.
    /// </summary>
    /// <value>The illegal path.</value>
    public string Path { get; private set; }

    /// <summary>
    ///   The default constructor.
    /// </summary>
    /// <param name="p_strPath">The illegal path.</param>
    public IllegalFilePathException(string p_strPath)
    {
      Path = p_strPath;
    }

    /// <summary>
    ///   A simple contructor that sets the exception's message.
    /// </summary>
    /// <param name="p_strPath">The illegal path.</param>
    /// <param name="message">The exception's message.</param>
    public IllegalFilePathException(string p_strPath, string message)
      : base(message)
    {
      Path = p_strPath;
    }

    /// <summary>
    ///   A simple constructor the sets the exception's message and inner exception.
    /// </summary>
    /// <param name="p_strPath">The illegal path.</param>
    /// <param name="message">The exception's message.</param>
    /// <param name="inner">The ineer exception.</param>
    public IllegalFilePathException(string p_strPath, string message, Exception inner)
      : base(message, inner)
    {
      Path = p_strPath;
    }
  }
}