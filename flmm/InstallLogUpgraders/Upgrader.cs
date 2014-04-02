using ChinhDo.Transactions;
using fomm.Transactions;
using System.Windows.Forms;
using Fomm.PackageManager.ModInstallLog;

namespace Fomm.InstallLogUpgraders
{
  /// <summary>
  /// Upgrades the Install Log from a specific version to the latest version.
  /// </summary>
  /// <remarks>
  /// This base class handles setting up the common resources and transaction required for all
  /// log upgrades.
  /// </remarks>
  internal abstract class Upgrader
  {
    private static object m_objLock = new object();
    private BackgroundWorkerProgressDialog m_pgdProgress;
    private TxFileManager m_tfmFileManager;

    #region Properties

    /// <summary>
    /// Gets the <see cref="BackgroundWorkerProgressDialog"/> that performs
    /// the upgrade and shows progress.
    /// </summary>
    /// <value>The <see cref="BackgroundWorkerProgressDialog"/> that performs
    /// the upgrade and shows progress.</value>
    protected BackgroundWorkerProgressDialog ProgressWorker
    {
      get
      {
        return m_pgdProgress;
      }
    }

    /// <summary>
    /// Gets the transactional file manager to be used in the upgrade.
    /// </summary>
    protected TxFileManager FileManager
    {
      get
      {
        return m_tfmFileManager;
      }
    }

    #endregion

    #region Constructor

    #endregion

    /// <summary>
    /// Called to perform the upgrade.
    /// </summary>
    /// <remarks>
    /// Sets up the resources required to upgrade the install log, and then
    /// call <see cref="DoUpgrade()"/> so implementers can do the upgrade.
    /// </remarks>
    /// <returns><lang cref="true"/> if the upgrade completed; <lang cref="false"/>
    /// if the user cancelled.</returns>
    internal bool PerformUpgrade()
    {
      m_tfmFileManager = new TxFileManager();
      bool booComplete = false;
      using (TransactionScope tsTransaction = new TransactionScope())
      {
        m_tfmFileManager.Snapshot(InstallLog.Current.InstallLogPath);

        using (m_pgdProgress = new BackgroundWorkerProgressDialog(DoUpgrade))
        {
          m_pgdProgress.OverallMessage = "Upgrading FOMM Files";
          if (m_pgdProgress.ShowDialog() == DialogResult.OK)
          {
            booComplete = true;
            tsTransaction.Complete();
          }
        }
        m_tfmFileManager = null;
      }
      return booComplete;
    }

    /// <summary>
    /// This is overridden by implementers to perform the actual upgrade.
    /// </summary>
    protected abstract void DoUpgrade();
  }
}