using System;
using System.IO;
using System.Collections.Generic;
using MessageBox = System.Windows.Forms.MessageBox;
using MessageBoxButtons = System.Windows.Forms.MessageBoxButtons;
using DialogResult = System.Windows.Forms.DialogResult;
using System.Windows.Forms;

namespace Fomm.Games.FalloutNewVegas.Tools
{
	public static class ArchiveInvalidation
	{
		private const string AiBsa = "ArchiveInvalidationInvalidated!.bsa";
		private const string BsaPath = "data\\" + AiBsa;

		private static string GetBSAList()
		{
			List<string> bsas = new List<string>(NativeMethods.GetPrivateProfileString("Archive", "SArchiveList", null, Program.GameMode.SettingsFiles[FalloutNewVegasGameMode.SettingsFile.FOIniPath]).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
			for (int i = 0; i < bsas.Count; i++)
			{
				bsas[i] = bsas[i].Trim(' ');
				if (bsas[i] == AiBsa) bsas.RemoveAt(i--);
			}
			return string.Join(", ", bsas.ToArray());
		}

		private static void ApplyAI()
		{
			throw new NotImplementedException();
			foreach (FileInfo fi in new DirectoryInfo(Program.GameMode.PluginsPath).GetFiles("Fallout - *.bsa")) fi.LastWriteTime = new DateTime(2008, 10, 1);
			
			NativeMethods.WritePrivateProfileIntA("Archive", "bInvalidateOlderFiles", 1, Program.GameMode.SettingsFiles[FalloutNewVegasGameMode.SettingsFile.FOIniPath]);
			NativeMethods.WritePrivateProfileIntA("General", "bLoadFaceGenHeadEGTFiles", 1, Program.GameMode.SettingsFiles[FalloutNewVegasGameMode.SettingsFile.FOIniPath]);
			NativeMethods.WritePrivateProfileStringA("Archive", "SInvalidationFile", "", Program.GameMode.SettingsFiles[FalloutNewVegasGameMode.SettingsFile.FOIniPath]);
			File.Delete("data\\archiveinvalidation.txt");
			File.WriteAllBytes(BsaPath, new byte[] {
                0x42, 0x53, 0x41, 0x00, 0x67, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x00, 0x03, 0x07, 0x00, 0x00,
                0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
                0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                0x36, 0x00, 0x00, 0x00, 0x01, 0x00, 0x61, 0x00, 0x01, 0x61, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x48, 0x00, 0x00, 0x00, 0x61, 0x00
            });
			NativeMethods.WritePrivateProfileStringA("Archive", "SArchiveList", AiBsa + ", " + GetBSAList(), Program.GameMode.SettingsFiles[FalloutNewVegasGameMode.SettingsFile.FOIniPath]);
		}

		private static void RemoveAI()
		{
			NativeMethods.WritePrivateProfileIntA("Archive", "bInvalidateOlderFiles", 0, Program.GameMode.SettingsFiles[FalloutNewVegasGameMode.SettingsFile.FOIniPath]);
			NativeMethods.WritePrivateProfileIntA("General", "bLoadFaceGenHeadEGTFiles", 0, Program.GameMode.SettingsFiles[FalloutNewVegasGameMode.SettingsFile.FOIniPath]);
			NativeMethods.WritePrivateProfileStringA("Archive", "SInvalidationFile", "ArchiveInvalidation.txt", Program.GameMode.SettingsFiles[FalloutNewVegasGameMode.SettingsFile.FOIniPath]);
			File.Delete(BsaPath);
			NativeMethods.WritePrivateProfileStringA("Archive", "SArchiveList", GetBSAList(), Program.GameMode.SettingsFiles[FalloutNewVegasGameMode.SettingsFile.FOIniPath]);
		}

		public static void Update()
		{
			if (!File.Exists(Program.GameMode.SettingsFiles[FalloutNewVegasGameMode.SettingsFile.FOIniPath]))
			{
				MessageBox.Show("You have no Fallout: New Vegas INI file. Please run Fallout: New Vegas to initialize the file.", "Missing INI", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			if (NativeMethods.GetPrivateProfileIntA("Archive", "bInvalidateOlderFiles", 0, Program.GameMode.SettingsFiles[FalloutNewVegasGameMode.SettingsFile.FOIniPath]) == 0)
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
			return NativeMethods.GetPrivateProfileIntA("Archive", "bInvalidateOlderFiles", 0, Program.GameMode.SettingsFiles[FalloutNewVegasGameMode.SettingsFile.FOIniPath]) != 0;
		}
	}
}
