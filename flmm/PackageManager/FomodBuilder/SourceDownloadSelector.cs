using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Fomm.PackageManager.FomodBuilder
{
  /// <summary>
  ///   A control that allows the specification of download locations for source files.
  /// </summary>
  /// <remarks>
  ///   Source files are used to build FOMods and PFPs.
  /// </remarks>
  public partial class SourceDownloadSelector : UserControl
  {
    #region Properties

    /// <summary>
    ///   Gets or sets the list of source download locations being managed.
    /// </summary>
    /// <value>The list of source download locations being managed.</value>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<SourceFile> DataSource
    {
      get
      {
        return (IList<SourceFile>) dgvSourceList.DataSource ?? new List<SourceFile>();
      }
      set
      {
        dgvSourceList.DataSource = value;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    ///   The default constructor.
    /// </summary>
    public SourceDownloadSelector()
    {
      InitializeComponent();
    }

    #endregion

    /// <summary>
    ///   Handles the <see cref="DataGridView.CurrentCellDirtyStateChanged" /> event of the
    ///   source list grid.
    /// </summary>
    /// <remarks>
    ///   This commits changes.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs" /> describing the event arguments.</param>
    private void dgvSourceList_CurrentCellDirtyStateChanged(object sender, EventArgs e)
    {
      if (dgvSourceList.IsCurrentCellDirty)
      {
        dgvSourceList.CommitEdit(DataGridViewDataErrorContexts.Commit);
      }
    }

    /// <summary>
    ///   Handles the <see cref="DataGridView.CellValueChanged" /> event of the
    ///   source list grid.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="DataGridViewCellEventArgs" /> describing the event arguments.</param>
    private void dgvSourceList_CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (dgvSourceList.Columns[e.ColumnIndex].Name == "clmIncluded")
      {
        HandleIncludedChange(e.RowIndex);
        Invalidate();
      }
    }

    /// <summary>
    ///   This changes the appearance of the specified row dependent upon whether the source
    ///   has been marked as included.
    /// </summary>
    /// <param name="p_intRowIndex"></param>
    private void HandleIncludedChange(Int32 p_intRowIndex)
    {
      var downloadLocationCell =
        (DataGridViewTextBoxCell) dgvSourceList.Rows[p_intRowIndex].Cells["clmURL"];
      var ckcHidden =
        (DataGridViewCheckBoxCell) dgvSourceList.Rows[p_intRowIndex].Cells["clmHidden"];
      var ckcGenerated =
        (DataGridViewCheckBoxCell) dgvSourceList.Rows[p_intRowIndex].Cells["clmGenerated"];

      var ckcIncluded =
        (DataGridViewCheckBoxCell) dgvSourceList.Rows[p_intRowIndex].Cells["clmIncluded"];
      downloadLocationCell.ReadOnly = (Boolean) ckcIncluded.Value;
      downloadLocationCell.Style.ForeColor = ((Boolean) ckcIncluded.Value)
        ? Color.FromKnownColor(KnownColor.InactiveCaptionText)
        : Color.FromKnownColor(KnownColor.WindowText);
      downloadLocationCell.Style.Font = ((Boolean) ckcIncluded.Value)
        ? new Font(downloadLocationCell.InheritedStyle.Font, FontStyle.Italic)
        : new Font(downloadLocationCell.InheritedStyle.Font, FontStyle.Regular);
      ckcHidden.ReadOnly = (Boolean) ckcIncluded.Value;
      ckcGenerated.ReadOnly = (Boolean) ckcIncluded.Value;
    }

    /// <summary>
    ///   Handles the <see cref="DataGridView.RowsAdded" /> event of the
    ///   source list grid.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="DataGridViewRowsAddedEventArgs" /> describing the event arguments.</param>
    private void dgvSourceList_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
    {
      HandleIncludedChange(e.RowIndex);
    }

    /// <summary>
    ///   Handles the <see cref="DataGridView.DataSourceChanged" /> event of the
    ///   source list grid.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="EventArgs" /> describing the event arguments.</param>
    private void dgvSourceList_DataSourceChanged(object sender, EventArgs e)
    {
      for (var i = 0; i < dgvSourceList.RowCount; i++)
      {
        HandleIncludedChange(i);
      }
    }
  }
}