using System;
using System.Collections.Generic;
using System.Text;

namespace fomm.Transactions
{
  /// <summary>
  /// Manages the ambient transaction.
  /// </summary>
  public class TransactionScope : IDisposable
  {
    private Transaction m_trnTransaction = null;
    private bool m_booCompleted = false;

    /// <summary>
    /// The default constructor.
    /// </summary>
    /// <remarks>
    /// This sets up the ambient transaction.
    /// </remarks>
    public TransactionScope()
    {
      m_trnTransaction = new Transaction();
      Transaction.Current = m_trnTransaction;
    }

    /// <summary>
    /// Completes the transaction.
    /// </summary>
    /// <remarks>
    /// This method gets votes from all the participants on whether or not the transaction should be committed.
    /// </remarks>
    public void Complete()
    {
      if (m_booCompleted)
      {
        throw new TransactionException("Complete has already been called.");
      }

      bool booVotedToCommit = false;
      booVotedToCommit = m_trnTransaction.Prepare();
      if (booVotedToCommit && (m_trnTransaction.TransactionInformation.Status == TransactionStatus.Active))
      {
        m_trnTransaction.Commit();
      }
      m_booCompleted = true;
    }

    #region IDisposable Members

    /// <summary>
    /// Disposes of the transaction scope, and removes the ambient transaction.
    /// </summary>
    /// <remarks>
    /// This makes sure the transaction is rolled back if the scope hasn't completed.
    /// </remarks>
    public void Dispose()
    {
      if (!m_booCompleted)
      {
        m_trnTransaction.Rollback();
      }
      Transaction.Current = null;
    }

    #endregion
  }
}