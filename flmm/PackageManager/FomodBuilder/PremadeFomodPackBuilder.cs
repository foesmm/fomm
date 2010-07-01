using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using SevenZip;

namespace Fomm.PackageManager.FomodBuilder
{
	/// <summary>
	/// This class builds premade fomod packs (PFPs).
	/// </summary>
	public class PremadeFomodPackBuilder : NewFomodBuilder
	{
		/// <summary>
		/// The arguments object to pass to the background worker when building a PFP.
		/// </summary>
		protected class BuildPFPArgs : BuildFomodArgs
		{
			private IDictionary<string, string> m_dicDownloadLocations = null;

			#region Properties

			/// <summary>
			/// Gets the download locations of the fomod sources.
			/// </summary>
			/// <value>The download locations of the fomod sources.</value>
			public IDictionary<string, string> DownloadLocations
			{
				get
				{
					return m_dicDownloadLocations;
				}
			}

			#endregion

			#region Constructors

			/// <summary>
			/// A simple constructor that initializes the object with the given values.
			/// </summary>
			/// <param name="p_strFomodName">The value with which to initialize the <see cref="FomodName"/> property.</param>
			/// <param name="p_lstCopyInstructions">The value with which to initialize the <see cref="CopyInstructions"/> property.</param>
			/// <param name="p_dicDownloadLocations">The value with which to initialize the <see cref="DownloadLocations"/> property.</param>
			/// <param name="p_rmeReadme">The value with which to initialize the <see cref="Readme"/> property.</param>
			/// <param name="p_xmlInfo">The value with which to initialize the <see cref="Info"/> property.</param>
			/// <param name="p_booSetScreenshot">The value with which to initialize the <see cref="SetScreenshot"/> property.</param>
			/// <param name="p_shtScreenshot">The value with which to initialize the <see cref="Screenshot"/> property.</param>
			/// <param name="p_fscScript">The value with which to initialize the <see cref="Script"/> property.</param>
			/// <param name="p_strPackedPath">The value with which to initialize the <see cref="PackedPath"/> property.</param>
			public BuildPFPArgs(string p_strFomodName, IList<KeyValuePair<string, string>> p_lstCopyInstructions, IDictionary<string, string> p_dicDownloadLocations, Readme p_rmeReadme, XmlDocument p_xmlInfo, bool p_booSetScreenshot, Screenshot p_shtScreenshot, FomodScript p_fscScript, string p_strPackedPath)
				: base(p_strFomodName, p_lstCopyInstructions, p_rmeReadme, p_xmlInfo, p_booSetScreenshot, p_shtScreenshot, p_fscScript, p_strPackedPath)
			{
				m_dicDownloadLocations = p_dicDownloadLocations;
			}

			#endregion
		}

		#region Properties

		protected override string OverallProgressMessage
		{
			get
			{
				return "Building Premade Fomod Pack...";
			}
		}

		#endregion

		#region Build Fomod

		/// <summary>
		/// Builds a premade fomod pack using the given information.
		/// </summary>
		/// <remarks>
		/// This method uses a <see cref="BackgroundWorkerProgressDialog"/> to display progress and
		/// allow cancellation.
		/// </remarks>
		/// <param name="p_strFileName">The name of the fomod file, excluding extension.</param>
		/// <param name="p_strVersion">The version of the fomod for which we are creating the PFP.</param>
		/// <param name="p_lstCopyInstructions">The list of files to copy into the fomod.</param>
		/// <param name="p_dicDownloadLocations">The list of download locations for the sources.</param>
		/// <param name="p_rmeReadme">The fomod readme.</param>
		/// <param name="p_xmlInfo">The fomod info file.</param>
		/// <param name="p_booSetScreenshot">Whether or not to set the fomod's screenshot.</param>
		/// <param name="p_shtScreenshot">The fomod screenshot.</param>
		/// <param name="p_fscScript">The fomod install script.</param>
		/// <param name="p_strPFPPath">The path where the Premade Fomod Pack will be created.</param>
		/// <returns>The path to the new premade fomod pack if it was successfully built; <lang cref="null"/> otherwise.</returns>
		public string BuildPFP(string p_strFileName, string p_strVersion, IList<KeyValuePair<string, string>> p_lstCopyInstructions, IDictionary<string, string> p_dicDownloadLocations, Readme p_rmeReadme, XmlDocument p_xmlInfo, bool p_booSetScreenshot, Screenshot p_shtScreenshot, FomodScript p_fscScript, string p_strPFPPath)
		{
			string strPFPExtension = null;
			switch ((OutArchiveFormat)Settings.GetInt("pfpCompressionFormat", (Int32)OutArchiveFormat.SevenZip))
			{
				case OutArchiveFormat.BZip2:
					strPFPExtension = ".bz2";
					break;
				case OutArchiveFormat.GZip:
					strPFPExtension = ".gz";
					break;
				case OutArchiveFormat.SevenZip:
					strPFPExtension = ".7z";
					break;
				case OutArchiveFormat.Tar:
					strPFPExtension = ".tar";
					break;
				case OutArchiveFormat.XZ:
					strPFPExtension = ".xz";
					break;
				case OutArchiveFormat.Zip:
					strPFPExtension = ".zip";
					break;
				default:
					throw new Exception("Unrecognized value for OutArchiveFormat enum.");
			}				
			string strPFPPath = Path.Combine(p_strPFPPath, String.Format("{0} {1}{2}", p_strFileName, p_strVersion, strPFPExtension));
			strPFPPath = GenerateFomod(new BuildPFPArgs(p_strFileName,
																p_lstCopyInstructions,
																p_dicDownloadLocations,
																p_rmeReadme,
																p_xmlInfo,
																p_booSetScreenshot,
																p_shtScreenshot,
																p_fscScript,
																strPFPPath
																));
			return strPFPPath;
		}

		/// <summary>
		/// This builds the fomod based on the given data.
		/// </summary>
		/// <remarks>
		/// This method is called by a <see cref="BackgroundWorkerProgressDialog"/>.
		/// </remarks>
		/// <param name="p_objArgs">A <see cref="BuildFomodArgs"/> describing the fomod to build.</param>
		protected override void DoGenerateFomod(object p_objArgs)
		{
			BuildPFPArgs bpaArgs = p_objArgs as BuildPFPArgs;
			if (bpaArgs == null)
				throw new ArgumentException("The given argument must be a BuildPFPArgs.", "p_objArgs");

			/**
			 * 1) Create tmp dirs for extraction of sources that will be stored in PFP
			 * 2) Extract sources
			 * 3) Create dest PFP dir
			 * 4) Copy sources to dest PFP dir
			 * 5) Create metadata.xml
			 * 6) Create readme
			 * 7) Create info.xml
			 * 8) Create screenshot
			 * 9) Create script
			 * 10) Pack PFP
			 * 
			 * Total steps	= 1 + (# sources to extract) + 1 + (# of copies needed) + 1 + 1 + 1 + 1 + 1 + 1
			 *				= 8 + (# sources to extract) + (# of copies needed)
			 */
			Int32 intBaseStepCount = 8;

			//get only the instructions for files that are to be included in the PFP
			// files will be included in the PFP only if they don't have a download location
			List<KeyValuePair<string, string>> lstPFPCopyInstructions = new List<KeyValuePair<string, string>>();
			List<KeyValuePair<string, string>> lstFomodCopyInstructions = new List<KeyValuePair<string, string>>();
			bool booIsPFPCopy = true;
			foreach (KeyValuePair<string, string> kvpInstruction in bpaArgs.CopyInstructions)
			{
				if (kvpInstruction.Key.StartsWith(Archive.ARCHIVE_PREFIX))
				{
					KeyValuePair<string, string> kvpArchive = Archive.ParseArchivePath(kvpInstruction.Key);
					booIsPFPCopy = (bpaArgs.DownloadLocations[kvpArchive.Key] == null);
				}
				else
				{
					booIsPFPCopy = true;
					foreach (KeyValuePair<string, string> kvpLocation in bpaArgs.DownloadLocations)
					{
						if (kvpLocation.Key.StartsWith(Archive.ARCHIVE_PREFIX))
							continue;
						if (kvpInstruction.Key.StartsWith(kvpLocation.Key))
						{
							booIsPFPCopy = (kvpLocation.Value == null);
							break;
						}
					}
				}
				if (booIsPFPCopy)
					lstPFPCopyInstructions.Add(kvpInstruction);
				else
					lstFomodCopyInstructions.Add(kvpInstruction);
			}

			// 1) Create tmp dirs for extraction of sources that will be stored in PFP
			Dictionary<string, string> dicSources = CreateExtractionDirectories(lstPFPCopyInstructions);
			ProgressDialog.OverallProgressMaximum = intBaseStepCount + dicSources.Count + lstPFPCopyInstructions.Count;
			if (ProgressDialog.Cancelled())
				return;
			ProgressDialog.StepOverallProgress();

			// 2) Extract sources
			foreach (KeyValuePair<string, string> kvpArchive in dicSources)
			{
				UnpackArchive(kvpArchive.Key, kvpArchive.Value);
				if (ProgressDialog.Cancelled())
					return;
				ProgressDialog.StepOverallProgress();
			}

			// 3) Create dest PFP dir
			string strTempPFPFolder = CreateFomodDirectory();
			string strTempPFPPremadeFolder = Path.Combine(strTempPFPFolder, "Premade " + bpaArgs.FomodName);
			string strTempPFPPremadeFomodFolder = Path.Combine(strTempPFPPremadeFolder, "fomod");
			if (!Directory.Exists(strTempPFPPremadeFolder))
				Directory.CreateDirectory(strTempPFPPremadeFolder);
			if (ProgressDialog.Cancelled())
				return;
			ProgressDialog.StepOverallProgress();

			// 4) Copy sources to dest PFP dir
			foreach (KeyValuePair<string, string> kvpCopyInstruction in lstPFPCopyInstructions)
			{
				CopyFiles(strTempPFPPremadeFolder, dicSources, kvpCopyInstruction);
				if (ProgressDialog.Cancelled())
					return;
				ProgressDialog.StepOverallProgress();
			}
			if (!Directory.Exists(strTempPFPPremadeFomodFolder))
				Directory.CreateDirectory(strTempPFPPremadeFomodFolder);

			// 5) Create metadata.xml
			CreateMetadataFile(strTempPFPFolder, bpaArgs.DownloadLocations, lstFomodCopyInstructions);
			if (ProgressDialog.Cancelled())
				return;
			ProgressDialog.StepOverallProgress();

			// 6) Create readme
			CreateReadmeFile(strTempPFPPremadeFolder, bpaArgs.FomodName, bpaArgs.Readme);
			if (ProgressDialog.Cancelled())
				return;
			ProgressDialog.StepOverallProgress();

			// 7) Create info.xml
			CreateInfoFile(strTempPFPPremadeFomodFolder, bpaArgs.InfoFile);
			if (ProgressDialog.Cancelled())
				return;
			ProgressDialog.StepOverallProgress();

			// 8) Create screenshot
			CreateScreenshot(strTempPFPPremadeFomodFolder, bpaArgs.SetScreenshot, bpaArgs.Screenshot);
			if (ProgressDialog.Cancelled())
				return;
			ProgressDialog.StepOverallProgress();

			// 9) Create script
			CreateScriptFile(strTempPFPPremadeFomodFolder, bpaArgs.Script);
			if (ProgressDialog.Cancelled())
				return;
			ProgressDialog.StepOverallProgress();

			// 10) Pack PFP
			PackPFP(strTempPFPFolder, bpaArgs.PackedPath);
			ProgressDialog.StepOverallProgress();
		}

		#endregion

		/// <summary>
		/// Generates the metadata file for the Premade Fomod Pack (PFP).
		/// </summary>
		/// <remarks>
		/// The metadata file contains the information for the automated installation of PFPs.
		/// </remarks>
		/// <param name="p_strPFPFolder">The folder in which to create the metadata file.</param>
		/// <param name="p_dicDownloadLocations">The list of source download locations.</param>
		/// <param name="p_lstCopyInstructions">The list of copy instructions to execute to create the fomod.
		/// This list should not include copy instructions for files included in the PFP.</param>
		protected void CreateMetadataFile(string p_strPFPFolder, IDictionary<string, string> p_dicDownloadLocations, IList<KeyValuePair<string, string>> p_lstCopyInstructions)
		{
			ProgressDialog.ItemProgress = 0;
			ProgressDialog.ItemProgressMaximum = p_dicDownloadLocations.Count + p_lstCopyInstructions.Count;
			ProgressDialog.ItemProgressStep = 1;
			ProgressDialog.ItemMessage = String.Format("Creating metadat file...");

			XmlDocument xmlMeta = new XmlDocument();
			XmlNode xndRoot = xmlMeta.AppendChild(xmlMeta.CreateElement("premadeFomodPack"));
			XmlNode xndSources = xndRoot.AppendChild(xmlMeta.CreateElement("sources"));
			XmlNode xndInstructions = xndRoot.AppendChild(xmlMeta.CreateElement("copyInstructions"));
			foreach (KeyValuePair<string, string> kvpSource in p_dicDownloadLocations)
			{
				if (kvpSource.Value != null)
				{
					XmlNode xndSource = xndSources.AppendChild(xmlMeta.CreateElement("source"));
					xndSource.Attributes.Append(xmlMeta.CreateAttribute("name")).Value = Path.GetFileName(kvpSource.Key);
					xndSource.Attributes.Append(xmlMeta.CreateAttribute("url")).Value = kvpSource.Value;
				}
				ProgressDialog.StepItemProgress();
				if (ProgressDialog.Cancelled())
					return;
			}

			foreach (KeyValuePair<string, string> kvpInstruction in p_lstCopyInstructions)
			{
				XmlNode xndInstruction = xndInstructions.AppendChild(xmlMeta.CreateElement("instruction"));

				if (kvpInstruction.Key.StartsWith(Archive.ARCHIVE_PREFIX))
				{
					KeyValuePair<string, string> kvpArchive = Archive.ParseArchivePath(kvpInstruction.Key);
					xndInstruction.Attributes.Append(xmlMeta.CreateAttribute("source")).Value = Archive.GenerateArchivePath(Path.GetFileName(kvpArchive.Key), kvpArchive.Value);
					xndInstruction.Attributes.Append(xmlMeta.CreateAttribute("destination")).Value = kvpInstruction.Value;
				}
				else
				{
					foreach (KeyValuePair<string, string> kvpSource in p_dicDownloadLocations)
					{
						if (kvpSource.Key.StartsWith(Archive.ARCHIVE_PREFIX))
							continue;
						if (kvpInstruction.Key.StartsWith(kvpSource.Key))
						{
							xndInstruction.Attributes.Append(xmlMeta.CreateAttribute("source")).Value = kvpInstruction.Key.Substring(kvpSource.Key.Length);
							xndInstruction.Attributes.Append(xmlMeta.CreateAttribute("destination")).Value = kvpInstruction.Value;
							break;
						}
					}
				}
				ProgressDialog.StepItemProgress();
				if (ProgressDialog.Cancelled())
					return;
			}

			xmlMeta.Save(Path.Combine(p_strPFPFolder, "metadata.xml"));
			return;
		}

		/// <summary>
		/// Creates a Premade Fomod Pack (PFP) file at the given path from the given directory.
		/// </summary>
		/// <param name="p_strPFPFolder">The folder from which to create the PFP.</param>
		/// <param name="p_strPackedPFPPath">The path of the new PFP file to create.</param>
		protected void PackPFP(string p_strPFPFolder, string p_strPackedPFPPath)
		{
			ProgressDialog.ItemProgress = 0;
			ProgressDialog.ItemProgressMaximum = Directory.GetFiles(p_strPFPFolder, "*", SearchOption.AllDirectories).Length;
			ProgressDialog.ItemProgressStep = 1;
			ProgressDialog.ItemMessage = String.Format("Compressing Premade Fomod Pack...");

			SevenZipCompressor szcCompressor = new SevenZipCompressor();
			szcCompressor.CompressionLevel = (CompressionLevel)Settings.GetInt("pfpCompressionLevel", (Int32)CompressionLevel.Ultra);
			szcCompressor.ArchiveFormat = (OutArchiveFormat)Settings.GetInt("pfpCompressionFormat", (Int32)OutArchiveFormat.SevenZip);
			szcCompressor.CompressionMethod = CompressionMethod.Default;
			szcCompressor.FileCompressionStarted += new EventHandler<FileNameEventArgs>(FileCompressionStarted);
			szcCompressor.FileCompressionFinished += new EventHandler(FileCompressionFinished);
			szcCompressor.CompressDirectory(p_strPFPFolder, p_strPackedPFPPath);
		}
	}
}
