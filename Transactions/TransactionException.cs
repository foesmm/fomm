﻿using System;
using System.Collections.Generic;
using System.Text;

namespace fomm.Transactions
{
	/// <summary>
	/// The exception that is thrown if an error occurs during the processing of a transaction.
	/// </summary>
	public class TransactionException : Exception
	{
		/// <summary>
		/// The default constructor.
		/// </summary>
		public TransactionException()
		{
		}

		/// <summary>
		/// A simple contructor that sets the exception's message.
		/// </summary>
		/// <param name="message">The exception's message.</param>
		public TransactionException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// A simple constructor the sets the exception's message and inner exception.
		/// </summary>
		/// <param name="message">The exception's message.</param>
		/// <param name="inner">The ineer exception.</param>
		public TransactionException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
