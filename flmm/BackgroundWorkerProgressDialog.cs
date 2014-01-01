﻿using System;
using System.Windows.Forms;
using System.ComponentModel;
using Action = Fomm.Util.SystemUtil.Action;

namespace Fomm
{
	/// <summary>
	/// Performs work in the background and provides a UI to report progress.
	/// </summary>
	public partial class BackgroundWorkerProgressDialog : Form, IDisposable
	{
		public delegate void WorkerMethod();
		public delegate void ParamWorkerMethod(object p_objArgument);

		private WorkerMethod m_wkmWorkMethod = null;
		private ParamWorkerMethod m_pwmWorkerMethod = null;
		private object m_objWorkMethodParam = null;
		private DoWorkEventArgs m_weaDoWorkEventArgs = null;
		private BackgroundWorker m_bgwWorker = null;
		private Exception m_exError = null;

		#region Properties

		/// <summary>
		/// Sets the argument object to pass to the backgroun worker work method.
		/// </summary>
		public object WorkMethodArguments
		{
			set
			{
				m_objWorkMethodParam = value;
			}
		}

		/// <summary>
		/// Gets the exception that was thrown during the execution of the background work.
		/// </summary>
		/// <value>The exception that was thrown during the execution of the background work,
		/// or <lang cref="null"/> if now exception was thrown.</value>
		public Exception Error
		{
			get
			{
				return m_exError;
			}
		}

		/// <summary>
		/// Gets or sets whether the item progress is visible.
		/// </summary>
		/// <value>Whether the item progress is visible.</value>
		public bool ShowItemProgress
		{
			get
			{
				return pnlItemProgress.Visible;
			}
			set
			{
				if (pnlItemProgress.InvokeRequired)
					pnlItemProgress.Invoke(new Action(() => { pnlItemProgress.Visible = value; }));
				else
					pnlItemProgress.Visible = value;
			}
		}

		/// <summary>
		/// Sets the message shown above the item progress bar.
		/// </summary>
		/// <value>The message shown above the item progress bar.</value>
		public string ItemMessage
		{
			set
			{
				if (lblItemMessage.InvokeRequired)
					lblItemMessage.Invoke(new Action(() => { lblItemMessage.Text = value; }));
				else
					lblItemMessage.Text = value;
			}
		}

		/// <summary>
		/// Sets the message shown above the total progress bar.
		/// </summary>
		/// <value>The message shown above the total progress bar.</value>
		public string OverallMessage
		{
			set
			{
				if (lblTotalMessage.InvokeRequired)
					lblTotalMessage.Invoke(new Action(() => { lblTotalMessage.Text = value; }));
				else
					lblTotalMessage.Text = value;
			}
		}

		/// <summary>
		/// Sets the progress on current item of work.
		/// </summary>
		/// <value>The progress on current item of work.</value>
		public Int32 ItemProgress
		{
			set
			{
				if (pbrItemProgress.InvokeRequired)
					pbrItemProgress.Invoke(new Action(() => { pbrItemProgress.Value = value; }));
				else
					pbrItemProgress.Value = value;
			}
		}

		/// <summary>
		/// Sets the progress on the overall work.
		/// </summary>
		/// <value>The progress on the overall work.</value>
		public Int32 OverallProgress
		{
			set
			{
				if (pbrTotalProgress.InvokeRequired)
					pbrTotalProgress.Invoke(new Action(() => { pbrTotalProgress.Value = value; }));
				else
					pbrTotalProgress.Value = value;
			}
		}

		/// <summary>
		/// Sets the minimum value on the item progress bar.
		/// </summary>
		/// <value>The minimum value on the item progress bar.</value>
		public Int32 ItemProgressMinimum
		{
			set
			{
				if (pbrItemProgress.InvokeRequired)
					pbrItemProgress.Invoke(new Action(() => { pbrItemProgress.Minimum = value; }));
				else
					pbrItemProgress.Minimum = value;
			}
		}

		/// <summary>
		/// Sets the minimum value on the overall progress bar.
		/// </summary>
		/// <value>The minimum value on the overall progress bar.</value>
		public Int32 OverallProgressMinimum
		{
			set
			{
				if (pbrTotalProgress.InvokeRequired)
					pbrTotalProgress.Invoke(new Action(() => { pbrTotalProgress.Minimum = value; }));
				else
					pbrTotalProgress.Minimum = value;
			}
		}

		/// <summary>
		/// Sets the maximum value on the item progress bar.
		/// </summary>
		/// <value>The maximum value on the item progress bar.</value>
		public Int32 ItemProgressMaximum
		{
			set
			{
				if (pbrItemProgress.InvokeRequired)
					pbrItemProgress.Invoke(new Action(() => { pbrItemProgress.Maximum = value; }));
				else
					pbrItemProgress.Maximum = value;
			}
		}

		/// <summary>
		/// Sets the maximum value on the overall progress bar.
		/// </summary>
		/// <value>The maximum value on the overall progress bar.</value>
		public Int32 OverallProgressMaximum
		{
			set
			{
				if (pbrTotalProgress.InvokeRequired)
					pbrTotalProgress.Invoke(new Action(() => { pbrTotalProgress.Maximum = value; }));
				else
					pbrTotalProgress.Maximum = value;
			}
		}

		/// <summary>
		/// Sets whether the overall progress bar should be a marquee.
		/// </summary>
		/// <value>Whether the overall progress bar should be a marquee.</value>
		public bool OverallProgressMarquee
		{
			set
			{
				pbrTotalProgress.Style = value ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
			}
		}

		/// <summary>
		/// Sets the step size of the item progress bar.
		/// </summary>
		/// <value>The step size of the item progress bar.</value>
		public Int32 ItemProgressStep
		{
			set
			{
				if (pbrItemProgress.InvokeRequired)
					pbrTotalProgress.Invoke(new Action(() => { pbrItemProgress.Step = value; }));
				else
					pbrItemProgress.Step = value;
			}
		}

		/// <summary>
		/// Sets the step size of the overall progress bar.
		/// </summary>
		/// <value>The step size of the overall progress bar.</value>
		public Int32 OverallProgressStep
		{
			set
			{
				if (pbrTotalProgress.InvokeRequired)
					pbrTotalProgress.Invoke(new Action(() => { pbrTotalProgress.Step = value; }));
				else
					pbrTotalProgress.Step = value;
			}
		}

		#endregion

		/// <summary>
		/// The default constructor.
		/// </summary>
		public BackgroundWorkerProgressDialog()
		{
			InitializeComponent();
			m_bgwWorker = new BackgroundWorker();
			m_bgwWorker.WorkerReportsProgress = true;
			m_bgwWorker.WorkerSupportsCancellation = true;
			m_bgwWorker.DoWork += new DoWorkEventHandler(m_bgwWorker_DoWork);
			m_bgwWorker.ProgressChanged += new ProgressChangedEventHandler(m_bgwWorker_ProgressChanged);
			m_bgwWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(m_bgwWorker_RunWorkerCompleted);
		}

		/// <summary>
		/// Sets up the background worker.
		/// </summary>
		/// <remarks>
		/// The method passed to this constructor can't have any arguments.
		/// </remarks>
		/// <param name="p_dlgWorker">The method that will do the work.</param>
		public BackgroundWorkerProgressDialog(WorkerMethod p_dlgWorker)
			: this()
		{
			m_wkmWorkMethod = p_dlgWorker;
		}

		/// <summary>
		/// Sets up the background worker.
		/// </summary>
		/// <remarks>
		/// The method passed to this constructor must have one argument.
		/// </remarks>
		/// <param name="p_dlgWorker">The method that will do the work.</param>
		public BackgroundWorkerProgressDialog(ParamWorkerMethod p_dlgWorker)
			: this()
		{
			m_pwmWorkerMethod = p_dlgWorker;
		}

		#region Form Events

		/// <summary>
		/// Handles the <see cref="Button.Click"/> event of the cancel button.
		/// </summary>
		/// <remarks>
		/// This asks the <see cref="BackgroundWorker"/> to cancel. It also disables the
		/// cancel button to let the user know the process is cancelling.
		/// </remarks>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> that describes the event arguments.</param>
		private void butCancel_Click(object sender, EventArgs e)
		{
			butCancel.Enabled = false;
			butCancel.Text = "Cancelling";
			DialogResult = DialogResult.Cancel;
			m_bgwWorker.CancelAsync();
		}

		/// <summary>
		/// Raises the <see cref="Form.OnShown"/> event.
		/// </summary>
		/// <remarks>
		/// This starts the background worker.
		/// </remarks>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> that describes the event arguments.</param>
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			m_bgwWorker.RunWorkerAsync(m_objWorkMethodParam);
		}

		/// <summary>
		/// Raises the <see cref="Form.OnClosing"/> event.
		/// </summary>
		/// <remarks>
		/// This prevents the form from closing if the background worker hasn't been
		/// closed down.
		/// </remarks>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> that describes the event arguments.</param>
		protected override void OnClosing(CancelEventArgs e)
		{
			if (m_bgwWorker.IsBusy)
				e.Cancel = true;
			base.OnClosing(e);
		}

		#endregion

		#region Progress Helpers

		/// <summary>
		/// Reports that the given percent of work has been completed overall.
		/// </summary>
		/// <param name="p_intPercent">The percent of work, overall, that has been completed.</param>
		public void ReportOverallProgress(Int32 p_intPercent)
		{
			m_bgwWorker.ReportProgress(p_intPercent, true);
		}

		/// <summary>
		/// Reports that the given percent of work has been completed for the current item.
		/// </summary>
		/// <param name="p_intPercent">The percent of work for the current item that has been completed.</param>
		public void ReportItemProgress(Int32 p_intPercent)
		{
			m_bgwWorker.ReportProgress(p_intPercent, false);
		}

		/// <summary>
		/// Steps the overall progress bar.
		/// </summary>
		public void StepOverallProgress()
		{
			m_bgwWorker.ReportProgress(-1, true);
		}

		/// <summary>
		/// Steps the item progress bar.
		/// </summary>
		public void StepItemProgress()
		{
			m_bgwWorker.ReportProgress(-1, false);
		}

		/// <summary>
		/// Checks if the user has cancelled.
		/// </summary>
		/// <returns><lang cref="true"/> if the user has cancelled and work needs to stop;
		/// <lang cref="false"/> otherwise.</returns>
		public bool Cancelled()
		{
			if (m_bgwWorker.CancellationPending)
			{
				m_weaDoWorkEventArgs.Cancel = true;
				return true;
			}
			return false;
		}

		#endregion

		#region Background Worker Event Handling

		/// <summary>
		/// Handles the <see cref="BackgroundWorker.DoWork"/> event of the
		/// brackground worker.
		/// </summary>
		/// <remarks>
		/// This calls the method supplied in the constructor to do the work.
		/// </remarks>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">An <see cref="DoWorkEventArgs"/> that describes the event arguments.</param>
		protected void m_bgwWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			m_weaDoWorkEventArgs = e;
			if (m_wkmWorkMethod != null)
				m_wkmWorkMethod();
			else if (m_pwmWorkerMethod != null)
				m_pwmWorkerMethod(e.Argument);
		}

		/// <summary>
		/// Handles the <see cref="BackgroundWorker.RunWorkerCompleted"/> event of the
		/// brackground worker.
		/// </summary>
		/// <remarks>
		/// This sets the <see cref="Form.DialogResult"/>, dependant upon whether or not the
		/// worker was cancelled.
		/// </remarks>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">An <see cref="RunWorkerCompletedEventArgs"/> that describes the event arguments.</param>
		void m_bgwWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Cancelled)
				DialogResult = DialogResult.Cancel;
			else
				DialogResult = DialogResult.OK;
			m_exError = e.Error;
			this.Close();
		}

		/// <summary>
		/// Handles the <see cref="BackgroundWorker.ProgressChanged"/> event of the
		/// brackground worker.
		/// </summary>
		/// <remarks>
		/// This updates the progress bars.
		/// </remarks>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">An <see cref="ProgressChangedEventArgs"/> that describes the event arguments.</param>
		void m_bgwWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if ((bool)e.UserState)
			{
				if (e.ProgressPercentage < 0)
					pbrTotalProgress.PerformStep();
				else
					pbrTotalProgress.Value = (Int32)(e.ProgressPercentage / 100m * (pbrTotalProgress.Maximum - pbrTotalProgress.Minimum));
			}
			else
			{
				if (e.ProgressPercentage < 0)
					pbrItemProgress.PerformStep();
				else
					pbrItemProgress.Value = (Int32)(e.ProgressPercentage / 100m * (pbrItemProgress.Maximum - pbrItemProgress.Minimum));
			}
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Throws any exceptions that were raised during the background worker process.
		/// </summary>
		void IDisposable.Dispose()
		{
			base.Dispose();
			if (Error != null)
				throw Error;
		}

		#endregion
	}
}
