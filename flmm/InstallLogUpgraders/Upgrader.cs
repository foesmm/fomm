using System.Windows.Forms;
using ChinhDo.Transactions;
using Fomm.PackageManager.ModInstallLog;
using fomm.Transactions;

namespace Fomm.InstallLogUpgraders
{
  /// <summary>
  ///   Upgrades the Install Log from a specific version to the latest version.
  /// </summary>
  /// <remarks>
  ///   This base class handles setting up the common resources and transaction required for all
  ///   log upgrades.
  /// </remarks>
  internal abstract class Upgrader
  {
    #region Properties

    /// <summary>
    ///   Gets the <see cref="BackgroundWorkerProgressDialog" /> that performs
    ///   the upgrade and shows progress.
    /// </summary>
    /// <value>
    ///   The <see cref="BackgroundWorkerProgressDialog" /> that performs
    ///   the upgrade and shows progress.
    /// </value>
    protected BackgroundWorkerProgressDialog ProgressWorker { get; private set; }

    /// <summary>
    ///   Gets the transactional file manager to be used in the upgrade.
    /// </summary>
    protected TxFileManager FileManager { get; private set; }

    #endregion

    #region Constructor

    #endregion

    /// <summary>
    ///   Called to perform the upgrade.
    /// </summary>
    /// <remarks>
    ///   Sets up the resources required to upgrade the install log, and then
    ///   call <see cref="DoUpgrade()" /> so implementers can do the upgrade.
    /// </remarks>
    /// <returns>
    ///   <lang langref="true" /> if the upgrade completed; <lang langref="false" />
    ///   if the user cancelled.
    /// </returns>
    internal bool PerformUpgrade()
    {
      FileManager = new TxFileManager();
      var booComplete = false;
      using (var tsTransaction = new TransactionScope())
      {
        FileManager.Snapshot(InstallLog.Current.InstallLogPath);

        using (ProgressWorker = new BackgroundWorkerProgressDialog(DoUpgrade))
        {
          ProgressWorker.OverallMessage = "Upgrading FOMM Files";
          if (ProgressWorker.ShowDialog() == DialogResult.OK)
          {
            booComplete = true;
            tsTransaction.Complete();
          }
        }
        FileManager = null;
      }
      return booComplete;
    }

    /// <summary>
    ///   This is overridden by implementers to perform the actual upgrade.
    /// </summary>
    protected abstract void DoUpgrade();
  }
}