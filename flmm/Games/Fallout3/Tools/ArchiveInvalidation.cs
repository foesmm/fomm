using System;
using System.IO;
using System.Collections.Generic;
using MessageBox = System.Windows.Forms.MessageBox;
using MessageBoxButtons = System.Windows.Forms.MessageBoxButtons;
using DialogResult = System.Windows.Forms.DialogResult;
using System.Windows.Forms;

namespace Fomm.Games.Fallout3.Tools
{
	public static class ArchiveInvalidation
	{
		private const string AiBsa = "ArchiveInvalidationInvalidated!.bsa";

		private static string GetBSAList()
		{
			List<string> bsas = new List<string>(NativeMethods.GetPrivateProfileString("Archive", "SArchiveList", null, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
			for (int i = 0; i < bsas.Count; i++)
			{
				bsas[i] = bsas[i].Trim(' ');
				if (bsas[i] == AiBsa) bsas.RemoveAt(i--);
			}
			return string.Join(", ", bsas.ToArray());
		}

		private static void ApplyAI()
		{
			foreach (FileInfo fi in new DirectoryInfo(Program.GameMode.PluginsPath).GetFiles("Fallout - *.bsa")) fi.LastWriteTime = new DateTime(2008, 10, 1);
			foreach (FileInfo fi in new DirectoryInfo(Program.GameMode.PluginsPath).GetFiles("Anchorage - *.bsa")) fi.LastWriteTime = new DateTime(2008, 10, 2);
			foreach (FileInfo fi in new DirectoryInfo(Program.GameMode.PluginsPath).GetFiles("ThePitt - *.bsa")) fi.LastWriteTime = new DateTime(2008, 10, 3);
			foreach (FileInfo fi in new DirectoryInfo(Program.GameMode.PluginsPath).GetFiles("BrokenSteel - *.bsa")) fi.LastWriteTime = new DateTime(2008, 10, 4);
			foreach (FileInfo fi in new DirectoryInfo(Program.GameMode.PluginsPath).GetFiles("PointLookout - *.bsa")) fi.LastWriteTime = new DateTime(2008, 10, 5);
			foreach (FileInfo fi in new DirectoryInfo(Program.GameMode.PluginsPath).GetFiles("Zeta - *.bsa")) fi.LastWriteTime = new DateTime(2008, 10, 6);

			NativeMethods.WritePrivateProfileIntA("Archive", "bInvalidateOlderFiles", 1, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath);
			NativeMethods.WritePrivateProfileIntA("General", "bLoadFaceGenHeadEGTFiles", 1, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath);
			NativeMethods.WritePrivateProfileStringA("Archive", "SInvalidationFile", "", ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath);
			File.Delete(Path.Combine(Program.GameMode.PluginsPath, "archiveinvalidation.txt"));
			File.WriteAllBytes(Path.Combine(Program.GameMode.PluginsPath, AiBsa), new byte[] {
                0x42, 0x53, 0x41, 0x00, 0x67, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x00, 0x03, 0x07, 0x00, 0x00,
                0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
                0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                0x36, 0x00, 0x00, 0x00, 0x01, 0x00, 0x61, 0x00, 0x01, 0x61, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x48, 0x00, 0x00, 0x00, 0x61, 0x00
            });
			NativeMethods.WritePrivateProfileStringA("Archive", "SArchiveList", AiBsa + ", " + GetBSAList(), ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath);
		}

		private static void RemoveAI()
		{
			NativeMethods.WritePrivateProfileIntA("Archive", "bInvalidateOlderFiles", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath);
			NativeMethods.WritePrivateProfileIntA("General", "bLoadFaceGenHeadEGTFiles", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath);
			NativeMethods.WritePrivateProfileStringA("Archive", "SInvalidationFile", "ArchiveInvalidation.txt", ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath);
			File.Delete(Path.Combine(Program.GameMode.PluginsPath, AiBsa));
			NativeMethods.WritePrivateProfileStringA("Archive", "SArchiveList", GetBSAList(), ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath);
		}

		public static bool Update()
		{
			if (!File.Exists(((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath))
			{
				MessageBox.Show("You have no Fallout INI file. Please run Fallout to initialize the file.", "Missing INI", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return false;
			}
			if (NativeMethods.GetPrivateProfileIntA("Archive", "bInvalidateOlderFiles", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath) == 0)
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
			return NativeMethods.GetPrivateProfileIntA("Archive", "bInvalidateOlderFiles", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOIniPath) != 0;
		}
	}
}
