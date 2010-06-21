using System;
using System.Collections.Generic;
using System.Xml;
using SevenZip;
using System.IO;
using System.Windows.Forms;
using Fomm.Util;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

namespace Fomm.PackageManager.FomodBuilder
{
	/// <summary>
	/// This class builds fomods and premade fomod packs.
	/// </summary>
	public class FomodFromSourceBuilder : FomodGenerator
	{
		/// <summary>
		/// The arguments object to pass to the background worker when building a fomod.
		/// </summary>
		private class BuildFomodArgs
		{
			private string m_strFomodName = null;
			private string m_strSource = null;
			private string m_strPackedFomodPath = null;
			private string m_strUrl = null;

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
			/// Gets the source folder from which to make the fomod.
			/// </summary>
			/// <value>The source folder from which to make the fomod.</value>
			public string SourcePath
			{
				get
				{
					return m_strSource;
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

			/// <summary>
			/// Gets the URL of the mod's website.
			/// </summary>
			/// <value>The URL of the mod's website.</value>
			public string Url
			{
				get
				{
					return m_strUrl;
				}
			}

			#endregion

			#region Constructors

			/// <summary>
			/// A simple constructor that initializes the object with the given values.
			/// </summary>
			/// <param name="p_strFomodName">The value with which to initialize the <see cref="FomodName"/> property.</param>
			/// <param name="p_strSourcePath">The value with which to initialize the <see cref="SourcePath"/> property.</param>
			/// <param name="p_strUrl">The value with which to initialize the <see cref="Url"/> property.</param>
			/// <param name="p_strPackedFomodPath">The value with which to initialize the <see cref="PackedFomodPath"/> property.</param>
			public BuildFomodArgs(string p_strFomodName, string p_strSourcePath, string p_strUrl, string p_strPackedFomodPath)
			{
				m_strFomodName = p_strFomodName;
				m_strSource = p_strSourcePath;
				m_strPackedFomodPath = p_strPackedFomodPath;
				m_strUrl = p_strUrl;
			}

			#endregion
		}

		#region Build Fomod From Source

		/// <summary>
		/// Creates a fomod from a source.
		/// </summary>
		/// <remarks>
		/// The source can be a folder or an archive.
		/// </remarks>
		/// <param name="p_strPath">The path to the source from which to create the fomod.</param>
		/// <returns>The path to the new fomod if it was successfully built; <lang cref="null"/> otherwise.</returns>
		public string BuildFomodFromSource(string p_strPath)
		{
			string strSource = p_strPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string strFomodName = Path.GetFileName(strSource);

			bool booRepack = false;
			string strTesNexusUrl = null;
			string strPackedFomodPath = null;
			if (File.Exists(strSource))
			{
				if (strSource.EndsWith(".fomod", StringComparison.OrdinalIgnoreCase))
					strPackedFomodPath = Path.Combine(Program.PackageDir, Path.GetFileName(strSource));
				else if (strSource.EndsWith(".fomod.zip", StringComparison.OrdinalIgnoreCase))
					strPackedFomodPath = Path.Combine(Program.PackageDir, Path.GetFileNameWithoutExtension(strSource));
				else if (strSource.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) && strSource.Contains("-fomod-"))
				{
					string strFileName = Path.GetFileName(strSource);
					strPackedFomodPath = Path.Combine(Program.PackageDir, Path.GetFileName(strFileName.Substring(0, strFileName.IndexOf("-fomod-")))) + ".fomod";
				}
				else
				{
					strPackedFomodPath = Path.Combine(Program.PackageDir, Path.ChangeExtension(Path.GetFileName(strSource), ".fomod"));
					booRepack = true;
				}

				strTesNexusUrl = Path.GetFileNameWithoutExtension(strPackedFomodPath);
				Int32 intFileId;
				if (strTesNexusUrl.Contains("-") && int.TryParse(strTesNexusUrl.Substring(strTesNexusUrl.LastIndexOf('-') + 1), out intFileId))
				{
					strPackedFomodPath = Path.Combine(Path.GetDirectoryName(strPackedFomodPath), strTesNexusUrl.Remove(strTesNexusUrl.LastIndexOf('-'))) + Path.GetExtension(strPackedFomodPath);
					strTesNexusUrl = @"http://www.fallout3nexus.com/downloads/file.php?id=" + intFileId;
				}
				else
					strTesNexusUrl = null;
			}
			else
			{
				booRepack = true;
				strPackedFomodPath = Path.Combine(Program.PackageDir, strFomodName + ".fomod");
			}

			if (!booRepack)
			{
				if (MessageBox.Show("Make a copy of the original file?", "", MessageBoxButtons.YesNo) != DialogResult.Yes)
					File.Move(strSource, strPackedFomodPath);
				else
					File.Copy(strSource, strPackedFomodPath);
			}
			else
				strPackedFomodPath = GenerateFomod(strPackedFomodPath, new BuildFomodArgs(strFomodName, strSource, strTesNexusUrl, strPackedFomodPath));
			return strPackedFomodPath;
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
			BuildFomodArgs bfaArgs = p_objArgs as BuildFomodArgs;
			if (bfaArgs == null)
				throw new ArgumentException("The given argument must be a BuildFomodArgs.", "p_objArgs");

			string strSource = bfaArgs.SourcePath;

			/**
			 * 1) Unpack source, if required
			 * 2) Delete unwanted files
			 * 3) Remove extraneous top-level folders
			 * 4) Create readme
			 * 5) Create info file
			 * 6) Pack fomod
			 * 
			 * Total steps	= 1 + 1 + 1 + 1 + 1 + 1
			 *				= 6
			 */
			ProgressDialog.OverallProgressMaximum = 6;

			// 1) Unpack source, if required
			if (File.Exists(bfaArgs.SourcePath))
			{
				strSource = CreateTemporaryDirectory();
				UnpackArchive(bfaArgs.SourcePath, strSource);
				if (ProgressDialog.Cancelled())
					return;
				ProgressDialog.StepOverallProgress();
			}

			// 2) Delete unwanted files
			DeleteUnwantedFiles(strSource);
			if (ProgressDialog.Cancelled())
				return;
			ProgressDialog.StepOverallProgress();

			// 3) Remove extraneous top-level folders
			strSource = DescendToFomodFolder(strSource);
			if (ProgressDialog.Cancelled())
				return;
			ProgressDialog.StepOverallProgress();

			//warn if script is required but missing
			if (Program.GetFiles(strSource, "*.esp", SearchOption.AllDirectories).Length + Program.GetFiles(strSource, "*.esm", SearchOption.AllDirectories).Length >
					Program.GetFiles(strSource, "*.esp", SearchOption.TopDirectoryOnly).Length + Program.GetFiles(strSource, "*.esm", SearchOption.TopDirectoryOnly).Length)
			{
				bool booHasScript = false;
				foreach (string strScriptName in FomodScript.ScriptNames)
					if (File.Exists(Path.Combine(strSource, "fomod\\" + strScriptName)))
					{
						booHasScript = true;
						break;
					}
				if (!booHasScript &&
					(MessageBox.Show("This archive contains plugins in subdirectories, and will need a script attached for fomm to install it correctly.", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel))
					return;
			}

			// 4) Create readme
			string[] strReadmes = Directory.GetFiles(strSource, "readme - " + bfaArgs.FomodName + ".*", SearchOption.TopDirectoryOnly);
			if (strReadmes.Length == 0)
			{
				strReadmes = Directory.GetFiles(strSource, "*readme*.*", SearchOption.AllDirectories);
				foreach (string strExtension in Readme.ValidExtensions)
				{
					if (strReadmes.Length > 0)
						break;
					strReadmes = Program.GetFiles(strSource, "*" + strExtension, SearchOption.AllDirectories);
				}
				Readme rmeReadme = null;
				foreach (string strReadme in strReadmes)
				{
					if (Readme.IsValidReadme(strReadme))
					{
						rmeReadme = new Readme(strReadme, File.ReadAllText(strReadme));
						break;
					}
				}
				CreateReadmeFile(strSource, bfaArgs.FomodName, rmeReadme);
			}
			if (ProgressDialog.Cancelled())
				return;
			ProgressDialog.StepOverallProgress();

			// 5) Create info.xml
			if (!String.IsNullOrEmpty(bfaArgs.Url))
			{
				string strFomodFomodPath = Path.Combine(strSource, "fomod");
				if (!Directory.Exists(strFomodFomodPath))
					Directory.CreateDirectory(strFomodFomodPath);
				if (!File.Exists(Path.Combine(strFomodFomodPath, "info.xml")))
				{
					XmlDocument xmlInfo = new XmlDocument();
					xmlInfo.AppendChild(xmlInfo.CreateXmlDeclaration("1.0", "UTF-16", null));
					XmlNode xndRoot = xmlInfo.AppendChild(xmlInfo.CreateElement("fomod"));
					XmlNode xndWebsite = xndRoot.AppendChild(xmlInfo.CreateElement("Website"));
					xndWebsite.InnerText = bfaArgs.Url;
					CreateInfoFile(strFomodFomodPath, xmlInfo);
				}
			}			
			if (ProgressDialog.Cancelled())
				return;
			ProgressDialog.StepOverallProgress();

			// 6) Pack fomod
			PackFomod(strSource, bfaArgs.PackedFomodPath);
			ProgressDialog.StepOverallProgress();
		}

		#endregion

		#region Helper Methods

		/// <summary>
		/// This method removes any extraneous files from the source.
		/// </summary>
		/// <remarks>
		/// This removes OS-specific metadat files, such as desktop.ini and thumbs.db.
		/// </remarks>
		/// <param name="p_strSourcePath">The path from which to removed extraneous files.</param>
		protected void DeleteUnwantedFiles(string p_strSourcePath)
		{
			List<string> lstUnwantedFiles = new List<string>();
			lstUnwantedFiles.AddRange(Program.GetFiles(p_strSourcePath, "ArchiveInvalidation.txt", SearchOption.AllDirectories));
			lstUnwantedFiles.AddRange(Program.GetFiles(p_strSourcePath, "thumbs.db", SearchOption.AllDirectories));
			lstUnwantedFiles.AddRange(Program.GetFiles(p_strSourcePath, "desktop.ini", SearchOption.AllDirectories));

			ProgressDialog.ItemProgress = 0;
			ProgressDialog.ItemProgressMaximum = lstUnwantedFiles.Count;
			ProgressDialog.ItemProgressStep = 1;
			ProgressDialog.ItemMessage = String.Format("Deleting Unwanted Files...");
			foreach (string strFile in lstUnwantedFiles)
			{
				FileUtil.ForceDelete(strFile);
				ProgressDialog.StepItemProgress();
			}
		}

		/// <summary>
		/// This descends through the file structure until a FOMOD structure is located.
		/// </summary>
		/// <remarks>
		/// This bypasses any top-level container folders. For example, a mod may be in an archive Mod.zip.
		/// That archive, instead of directly containing the fomod directory and files, will mave a top-level
		/// 'Mod' folder that contains the fomod files. The method will bypasses that folder.
		/// </remarks>
		/// <param name="p_strSourcePath">The directory through which to seach for the fomod files.</param>
		/// <returns>The path to the fomod files.</returns>
		protected string DescendToFomodFolder(string p_strSourcePath)
		{
			ProgressDialog.ItemProgress = 0;
			ProgressDialog.ItemProgressMaximum = 50;
			ProgressDialog.ItemProgressStep = 1;
			ProgressDialog.ItemMessage = String.Format("Finding FOMOD Folder...");

			string strSourcePath = p_strSourcePath;
			//this code removes any top-level folders until it finds esp/esm/bsa, or the top-level folder
			// is a fomod/textures/meshes/music/shaders/video/facegen/menus/lodsettings/lsdata/sound folder.
			string[] directories = Directory.GetDirectories(strSourcePath);
			while (directories.Length == 1 &&
					Program.GetFiles(strSourcePath, "*.esp").Length == 0 &&
					Program.GetFiles(strSourcePath, "*.esm").Length == 0 &&
					Program.GetFiles(strSourcePath, "*.bsa").Length == 0)
			{
				directories = directories[0].Split(Path.DirectorySeparatorChar);
				string name = directories[directories.Length - 1].ToLowerInvariant();
				if ((name != "fomod") && (name != "textures") && (name != "meshes") && (name != "music") &&
					(name != "shaders") && (name != "video") && (name != "facegen") && (name != "menus") &&
					(name != "lodsettings") && (name != "lsdata") && (name != "sound"))
				{
					foreach (string file in Directory.GetFiles(strSourcePath))
					{
						string newpath2 = Path.Combine(Path.Combine(Path.GetDirectoryName(file), name), Path.GetFileName(file));
						if (!File.Exists(newpath2))
							File.Move(file, newpath2);
					}
					strSourcePath = Path.Combine(strSourcePath, name);
					directories = Directory.GetDirectories(strSourcePath);
				}
				else
					break;
				ProgressDialog.StepItemProgress();
			}
			return strSourcePath;
		}

		#endregion
	}
}
