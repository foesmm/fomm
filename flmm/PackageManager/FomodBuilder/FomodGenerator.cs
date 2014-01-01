using System;
using System.Collections.Generic;
using System.Xml;
using SevenZip;
using System.IO;
using System.Windows.Forms;
using Fomm.Util;
using System.Drawing;
using System.Drawing.Imaging;
using GeMod.Interface;

namespace Fomm.PackageManager.FomodBuilder
{
	/// <summary>
	/// This class builds fomods and premade fomod packs.
	/// </summary>
	public abstract class FomodGenerator
	{
		/// <summary>
		/// The arguments object to pass to the background worker when generating a fomod.
		/// </summary>
		protected abstract class GenerateFomodArgs
		{
			private string m_strPackedPath = null;

			#region Properties

			/// <summary>
			/// Gets or sets the path where the packed file will be created.
			/// </summary>
			/// <value>The path where the packed file will be created.</value>
			public string PackedPath
			{
				get
				{
					return m_strPackedPath;
				}
				set
				{
					m_strPackedPath = value;
				}
			}

			#endregion

			#region Constructors

			/// <summary>
			/// A simple constructor that initializes the object with the given values.
			/// </summary>
			/// <param name="p_strPackedPath">The value with which to initialize the <see cref="PackedPath"/> property.</param>
			public GenerateFomodArgs(string p_strPackedPath)
			{
				m_strPackedPath = p_strPackedPath;
			}

			#endregion
		}

		private BackgroundWorkerProgressDialog m_bwdProgress = null;
		private LinkedList<string> m_lltTempFolders = new LinkedList<string>();

		#region Properties

		/// <summary>
		/// Gets the <see cref="BackgroundWorkerProgressDialog"/> used to generate
		/// the fomod.
		/// </summary>
		/// <value>The <see cref="BackgroundWorkerProgressDialog"/> used to generate
		/// the fomod.</value>
		protected BackgroundWorkerProgressDialog ProgressDialog
		{
			get
			{
				return m_bwdProgress;
			}
		}

		/// <summary>
		/// Gets the overall message to display in the progress dialog.
		/// </summary>
		protected virtual string OverallProgressMessage
		{
			get
			{
				return "Building Fomod...";
			}
		}

		#endregion

		#region Fomod Generation

		/// <summary>
		/// This starts the <see cref="BackgroundWorkerProgressDialog"/> to build the fomod.
		/// </summary>
		/// <remarks>
		/// This method is called by implementers of this abstract class to instantiate the
		/// <see cref="BackgroundWorkerProgressDialog"/> that will be used to generate the fomod. The
		/// <see cref="BackgroundWorkerProgressDialog"/> calls <see cref="DoGenerateFomod(object p_objArgs)"/>,
		/// which must be overridden in the implementer, to actually do the work.
		/// 
		/// This method deals with the cases where <paramref name="p_strPackedFomodPath"/> points
		/// to an existing file. It also performs housecleaning in case the user cancels the operation.
		/// </remarks>
		/// <param name="p_gfaArgs">The arguments to pass the the <see cref="DoGenerateFomod(object p_objArgs)"/>
		/// method.</param>
		/// <returns>The atual path of the generated fomod. This could be <see cref="p_strPackedFomodPath"/>, but
		/// may be different if the given path pointed to an existing file.</returns>
		protected string GenerateFomod(GenerateFomodArgs p_gfaArgs)
		{
			string strPackedPath = p_gfaArgs.PackedPath;
			if (!CheckFileName(ref strPackedPath))
				return null;
			p_gfaArgs.PackedPath = strPackedPath;

			try
			{
				using (m_bwdProgress = new BackgroundWorkerProgressDialog(DoGenerateFomod))
				{
					m_bwdProgress.OverallMessage = OverallProgressMessage;
					m_bwdProgress.ShowItemProgress = true;
					m_bwdProgress.OverallProgressStep = 1;
					m_bwdProgress.WorkMethodArguments = p_gfaArgs;
					if (m_bwdProgress.ShowDialog() == DialogResult.Cancel)
					{
						FileUtil.ForceDelete(strPackedPath);
						return null;
					}
				}
			}
			finally
			{
				foreach (string strFolder in m_lltTempFolders)
					FileUtil.ForceDelete(strFolder);
			}
			return strPackedPath;
		}

		/// <summary>
		/// This method is overridden by implementers to perform the actual fomod generation.
		/// </summary>
		/// <param name="p_objArgs">The arguments the implementer passed to
		/// <see cref="GenerateFomod(string p_strPackedFomodPath, object p_objArgs)"/>.</param>
		protected abstract void DoGenerateFomod(object p_objArgs);

		#endregion

		#region Helper Methods

		/// <summary>
		/// Creates a temporary directory.
		/// </summary>
		/// <remarks>
		/// This method tracks every directory it creates so they can be deleted upon completion of the
		/// fomod generation.
		/// </remarks>
		/// <returns>The path to the new temporary directory.</returns>
		protected string CreateTemporaryDirectory()
		{
			string strTempDirectory = Program.CreateTempDirectory();
			m_lltTempFolders.AddLast(strTempDirectory);
			return strTempDirectory;
		}

		/// <summary>
		/// Determines if the file specified by the given path exists. If so, it
		/// prompts the user if they would like to use a different path.
		/// </summary>
		/// <param name="newpath">The path for which it is to be detemined if a file exists.
		/// It is updated with the new path if the given path was already in use.</param>
		/// <returns><lang cref="true"/> if the returned value of <see cref="newpath"/> does not
		/// specify an existing file; <lang cref="false"/> otherwise.</returns>
		protected bool CheckFileName(ref string newpath)
		{
			string strNewPath = newpath;
			string strExtension = Path.GetExtension(newpath);
			for (Int32 i = 2; i < 999 && File.Exists(strNewPath); i++)
			{
				strNewPath = String.Format("{0} ({1}){2}", Path.ChangeExtension(newpath, null), i, strExtension);
			}
			if (File.Exists(strNewPath))
			{
				MessageBox.Show("File '" + newpath + "' already exists.", "Error");
				return false;
			}
			if (!newpath.Equals(strNewPath))
			{
				switch (MessageBox.Show("File '" + newpath + "' already exists. The old file can be replaced, or the new file can be named '" + strNewPath + "'." + Environment.NewLine + "Do you want to overwrite the old file?", "Warning", MessageBoxButtons.YesNoCancel))
				{
					case DialogResult.Yes:
						return true;
					case DialogResult.No:
						newpath = strNewPath;
						return true;
					case DialogResult.Cancel:
						return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Creates a fomod file at the given path from the given directory.
		/// </summary>
		/// <param name="p_strFomodFolder">The folder from which to create the fomod.</param>
		/// <param name="p_strPackedFomodPath">The path of the new fomod file to create.</param>
		protected void PackFomod(string p_strFomodFolder, string p_strPackedFomodPath)
		{
			ProgressDialog.ItemProgress = 0;
			ProgressDialog.ItemProgressMaximum = Directory.GetFiles(p_strFomodFolder, "*", SearchOption.AllDirectories).Length;
			ProgressDialog.ItemProgressStep = 1;
			ProgressDialog.ItemMessage = String.Format("Compressing FOMod...");

			SevenZipCompressor szcCompressor = new SevenZipCompressor();
			szcCompressor.CompressionLevel = Properties.Settings.Default.fomodCompressionLevel;
			szcCompressor.ArchiveFormat = Properties.Settings.Default.fomodCompressionFormat;
			szcCompressor.CompressionMethod = CompressionMethod.Default;
			switch (szcCompressor.ArchiveFormat)
			{
				case OutArchiveFormat.Zip:
				case OutArchiveFormat.GZip:
				case OutArchiveFormat.BZip2:
					szcCompressor.CustomParameters.Add("mt", "on");
					break;
				case OutArchiveFormat.SevenZip:
				case OutArchiveFormat.XZ:
					szcCompressor.CustomParameters.Add("mt", "on");
					szcCompressor.CustomParameters.Add("s", "off");
					break;
			}
			szcCompressor.CompressionMode = CompressionMode.Create;
			szcCompressor.FileCompressionStarted += new EventHandler<FileNameEventArgs>(FileCompressionStarted);
			szcCompressor.FileCompressionFinished += new EventHandler<EventArgs>(FileCompressionFinished);
			szcCompressor.CompressDirectory(p_strFomodFolder, p_strPackedFomodPath);
		}

		/// <summary>
		/// This creates a script file in the specified folder using the given <see cref="FomodScript"/>
		/// metadata.
		/// </summary>
		/// <param name="p_strFomodFomodFolder">The folder in which to create the script file.</param>
		/// <param name="p_fscScript">The metadata to use to create the file.</param>
		protected void CreateScriptFile(string p_strFomodFomodFolder, FomodScript p_fscScript)
		{
			ProgressDialog.ItemProgress = 0;
			ProgressDialog.ItemProgressMaximum = 1;
			ProgressDialog.ItemProgressStep = 1;
			ProgressDialog.ItemMessage = String.Format("Creating Script File...");
			if ((p_fscScript != null) && !String.IsNullOrEmpty(p_fscScript.Text))
				File.WriteAllText(Path.Combine(p_strFomodFomodFolder, p_fscScript.FileName), p_fscScript.Text);
			ProgressDialog.StepItemProgress();
		}

		/// <summary>
		/// This creates a screenshot file in the specified folder using the given <see cref="Screenshot"/>
		/// metadata.
		/// </summary>
		/// <remarks>
		/// The file is only created if <paramref name="p_booSetScreenshot"/> is <lang cref="true"/>. If
		/// <paramref name="p_booSetScreenshot"/> is <lang cref="true"/> and <paramref name="p_shtScreenshot"/>
		/// is <lang cref="null"/>, then any existing screenshot files will be deleted.
		/// </remarks>
		/// <param name="p_strFomodFomodFolder">The folder in which to create the screenshot file.</param>
		/// <param name="p_booSetScreenshot">Whether or not to create the file.</param>
		/// <param name="p_shtScreenshot">The metadata to use to create the file.</param>
		protected void CreateScreenshot(string p_strFomodFomodFolder, bool p_booSetScreenshot, Screenshot p_shtScreenshot)
		{
			ProgressDialog.ItemProgress = 0;
			ProgressDialog.ItemProgressMaximum = 1;
			ProgressDialog.ItemProgressStep = 1;
			ProgressDialog.ItemMessage = String.Format("Creating Screenshot...");
			if (p_booSetScreenshot)
			{
				string[] strScreenshots = Directory.GetFiles(p_strFomodFomodFolder, "screenshot.*", SearchOption.TopDirectoryOnly);
				foreach (String strScreenshot in strScreenshots)
					FileUtil.ForceDelete(strScreenshot);
				if (p_shtScreenshot != null)
					File.WriteAllBytes(Path.Combine(p_strFomodFomodFolder, "screenshot" + p_shtScreenshot.Extension), p_shtScreenshot.Data);
			}
			ProgressDialog.StepItemProgress();
		}

		/// <summary>
		/// This writes the given fomod info file in the specified directory.
		/// </summary>
		/// <param name="p_strFomodFomodFolder">The folder in which to create the fomod info file.</param>
		/// <param name="p_xmlInfo">The file to write.</param>
		protected void CreateInfoFile(string p_strFomodFomodFolder, XmlDocument p_xmlInfo)
		{
			ProgressDialog.ItemProgress = 0;
			ProgressDialog.ItemProgressMaximum = 1;
			ProgressDialog.ItemProgressStep = 1;
			ProgressDialog.ItemMessage = String.Format("Creating Info File...");
			if (p_xmlInfo != null)
				p_xmlInfo.Save(Path.Combine(p_strFomodFomodFolder, "info.xml"));
			ProgressDialog.StepItemProgress();
		}

		/// <summary>
		/// This creates a readme file in the specified folder using the given <see cref="Readme"/>
		/// metadata.
		/// </summary>
		/// <param name="p_strFomodFomodFolder">The folder in which to create the readme file.</param>
		/// <param name="p_rmeReadme">The metadata to use to create the file.</param>
		protected void CreateReadmeFile(string p_strFomodFolder, string p_strFomodName, Readme p_rmeReadme)
		{
			ProgressDialog.ItemProgress = 0;
			ProgressDialog.ItemProgressMaximum = 1;
			ProgressDialog.ItemProgressStep = 1;
			ProgressDialog.ItemMessage = String.Format("Creating Readme File...");
			if ((p_rmeReadme != null) && !String.IsNullOrEmpty(p_rmeReadme.Text))
			{
				string strReadmeFileName = String.Format("Readme - {0}{1}", p_strFomodName, p_rmeReadme.Extension);
				if (Properties.Settings.Default.UseDocsFolder)
					strReadmeFileName = Path.Combine("docs", strReadmeFileName);
				File.WriteAllText(Path.Combine(p_strFomodFolder, strReadmeFileName), p_rmeReadme.Text);
			}
			ProgressDialog.StepItemProgress();
		}

		/// <summary>
		/// This unpacks the specified archive to the specified path.
		/// </summary>
		/// <param name="p_strArchivePath">The path to the archive to unpack.</param>
		/// <param name="p_strExtractionPath">The path to the directory to which to unpack the archive.</param>
		protected void UnpackArchive(string p_strArchivePath, string p_strExtractionPath)
		{
			using (SevenZipExtractor szeExtractor = Archive.GetExtractor(p_strArchivePath))
			{
				szeExtractor.FileExtractionFinished += new EventHandler<FileInfoEventArgs>(FileExtractionFinished);
				szeExtractor.FileExtractionStarted += new EventHandler<FileInfoEventArgs>(FileExtractionStarted);
				ProgressDialog.ItemProgress = 0;
				ProgressDialog.ItemProgressMaximum = (Int32)szeExtractor.FilesCount;
				ProgressDialog.ItemProgressStep = 1;
				ProgressDialog.ItemMessage = String.Format("Extracting Source Files ({0})...", Path.GetFileName(p_strArchivePath));
				szeExtractor.ExtractArchive(p_strExtractionPath);
			}
		}

		#endregion

		#region Callbacks

		/// <summary>
		/// Updates the progress window when a file is copied.
		/// </summary>
		/// <remarks>
		/// This is passed as the callback method for the <see cref="FileUtil.Copy"/> used to
		/// execute the copy instructions.
		/// </remarks>
		/// <param name="p_strFile">The file the was copied.</param>
		/// <returns><lang cref="true"/> if the user cancelled the operation; <lang cref="false"/> otherwise.</returns>
		protected bool FileCopied(string p_strFile)
		{
			ProgressDialog.StepItemProgress();
			return ProgressDialog.Cancelled();
		}

		/// <summary>
		/// Called when a file has been extracted from a source archive.
		/// </summary>
		/// <remarks>
		/// This steps the progress of the create fomod from archive progress dialog.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void FileExtractionFinished(object sender, FileInfoEventArgs e)
		{
			ProgressDialog.StepItemProgress();
		}

		/// <summary>
		/// Called when a file is about to be extracted from a source archive.
		/// </summary>
		/// <remarks>
		/// This cancels the compression if the user has clicked the cancel button of the progress dialog.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="FileNameEventArgs"/> describing the event arguments.</param>
		private void FileExtractionStarted(object sender, FileInfoEventArgs e)
		{
			e.Cancel = ProgressDialog.Cancelled();
		}

		/// <summary>
		/// Called when a file has been added to a new fomod.
		/// </summary>
		/// <remarks>
		/// This steps the progress of the create fomod from folder progress dialog.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		protected void FileCompressionFinished(object sender, EventArgs e)
		{
			ProgressDialog.StepItemProgress();
		}

		/// <summary>
		/// Called when a file is about to be added to a new fomod.
		/// </summary>
		/// <remarks>
		/// This cancels the compression if the user has clicked the cancel button of the progress dialog.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="FileNameEventArgs"/> describing the event arguments.</param>
		protected void FileCompressionStarted(object sender, FileNameEventArgs e)
		{
			e.Cancel = ProgressDialog.Cancelled();
		}

		#endregion
	}
}
