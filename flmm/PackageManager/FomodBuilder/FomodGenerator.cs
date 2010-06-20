using System;
using System.Collections.Generic;
using System.Xml;
using SevenZip;
using System.IO;
using System.Windows.Forms;
using Fomm.Util;
using System.Drawing;
using System.Drawing.Imaging;

namespace Fomm.PackageManager.FomodBuilder
{
	/// <summary>
	/// This class builds fomods and premade fomod packs.
	/// </summary>
	public class FomodGenerator
	{
		/// <summary>
		/// The arguments object to pass to the background worker when building a fomod.
		/// </summary>
		private class BuildFomodArgs
		{
			private string m_strFomodName = null;
			private IList<KeyValuePair<string, string>> m_lstCopyInstructions = null;
			private Readme m_rmeReadme = null;
			private XmlDocument m_xmlInfo = null;
			private bool m_booSetScreenshot = false;
			private Screenshot m_shtScreenshot = null;
			private FomodScript m_fscScript = null;
			private string m_strPackedFomodPath = null;

			#region Properties

			/// <summary>
			/// Gets or sets the fomodName.
			/// </summary>
			/// <value>The fomodName.</value>
			public string FomodName
			{
				get
				{
					return m_strFomodName;
				}
			}

			/// <summary>
			/// Gets or sets the copy instructions that need to be executed to create the fomod.
			/// </summary>
			/// <value>The copy instructions that need to be executed to create the fomod.</value>
			public IList<KeyValuePair<string, string>> CopyInstructions
			{
				get
				{
					return m_lstCopyInstructions;
				}
			}

			/// <summary>
			/// Gets or sets the readme.
			/// </summary>
			/// <value>The readme.</value>
			public Readme Readme
			{
				get
				{
					return m_rmeReadme;
				}
			}

			/// <summary>
			/// Gets or sets the info file.
			/// </summary>
			/// <value>The info file.</value>
			public XmlDocument InfoFile
			{
				get
				{
					return m_xmlInfo;
				}
			}

			/// <summary>
			/// Gets or sets the setScreenshot.
			/// </summary>
			/// <value>The setScreenshot.</value>
			public bool SetScreenshot
			{
				get
				{
					return m_booSetScreenshot;
				}
			}

			/// <summary>
			/// Gets or sets the screenshot.
			/// </summary>
			/// <value>The screenshot.</value>
			public Screenshot Screenshot
			{
				get
				{
					return m_shtScreenshot;
				}
			}

			/// <summary>
			/// Gets or sets the script.
			/// </summary>
			/// <value>The script.</value>
			public FomodScript Script
			{
				get
				{
					return m_fscScript;
				}
			}

			/// <summary>
			/// Gets or sets the packedFomodPath.
			/// </summary>
			/// <value>The packedFomodPath.</value>
			public string PackedFomodPath
			{
				get
				{
					return m_strPackedFomodPath;
				}
			}

			#endregion

			#region Constructors

			/// <summary>
			/// A simple constructor that initializes the object with the given values.
			/// </summary>
			/// <param name="p_strFomodName">The value with which to initialize the <see cref="FomodName"/> property.</param>
			/// <param name="p_lstCopyPaths">The value with which to initialize the <see cref="CopyPaths"/> property.</param>
			/// <param name="p_rmeReadme">The value with which to initialize the <see cref="Readme"/> property.</param>
			/// <param name="p_xmlInfo">The value with which to initialize the <see cref="Info"/> property.</param>
			/// <param name="p_booSetScreenshot">The value with which to initialize the <see cref="SetScreenshot"/> property.</param>
			/// <param name="p_shtScreenshot">The value with which to initialize the <see cref="Screenshot"/> property.</param>
			/// <param name="p_fscScript">The value with which to initialize the <see cref="Script"/> property.</param>
			/// <param name="p_strPackedFomodPath">The value with which to initialize the <see cref="PackedFomodPath"/> property.</param>
			public BuildFomodArgs(string p_strFomodName, IList<KeyValuePair<string, string>> p_lstCopyPaths, Readme p_rmeReadme, XmlDocument p_xmlInfo, bool p_booSetScreenshot, Screenshot p_shtScreenshot, FomodScript p_fscScript, string p_strPackedFomodPath)
			{
				m_strFomodName = p_strFomodName;
				m_lstCopyInstructions = p_lstCopyPaths;
				m_rmeReadme = p_rmeReadme;
				m_xmlInfo = p_xmlInfo;
				m_booSetScreenshot = p_booSetScreenshot;
				m_shtScreenshot = p_shtScreenshot;
				m_fscScript = p_fscScript;
				m_strPackedFomodPath = p_strPackedFomodPath;
			}

			#endregion
		}

		private BackgroundWorkerProgressDialog m_bwdProgress = null;
		private Dictionary<string, string> m_dicSources = new Dictionary<string, string>();
		private string m_strTempFomodFolder = null;

		/// <summary>
		/// Determines if the file specified by the given fomod path exists. If so, it
		/// prompts the user if they would like to use a different path.
		/// </summary>
		/// <param name="newpath">The path for which it is to be detemined if a file exists.
		/// It is updated with the new path if the given path was already in use.</param>
		/// <returns><lang cref="true"/> if the returned value of <see cref="newpath"/> does not
		/// specify an existing file; <lang cref="false"/> otherwise.</returns>
		public static bool CheckFomodName(ref string newpath)
		{
			if (File.Exists(newpath))
			{
				string newpath2 = null;
				bool match = false;
				for (int i = 2; i < 999; i++)
				{
					newpath2 = Path.ChangeExtension(newpath, null) + "(" + i + ").fomod";
					if (!File.Exists(newpath2))
					{
						match = true;
						break;
					}
				}
				if (!match)
				{
					MessageBox.Show("File '" + newpath + "' already exists.", "Error");
					return false;
				}
				if (MessageBox.Show("File '" + newpath + "' already exists. Continue anyway?\nA new file named '" + newpath2 + "' will be created", "Warning", MessageBoxButtons.YesNo) != DialogResult.Yes)
					return false;
				newpath = newpath2;
			}
			return true;
		}

		#region Build Fomod

		/// <summary>
		/// Builds a fomod using the given information.
		/// </summary>
		/// <remarks>
		/// This method uses a <see cref="BackgroundWorkerProgressDialog"/> to display progress and
		/// allow cancellation.
		/// </remarks>
		/// <param name="p_strFileName">The name of the fomod file, excluding extension.</param>
		/// <param name="p_lstCopyInstructions">The list of files to copy into the fomod.</param>
		/// <param name="p_rmeReadme">The fomod readme.</param>
		/// <param name="p_xmlInfo">The fomod info file.</param>
		/// <param name="p_booSetScreenshot">Whether or not to set the fomod's screenshot.</param>
		/// <param name="p_shtScreenshot">The fomod screenshot.</param>
		/// <param name="p_fscScript">The fomod install script.</param>
		/// <returns><lang cref="true"/> if the fomod was successfully built;
		/// <lang cref="false"/> otherwise.</returns>
		public bool BuildFomod(string p_strFileName, IList<KeyValuePair<string, string>> p_lstCopyInstructions, Readme p_rmeReadme, XmlDocument p_xmlInfo, bool p_booSetScreenshot, Screenshot p_shtScreenshot, FomodScript p_fscScript)
		{
			string strFomodPath = Path.Combine(Program.PackageDir, p_strFileName + ".fomod");
			if (!CheckFomodName(ref strFomodPath))
				return false;

			bool booSucceeded = true;
			try
			{
				using (m_bwdProgress = new BackgroundWorkerProgressDialog(GenerateFomod))
				{
					m_bwdProgress.OverallMessage = "Building Fomod...";
					m_bwdProgress.ShowItemProgress = true;
					m_bwdProgress.OverallProgressStep = 1;
					m_bwdProgress.WorkMethodArguments = new BuildFomodArgs(p_strFileName,
																	p_lstCopyInstructions,
																	p_rmeReadme,
																	p_xmlInfo,
																	p_booSetScreenshot,
																	p_shtScreenshot,
																	p_fscScript,
																	strFomodPath
																	);
					if (m_bwdProgress.ShowDialog() == DialogResult.Cancel)
					{
						booSucceeded = false;
						FileUtil.ForceDelete(strFomodPath);
					}
				}
			}
			finally
			{
				foreach (string strFolder in m_dicSources.Values)
					FileUtil.ForceDelete(strFolder);
				FileUtil.ForceDelete(m_strTempFomodFolder);
			}
			return booSucceeded;
		}

		/// <summary>
		/// This builds the fomod based on the given data.
		/// </summary>
		/// <remarks>
		/// This method is called by a <see cref="BackgroundWorkerProgressDialog"/>.
		/// </remarks>
		/// <param name="p_objArgs">A <see cref="BuildFomodArgs"/> describing the fomod to build.</param>
		protected void GenerateFomod(object p_objArgs)
		{
			BuildFomodArgs bfaArgs = p_objArgs as BuildFomodArgs;
			if (bfaArgs == null)
				throw new ArgumentException("The given argument must be a BuildFomodArgs.", "p_objArgs");
			
			/**
			 * 1) Create tmp dirs for source extraction
			 * 2) Extract sources
			 * 3) Create dest fomod dir
			 * 4) Copy sources to dest fomod dir
			 * 5) Create readme
			 * 6) Create info.xml
			 * 7) Create screenshot
			 * 8) Create script
			 * 9) Pack fomod
			 * 
			 * Total steps	= 1 + (# sources to extract) + 1 + (# of copies needed) + 1 + 1 + 1 + 1 + 1
			 *				= 7 + (# sources to extract) + (# of copies needed)
			 */
			Int32 intBaseStepCount = 7;

			// 1) Create tmp dirs for source extraction
			CreateExtractionDirectories(bfaArgs.CopyInstructions);
			m_bwdProgress.OverallProgressMaximum = intBaseStepCount + m_dicSources.Count + bfaArgs.CopyInstructions.Count;
			if (m_bwdProgress.Cancelled())
				return;
			m_bwdProgress.StepOverallProgress();

			// 2) Extract sources
			foreach (KeyValuePair<string, string> kvpArchive in m_dicSources)
			{
				UnpackArchive(kvpArchive.Key, kvpArchive.Value);
				if (m_bwdProgress.Cancelled())
					return;
				m_bwdProgress.StepOverallProgress();
			}

			// 3) Create dest fomod dir
			m_strTempFomodFolder = CreateFomodDirectory();
			string strTempFomodFomodFolder = Path.Combine(m_strTempFomodFolder, "fomod");
			if (m_bwdProgress.Cancelled())
				return;
			m_bwdProgress.StepOverallProgress();

			// 4) Copy sources to dest fomod dir
			foreach (KeyValuePair<string, string> kvpCopyInstruction in bfaArgs.CopyInstructions)
			{
				CopyFiles(m_strTempFomodFolder, kvpCopyInstruction);
				if (m_bwdProgress.Cancelled())
					return;
				m_bwdProgress.StepOverallProgress();
			}
			if (!Directory.Exists(strTempFomodFomodFolder))
				Directory.CreateDirectory(strTempFomodFomodFolder);

			// 5) Create readme
			CreateReadmeFile(m_strTempFomodFolder, bfaArgs.FomodName, bfaArgs.Readme);
			if (m_bwdProgress.Cancelled())
				return;
			m_bwdProgress.StepOverallProgress();

			// 6) Create info.xml
			CreateInfoFile(strTempFomodFomodFolder, bfaArgs.InfoFile);
			if (m_bwdProgress.Cancelled())
				return;
			m_bwdProgress.StepOverallProgress();

			// 7) Create screenshot
			CreateScreenshot(strTempFomodFomodFolder, bfaArgs.SetScreenshot, bfaArgs.Screenshot);
			if (m_bwdProgress.Cancelled())
				return;
			m_bwdProgress.StepOverallProgress();

			// 8) Create script
			CreateScriptFile(strTempFomodFomodFolder, bfaArgs.Script);
			if (m_bwdProgress.Cancelled())
				return;
			m_bwdProgress.StepOverallProgress();

			// 9) Pack fomod
			PackFomod(m_strTempFomodFolder, bfaArgs.PackedFomodPath);
			m_bwdProgress.StepOverallProgress();
		}

		#endregion

		#region Helper Methods

		/// <summary>
		/// Creates a fomod file at the given path from the given directory.
		/// </summary>
		/// <param name="p_strFomodFolder">The folder from which to create the fomod.</param>
		/// <param name="p_strPackedFomodPath">The path of the new fomod file to create.</param>
		protected void PackFomod(string p_strFomodFolder, string p_strPackedFomodPath)
		{
			m_bwdProgress.ItemProgress = 0;
			m_bwdProgress.ItemProgressMaximum = Directory.GetFiles(p_strFomodFolder, "*", SearchOption.AllDirectories).Length;
			m_bwdProgress.ItemProgressStep = 1;
			m_bwdProgress.ItemMessage = String.Format("Compressing FOMOD...");

			SevenZipCompressor szcCompressor = new SevenZipCompressor();
			szcCompressor.CompressionLevel = (CompressionLevel)Settings.GetInt("fomodCompressionLevel", (Int32)CompressionLevel.Ultra);
			szcCompressor.ArchiveFormat = (OutArchiveFormat)Settings.GetInt("fomodCompressionFormat", (Int32)OutArchiveFormat.Zip);
			szcCompressor.CompressionMethod = CompressionMethod.Default;
			szcCompressor.FileCompressionStarted += new EventHandler<FileNameEventArgs>(FileCompressionStarted);
			szcCompressor.FileCompressionFinished += new EventHandler(FileCompressionFinished);
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
			m_bwdProgress.ItemProgress = 0;
			m_bwdProgress.ItemProgressMaximum = 1;
			m_bwdProgress.ItemProgressStep = 1;
			m_bwdProgress.ItemMessage = String.Format("Creating Script File...");
			if (!String.IsNullOrEmpty(p_fscScript.Text))
				File.WriteAllText(Path.Combine(p_strFomodFomodFolder, p_fscScript.FileName), p_fscScript.Text);
			m_bwdProgress.StepItemProgress();
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
			m_bwdProgress.ItemProgress = 0;
			m_bwdProgress.ItemProgressMaximum = 1;
			m_bwdProgress.ItemProgressStep = 1;
			m_bwdProgress.ItemMessage = String.Format("Creating Screenshot...");
			if (p_booSetScreenshot)
			{
				string[] strScreenshots = Directory.GetFiles(p_strFomodFomodFolder, "screenshot.*", SearchOption.TopDirectoryOnly);
				foreach (String strScreenshot in strScreenshots)
					FileUtil.ForceDelete(strScreenshot);
				if (p_shtScreenshot != null)
					File.WriteAllBytes(Path.Combine(p_strFomodFomodFolder, "screenshot" + p_shtScreenshot.Extension), p_shtScreenshot.Data);
			}
			m_bwdProgress.StepItemProgress();
		}

		/// <summary>
		/// This writes the given fomod info file in the specified directory.
		/// </summary>
		/// <param name="p_strFomodFomodFolder">The folder in which to create the fomod info file.</param>
		/// <param name="p_xmlInfo">The file to write.</param>
		protected void CreateInfoFile(string p_strFomodFomodFolder, XmlDocument p_xmlInfo)
		{
			m_bwdProgress.ItemProgress = 0;
			m_bwdProgress.ItemProgressMaximum = 1;
			m_bwdProgress.ItemProgressStep = 1;
			m_bwdProgress.ItemMessage = String.Format("Creating Info File...");
			if (p_xmlInfo != null)
				p_xmlInfo.Save(Path.Combine(p_strFomodFomodFolder, "info.xml"));
			m_bwdProgress.StepItemProgress();
		}

		/// <summary>
		/// This creates a readme file in the specified folder using the given <see cref="Readme"/>
		/// metadata.
		/// </summary>
		/// <param name="p_strFomodFomodFolder">The folder in which to create the readme file.</param>
		/// <param name="p_rmeReadme">The metadata to use to create the file.</param>
		protected void CreateReadmeFile(string p_strFomodFolder, string p_strFomodName, Readme p_rmeReadme)
		{
			m_bwdProgress.ItemProgress = 0;
			m_bwdProgress.ItemProgressMaximum = 1;
			m_bwdProgress.ItemProgressStep = 1;
			m_bwdProgress.ItemMessage = String.Format("Creating Readme File...");
			if (!String.IsNullOrEmpty(p_rmeReadme.Text))
			{
				string strReadmeFileName = String.Format("Readme - {0}{1}", p_strFomodName, p_rmeReadme.Extension);
				File.WriteAllText(Path.Combine(p_strFomodFolder, strReadmeFileName), p_rmeReadme.Text);
			}
			m_bwdProgress.StepItemProgress();
		}

		/// <summary>
		/// This executes the given copy instruction, using the specified path as the destination.
		/// </summary>
		/// <param name="p_strFomodFolder">The destination folder for the copy instruction.</param>
		/// <param name="p_kvpCopyPath">The copy instruction to execute.</param>
		protected void CopyFiles(string p_strFomodFolder, KeyValuePair<string, string> p_kvpCopyInstruction)
		{
			m_bwdProgress.ItemProgress = 0;
			m_bwdProgress.ItemProgressStep = 1;

			string strSource = p_kvpCopyInstruction.Key;
			if (strSource.StartsWith(Archive.ARCHIVE_PREFIX))
			{
				KeyValuePair<string, string> kvpArchive = Archive.ParseArchive(strSource);
				strSource = Path.Combine(m_dicSources[kvpArchive.Key], kvpArchive.Value);
			}
			m_bwdProgress.ItemMessage = String.Format("Copying Source Files {0}...", Path.GetFileName(strSource));
			m_bwdProgress.ItemProgressMaximum = File.Exists(strSource) ? 1 : Directory.GetFiles(strSource, "*", SearchOption.AllDirectories).Length;

			string strDestination = Path.Combine(p_strFomodFolder, p_kvpCopyInstruction.Value);
			FileUtil.Copy(strSource, strDestination, FileCopied);
		}

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
			m_bwdProgress.StepItemProgress();
			return m_bwdProgress.Cancelled();
		}

		/// <summary>
		/// Creates the temporary folder in which the fomod is to be built.
		/// </summary>
		/// <returns>The temporary folder in which the fomod is to be built.</returns>
		protected string CreateFomodDirectory()
		{
			m_bwdProgress.ItemProgress = 0;
			m_bwdProgress.ItemProgressMaximum = 1;
			m_bwdProgress.ItemProgressStep = 1;
			m_bwdProgress.ItemMessage = String.Format("Creating Temporary Folders...");
			string strPath = Program.CreateTempDirectory();
			m_bwdProgress.StepItemProgress();
			return strPath;
		}

		/// <summary>
		/// Creates the temporary directories to which the source archives will be extracted.
		/// </summary>
		/// <remarks>
		/// This method looks through the copy instructions needed to be executed to build the fomod and creates
		/// a temporary directory for every unique archive listed as a source.
		/// </remarks>
		/// <param name="p_lstCopyPaths">The list of copy instructions needed to be executed to build the fomod.</param>
		protected void CreateExtractionDirectories(IList<KeyValuePair<string, string>> p_lstCopyPaths)
		{
			m_bwdProgress.ItemProgress = 0;
			m_bwdProgress.ItemProgressMaximum = p_lstCopyPaths.Count;
			m_bwdProgress.ItemProgressStep = 1;
			m_bwdProgress.ItemMessage = String.Format("Creating temporary folders...");

			m_dicSources.Clear();
			foreach (KeyValuePair<string, string> kvpCopyPath in p_lstCopyPaths)
			{
				if (kvpCopyPath.Key.StartsWith(Archive.ARCHIVE_PREFIX))
				{
					KeyValuePair<string, string> kvpArchive = Archive.ParseArchive(kvpCopyPath.Key);
					if (!m_dicSources.ContainsKey(kvpArchive.Key))
						m_dicSources[kvpArchive.Key] = Program.CreateTempDirectory();
				}
				m_bwdProgress.StepItemProgress();
			}
		}

		/// <summary>
		/// This unpacks the specified archive to the specified path.
		/// </summary>
		/// <param name="p_strArchivePath">The path to the archive to unpack.</param>
		/// <param name="p_strExtractionPath">The path to the directory to which to unpack the archive.</param>
		protected void UnpackArchive(string p_strArchivePath, string p_strExtractionPath)
		{
			SevenZipExtractor szeExtractor = new SevenZipExtractor(p_strArchivePath);
			szeExtractor.FileExtractionFinished += new EventHandler(FileExtractionFinished);
			szeExtractor.FileExtractionStarted += new EventHandler<FileInfoEventArgs>(FileExtractionStarted);
			m_bwdProgress.ItemProgress = 0;
			m_bwdProgress.ItemProgressMaximum = (Int32)szeExtractor.FilesCount;
			m_bwdProgress.ItemProgressStep = 1;
			m_bwdProgress.ItemMessage = String.Format("Extracting Source Files ({0})...", Path.GetFileName(p_strArchivePath));
			szeExtractor.ExtractArchive(p_strExtractionPath);
		}

		/// <summary>
		/// Called when a file has been extracted from a source archive.
		/// </summary>
		/// <remarks>
		/// This steps the progress of the create fomod from archive progress dialog.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void FileExtractionFinished(object sender, EventArgs e)
		{
			m_bwdProgress.StepItemProgress();
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
			e.Cancel = m_bwdProgress.Cancelled();
		}

		/// <summary>
		/// Called when a file has been added to a new fomod.
		/// </summary>
		/// <remarks>
		/// This steps the progress of the create fomod from folder progress dialog.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void FileCompressionFinished(object sender, EventArgs e)
		{
			m_bwdProgress.StepOverallProgress();
		}

		/// <summary>
		/// Called when a file is about to be added to a new fomod.
		/// </summary>
		/// <remarks>
		/// This cancels the compression if the user has clicked the cancel button of the progress dialog.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="FileNameEventArgs"/> describing the event arguments.</param>
		private void FileCompressionStarted(object sender, FileNameEventArgs e)
		{
			e.Cancel = m_bwdProgress.Cancelled();
		}

		#endregion
	}
}
