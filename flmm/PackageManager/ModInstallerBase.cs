using System;
using System.ComponentModel;
using ChinhDo.Transactions;
using Fomm.PackageManager.ModInstallLog;
using fomm.Transactions;
using System.Windows.Forms;
using System.Text;
using System.IO;

namespace Fomm.PackageManager
{
  public abstract class ModInstallerBase : IDisposable
  {
    protected static readonly object objInstallLock = new object();
    private BackgroundWorkerProgressDialog m_bwdProgress;
    private TxFileManager m_tfmFileManager;
    private InstallLogMergeModule m_ilmModInstallLog;
    private fomod m_fomodMod;
    private ModInstallScript m_misScript;

    #region Properties

    public ModInstallScript Script
    {
      get
      {
        return m_misScript;
      }
    }

    /// <summary>
    /// Gets the transactional file manager the script is using.
    /// </summary>
    /// <value>The transactional file manager the script is using.</value>
    public TxFileManager TransactionalFileManager
    {
      get
      {
        if (m_tfmFileManager == null)
        {
          throw new InvalidOperationException(
            "The transactional file manager must be initialized by calling InitTransactionalFileManager() before it is used.");
        }
        return m_tfmFileManager;
      }
    }

    /// <summary>
    /// Gets or sets the merge module we are using.
    /// </summary>
    /// <value>The merge module we are using.</value>
    public InstallLogMergeModule MergeModule
    {
      get
      {
        return m_ilmModInstallLog;
      }
      set
      {
        m_ilmModInstallLog = value;
      }
    }

    /// <summary>
    /// Gets the mod that is being scripted against.
    /// </summary>
    /// <value>The mod that is being scripted against.</value>
    public fomod Fomod
    {
      get
      {
        return m_fomodMod;
      }
    }

    /// <summary>
    /// Gets the message to display to the user when an exception is caught.
    /// </summary>
    /// <remarks>
    /// In order to display the exception message, the placeholder {0} should be used.
    /// </remarks>
    /// <value>The message to display to the user when an exception is caught.</value>
    protected abstract string ExceptionMessage { get; }

    /// <summary>
    /// Gets the message to display upon failure of the script.
    /// </summary>
    /// <remarks>
    /// If the value of this property is <lang cref="null"/> then no message will be
    /// displayed.
    /// </remarks>
    /// <value>The message to display upon failure of the script.</value>
    protected abstract string FailMessage { get; }

    /// <summary>
    /// Gets the message to display upon success of the script.
    /// </summary>
    /// <remarks>
    /// If the value of this property is <lang cref="null"/> then no message will be
    /// displayed.
    /// </remarks>
    /// <value>The message to display upon success of the script.</value>
    protected abstract string SuccessMessage { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// A simple constructor that initializes the object.
    /// </summary>
    /// <param name="p_fomodMod">The <see cref="fomod"/> to be installed or uninstalled.</param>
    public ModInstallerBase(fomod p_fomodMod)
    {
      m_fomodMod = p_fomodMod;
    }

    #endregion

    #region Installer Execution

    /// <summary>
    /// Checks to see if the script work has already been done.
    /// </summary>
    /// <returns><lang cref="true"/> if the script work has already been done and the script
    /// doesn't need to execute; <lang cref="false"/> otherwise.</returns>
    protected virtual bool CheckAlreadyDone()
    {
      return true;
    }

    /// <summary>
    /// Does the script-specific work.
    /// </summary>
    /// <remarks>
    /// This is the method that needs to be overridden by implementers to do
    /// their script-specific work.
    /// </remarks>
    /// <returns><lang cref="true"/> if the script work was completed successfully and needs to
    /// be committed; <lang cref="false"/> otherwise.</returns>
    protected abstract bool DoScript();

    /// <summary>
    /// Runs the install script.
    /// </summary>
    protected bool Run()
    {
      return Run(false, true);
    }

    /// <summary>
    /// Runs the install script.
    /// </summary>
    /// <remarks>
    /// This contains the boilerplate code that needs to be done for all install-type
    /// scripts. Implementers must override the <see cref="DoScript()"/> method to
    /// implement their script-specific functionality.
    /// </remarks>
    /// <param name="p_booSuppressSuccessMessage">Indicates whether to
    /// supress the success message. This is useful for batch installs.</param>
    /// <seealso cref="DoScript()"/>
    protected bool Run(bool p_booSuppressSuccessMessage, bool p_booSetFOModReadOnly)
    {
      var booSuccess = false;
      if (CheckAlreadyDone())
      {
        booSuccess = true;
      }

      if (!booSuccess)
      {
        try
        {
          //the install process modifies INI and config files.
          // if multiple sources (i.e., installs) try to modify
          // these files simultaneously the outcome is not well known
          // (e.g., one install changes SETTING1 in a config file to valueA
          // while simultaneously another install changes SETTING1 in the
          // file to value2 - after each install commits its changes it is
          // not clear what the value of SETTING1 will be).
          // as a result, we only allow one mod to be installed at a time,
          // hence the lock.
          lock (objInstallLock)
          {
            using (var tsTransaction = new TransactionScope())
            {
              m_tfmFileManager = new TxFileManager();
              using (m_misScript = CreateInstallScript())
              {
                var booCancelled = false;
                if (p_booSetFOModReadOnly && (Fomod != null))
                {
                  if (Fomod.ReadOnlyInitStepCount > 1)
                  {
                    using (m_bwdProgress = new BackgroundWorkerProgressDialog(BeginFOModReadOnlyTransaction))
                    {
                      m_bwdProgress.OverallMessage = "Preparing FOMod...";
                      m_bwdProgress.ShowItemProgress = false;
                      m_bwdProgress.OverallProgressMaximum = Fomod.ReadOnlyInitStepCount;
                      m_bwdProgress.OverallProgressStep = 1;
                      try
                      {
                        Fomod.ReadOnlyInitStepStarted += Fomod_ReadOnlyInitStepStarted;
                        Fomod.ReadOnlyInitStepFinished += Fomod_ReadOnlyInitStepFinished;
                        if (m_bwdProgress.ShowDialog() == DialogResult.Cancel)
                        {
                          booCancelled = true;
                        }
                      }
                      finally
                      {
                        Fomod.ReadOnlyInitStepStarted -= Fomod_ReadOnlyInitStepStarted;
                        Fomod.ReadOnlyInitStepFinished -= Fomod_ReadOnlyInitStepFinished;
                      }
                    }
                  }
                  else
                  {
                    Fomod.BeginReadOnlyTransaction();
                  }
                }
                if (!booCancelled)
                {
                  booSuccess = DoScript();
                  if (booSuccess)
                  {
                    tsTransaction.Complete();
                  }
                }
              }
            }
          }
        }
        catch (Exception e)
        {
          var stbError = new StringBuilder(e.Message);
          if (e is FileNotFoundException)
          {
            stbError.Append(" (" + ((FileNotFoundException) e).FileName + ")");
          }
          if (e is IllegalFilePathException)
          {
            stbError.Append(" (" + ((IllegalFilePathException) e).Path + ")");
          }
          if (e.InnerException != null)
          {
            stbError.AppendLine().AppendLine(e.InnerException.Message);
          }
          if (e is RollbackException)
          {
            foreach (var erm in ((RollbackException) e).ExceptedResourceManagers)
            {
              stbError.AppendLine(erm.ResourceManager.ToString());
              stbError.AppendLine(erm.Exception.Message);
              if (erm.Exception.InnerException != null)
              {
                stbError.AppendLine(erm.Exception.InnerException.Message);
              }
            }
          }
          var strMessage = String.Format(ExceptionMessage, stbError);
          MessageBox.Show(strMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return false;
        }
        finally
        {
          m_tfmFileManager = null;
          m_ilmModInstallLog = null;
          if (Fomod != null)
          {
            Fomod.EndReadOnlyTransaction();
          }
        }
      }
      if (booSuccess && !p_booSuppressSuccessMessage && !String.IsNullOrEmpty(SuccessMessage))
      {
        MessageBox.Show(SuccessMessage, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
      else if (!booSuccess && !String.IsNullOrEmpty(FailMessage))
      {
        MessageBox.Show(FailMessage, "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      return booSuccess;
    }

    protected virtual ModInstallScript CreateInstallScript()
    {
      return Program.GameMode.CreateInstallScript(Fomod, this);
    }

    /// <summary>
    /// Handles the <see cref="fomod.ReadOnlyInitStepFinished"/> event of the FOMod.
    /// </summary>
    /// <remarks>
    /// This steps the progress in the progress dialog.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">A <see cref="CancelEventArgs"/> describing the event arguments.</param>
    private void Fomod_ReadOnlyInitStepFinished(object sender, CancelEventArgs e)
    {
      m_bwdProgress.StepOverallProgress();
    }

    /// <summary>
    /// Handles the <see cref="fomod.ReadOnlyInitStepStarted"/> event of the FOMod.
    /// </summary>
    /// <remarks>
    /// This cancels the operation if the user has clicked cancel.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">A <see cref="CancelEventArgs"/> describing the event arguments.</param>
    private void Fomod_ReadOnlyInitStepStarted(object sender, CancelEventArgs e)
    {
      e.Cancel = m_bwdProgress.Cancelled();
    }

    /// <summary>
    /// Puts the FOMod into read-only mode.
    /// </summary>
    /// <remarks>
    /// This method is called by a <see cref="BackgroundWorkerProgressDialog"/>.
    /// </remarks>
    private void BeginFOModReadOnlyTransaction()
    {
      Fomod.BeginReadOnlyTransaction();
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
      if (m_misScript != null)
      {
        m_misScript.Dispose();
      }
    }

    #endregion
  }
}