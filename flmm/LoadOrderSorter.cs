using System;
using System.Collections.Generic;
using System.IO;

namespace Fomm {
    static class LoadOrderSorter {
        private struct ModInfo {
            public readonly string name;
            public readonly double id;
            public readonly bool hadEntry;

            public ModInfo(string s, double id, bool hadEntry) {
                name=s;
                this.id=id;
                this.hadEntry=hadEntry;
            }
        }

        private static readonly string localDataPath=Path.Combine(Program.fommDir, "FOLOT.ini");
        private static Dictionary<string, int> order;

        private static string[] GetDataFile() {
            return File.ReadAllLines(localDataPath);
        }

        private static void LoadList() {
            string[] fileLines=GetDataFile();
            int upto=0;
            order=new Dictionary<string, int>(fileLines.Length);
            for(int i=0;i<fileLines.Length;i++) {
                int comment=fileLines[i].IndexOf(';');
                if(comment!=-1) fileLines[i]=fileLines[i].Remove(comment);
                fileLines[i]=fileLines[i].Trim();
                if(fileLines[i]!=string.Empty) {
                    order.Add(fileLines[i].ToLowerInvariant(), upto++);
                }
            }
        }

        private static ModInfo[] BuildModInfo(string[] plugins) {
            ModInfo[] mi=new ModInfo[plugins.Length];
            int addcount=1;
            int lastPos=-1;
            for(int i=0;i<mi.Length;i++) {
                string lplugin=plugins[i].ToLowerInvariant();
                if(order.ContainsKey(lplugin)) {
                    lastPos=order[lplugin];
                    mi[i]=new ModInfo(plugins[i], lastPos, true);
                    addcount=1;
                } else {
                    mi[i]=new ModInfo(plugins[i], lastPos + addcount*0.001, false);
                    addcount++;
                }
            }
            return mi;
        }

        public static void SortList(string[] plugins) {
            if(order==null) LoadList();
            ModInfo[] mi=BuildModInfo(plugins);
            Array.Sort<ModInfo>(mi, delegate(ModInfo a, ModInfo b) { return a.id.CompareTo(b.id); });
            for(int i=0;i<mi.Length;i++) plugins[i]=mi[i].name;
        }

        public static bool CheckList(string[] plugins) {
            if(order==null) LoadList();
            ModInfo[] mi=BuildModInfo(plugins);
            double upto=0;
            for(int i=0;i<mi.Length;i++) {
                if(mi[i].id<upto) return false;
                else upto=mi[i].id;
            }
            return true;
        }

        public static int GetInsertionPos(string[] plugins, string plugin) {
            if(order==null) LoadList();
            plugin=plugin.ToLowerInvariant();
            if(!order.ContainsKey(plugin)) return plugins.Length;
            ModInfo[] mi=BuildModInfo(plugins);
            int target=order[plugin];
            for(int i=0;i<mi.Length;i++) {
                if(mi[i].id>=target) return i;
            }
            return plugins.Length;
        }
    }
}
