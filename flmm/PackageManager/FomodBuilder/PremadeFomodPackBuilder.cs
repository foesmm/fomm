using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using Fomm.Properties;
using SevenZip;
using System.Text;
using Fomm.Util;
using Fomm.PackageManager.XmlConfiguredInstall.Parsers;
using GeMod.Interface;

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
      private IList<SourceFile> m_lstSourceFiles;
      private string m_strCustomHowToSteps;

      #region Properties

      /// <summary>
      /// Gets the fomod sources.
      /// </summary>
      /// <value>The fomod sources.</value>
      public IList<SourceFile> SourceFiles
      {
        get
        {
          return m_lstSourceFiles;
        }
      }

      /// <summary>
      /// Gets custom howto steps that should be included in the PFP HowTo.
      /// </summary>
      /// <value>Custom howto steps that should be included in the PFP HowTo.</value>
      public string CustomHowToSteps
      {
        get
        {
          return m_strCustomHowToSteps;
        }
      }

      #endregion

      #region Constructors

      /// <summary>
      /// A simple constructor that initializes the object with the given values.
      /// </summary>
      /// <param name="p_strFomodName">The value with which to initialize the <see cref="FomodName"/> property.</param>
      /// <param name="p_lstCopyInstructions">The value with which to initialize the <see cref="CopyInstructions"/> property.</param>
      /// <param name="p_lstSourceFiles">The value with which to initialize the <see cref="SourceFiles"/> property.</param>
      /// <param name="p_strCustomHowToSteps">Custom howto steps that should be included in the PFP HowTo.</param>
      /// <param name="p_rmeReadme">The value with which to initialize the <see cref="Readme"/> property.</param>
      /// <param name="p_xmlInfo">The value with which to initialize the <see cref="Info"/> property.</param>
      /// <param name="p_booSetScreenshot">The value with which to initialize the <see cref="SetScreenshot"/> property.</param>
      /// <param name="p_shtScreenshot">The value with which to initialize the <see cref="Screenshot"/> property.</param>
      /// <param name="p_fscScript">The value with which to initialize the <see cref="Script"/> property.</param>
      /// <param name="p_strPackedPath">The value with which to initialize the <see cref="PackedPath"/> property.</param>
      public BuildPFPArgs(string p_strFomodName, IList<KeyValuePair<string, string>> p_lstCopyInstructions,
                          IList<SourceFile> p_lstSourceFiles, string p_strCustomHowToSteps, Readme p_rmeReadme,
                          XmlDocument p_xmlInfo, bool p_booSetScreenshot, Screenshot p_shtScreenshot,
                          FomodScript p_fscScript, string p_strPackedPath)
        : base(
          p_strFomodName, p_lstCopyInstructions, p_rmeReadme, p_xmlInfo, p_booSetScreenshot, p_shtScreenshot,
          p_fscScript, p_strPackedPath)
      {
        m_lstSourceFiles = p_lstSourceFiles;
        m_strCustomHowToSteps = p_strCustomHowToSteps;
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
    /// <param name="p_strMachineVersion">The machine version of the fomod for which we are creating the PFP.</param>
    /// <param name="p_lstCopyInstructions">The list of files to copy into the fomod.</param>
    /// <param name="p_lstSourceFiles">The list of source files.</param>
    /// <param name="p_rmeReadme">The fomod readme.</param>
    /// <param name="p_xmlInfo">The fomod info file.</param>
    /// <param name="p_booSetScreenshot">Whether or not to set the fomod's screenshot.</param>
    /// <param name="p_shtScreenshot">The fomod screenshot.</param>
    /// <param name="p_fscScript">The fomod install script.</param>
    /// <param name="p_strPFPPath">The path where the Premade Fomod Pack will be created.</param>
    /// <returns>The path to the new premade fomod pack if it was successfully built; <lang cref="null"/> otherwise.</returns>
    public string BuildPFP(string p_strFileName, string p_strVersion, string p_strMachineVersion,
                           IList<KeyValuePair<string, string>> p_lstCopyInstructions, IList<SourceFile> p_lstSourceFiles,
                           string p_strCustomHowToSteps, Readme p_rmeReadme, XmlDocument p_xmlInfo,
                           bool p_booSetScreenshot, Screenshot p_shtScreenshot, FomodScript p_fscScript,
                           string p_strPFPPath)
    {
      string strPFPExtension = null;
      switch (Settings.Default.pfpCompressionFormat)
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
      var strVersion = p_strVersion;
      if (strVersion.Length > 8)
      {
        strVersion = p_strMachineVersion;
      }
      var strPFPPath = Path.Combine(p_strPFPPath,
                                       String.Format("{0} {1}{2}", p_strFileName, strVersion, strPFPExtension));
      strPFPPath = GenerateFomod(new BuildPFPArgs(p_strFileName,
                                                  p_lstCopyInstructions,
                                                  p_lstSourceFiles,
                                                  p_strCustomHowToSteps,
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
      var bpaArgs = p_objArgs as BuildPFPArgs;
      if (bpaArgs == null)
      {
        throw new ArgumentException("The given argument must be a BuildPFPArgs.", "p_objArgs");
      }

      /**
       * 1) Create tmp dirs for extraction of sources that will be stored in PFP
       * 2) Extract sources
       * 3) Create dest PFP dir
       * 4) Copy sources to dest PFP dir
       * 5) Create metadata.xml
       * 6) Create fomod readme
       * 7) Create info.xml
       * 8) Create screenshot
       * 9) Create script
       * 10) Pack PFP
       * 
       * Total steps  = 1 + (# sources to extract) + 1 + (# of copies needed) + 1 + 1 + 1 + 1 + 1 + 1
       *        = 8 + (# sources to extract) + (# of copies needed)
       */
      var intBaseStepCount = 8;

      //get only the instructions for files that are to be included in the PFP
      // files will be included in the PFP only if they don't have a download location
      var lstPFPCopyInstructions = new List<KeyValuePair<string, string>>();
      var lstFomodCopyInstructions = new List<KeyValuePair<string, string>>();
      var setUsedSources = new Set<SourceFile>();
      foreach (var kvpInstruction in bpaArgs.CopyInstructions)
      {
        var strPath = kvpInstruction.Key;
        if (kvpInstruction.Key.StartsWith(Archive.ARCHIVE_PREFIX))
        {
          while (strPath.StartsWith(Archive.ARCHIVE_PREFIX))
          {
            strPath = Archive.ParseArchivePath(strPath).Key;
          }
        }

        foreach (var sflSource in bpaArgs.SourceFiles)
        {
          if (strPath.StartsWith(sflSource.Source))
          {
            if (sflSource.Included)
            {
              lstPFPCopyInstructions.Add(kvpInstruction);
            }
            else
            {
              lstFomodCopyInstructions.Add(kvpInstruction);
              setUsedSources.Add(sflSource);
            }
            break;
          }
        }
      }

      //add the hidden and generated sources to the used source list
      foreach (var sflSource in bpaArgs.SourceFiles)
      {
        if (sflSource.Hidden || sflSource.Generated)
        {
          setUsedSources.Add(sflSource);
        }
      }

      // 1) Create tmp dirs for extraction of sources that will be stored in PFP
      var dicSources = CreateExtractionDirectories(lstPFPCopyInstructions);
      ProgressDialog.OverallProgressMaximum = intBaseStepCount + dicSources.Count + lstPFPCopyInstructions.Count;
      if (ProgressDialog.Cancelled())
      {
        return;
      }
      ProgressDialog.StepOverallProgress();

      // 2) Extract sources
      foreach (var kvpArchive in dicSources)
      {
        UnpackArchive(kvpArchive.Key, kvpArchive.Value);
        if (ProgressDialog.Cancelled())
        {
          return;
        }
        ProgressDialog.StepOverallProgress();
      }

      // 3) Create dest PFP dir
      var strTempPFPFolder = CreateFomodDirectory();
      var strTempPFPPremadeFolder = Path.Combine(strTempPFPFolder, "Premade " + bpaArgs.FomodName);
      var strTempPFPPremadeFomodFolder = Path.Combine(strTempPFPPremadeFolder, "fomod");
      if (!Directory.Exists(strTempPFPPremadeFolder))
      {
        Directory.CreateDirectory(strTempPFPPremadeFolder);
      }
      if (ProgressDialog.Cancelled())
      {
        return;
      }
      ProgressDialog.StepOverallProgress();

      // 4) Copy sources to dest PFP dir
      foreach (var kvpCopyInstruction in lstPFPCopyInstructions)
      {
        CopyFiles(strTempPFPPremadeFolder, dicSources, kvpCopyInstruction);
        if (ProgressDialog.Cancelled())
        {
          return;
        }
        ProgressDialog.StepOverallProgress();
      }
      if (!Directory.Exists(strTempPFPPremadeFomodFolder))
      {
        Directory.CreateDirectory(strTempPFPPremadeFomodFolder);
      }

      // 5) Create metadata.xml
      CreateMetadataFile(strTempPFPFolder, setUsedSources, lstFomodCopyInstructions, bpaArgs.CustomHowToSteps);
      if (ProgressDialog.Cancelled())
      {
        return;
      }
      ProgressDialog.StepOverallProgress();

      CreatePFPHowTo(strTempPFPFolder, bpaArgs.FomodName, setUsedSources, lstFomodCopyInstructions,
                     bpaArgs.CustomHowToSteps, bpaArgs.Script);

      // 6) Create fomod readme
      CreateReadmeFile(strTempPFPPremadeFolder, bpaArgs.FomodName, bpaArgs.Readme);
      if (ProgressDialog.Cancelled())
      {
        return;
      }
      ProgressDialog.StepOverallProgress();

      // 7) Create info.xml
      CreateInfoFile(strTempPFPPremadeFomodFolder, bpaArgs.InfoFile);
      if (ProgressDialog.Cancelled())
      {
        return;
      }
      ProgressDialog.StepOverallProgress();

      // 8) Create screenshot
      CreateScreenshot(strTempPFPPremadeFomodFolder, bpaArgs.SetScreenshot, bpaArgs.Screenshot);
      if (ProgressDialog.Cancelled())
      {
        return;
      }
      ProgressDialog.StepOverallProgress();

      // 9) Create script
      CreateScriptFile(strTempPFPPremadeFomodFolder, bpaArgs.Script);
      if (ProgressDialog.Cancelled())
      {
        return;
      }
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
    /// <param name="p_lstSourceFiles">The list of source files. It is expected that this
    /// list only contains sources used by the given instructions.</param>
    /// <param name="p_lstCopyInstructions">The list of copy instructions to execute to create the fomod.
    /// This list should not include copy instructions for files included in the PFP.</param>
    /// <param name="p_strCustomHowToSteps">Custom howto steps that should be included in the PFP HowTo.</param>
    protected void CreateMetadataFile(string p_strPFPFolder, IList<SourceFile> p_lstSourceFiles,
                                      IList<KeyValuePair<string, string>> p_lstCopyInstructions,
                                      string p_strCustomHowToSteps)
    {
      ProgressDialog.ItemProgress = 0;
      ProgressDialog.ItemProgressMaximum = p_lstSourceFiles.Count + p_lstCopyInstructions.Count;
      ProgressDialog.ItemProgressStep = 1;
      ProgressDialog.ItemMessage = String.Format("Creating metadat file...");

      var xmlMeta = new XmlDocument();
      var xndRoot = xmlMeta.AppendChild(xmlMeta.CreateElement("premadeFomodPack"));
      var xndSources = xndRoot.AppendChild(xmlMeta.CreateElement("sources"));
      var xndCustomHowToSteps = xndRoot.AppendChild(xmlMeta.CreateElement("customHowToSteps"));
      var xndInstructions = xndRoot.AppendChild(xmlMeta.CreateElement("copyInstructions"));

      if (!String.IsNullOrEmpty(p_strCustomHowToSteps))
      {
        xndCustomHowToSteps.InnerXml = p_strCustomHowToSteps;
      }

      foreach (var kvpInstruction in p_lstCopyInstructions)
      {
        var xndInstruction = xndInstructions.AppendChild(xmlMeta.CreateElement("instruction"));
        if (kvpInstruction.Key.StartsWith(Archive.ARCHIVE_PREFIX))
        {
          xndInstruction.Attributes.Append(xmlMeta.CreateAttribute("source")).Value =
            Archive.ChangeArchiveDirectory(kvpInstruction.Key, "");
          xndInstruction.Attributes.Append(xmlMeta.CreateAttribute("destination")).Value = kvpInstruction.Value;
        }
        else
        {
          foreach (var sflSource in p_lstSourceFiles)
          {
            if (kvpInstruction.Key.StartsWith(sflSource.Source))
            {
              xndInstruction.Attributes.Append(xmlMeta.CreateAttribute("source")).Value =
                kvpInstruction.Key.Substring(Path.GetDirectoryName(sflSource.Source).Length + 1);
              xndInstruction.Attributes.Append(xmlMeta.CreateAttribute("destination")).Value = kvpInstruction.Value;
              break;
            }
          }
        }
        ProgressDialog.StepItemProgress();
        if (ProgressDialog.Cancelled())
        {
          return;
        }
      }

      foreach (var sflSource in p_lstSourceFiles)
      {
        var xndSource = xndSources.AppendChild(xmlMeta.CreateElement("source"));
        xndSource.Attributes.Append(xmlMeta.CreateAttribute("name")).Value = sflSource.SourceFileName;
        xndSource.Attributes.Append(xmlMeta.CreateAttribute("url")).Value = sflSource.URL;
        if (sflSource.Hidden)
        {
          xndSource.Attributes.Append(xmlMeta.CreateAttribute("hidden")).Value = true.ToString();
        }
        if (sflSource.Generated)
        {
          xndSource.Attributes.Append(xmlMeta.CreateAttribute("generated")).Value = true.ToString();
        }
        ProgressDialog.StepItemProgress();
        if (ProgressDialog.Cancelled())
        {
          return;
        }
      }

      xmlMeta.Save(Path.Combine(p_strPFPFolder, "metadata.xml"));
      return;
    }

    /// <summary>
    /// Creates the howto file explaining how to use the PFP.
    /// </summary>
    /// <param name="p_strPFPFolder">The folder in which to create the howto file.</param>
    /// <param name="p_strModBaseName">The base name of the FOMod for which we are creating a PFP.</param>
    /// <param name="p_lstSourceFiles">The list of source files. It is expected that this
    /// list only contains sources used by the given instructions.</param>
    /// <param name="p_lstCopyInstructions">The list of copy instructions to execute to create the fomod.
    /// This list should not include copy instructions for files included in the PFP.</param>
    /// <param name="p_strCustomHowToSteps">Custom howto steps that should be included in the PFP HowTo.</param>
    /// <param name="p_fscScript">The FOMod script.</param>
    protected void CreatePFPHowTo(string p_strPFPFolder, string p_strModBaseName, IList<SourceFile> p_lstSourceFiles,
                                  IList<KeyValuePair<string, string>> p_lstCopyInstructions,
                                  string p_strCustomHowToSteps, FomodScript p_fscScript)
    {
      var dicSources = new Dictionary<string, Set<string>>();
      foreach (var sflSource in p_lstSourceFiles)
      {
        if (sflSource.Generated)
        {
          continue;
        }
        if (!dicSources.ContainsKey(sflSource.URL))
        {
          dicSources[sflSource.URL] = new Set<string>(StringComparer.InvariantCultureIgnoreCase);
        }
        dicSources[sflSource.URL].Add(sflSource.SourceFileName);
      }

      var stbHowTo = new StringBuilder();
      var intStepCounter = 1;
      stbHowTo.AppendLine("Instructions");
      stbHowTo.AppendLine("------------").AppendLine();
      var intDownloadStep = intStepCounter;
      AppendWrappedFormat(stbHowTo, "{0}) Download the files required to build the FOMod:", intStepCounter++)
        .AppendLine();
      foreach (var kvpSource in dicSources)
      {
        stbHowTo.AppendLine(kvpSource.Value.Count > 1 ? "\tThese files:" : "\tThis file:");
        foreach (var strSource in kvpSource.Value)
        {
          stbHowTo.Append("\t\t").AppendLine(strSource);
        }
        stbHowTo.AppendLine("\tcan be downloaded from:");
        stbHowTo.Append("\t\t").AppendLine(kvpSource.Key).AppendLine();
      }

      //insert any custom steps
      string strCollateStepFormat = null;
      if (!String.IsNullOrEmpty(p_strCustomHowToSteps))
      {
        var strCustomSteps = p_strCustomHowToSteps.Split(new string[]
        {
          Environment.NewLine
        }, StringSplitOptions.RemoveEmptyEntries);
        stbHowTo.AppendLine();
        var intCustomStartStep = intStepCounter;
        foreach (var strCustomStep in strCustomSteps)
        {
          if (strCustomStep.Contains("#)"))
          {
            AppendWrappedFormat(stbHowTo, strCustomStep.Replace("#)", "{0})"), intStepCounter++).AppendLine();
          }
          else
          {
            AppendWrappedFormat(stbHowTo, strCustomStep).AppendLine();
          }
        }
        stbHowTo.AppendLine();
        strCollateStepFormat =
          String.Format(
            "{{0}}) Put all the files you downloaded in Step {{1}}, as well as any files or folders you created in Steps {0}-{1}, into the same folder.",
            intCustomStartStep, intStepCounter - 1);
      }
      else
      {
        strCollateStepFormat = "{0}) Put all the files you downloaded in Step {1} into the same folder.";
      }

      //decide if you are using manual or auto install
      AppendWrappedFormat(stbHowTo,
                          "{0}) If you are using FOMM 0.12.0 or newer, proceed to Step {1}, otherwise proceed to Step {2}.",
                          intStepCounter++, intStepCounter, intStepCounter + 8).AppendLine().AppendLine();

      //auto install
      var intSourceFolderStep = intStepCounter;
      AppendWrappedFormat(stbHowTo, strCollateStepFormat, intStepCounter++, intDownloadStep).AppendLine();
      AppendWrappedFormat(stbHowTo, "{0}) Start FOMM.", intStepCounter++).AppendLine();
      AppendWrappedFormat(stbHowTo, "{0}) Click the 'Package Manager' button.", intStepCounter++).AppendLine();
      AppendWrappedFormat(stbHowTo,
                          "{0}) Click the arrow on the top button in the 'Package Manager' window. Select 'Add PFP' from the menu.",
                          intStepCounter++).AppendLine();
      AppendWrappedFormat(stbHowTo,
                          "{0}) Enter the path to the Premade FOMod Pack (PFP) in the 'Premade FOMod Pack' textbox. You can click the '...' button next to the textbox to select the file, if desired. The PFP file is the archive containing this HowTo file.",
                          intStepCounter++).AppendLine();
      AppendWrappedFormat(stbHowTo,
                          "{0}) Enter the path you put all of the downloaded files into in Step {1} into the 'Source Files Folder' textbox. You can click the '...' button next to the textbox to select the folder, if desired.",
                          intStepCounter++, intSourceFolderStep).AppendLine();
      AppendWrappedFormat(stbHowTo, "{0}) Click OK.", intStepCounter++).AppendLine();
      AppendWrappedFormat(stbHowTo, "{0}) Enjoy!", intStepCounter++).AppendLine();
      stbHowTo.AppendLine();

      //manual install
      var stbExtractions = new StringBuilder();
      var stbCopies = new StringBuilder();
      var lstExtracted = new List<string>();
      foreach (var kvpInstruction in p_lstCopyInstructions)
      {
        var strSource = kvpInstruction.Key;
        if (strSource.StartsWith(Archive.ARCHIVE_PREFIX))
        {
          var kvpSource = Archive.ParseArchivePath(strSource);
          var stkArchives = new Stack<string>();
          var kvpArchive = kvpSource;
          while (kvpArchive.Key.StartsWith(Archive.ARCHIVE_PREFIX))
          {
            kvpArchive = Archive.ParseArchivePath(kvpArchive.Key);
            stkArchives.Push(kvpArchive.Value);
          }
          stkArchives.Push(Path.GetFileName(kvpArchive.Key));
          strSource = "";
          while (stkArchives.Count > 0)
          {
            var strArchive = Path.Combine(strSource, stkArchives.Peek());
            strSource = Path.Combine(strSource, Path.ChangeExtension(stkArchives.Pop(), null));
            if (!lstExtracted.Contains(strArchive))
            {
              stbExtractions.AppendFormat("\tExtract '{0}'", strArchive).AppendLine();
              stbExtractions.AppendFormat("\t\tto a folder named '{0}'.", strSource).AppendLine();
              lstExtracted.Add(strArchive);
            }
          }
          strSource = Path.Combine(strSource, kvpSource.Value);
        }
        else
        {
          foreach (var sflSource in p_lstSourceFiles)
          {
            if (strSource.StartsWith(sflSource.Source))
            {
              strSource = strSource.Substring(Path.GetDirectoryName(sflSource.Source).Length + 1);
              break;
            }
          }
        }
        stbCopies.AppendFormat("\tCopy '{0}'", strSource).AppendLine();
        stbCopies.AppendFormat("\t\tto '{0}'.", Path.Combine(p_strModBaseName, kvpInstruction.Value)).AppendLine();
      }

      if (stbExtractions.Length > 0)
      {
        AppendWrappedFormat(stbHowTo, "{0}) Extract the source files to the following folders:", intStepCounter++)
          .AppendLine();
        stbHowTo.Append(stbExtractions);
      }

      var intCreateFomodFolderStep = intStepCounter;
      AppendWrappedFormat(stbHowTo, "{0}) Create a folder named '{1}'.", intStepCounter++, p_strModBaseName)
        .AppendLine();
      AppendWrappedFormat(stbHowTo, "{0}) Make the following copies:", intStepCounter++).AppendLine();
      stbHowTo.Append(stbCopies);

      AppendWrappedFormat(stbHowTo,
                          "{0}) Copy the files in the included 'Premade {1}' folder into the '{1}' folder you created in Step {2}.",
                          intStepCounter++, p_strModBaseName, intCreateFomodFolderStep).AppendLine();
      if ((p_fscScript != null) && (p_fscScript.Type == FomodScriptType.XMLConfig))
      {
        var strXMLVersion = Parser.GetConfigVersion(p_fscScript.Text);
        string strMinFOMMVersion = null;
        switch (strXMLVersion)
        {
          case "1.0":
            strMinFOMMVersion = "0.11.5";
            break;
          case "2.0":
            strMinFOMMVersion = "0.11.7";
            break;
          case "3.0":
            strMinFOMMVersion = "0.11.9";
            break;
          case "4.0":
            strMinFOMMVersion = "0.12.4";
            break;
          case "5.0":
            strMinFOMMVersion = "0.13.0";
            break;
        }
        AppendWrappedFormat(stbHowTo,
                            "{0}) If you are using FOMM {1} or newer, proceed to Step {2}, otherwise proceed to Step {3}.",
                            intStepCounter++, strMinFOMMVersion, intStepCounter + 2, intStepCounter).AppendLine();
        stbHowTo.AppendLine();
        AppendWrappedFormat(stbHowTo, "{0}) Download the 'Old FOMM Compatibility' file.", intStepCounter++).AppendLine();
        AppendWrappedFormat(stbHowTo,
                            "{0}) In the file you downloaded in Step {1} is a 'fomod' folder. Copy the contents of that folder into the '{2}/fomod' folder you created in Step {3}.",
                            intStepCounter++, intStepCounter - 2, p_strModBaseName, intCreateFomodFolderStep)
          .AppendLine();
        stbHowTo.AppendLine();
      }
      AppendWrappedFormat(stbHowTo, "{0}) Start FOMM.", intStepCounter++).AppendLine();
      AppendWrappedFormat(stbHowTo, "{0}) Click the 'Package Manager' button.", intStepCounter++).AppendLine();
      AppendWrappedFormat(stbHowTo,
                          "{0}) Click the arrow on the top button in the 'Package Manager' window. Select 'Create From Folder' from the menu.",
                          intStepCounter++).AppendLine();
      AppendWrappedFormat(stbHowTo, "{0}) Select the '{1}' folder you created in Step {2}.", intStepCounter++,
                          p_strModBaseName, intCreateFomodFolderStep).AppendLine();
      AppendWrappedFormat(stbHowTo, "{0}) Click OK.", intStepCounter++).AppendLine();
      AppendWrappedFormat(stbHowTo, "{0}) Enjoy!", intStepCounter++).AppendLine();

      File.WriteAllText(Path.Combine(p_strPFPFolder, "howto.txt"), stbHowTo.ToString().Replace("\t", "    "));
    }

    /// <summary>
    /// Appends a line to the given <see cref="StringBuilder"/> that is wrapped at 80 characters.
    /// </summary>
    /// <param name="p_stbBuilder">The <see cref="StringBuilder"/> to which to append the string.</param>
    /// <param name="p_strFormat">The format of the string to append.</param>
    /// <param name="p_objParams">The parameters to use with the given format.</param>
    /// <returns>The given <see cref="StringBuilder"/>.</returns>
    public static StringBuilder AppendWrappedFormat(StringBuilder p_stbBuilder, string p_strFormat,
                                                    params object[] p_objParams)
    {
      var intWidth = 80;
      var strFormatted = String.Format(p_strFormat, p_objParams).Replace("\t", "    ");
      while (strFormatted.Length > intWidth)
      {
        var intBreakPos = strFormatted.LastIndexOf(' ', intWidth);
        p_stbBuilder.AppendLine(strFormatted.Substring(0, intBreakPos));
        strFormatted = "    " + strFormatted.Substring(intBreakPos + 1);
      }
      p_stbBuilder.Append(strFormatted);
      return p_stbBuilder;
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

      var szcCompressor = new SevenZipCompressor();
      szcCompressor.CompressionLevel = Settings.Default.pfpCompressionLevel;
      szcCompressor.ArchiveFormat = Settings.Default.pfpCompressionFormat;
      szcCompressor.CompressionMethod = CompressionMethod.Default;
      szcCompressor.CompressionMode = CompressionMode.Create;
      szcCompressor.FileCompressionStarted += new EventHandler<FileNameEventArgs>(FileCompressionStarted);
      szcCompressor.FileCompressionFinished += new EventHandler<EventArgs>(FileCompressionFinished);
      if (!Directory.Exists(Path.GetDirectoryName(p_strPackedPFPPath)))
      {
        Directory.CreateDirectory(Path.GetDirectoryName(p_strPackedPFPPath));
      }
      szcCompressor.CompressDirectory(p_strPFPFolder, p_strPackedPFPPath);
    }
  }
}