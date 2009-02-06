using System;
using System.IO;
using System.Collections.Generic;
using MessageBox=System.Windows.Forms.MessageBox;
using MessageBoxButtons=System.Windows.Forms.MessageBoxButtons;
using DialogResult=System.Windows.Forms.DialogResult;

namespace Fomm {
    static class ArchiveInvalidation {
        /*private static void GenAItext() {
            string ArchiveInvalidationFile="ArchiveInvalidation.txt";
            StreamWriter sw=new StreamWriter(ArchiveInvalidationFile, false, System.Text.Encoding.Default);
            string Dir=Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory("Data");
            string[] paths=Directory.GetFiles(".", "*.dds", SearchOption.AllDirectories);
            foreach(string path in paths) {
                if(path.Length>6&&path[path.Length-6]=='_') continue;
                sw.WriteLine(path.Replace('/', '\\').ToLowerInvariant().Substring(2));
            }
            sw.Close();
            Directory.SetCurrentDirectory(Dir);
        }*/

        private const string AiBsa="ArchiveInvalidationInvalidated!.bsa";
        private const string BsaPath="data\\"+AiBsa;

        private static string GetBSAList() {
            List<string> bsas=new List<string>(NativeMethods.GetPrivateProfileString("Archive", "SArchiveList", null, Program.FOIniPath).Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries));
            for(int i=0;i<bsas.Count;i++) {
                bsas[i]=bsas[i].Trim(' ');
                if(bsas[i]==AiBsa) bsas.RemoveAt(i--);
            }
            return string.Join(", ", bsas.ToArray());
        }

        private static void ApplyAI() {
            NativeMethods.WritePrivateProfileIntA("Archive", "bInvalidateOlderFiles", 1, Program.FOIniPath);
            File.Delete("data\\archiveinvalidation.txt");
            File.WriteAllBytes(BsaPath, new byte[] {
                0x42, 0x53, 0x41, 0x00, 0x67, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x00, 0x03, 0x07, 0x00, 0x00,
                0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
                0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                0x36, 0x00, 0x00, 0x00, 0x01, 0x00, 0x61, 0x00, 0x01, 0x61, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x48, 0x00, 0x00, 0x00, 0x61, 0x00
            });
            NativeMethods.WritePrivateProfileStringA("Archive", "SArchiveList", AiBsa+", "+GetBSAList(), Program.FOIniPath);
        }

        private static void RemoveAI() {
            NativeMethods.WritePrivateProfileIntA("Archive", "bInvalidateOlderFiles", 0, Program.FOIniPath);
            File.Delete(BsaPath);
            NativeMethods.WritePrivateProfileStringA("Archive", "SArchiveList", GetBSAList(), Program.FOIniPath);
        }

        public static void Update() {
            if(NativeMethods.GetPrivateProfileIntA("Archive", "bInvalidateOlderFiles", 0, Program.FOIniPath)==0) {
                if(MessageBox.Show("Apply archive invalidation?", "", MessageBoxButtons.YesNo)==DialogResult.Yes) ApplyAI();
            } else {
                if(MessageBox.Show("Remove archive invalidation?", "", MessageBoxButtons.YesNo)==DialogResult.Yes) RemoveAI();
            }
        }
    }
}
