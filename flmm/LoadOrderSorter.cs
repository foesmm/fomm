using System;
using System.Collections.Generic;
using System.IO;

namespace Fomm {
    static class LoadOrderSorter {
        private struct ModInfo {
            public readonly string name;
            public double id;
            public readonly bool hadEntry;

            public ModInfo(string s, double id, bool hadEntry) {
                name=s;
                this.id=id;
                this.hadEntry=hadEntry;
            }
        }
        private struct RecordInfo {
            public readonly int id;
            public string[] requires;
            public string[] conflicts;
            public string[] comments;

            public RecordInfo(int id) {
                this.id=id;
                requires=null;
                conflicts=null;
                comments=null;
            }
        }

        private static readonly string localDataPath=Path.Combine(Program.fommDir, "FOLOT.ini");
        private static Dictionary<string, RecordInfo> order;
        private static int duplicateCount;

        private static string[] GetDataFile() {
            return File.ReadAllLines(localDataPath);
        }

        private static void LoadList() {
            string[] fileLines=GetDataFile();
            int upto=0;
            List<string> requires=new List<string>();
            List<string> conflicts=new List<string>();
            List<string> comments=new List<string>();
            order=new Dictionary<string, RecordInfo>(fileLines.Length);
            for(int i=0;i<fileLines.Length;i++) {
                int comment=fileLines[i].IndexOf('\\');
                if(comment!=-1) fileLines[i]=fileLines[i].Remove(comment);
                fileLines[i]=fileLines[i].Trim();
                if(fileLines[i]!=string.Empty) {
                    RecordInfo ri=new RecordInfo(upto++);
                    int skiplines=0;
                    for(int j=i+1;j<fileLines.Length;j++) {
                        fileLines[j]=fileLines[j].Trim();
                        if(fileLines[j].Length>0) {
                            switch(fileLines[j][0]) {
                            case ':':
                                requires.Add(fileLines[j].Substring(1).ToLowerInvariant());
                                skiplines++;
                                continue;
                            case '"':
                                conflicts.Add(fileLines[j].Substring(1).ToLowerInvariant());
                                skiplines++;
                                continue;
                            case '?':
                                comments.Add(fileLines[j].Substring(1));
                                skiplines++;
                                continue;
                            }
                            break;
                        }
                        skiplines++;
                    }
                    if(requires.Count>0) {
                        ri.requires=requires.ToArray();
                        requires.Clear();
                    }
                    if(conflicts.Count>0) {
                        ri.conflicts=conflicts.ToArray();
                        conflicts.Clear();
                    }
                    if(comments.Count>0) {
                        ri.comments=comments.ToArray();
                        comments.Clear();
                    }
                    //order.Add(fileLines[i].ToLowerInvariant(), ri);
                    fileLines[i]=fileLines[i].ToLowerInvariant();
                    if(order.ContainsKey(fileLines[i])) duplicateCount++;
                    order[fileLines[i]]=ri;
                    i+=skiplines;
                }
            }
        }

        private static ModInfo[] BuildModInfo(string[] plugins) {
            ModInfo[] mi=new ModInfo[plugins.Length];
            int addcount=1;
            int lastPos=-1;
            int maxPos=0;
            for(int i=0;i<mi.Length;i++) {
                string lplugin=plugins[i].ToLowerInvariant();
                if(order.ContainsKey(lplugin)) {
                    lastPos=order[lplugin].id;
                    if(lastPos>maxPos) maxPos=lastPos;
                    mi[i]=new ModInfo(plugins[i], lastPos, true);
                    addcount=1;
                } else {
                    mi[i]=new ModInfo(plugins[i], lastPos + addcount*0.001, false);
                    addcount++;
                }
            }
            addcount=1;
            maxPos++;
            for(int i=mi.Length-1;i>=0;i--) {
                if(mi[i].hadEntry) break;
                mi[i].id=maxPos - addcount*0.001;
                addcount++;
            }
            return mi;
        }

        public static string GenerateReport(string[] plugins) {
            if(order==null) LoadList();
            System.Text.StringBuilder sb=new System.Text.StringBuilder(plugins.Length*32);
            string[] lplugins=new string[plugins.Length];
            for(int i=0;i<plugins.Length;i++) lplugins[i]=plugins[i].ToLowerInvariant();
            double latestPosition=0;
            sb.AppendLine("Mod load order report");
            if(duplicateCount>0) sb.AppendLine("! Warning: current load order template contains "+duplicateCount+" duplicate entries");
            sb.AppendLine();
            bool LoadOrderWrong=false;
            for(int i=0;i<plugins.Length;i++) {
                sb.AppendLine(plugins[i]);
                plugins[i]=plugins[i].ToLowerInvariant();
                if(order.ContainsKey(plugins[i])) {
                    RecordInfo ri=order[plugins[i]];
                    if(ri.id<latestPosition) {
                        sb.AppendLine("! The current load order of this mod does not match the current template");
                        LoadOrderWrong=true;
                    } else latestPosition=ri.id;
                    if(ri.requires!=null) {
                        for(int k=0;k<ri.requires.Length;k++) {
                            bool found=false;
                            for(int j=0;j<lplugins.Length;j++) {
                                if(lplugins[j]==ri.requires[k]) {
                                    found=true;
                                    break;
                                }
                            }
                            if(!found) {
                                sb.AppendLine("! This plugin requires '"+ri.requires[k]+"', which was not found");
                            }
                        }
                    }
                    if(ri.conflicts!=null) {
                        for(int k=0;k<ri.conflicts.Length;k++) {
                            for(int j=0;j<lplugins.Length;j++) {
                                if(lplugins[j]==ri.conflicts[k]) {
                                    sb.AppendLine("! This plugin conflicts with '"+ri.conflicts[k]+"'");
                                    break;
                                }
                            }
                        }
                    }
                    if(ri.comments!=null) {
                        for(int k=0;k<ri.comments.Length;k++) {
                            sb.AppendLine("  "+ri.comments[k]);
                        }
                    }
                } else {
                    sb.AppendLine("! This mod does not exist in the current template");
                }
                sb.AppendLine();
            }
            if(LoadOrderWrong) {
                string[] dup=(string[])plugins.Clone();
                SortList(dup);
                sb.AppendLine("The order that the current template suggests is as follows:");
                for(int i=0;i<dup.Length;i++) sb.AppendLine(dup[i]);
            }
            return sb.ToString();
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
            int target=order[plugin].id;
            for(int i=0;i<mi.Length;i++) {
                if(mi[i].id>=target) return i;
            }
            return plugins.Length;
        }
    }
}
