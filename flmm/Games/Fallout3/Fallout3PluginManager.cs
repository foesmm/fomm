using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Fomm.Games.Fallout3.Tools.TESsnip;
using System.Text;
using Fomm.Util;

namespace Fomm.Games.Fallout3
{
  /// <summary>
  /// Activates/deactivates Fallout 3 plugins.
  /// </summary>
  public class Fallout3PluginManager : PluginManager
  {
    #region Plugin Activation/Deactivation

    /// <summary>
    /// Gets the set of active plugins.
    /// </summary>
    /// <value>The set of active plugins.</value>
    public override Set<string> ActivePluginList
    {
      get
      {
        string strPluginsFilePath = ((Fallout3GameMode)Program.GameMode).PluginsFilePath;

        Set<string> setActivePlugins = new Set<string>(StringComparer.InvariantCultureIgnoreCase);
        if (File.Exists(strPluginsFilePath))
        {
          string[] strPlugins = File.ReadAllLines(strPluginsFilePath);
          char[] strInvalidChars = Path.GetInvalidFileNameChars();
          for (int i = 0; i < strPlugins.Length; i++)
          {
            strPlugins[i] = strPlugins[i].Trim();
            if (strPlugins[i].Length == 0 || strPlugins[i][0] == '#' || strPlugins[i].IndexOfAny(strInvalidChars) != -1)
              continue;
            string strPluginPath = Path.Combine(Program.GameMode.PluginsPath, strPlugins[i]);
            if (!File.Exists(strPluginPath))
              continue;
            setActivePlugins.Add(strPluginPath);
          }
        }
        return setActivePlugins;
      }
    }

    /// <summary>
    /// Commits any changes made to plugin activation status.
    /// </summary>
    /// <param name="p_setActivePlugins">The complete set of active plugins.</param>
    public override void SetActivePlugins(Set<string> p_setActivePlugins)
    {
      string strPluginsFilePath = ((Fallout3GameMode)Program.GameMode).PluginsFilePath;
      Set<string> setPluginFilenames = new Set<string>(StringComparer.InvariantCultureIgnoreCase);
      foreach (string strPlugin in p_setActivePlugins)
        setPluginFilenames.Add(Path.GetFileName(strPlugin));
      if (!Directory.Exists(Path.GetDirectoryName(strPluginsFilePath)))
        Directory.CreateDirectory(Path.GetDirectoryName(strPluginsFilePath));
      File.WriteAllLines(strPluginsFilePath, setPluginFilenames.ToArray(), System.Text.Encoding.Default);
    }

    /// <summary>
    /// Determines if the specified plugin is active.
    /// </summary>
    /// <param name="p_strPath">The path to the plugin whose active state is to be determined.</param>
    /// <returns><lange cref="true"/> if the specified plugin is active;
    /// <lang cref="false"/> otherwise.</returns>
    public override bool IsPluginActive(string p_strPath)
    {
      string strPluginFilename = Path.GetFileName(p_strPath);
      string strPluginPath = Path.Combine(Program.GameMode.PluginsPath, strPluginFilename);
      return ActivePluginList.Contains(strPluginPath);
    }

    #endregion

    #region Plugin Ordering

    /// <summary>
    /// Gets an ordered list of plugins.
    /// </summary>
    /// <value>An ordered list of plugins.</value>
    public override string[] OrderedPluginList
    {
      get
      {
        DirectoryInfo difPluginsDirectory = new DirectoryInfo(Program.GameMode.PluginsPath);
        List<FileInfo> lstPlugins = new List<FileInfo>(Program.GetFiles(difPluginsDirectory, "*.esp"));
        lstPlugins.AddRange(Program.GetFiles(difPluginsDirectory, "*.esm"));

        lstPlugins.Sort(delegate(FileInfo a, FileInfo b)
        {
          if (Tools.TESsnip.Plugin.GetIsEsm(a.FullName) == Tools.TESsnip.Plugin.GetIsEsm(b.FullName))
            return a.LastWriteTime.CompareTo(b.LastWriteTime);
          return Tools.TESsnip.Plugin.GetIsEsm(a.FullName) ? -1 : 1;
        });

        List<string> lstPluginPaths = new List<string>();
        for (Int32 i = 0; i < lstPlugins.Count; i++)
          lstPluginPaths.Add(lstPlugins[i].FullName);

        return lstPluginPaths.ToArray();
      }
    }

    /// <summary>
    /// Sorts the list of plugins paths.
    /// </summary>
    /// <remarks>
    /// This sorts the plugin paths based on the load order of the plugins the paths represent.
    /// </remarks>
    /// <param name="p_strPlugins">The list of plugin paths to sort.</param>
    /// <returns>The sorted list of plugin paths.</returns>
    public override string[] SortPluginList(string[] p_strPlugins)
    {
      List<FileInfo> lstPlugins = new List<FileInfo>();
      string strPlugin = null;
      for (Int32 i = 0; i < p_strPlugins.Length; i++)
      {
        strPlugin = p_strPlugins[i];
        if (!strPlugin.StartsWith(Program.GameMode.PluginsPath, StringComparison.InvariantCultureIgnoreCase) && !File.Exists(strPlugin))
          strPlugin = Path.Combine(Program.GameMode.PluginsPath, strPlugin);
        if (File.Exists(strPlugin))
          lstPlugins.Add(new FileInfo(strPlugin));
      }
      lstPlugins.Sort(delegate(FileInfo a, FileInfo b)
      {
        if (Tools.TESsnip.Plugin.GetIsEsm(a.FullName) == Tools.TESsnip.Plugin.GetIsEsm(b.FullName))
          return a.LastWriteTime.CompareTo(b.LastWriteTime);
        return Tools.TESsnip.Plugin.GetIsEsm(a.FullName) ? -1 : 1;
      });
      List<string> lstPluginPaths = new List<string>();
      foreach (FileInfo fifPlugin in lstPlugins)
        lstPluginPaths.Add(fifPlugin.FullName);
      return lstPluginPaths.ToArray();
    }

    /// <summary>
    /// Sets the load order of the specifid plugin.
    /// </summary>
    /// <param name="p_strPluginPath">The full path to the plugin file whose load order is to be set.</param>
    /// <param name="p_intPluginLoadOrderIndex">The new load order index of the plugin.</param>
    public override void SetLoadOrder(string p_strPluginPath, Int32 p_intOrderIndex)
    {
      DateTime dteTimestamp = new DateTime(2008, 1, 1);
      TimeSpan tspTwoMins = TimeSpan.FromMinutes(2 + p_intOrderIndex);
      dteTimestamp += tspTwoMins;
      File.SetLastWriteTime(p_strPluginPath, dteTimestamp);
    }

    #endregion

    /// <summary>
    /// Gets the plugin info for the specified plugin.
    /// </summary>
    /// <param name="p_strPluginPath">The full path to the plugin for which to get the info.</param>
    /// <returns>The plugin info for the specified plugin.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the specified plug in does not exist.</exception>
    public override PluginInfo GetPluginInfo(string p_strPluginPath)
    {
      if (!File.Exists(p_strPluginPath))
        throw new FileNotFoundException("The specified plugin does not exist.", p_strPluginPath);

      string strPluginName = Path.GetFileName(p_strPluginPath);
      Plugin plgPlugin;
      try
      {
        plgPlugin = new Plugin(p_strPluginPath, true);
      }
      catch
      {
        plgPlugin = null;
      }
      if (plgPlugin == null || plgPlugin.Records.Count == 0 || plgPlugin.Records[0].Name != "TES4")
      {
        string strDescription = strPluginName + Environment.NewLine + "Warning: Plugin appears corrupt";
        return new PluginInfo(strDescription, null);
      }

      StringBuilder stbDescription = new StringBuilder(@"{\rtf1\ansi\ansicpg1252\deff0\deflang4105{\fonttbl{\f0\fnil\fcharset0 MS Sans Serif;}{\f1\fnil\fcharset2 Symbol;}}");
      stbDescription.AppendLine().AppendLine(@"{\colortbl ;\red255\green0\blue0;\red255\green215\blue0;\red51\green153\blue255;}");
      stbDescription.AppendLine().Append(@"{\*\generator Msftedit 5.41.21.2509;}\viewkind4\uc1\pard\sl240\slmult1\lang9\f0\fs17 ");
      string name = null;
      string desc = null;
      byte[] pic = null;
      List<string> masters = new List<string>();
      foreach (SubRecord sr in ((Record)plgPlugin.Records[0]).SubRecords)
      {
        switch (sr.Name)
        {
          case "CNAM":
            name = sr.GetStrData();
            break;
          case "SNAM":
            desc = sr.GetStrData();
            break;
          case "MAST":
            masters.Add(sr.GetStrData());
            break;
          case "SCRN":
            pic = sr.GetData();
            break;
        }
      }
      if ((Path.GetExtension(p_strPluginPath).CompareTo(".esp") == 0) != ((((Record)plgPlugin.Records[0]).Flags1 & 1) == 0))
      {
        if ((((Record)plgPlugin.Records[0]).Flags1 & 1) == 0)
          stbDescription.Append(@"\cf1 \b WARNING: This plugin has the file extension .esm, but its file header marks it as an esp! \b0 \cf0 \line \line ");
        else
          stbDescription.Append(@"\cf1 \b WARNING: This plugin has the file extension .esp, but its file header marks it as an esm! \b0 \cf0 \line \line ");
      }
      stbDescription.AppendFormat(@"\b \ul {0} \ulnone \b0 \line ", strPluginName);
      if (name != null)
        stbDescription.AppendFormat(@"\b Author: \b0 {0} \line ", name);
      if (desc != null)
      {
        desc = desc.Replace("\r\n", "\n").Replace("\n\r", "\n").Replace("\n", "\\line ");
        stbDescription.AppendFormat(@"\b Description: \b0 \line {0} \line ", desc);
      }
      if (masters.Count > 0)
      {
        stbDescription.Append(@"\b Masters: \b0 \par \pard{\*\pn\pnlvlblt\pnf1\pnindent0{\pntxtb\'B7}}\fi-360\li720\sl240\slmult1 ");
        for (int i = 0; i < masters.Count; i++)
        {
          stbDescription.AppendFormat("{{\\pntext\\f1\\'B7\\tab}}{0}\\par ", masters[i]);
          stbDescription.AppendLine();
        }
        stbDescription.Append(@"\pard\sl240\slmult1 ");
      }

      PluginInfo pifInfo = new PluginInfo(stbDescription.ToString(), null);
      if (pic != null)
        pifInfo.Picture = System.Drawing.Bitmap.FromStream(new MemoryStream(pic));
      return pifInfo;
    }

    /// <summary>
    /// Determines if the specified plugin is critical to the current game.
    /// </summary>
    /// <param name="p_strPluginPath">The full path to the plugin for which it is to be determined whether or not it is critical.</param>
    /// <returns><lang cref="true"/> if the specified pluing is critical;
    /// <lang cref="false"/> otherwise.</returns>
    public override bool IsCriticalPlugin(string p_strPluginPath)
    {
      return Path.GetFileName(p_strPluginPath).Equals("fallout3.esm", StringComparison.OrdinalIgnoreCase);
    }
  }
}
