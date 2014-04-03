using System;
using System.Collections.Generic;

namespace fomm.Transactions
{
  /// <summary>
  /// The possible options for enlistment.
  /// </summary>
  public enum EnlistmentOptions
  {
    None
  }

  /// <summary>
  /// A transaction.
  /// </summary>
  /// <remarks>
  /// This transaction class has no timeout.
  /// </remarks>
  public class Transaction : IDisposable
  {
    /// <summary>
    /// Gets or sets the ambient transaction.
    /// </summary>
    /// <value>The ambient transaction.</value>
    public static Transaction Current { get; set; }

    private List<IEnlistmentNotification> m_lstNotifications = new List<IEnlistmentNotification>();
    private TransactionInformation m_tinInfo = new TransactionInformation();

    /// <summary>
    /// Gets the information about this transaction.
    /// </summary>
    /// <value>The information about this transaction.</value>
    public TransactionInformation TransactionInformation
    {
      get
      {
        return m_tinInfo;
      }
    }

    /// <summary>
    /// Enlists a resource manager in this transaction.
    /// </summary>
    /// <param name="p_entNotification">The resource manager to enlist.</param>
    /// <param name="p_eopOptions">The enlistment options. This value must be <see cref="EnlistmentOptions.None"/>.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="p_eopOptions"/> is not
    /// <see cref="EnlistmentOptions.None"/>.</exception>
    public void EnlistVolatile(IEnlistmentNotification p_entResourceManager, EnlistmentOptions p_eopOptions)
    {
      if (p_eopOptions != EnlistmentOptions.None)
      {
        throw new ArgumentException("EnlistmentOptions must be None.", "p_eopOptions");
      }

      m_lstNotifications.Add(p_entResourceManager);
    }

    /// <summary>
    /// Prepares the enlited resource managers for committal.
    /// </summary>
    /// <returns><lang langref="true"/> if all polled participants voted to commit;
    /// <lang langref="false"/> otherwise.</returns>
    internal bool Prepare()
    {
      if (TransactionInformation.Status != TransactionStatus.Active)
      {
        throw new TransactionException("Cannot prepare transaction, as it is not active. Trasnaction Status: " +
                                       TransactionInformation.Status);
      }

      var booVoteToCommit = true;

      for (var i = m_lstNotifications.Count - 1; i >= 0; i--)
      {
        var entNotification = m_lstNotifications[i];
        var lpeEnlistment = new PreparingEnlistment();
        entNotification.Prepare(lpeEnlistment);
        if (lpeEnlistment.VoteToCommit.HasValue)
        {
          booVoteToCommit &= lpeEnlistment.VoteToCommit.Value;
          if (lpeEnlistment.DoneProcessing)
          {
            m_lstNotifications.RemoveAt(i);
          }
        }
        else
        {
          booVoteToCommit = false;
          TransactionInformation.Status = TransactionStatus.InDoubt;
        }
      }
      if (TransactionInformation.Status == TransactionStatus.InDoubt)
      {
        NotifyInDoubt();
      }
      return booVoteToCommit;
    }

    /// <summary>
    /// Tells al participanting resource managers to commit their changes.
    /// </summary>
    internal void Commit()
    {
      if (TransactionInformation.Status != TransactionStatus.Active)
      {
        throw new TransactionException("Cannot commit transaction, as it is not active. Trasnaction Status: " +
                                       TransactionInformation.Status);
      }

      for (var i = m_lstNotifications.Count - 1; i >= 0; i--)
      {
        var entNotification = m_lstNotifications[i];
        var lpeEnlistment = new PreparingEnlistment();
        entNotification.Commit(lpeEnlistment);
        if (lpeEnlistment.DoneProcessing)
        {
          m_lstNotifications.RemoveAt(i);
        }
      }
      if (m_lstNotifications.Count > 0)
      {
        TransactionInformation.Status = TransactionStatus.InDoubt;
        NotifyInDoubt();
      }
      else
      {
        TransactionInformation.Status = TransactionStatus.Committed;
      }
    }

    /// <summary>
    /// Tells the participating resource managers that the transaction status is in doubt.
    /// </summary>
    protected void NotifyInDoubt()
    {
      if (TransactionInformation.Status != TransactionStatus.InDoubt)
      {
        return;
      }

      for (var i = m_lstNotifications.Count - 1; i >= 0; i--)
      {
        var entNotification = m_lstNotifications[i];
        var eltEnlistment = new Enlistment();
        entNotification.InDoubt(eltEnlistment);
        if (eltEnlistment.DoneProcessing)
        {
          m_lstNotifications.RemoveAt(i);
        }
      }
    }

    /// <summary>
    /// Tells the participating resource managers to rollback their changes.
    /// </summary>
    public void Rollback()
    {
      if (TransactionInformation.Status == TransactionStatus.Aborted)
      {
        return;
      }

      var lstExceptions =
        new List<RollbackException.ExceptedResourceManager>();
      for (var i = m_lstNotifications.Count - 1; i >= 0; i--)
      {
        var entNotification = m_lstNotifications[i];
        var eltEnlistment = new Enlistment();
        try
        {
          entNotification.Rollback(eltEnlistment);
        }
        catch (Exception e)
        {
          lstExceptions.Add(new RollbackException.ExceptedResourceManager(entNotification, e));
        }
        if (eltEnlistment.DoneProcessing)
        {
          m_lstNotifications.RemoveAt(i);
        }
      }
      if (m_lstNotifications.Count > 0)
      {
        TransactionInformation.Status = TransactionStatus.InDoubt;
        NotifyInDoubt();
      }
      else
      {
        TransactionInformation.Status = TransactionStatus.Aborted;
      }

      if (lstExceptions.Count > 0)
      {
        throw new RollbackException(lstExceptions);
      }
    }

    #region IDisposable Members

    /// <summary>
    /// Disposes of the transaction.
    /// </summary>
    public void Dispose()
    {
      if (TransactionInformation.Status == TransactionStatus.Active)
      {
        Rollback();
      }
    }

    #endregion
  }
}