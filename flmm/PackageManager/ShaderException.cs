﻿using System;

namespace Fomm.PackageManager
{
	/// <summary>
	/// The exception that is thrown if there is a problem working with a shader.
	/// </summary>
	public class ShaderException : Exception
	{
		/// <summary>
		/// The default constructor.
		/// </summary>
		public ShaderException()
		{
		}

		/// <summary>
		/// A simple contructor that sets the exception's message.
		/// </summary>
		/// <param name="message">The exception's message.</param>
		public ShaderException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// A simple constructor the sets the exception's message and inner exception.
		/// </summary>
		/// <param name="message">The exception's message.</param>
		/// <param name="inner">The ineer exception.</param>
		public ShaderException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
