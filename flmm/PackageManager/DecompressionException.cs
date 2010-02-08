using System;

namespace Fomm.PackageManager
{
	/// <summary>
	/// The exception that is thrown if a compressed file cannot be decompressed.
	/// </summary>
	public class DecompressionException : Exception
	{
		/// <summary>
		/// The default constructor.
		/// </summary>
		public DecompressionException()
		{
		}

		/// <summary>
		/// A simple contructor that sets the exception's message.
		/// </summary>
		/// <param name="message">The exception's message.</param>
		public DecompressionException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// A simple constructor the sets the exception's message and inner exception.
		/// </summary>
		/// <param name="message">The exception's message.</param>
		/// <param name="inner">The ineer exception.</param>
		public DecompressionException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
