using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Fomm.Games.FalloutNewVegas.Tools
{
  public static class ArchiveInvalidation
  {
    private const string AiBsa = "Fallout - AI!.bsa";
    private const string OldAiBsa = "ArchiveInvalidationInvalidated!.bsa";

    private static void WriteIniInt(string p_strSection, string p_strValueKey, Int32 p_intValue)
    {
      NativeMethods.WritePrivateProfileIntA(p_strSection, p_strValueKey, p_intValue,
                                            ((FalloutNewVegasGameMode.SettingsFilesSet) Program.GameMode.SettingsFiles)
                                              .FOIniPath);
      NativeMethods.WritePrivateProfileIntA(p_strSection, p_strValueKey, p_intValue,
                                            ((FalloutNewVegasGameMode.SettingsFilesSet) Program.GameMode.SettingsFiles)
                                              .FODefaultIniPath);
    }

    private static void WriteIniString(string p_strSection, string p_strValueKey, string p_strValue)
    {
      NativeMethods.WritePrivateProfileStringA(p_strSection, p_strValueKey, p_strValue,
                                               ((FalloutNewVegasGameMode.SettingsFilesSet)
                                                 Program.GameMode.SettingsFiles).FOIniPath);
      NativeMethods.WritePrivateProfileStringA(p_strSection, p_strValueKey, p_strValue,
                                               ((FalloutNewVegasGameMode.SettingsFilesSet)
                                                 Program.GameMode.SettingsFiles).FODefaultIniPath);
    }

    private static string GetBSAList(bool p_booInsertAI)
    {
      List<string> bsas =
        new List<string>(
          NativeMethods.GetPrivateProfileString("Archive", "SArchiveList", null,
                                                ((FalloutNewVegasGameMode.SettingsFilesSet)
                                                  Program.GameMode.SettingsFiles).FOIniPath).Split(new[]
                                                  {
                                                    ','
                                                  }, StringSplitOptions.RemoveEmptyEntries));
      List<string> lstNewBSAs = new List<string>();
      for (int i = 0; i < bsas.Count; i++)
      {
        bsas[i] = bsas[i].Trim(' ');
        if (bsas[i] == OldAiBsa)
        {
          continue;
        }
        if (bsas[i].Contains("Misc"))
        {
          lstNewBSAs.Insert(0, bsas[i]);
        }
        else if (bsas[i] != AiBsa)
        {
          lstNewBSAs.Add(bsas[i]);
        }
      }
      if (p_booInsertAI)
      {
        lstNewBSAs.Insert(0, AiBsa);
      }
      return string.Join(", ", lstNewBSAs.ToArray());
    }

    private static void ApplyAI()
    {
      foreach (FileInfo fi in new DirectoryInfo(Program.GameMode.PluginsPath).GetFiles("Fallout - *.bsa"))
      {
        fi.LastWriteTime = new DateTime(2008, 10, 1);
      }
      foreach (FileInfo fi in new DirectoryInfo(Program.GameMode.PluginsPath).GetFiles("ClassicPack - *.bsa"))
      {
        fi.LastWriteTime = new DateTime(2008, 10, 1);
      }

      WriteIniInt("Archive", "bInvalidateOlderFiles", 1);
      WriteIniInt("General", "bLoadFaceGenHeadEGTFiles", 1);
      WriteIniString("Archive", "SInvalidationFile", "");
      File.Delete(Path.Combine(Program.GameMode.PluginsPath, "archiveinvalidation.txt"));
      File.Delete(Path.Combine(Program.GameMode.PluginsPath, OldAiBsa));
      File.WriteAllBytes(Path.Combine(Program.GameMode.PluginsPath, AiBsa), new byte[]
      {
        0x42, 0x53, 0x41, 0x00, 0x67, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x00, 0x03, 0x07, 0x00, 0x00,
        0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
        0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
        0x36, 0x00, 0x00, 0x00, 0x01, 0x00, 0x61, 0x00, 0x01, 0x61, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x48, 0x00, 0x00, 0x00, 0x61, 0x00
      });
      WriteIniString("Archive", "SArchiveList", GetBSAList(true));
    }

    private static void RemoveAI()
    {
      WriteIniInt("Archive", "bInvalidateOlderFiles", 0);
      WriteIniInt("General", "bLoadFaceGenHeadEGTFiles", 0);
      WriteIniString("Archive", "SInvalidationFile", "ArchiveInvalidation.txt");
      File.Delete(Path.Combine(Program.GameMode.PluginsPath, AiBsa));
      File.Delete(Path.Combine(Program.GameMode.PluginsPath, OldAiBsa));
      WriteIniString("Archive", "SArchiveList", GetBSAList(false));
    }

    public static bool Update()
    {
      if (!File.Exists(((FalloutNewVegasGameMode.SettingsFilesSet) Program.GameMode.SettingsFiles).FOIniPath))
      {
        MessageBox.Show("You have no Fallout INI file. Please run Fallout to initialize the file.", "Missing INI",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
        return false;
      }
      if (
        NativeMethods.GetPrivateProfileIntA("Archive", "bInvalidateOlderFiles", 0,
                                            ((FalloutNewVegasGameMode.SettingsFilesSet) Program.GameMode.SettingsFiles)
                                              .FOIniPath) == 0)
      {
        if (MessageBox.Show("Apply archive invalidation?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
          ApplyAI();
          return true;
        }
      }
      else
      {
        if (MessageBox.Show("Remove archive invalidation?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
          RemoveAI();
          return true;
        }
      }
      return false;
    }

    public static bool IsActive()
    {
      List<string> bsas =
        new List<string>(
          NativeMethods.GetPrivateProfileString("Archive", "SArchiveList", null,
                                                ((FalloutNewVegasGameMode.SettingsFilesSet)
                                                  Program.GameMode.SettingsFiles).FOIniPath).Split(new[]
                                                  {
                                                    ','
                                                  }, StringSplitOptions.RemoveEmptyEntries));
      Int32 intInvalidate = NativeMethods.GetPrivateProfileIntA("Archive", "bInvalidateOlderFiles", 0,
                                                                ((FalloutNewVegasGameMode.SettingsFilesSet)
                                                                  Program.GameMode.SettingsFiles).FOIniPath);
      return bsas.Contains(AiBsa) || (intInvalidate != 0);
    }
  }
}