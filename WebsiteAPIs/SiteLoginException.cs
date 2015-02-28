using System;

namespace WebsiteAPIs
{
	/// <summary>
	/// The exception that is thrown if an API can't log into a site.
	/// </summary>
	public class SiteLoginException : Exception
	{
		/// <summary>
		/// The default constructor.
		/// </summary>
		public SiteLoginException()
		{
		}

		/// <summary>
		/// A simple contructor that sets the exception's message.
		/// </summary>
		/// <param name="message">The exception's message.</param>
		public SiteLoginException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// A simple constructor the sets the exception's message and inner exception.
		/// </summary>
		/// <param name="message">The exception's message.</param>
		/// <param name="inner">The ineer exception.</param>
		public SiteLoginException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
