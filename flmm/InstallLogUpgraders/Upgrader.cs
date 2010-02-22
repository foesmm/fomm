using System;
using ChinhDo.Transactions;
using System.Xml;
using Fomm.PackageManager;
using fomm.Transactions;
using System.Windows.Forms;

namespace Fomm.InstallLogUpgraders
{
	internal abstract class Upgrader
	{
		private static object m_objLock = new object();
		private BackgroundWorkerProgressDialog m_pgdProgress = null;
		private TxFileManager m_tfmFileManager = null;

		#region Properties

		protected BackgroundWorkerProgressDialog ProgressWorker
		{
			get
			{
				return m_pgdProgress;
			}
		}

		protected TxFileManager FileManager
		{
			get
			{
				return m_tfmFileManager;
			}
		}

		#endregion

		#region Constructor

		internal Upgrader()
		{
		}

		#endregion

		internal bool PerformUpgrade()
		{
			m_tfmFileManager = new TxFileManager();
			bool booComplete = false;
			using (TransactionScope tsTransaction = new TransactionScope())
			{
				m_tfmFileManager.Snapshot(InstallLog.Current.InstallLogPath);

				using (m_pgdProgress = new BackgroundWorkerProgressDialog(DoUpgrade))
				{
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

		protected abstract void DoUpgrade();
	}
}
