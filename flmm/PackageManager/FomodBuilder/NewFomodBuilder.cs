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
  /// This class builds fomods.
  /// </summary>
  public class NewFomodBuilder : FomodGenerator
  {
    /// <summary>
    /// The arguments object to pass to the background worker when building a fomod.
    /// </summary>
    protected class BuildFomodArgs : GenerateFomodArgs
    {
      private string m_strFomodName = null;
      private IList<KeyValuePair<string, string>> m_lstCopyInstructions = null;
      private Readme m_rmeReadme = null;
      private XmlDocument m_xmlInfo = null;
      private bool m_booSetScreenshot = false;
      private Screenshot m_shtScreenshot = null;
      private FomodScript m_fscScript = null;

      #region Properties

      /// <summary>
      /// Gets the fomodName.
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
      /// Gets the copy instructions that need to be executed to create the fomod.
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
      /// Gets the readme.
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
      /// Gets the info file.
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
      /// Gets the setScreenshot.
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
      /// Gets the screenshot.
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
      /// Gets the script.
      /// </summary>
      /// <value>The script.</value>
      public FomodScript Script
      {
        get
        {
          return m_fscScript;
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
      /// <param name="p_strPackedPath">The value with which to initialize the <see cref="PackedPath"/> property.</param>
      public BuildFomodArgs(string p_strFomodName, IList<KeyValuePair<string, string>> p_lstCopyPaths,
                            Readme p_rmeReadme, XmlDocument p_xmlInfo, bool p_booSetScreenshot,
                            Screenshot p_shtScreenshot, FomodScript p_fscScript, string p_strPackedPath)
        : base(p_strPackedPath)
      {
        m_strFomodName = p_strFomodName;
        m_lstCopyInstructions = p_lstCopyPaths;
        m_rmeReadme = p_rmeReadme;
        m_xmlInfo = p_xmlInfo;
        m_booSetScreenshot = p_booSetScreenshot;
        m_shtScreenshot = p_shtScreenshot;
        m_fscScript = p_fscScript;
      }

      #endregion
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
    /// <returns>The path to the new fomod if it was successfully built; <lang cref="null"/> otherwise.</returns>
    public string BuildFomod(string p_strFileName, IList<KeyValuePair<string, string>> p_lstCopyInstructions,
                             Readme p_rmeReadme, XmlDocument p_xmlInfo, bool p_booSetScreenshot,
                             Screenshot p_shtScreenshot, FomodScript p_fscScript)
    {
      string strFomodPath = Path.Combine(Program.GameMode.ModDirectory, p_strFileName + ".fomod");
      strFomodPath = GenerateFomod(new BuildFomodArgs(p_strFileName,
                                                      p_lstCopyInstructions,
                                                      p_rmeReadme,
                                                      p_xmlInfo,
                                                      p_booSetScreenshot,
                                                      p_shtScreenshot,
                                                      p_fscScript,
                                                      strFomodPath
                                     ));
      return strFomodPath;
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
      {
        throw new ArgumentException("The given argument must be a BuildFomodArgs.", "p_objArgs");
      }

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
       * Total steps  = 1 + (# sources to extract) + 1 + (# of copies needed) + 1 + 1 + 1 + 1 + 1
       *        = 7 + (# sources to extract) + (# of copies needed)
       */
      Int32 intBaseStepCount = 7;

      // 1) Create tmp dirs for source extraction
      Dictionary<string, string> dicSources = CreateExtractionDirectories(bfaArgs.CopyInstructions);
      ProgressDialog.OverallProgressMaximum = intBaseStepCount + dicSources.Count + bfaArgs.CopyInstructions.Count;
      if (ProgressDialog.Cancelled())
      {
        return;
      }
      ProgressDialog.StepOverallProgress();

      // 2) Extract sources
      foreach (KeyValuePair<string, string> kvpArchive in dicSources)
      {
        UnpackArchive(kvpArchive.Key, kvpArchive.Value);
        if (ProgressDialog.Cancelled())
        {
          return;
        }
        ProgressDialog.StepOverallProgress();
      }

      // 3) Create dest fomod dir
      string strTempFomodFolder = CreateFomodDirectory();
      string strTempFomodFomodFolder = Path.Combine(strTempFomodFolder, "fomod");
      if (ProgressDialog.Cancelled())
      {
        return;
      }
      ProgressDialog.StepOverallProgress();

      // 4) Copy sources to dest fomod dir
      foreach (KeyValuePair<string, string> kvpCopyInstruction in bfaArgs.CopyInstructions)
      {
        CopyFiles(strTempFomodFolder, dicSources, kvpCopyInstruction);
        if (ProgressDialog.Cancelled())
        {
          return;
        }
        ProgressDialog.StepOverallProgress();
      }
      if (!Directory.Exists(strTempFomodFomodFolder))
      {
        Directory.CreateDirectory(strTempFomodFomodFolder);
      }

      // 5) Create readme
      CreateReadmeFile(strTempFomodFolder, bfaArgs.FomodName, bfaArgs.Readme);
      if (ProgressDialog.Cancelled())
      {
        return;
      }
      ProgressDialog.StepOverallProgress();

      // 6) Create info.xml
      CreateInfoFile(strTempFomodFomodFolder, bfaArgs.InfoFile);
      if (ProgressDialog.Cancelled())
      {
        return;
      }
      ProgressDialog.StepOverallProgress();

      // 7) Create screenshot
      CreateScreenshot(strTempFomodFomodFolder, bfaArgs.SetScreenshot, bfaArgs.Screenshot);
      if (ProgressDialog.Cancelled())
      {
        return;
      }
      ProgressDialog.StepOverallProgress();

      // 8) Create script
      CreateScriptFile(strTempFomodFomodFolder, bfaArgs.Script);
      if (ProgressDialog.Cancelled())
      {
        return;
      }
      ProgressDialog.StepOverallProgress();

      // 9) Pack fomod
      PackFomod(strTempFomodFolder, bfaArgs.PackedPath);
      ProgressDialog.StepOverallProgress();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates the temporary directories to which the source archives will be extracted.
    /// </summary>
    /// <remarks>
    /// This method looks through the copy instructions needed to be executed to build the fomod and creates
    /// a temporary directory for every unique archive listed as a source.
    /// </remarks>
    /// <param name="p_lstCopyPaths">The list of copy instructions needed to be executed to build the fomod.</param>
    protected Dictionary<string, string> CreateExtractionDirectories(IList<KeyValuePair<string, string>> p_lstCopyPaths)
    {
      ProgressDialog.ItemProgress = 0;
      ProgressDialog.ItemProgressMaximum = p_lstCopyPaths.Count;
      ProgressDialog.ItemProgressStep = 1;
      ProgressDialog.ItemMessage = String.Format("Creating temporary folders...");

      Dictionary<string, string> dicSources = new Dictionary<string, string>();
      foreach (KeyValuePair<string, string> kvpCopyPath in p_lstCopyPaths)
      {
        if (kvpCopyPath.Key.StartsWith(Archive.ARCHIVE_PREFIX))
        {
          KeyValuePair<string, string> kvpArchive = Archive.ParseArchivePath(kvpCopyPath.Key);
          if (!dicSources.ContainsKey(kvpArchive.Key))
          {
            dicSources[kvpArchive.Key] = CreateTemporaryDirectory();
          }
        }
        ProgressDialog.StepItemProgress();
      }
      return dicSources;
    }

    /// <summary>
    /// Creates the temporary folder in which the fomod is to be built.
    /// </summary>
    /// <returns>The temporary folder in which the fomod is to be built.</returns>
    protected string CreateFomodDirectory()
    {
      ProgressDialog.ItemProgress = 0;
      ProgressDialog.ItemProgressMaximum = 1;
      ProgressDialog.ItemProgressStep = 1;
      ProgressDialog.ItemMessage = String.Format("Creating Temporary Folders...");
      string strPath = CreateTemporaryDirectory();
      ProgressDialog.StepItemProgress();
      return strPath;
    }

    /// <summary>
    /// This executes the given copy instruction, using the specified path as the destination.
    /// </summary>
    /// <param name="p_strFomodFolder">The destination folder for the copy instruction.</param>
    /// <param name="p_dicSources">The list of sources and the directories to which they are extracted.</param>
    /// <param name="p_kvpCopyInstruction">The copy instruction to execute.</param>
    protected void CopyFiles(string p_strFomodFolder, Dictionary<string, string> p_dicSources,
                             KeyValuePair<string, string> p_kvpCopyInstruction)
    {
      ProgressDialog.ItemProgress = 0;
      ProgressDialog.ItemProgressStep = 1;

      string strSource = p_kvpCopyInstruction.Key;
      if (strSource.StartsWith(Archive.ARCHIVE_PREFIX))
      {
        KeyValuePair<string, string> kvpArchive = Archive.ParseArchivePath(strSource);
        strSource = Path.Combine(p_dicSources[kvpArchive.Key], kvpArchive.Value);
      }
      ProgressDialog.ItemMessage = String.Format("Copying Source Files: {0}...", Path.GetFileName(strSource));
      ProgressDialog.ItemProgressMaximum = File.Exists(strSource)
        ? 1
        : Directory.GetFiles(strSource, "*", SearchOption.AllDirectories).Length;

      string strDestination = p_kvpCopyInstruction.Value;
      strDestination = strDestination.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
      strDestination = strDestination.Trim(new char[]
      {
        Path.DirectorySeparatorChar
      });
      strDestination = Path.Combine(p_strFomodFolder, strDestination);
      FileUtil.Copy(strSource, strDestination, FileCopied);
    }

    #endregion
  }
}