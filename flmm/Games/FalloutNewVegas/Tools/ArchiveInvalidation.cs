using System;
using System.IO;
using System.Collections.Generic;
using MessageBox = System.Windows.Forms.MessageBox;
using MessageBoxButtons = System.Windows.Forms.MessageBoxButtons;
using DialogResult = System.Windows.Forms.DialogResult;
using System.Windows.Forms;
using Fomm.Games.Fallout3;

namespace Fomm.Games.FalloutNewVegas.Tools
{
	public static class ArchiveInvalidation
	{
		private const string AiBsa = "Fallout - AI!.bsa";
		private const string OldAiBsa = "ArchiveInvalidationInvalidated!.bsa";
		
		private static string GetBSAList(bool p_booInsertAI)
		{
			List<string> bsas = new List<string>(NativeMethods.GetPrivateProfileString("Archive", "SArchiveList", null, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
			List<string> lstNewBSAs = new List<string>();
			for (int i = 0; i < bsas.Count; i++)
			{
				bsas[i] = bsas[i].Trim(' ');
				if (bsas[i] == OldAiBsa)
					continue;
				if (bsas[i].Contains("Misc"))
					lstNewBSAs.Insert(0, bsas[i]);
				else if (bsas[i] != AiBsa)
					lstNewBSAs.Add(bsas[i]);
			}
			if (p_booInsertAI)
				lstNewBSAs.Insert(0, AiBsa);
			return string.Join(", ", lstNewBSAs.ToArray());
		}

		private static void ApplyAI()
		{
			foreach (FileInfo fi in new DirectoryInfo(Program.GameMode.PluginsPath).GetFiles("Fallout - *.bsa"))
				fi.LastWriteTime = new DateTime(2008, 10, 1);
			foreach (FileInfo fi in new DirectoryInfo(Program.GameMode.PluginsPath).GetFiles("ClassicPack - *.bsa"))
				fi.LastWriteTime = new DateTime(2008, 10, 1);
			
			NativeMethods.WritePrivateProfileIntA("Archive", "bInvalidateOlderFiles", 1, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath);
			NativeMethods.WritePrivateProfileIntA("General", "bLoadFaceGenHeadEGTFiles", 1, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath);
			NativeMethods.WritePrivateProfileStringA("Archive", "SInvalidationFile", "", ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath);
			File.Delete(Path.Combine(Program.GameMode.PluginsPath, "archiveinvalidation.txt"));
			File.Delete(Path.Combine(Program.GameMode.PluginsPath, OldAiBsa));
			File.WriteAllBytes(Path.Combine(Program.GameMode.PluginsPath, AiBsa), new byte[] {
                0x42, 0x53, 0x41, 0x00, 0x67, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x00, 0x03, 0x07, 0x00, 0x00,
                0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
                0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                0x36, 0x00, 0x00, 0x00, 0x01, 0x00, 0x61, 0x00, 0x01, 0x61, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x48, 0x00, 0x00, 0x00, 0x61, 0x00
            });
			NativeMethods.WritePrivateProfileStringA("Archive", "SArchiveList", GetBSAList(true), ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath);
		}

		private static void RemoveAI()
		{
			NativeMethods.WritePrivateProfileIntA("Archive", "bInvalidateOlderFiles", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath);
			NativeMethods.WritePrivateProfileIntA("General", "bLoadFaceGenHeadEGTFiles", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath);
			NativeMethods.WritePrivateProfileStringA("Archive", "SInvalidationFile", "ArchiveInvalidation.txt", ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath);
			File.Delete(Path.Combine(Program.GameMode.PluginsPath, AiBsa));
			File.Delete(Path.Combine(Program.GameMode.PluginsPath, OldAiBsa));
			NativeMethods.WritePrivateProfileStringA("Archive", "SArchiveList", GetBSAList(false), ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath);
		}

		public static void Update()
		{
			if (!File.Exists(((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath))
			{
				MessageBox.Show("You have no Fallout INI file. Please run Fallout to initialize the file.", "Missing INI", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			if (NativeMethods.GetPrivateProfileIntA("Archive", "bInvalidateOlderFiles", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath) == 0)
			{
				if (MessageBox.Show("Apply archive invalidation?", "", MessageBoxButtons.YesNo) == DialogResult.Yes) ApplyAI();
			}
			else
			{
				if (MessageBox.Show("Remove archive invalidation?", "", MessageBoxButtons.YesNo) == DialogResult.Yes) RemoveAI();
			}
		}

		public static bool IsActive()
		{
			List<string> bsas = new List<string>(NativeMethods.GetPrivateProfileString("Archive", "SArchiveList", null, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
			Int32 intInvalidate = NativeMethods.GetPrivateProfileIntA("Archive", "bInvalidateOlderFiles", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath);
			return bsas.Contains(AiBsa) || (intInvalidate != 0);
		}
	}
}
