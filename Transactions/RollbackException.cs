using System;
using System.Collections.Generic;
using System.Text;

namespace fomm.Transactions
{
  public class RollbackException : Exception
  {
    public class ExceptedResourceManager
    {
      public IEnlistmentNotification ResourceManager { get; protected set; }
      public Exception Exception { get; protected set; }

      public ExceptedResourceManager(IEnlistmentNotification p_entResourceManager, Exception p_expException)
      {
        ResourceManager = p_entResourceManager;
        Exception = p_expException;
      }
    }

    public IList<ExceptedResourceManager> ExceptedResourceManagers { get; protected set; }

    public RollbackException(IList<ExceptedResourceManager> p_lstExceptedResourceManagers)
    {
      ExceptedResourceManagers = p_lstExceptedResourceManagers;
    }
  }
}