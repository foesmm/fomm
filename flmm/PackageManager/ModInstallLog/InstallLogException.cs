using System;
using System.Collections.Generic;
using System.Text;

namespace Fomm.PackageManager.ModInstallLog
{
	/// <summary>
	/// The exception that is thrown if a there is a problem with the install log.
	/// </summary>
	class InstallLogException: Exception
	{
		/// <summary>
		/// The default constructor.
		/// </summary>
		public InstallLogException()
		{
		}

		/// <summary>
		/// A simple contructor that sets the exception's message.
		/// </summary>
		/// <param name="message">The exception's message.</param>
		public InstallLogException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// A simple constructor the sets the exception's message and inner exception.
		/// </summary>
		/// <param name="message">The exception's message.</param>
		/// <param name="inner">The ineer exception.</param>
		public InstallLogException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
