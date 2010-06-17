using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;

namespace Fomm.PackageManager.FomodBuilder
{
	public partial class SourceDownloadSelector : UserControl
	{
		public class SourceDownloadLocation
		{
			public string Source { get; set; }

			public string SourceFileName
			{
				get
				{
					return Path.GetFileName(Source);
				}
			}

			public string URL { get; set; }
			public bool Included { get; set; }

			public SourceDownloadLocation()
			{
			}

			public SourceDownloadLocation(string p_strSource, string p_strUrl, bool p_booIncluded)
			{
				Source = p_strSource;
				URL = p_strUrl;
				Included = p_booIncluded;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IList<SourceDownloadLocation> DataSource
		{
			get
			{
				return (IList<SourceDownloadLocation>)dgvSourceList.DataSource ?? new List<SourceDownloadLocation>();
			}
			set
			{
				dgvSourceList.DataSource = value;
			}
		}

		public SourceDownloadSelector()
		{
			InitializeComponent();
		}

		private void dgvSourceList_CurrentCellDirtyStateChanged(object sender, EventArgs e)
		{
			if (dgvSourceList.IsCurrentCellDirty)
				dgvSourceList.CommitEdit(DataGridViewDataErrorContexts.Commit);
		}

		private void dgvSourceList_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (dgvSourceList.Columns[e.ColumnIndex].Name == "clmIncluded")
			{
				HandleIncludedChange(e.RowIndex);
				Invalidate();
			}
		}

		private void HandleIncludedChange(Int32 p_intRowIndex)
		{
			DataGridViewTextBoxCell downloadLocationCell = (DataGridViewTextBoxCell)dgvSourceList.Rows[p_intRowIndex].Cells["clmURL"];

			DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)dgvSourceList.Rows[p_intRowIndex].Cells["clmIncluded"];
			downloadLocationCell.ReadOnly = (Boolean)checkCell.Value;
			downloadLocationCell.Style.ForeColor = ((Boolean)checkCell.Value) ? Color.FromKnownColor(KnownColor.InactiveCaptionText) : Color.FromKnownColor(KnownColor.WindowText);
			downloadLocationCell.Style.Font = ((Boolean)checkCell.Value) ? new Font(downloadLocationCell.InheritedStyle.Font, FontStyle.Italic) : new Font(downloadLocationCell.InheritedStyle.Font, FontStyle.Regular);

		}

		private void dgvSourceList_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
		{
			HandleIncludedChange(e.RowIndex);
		}
	}
}
