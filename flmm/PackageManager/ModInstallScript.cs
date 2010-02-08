using System;
using ChinhDo.Transactions;

namespace Fomm.PackageManager
{
	/// <summary>
	/// the base script for scripts that install or uninstall mods.
	/// </summary>
	public abstract class ModInstallScript : ModScript
	{
		protected static readonly object objInstallLock = new object();
		private TxFileManager m_tfmFileManager = null;

		#region Properties

		/// <summary>
		/// Gets the transactional file manager the script is using.
		/// </summary>
		/// <value>The transactional file manager the script is using.</value>
		protected TxFileManager TransactionalFileManager
		{
			get
			{
				if (m_tfmFileManager == null)
					throw new InvalidOperationException("The transactional file manager must be initialized by calling InitTransactionalFileManager() before it is used.");
				return m_tfmFileManager;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object.
		/// </summary>
		/// <param name="p_fomodMod">The <see cref="fomod"/> to be installed or uninstalled.</param>
		public ModInstallScript(fomod p_fomodMod)
			: base(p_fomodMod)
		{
			m_tfmFileManager = new TxFileManager();
		}

		#endregion

		/// <summary>
		/// Initializes the current transactional file manager.
		/// </summary>
		protected void InitTransactionalFileManager()
		{
			m_tfmFileManager = new TxFileManager();
		}

		/// <summary>
		/// Releases the current transactional file manager.
		/// </summary>
		protected void ReleaseTransactionalFileManager()
		{
			m_tfmFileManager = null;
		}
	}
}
