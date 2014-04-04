using System;
using System.Timers;
using ICSharpCode.TextEditor;

namespace Fomm.Controls
{
  /// <summary>
  ///   An code text editor.
  /// </summary>
  /// <remarks>
  ///   This editor provides highlighting and code folding.
  /// </remarks>
  public class CodeEditor : TextEditorControl
  {
    private Timer m_tmrFoldUpdater = new Timer();

    #region Constructors

    /// <summary>
    ///   The default constructor.
    /// </summary>
    public CodeEditor()
    {
      Document.FoldingManager.FoldingStrategy = new CodeFoldingStrategy();
      m_tmrFoldUpdater.Elapsed += UpdateFolds;
      m_tmrFoldUpdater.Interval = 2000;
    }

    #endregion

    /// <summary>
    ///   Raises the <see cref="System.Windows.Forms.Control.Load" /> event.
    /// </summary>
    /// <remarks>
    ///   This sets the synchronizing object on the timers to our form. Doing so allows the timers
    ///   to update the UI.
    /// </remarks>
    /// <param name="e">An <see cref="EventArgs" /> describing the event arguments.</param>
    protected override void OnLoad(EventArgs e)
    {
      m_tmrFoldUpdater.SynchronizingObject = FindForm();
      base.OnLoad(e);
    }

    /// <summary>
    ///   Updates the code folds.
    /// </summary>
    /// <remarks>
    ///   This method is called by a timer after a set span after the text in the editor was last changed.
    /// </remarks>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">An <see cref="EventArgs" /> describing the event arguments.</param>
    private void UpdateFolds(object sender, EventArgs e)
    {
      Document.FoldingManager.UpdateFoldings(null, null);
      Document.FoldingManager.NotifyFoldingsChanged(null);
      m_tmrFoldUpdater.Stop();
    }

    /// <summary>
    ///   Starts the timers to update the code folds and validate the XML.
    /// </summary>
    /// <param name="e">An <see cref="EventArgs" /> describing the event arguments.</param>
    protected override void OnTextChanged(EventArgs e)
    {
      m_tmrFoldUpdater.Stop();
      m_tmrFoldUpdater.Start();
      base.OnTextChanged(e);
    }
  }
}