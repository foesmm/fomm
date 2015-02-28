using System;

namespace WebsiteAPIs
{
	/// <summary>
	/// The exception that is thrown if an API doesn't receive the expected response to a request.
	/// </summary>
	public class HttpException : Exception
	{
		/// <summary>
		/// The default constructor.
		/// </summary>
		public HttpException()
		{
		}

		/// <summary>
		/// A simple contructor that sets the exception's message.
		/// </summary>
		/// <param name="message">The exception's message.</param>
		public HttpException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// A simple constructor the sets the exception's message and inner exception.
		/// </summary>
		/// <param name="message">The exception's message.</param>
		/// <param name="inner">The ineer exception.</param>
		public HttpException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
