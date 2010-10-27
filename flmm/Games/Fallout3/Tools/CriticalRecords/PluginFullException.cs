using System;
using System.Collections.Generic;
using System.Text;

namespace Fomm.Games.Fallout3.Tools.CriticalRecords
{
	/// <summary>
	/// The exception that is thrown if pluing has no available form ids.
	/// </summary>
	public class PluginFullException : Exception
	{
		/// <summary>
		/// The default constructor.
		/// </summary>
		public PluginFullException()
		{
		}

		/// <summary>
		/// A simple contructor that sets the exception's message.
		/// </summary>
		/// <param name="message">The exception's message.</param>
		public PluginFullException(string message)
			: base(message)
		{
		}
	}
}
