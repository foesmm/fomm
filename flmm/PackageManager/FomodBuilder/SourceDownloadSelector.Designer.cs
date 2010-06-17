namespace Fomm.PackageManager.FomodBuilder
{
	partial class SourceDownloadSelector
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.dgvSourceList = new System.Windows.Forms.DataGridView();
			this.clmSource = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.clmURL = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.clmIncluded = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this.dgvSourceList)).BeginInit();
			this.SuspendLayout();
			// 
			// dgvSourceList
			// 
			this.dgvSourceList.AllowUserToAddRows = false;
			this.dgvSourceList.AllowUserToDeleteRows = false;
			this.dgvSourceList.AutoGenerateColumns = false;
			this.dgvSourceList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvSourceList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.clmSource,
            this.clmURL,
            this.clmIncluded});
			this.dgvSourceList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvSourceList.Location = new System.Drawing.Point(0, 0);
			this.dgvSourceList.Name = "dgvSourceList";
			this.dgvSourceList.Size = new System.Drawing.Size(544, 294);
			this.dgvSourceList.TabIndex = 0;
			this.dgvSourceList.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvSourceList_CellValueChanged);
			this.dgvSourceList.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.dgvSourceList_RowsAdded);
			this.dgvSourceList.CurrentCellDirtyStateChanged += new System.EventHandler(this.dgvSourceList_CurrentCellDirtyStateChanged);
			// 
			// clmSource
			// 
			this.clmSource.DataPropertyName = "SourceFileName";
			this.clmSource.Frozen = true;
			this.clmSource.HeaderText = "Source";
			this.clmSource.Name = "clmSource";
			this.clmSource.ReadOnly = true;
			// 
			// clmURL
			// 
			this.clmURL.DataPropertyName = "URL";
			this.clmURL.HeaderText = "Download Location";
			this.clmURL.Name = "clmURL";
			this.clmURL.Width = 300;
			// 
			// clmIncluded
			// 
			this.clmIncluded.DataPropertyName = "Included";
			this.clmIncluded.HeaderText = "Included";
			this.clmIncluded.Name = "clmIncluded";
			// 
			// SourceDownloadSelector
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.dgvSourceList);
			this.Name = "SourceDownloadSelector";
			this.Size = new System.Drawing.Size(544, 294);
			((System.ComponentModel.ISupportInitialize)(this.dgvSourceList)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView dgvSourceList;
		private System.Windows.Forms.DataGridViewTextBoxColumn clmSource;
		private System.Windows.Forms.DataGridViewTextBoxColumn clmURL;
		private System.Windows.Forms.DataGridViewCheckBoxColumn clmIncluded;
	}
}
