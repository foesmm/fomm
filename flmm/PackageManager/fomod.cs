using System;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Collections.Generic;

namespace fomm.PackageManager {

    [Serializable]
    internal class fomod {

        protected class PrivateData {
            internal ZipFile modFile=null;
            internal System.Drawing.Image image;
        }

        private static void WriteStreamToZip(BinaryWriter bw, Stream input) {
            input.Position=0;
            byte[] buffer=new byte[4096];
            int upto=0;
            while(input.Length-upto>4096) {
                input.Read(buffer, 0, 4096);
                bw.Write(buffer, 0, 4096);
                upto+=4096;
            }
            if(input.Length-upto>0) {
                input.Read(buffer, 0, (int)(input.Length-upto));
                bw.Write(buffer, 0, (int)(input.Length-upto));
            }
        }

        internal static void CreateOmod(omodCreationOptions ops, string omodFileName, ulong groups) {
            ZipOutputStream ZipStream;
            BinaryWriter omodStream;
            ZipEntry ze;
            FileStream DataCompressed;
            Stream DataInfo;
            omodFileName=Settings.omodDir+omodFileName;

            ZipStream=new ZipOutputStream(File.Open(omodFileName, FileMode.CreateNew));
            omodStream=new BinaryWriter(ZipStream);
            try {
                ZipStream.SetLevel(ZipHandler.GetCompressionLevel(ops.omodCompressionLevel));
                //The readme
                if(ops.readme!=""&&ops.readme!=null) {
                    ze=new ZipEntry("readme");
                    ZipStream.PutNextEntry(ze);
                    omodStream.Write(ops.readme);
                    omodStream.Flush();
                }
                //The script
                if(ops.script!=""&&ops.script!=null) {
                    ze=new ZipEntry("script");
                    ZipStream.PutNextEntry(ze);
                    omodStream.Write(ops.script);
                    omodStream.Flush();
                }
                //The image
                if(ops.Image!=""&&ops.Image!=null) {
                    ze=new ZipEntry("image");
                    ZipStream.PutNextEntry(ze);
                    FileStream fs=File.OpenRead(ops.Image);
                    WriteStreamToZip(omodStream, fs);
                    omodStream.Flush();
                    fs.Close();
                }
                //The config file
                ze=new ZipEntry("config");
                ZipStream.PutNextEntry(ze);
                omodStream.Write(Program.CurrentOmodVersion);
                omodStream.Write(ops.Name);
                omodStream.Write(ops.MajorVersion);
                omodStream.Write(ops.MinorVersion);
                omodStream.Write(ops.Author);
                omodStream.Write(ops.email);
                omodStream.Write(ops.website);
                omodStream.Write(ops.Description);
                omodStream.Write(DateTime.Now.ToBinary());
                omodStream.Write((byte)ops.CompressionType);
                omodStream.Write(ops.BuildVersion);
                omodStream.Flush();
                //plugins
                if(ops.esps.Length>0) {
                    GC.Collect();
                    ze=new ZipEntry("plugins.crc");
                    ZipStream.PutNextEntry(ze);
                    CompressionHandler.CompressFiles(ops.esps, ops.espPaths, out DataCompressed, out DataInfo,
                        ops.CompressionType, ops.DataFileCompresionLevel);
                    WriteStreamToZip(omodStream, DataInfo);
                    omodStream.Flush();
                    ZipStream.SetLevel(0);
                    ze=new ZipEntry("plugins");
                    ZipStream.PutNextEntry(ze);
                    WriteStreamToZip(omodStream, DataCompressed);
                    omodStream.Flush();
                    ZipStream.SetLevel(ZipHandler.GetCompressionLevel(ops.omodCompressionLevel));
                    DataCompressed.Close();
                    DataInfo.Close();
                }
                //data files
                if(ops.DataFiles.Length>0) {
                    GC.Collect();
                    ze=new ZipEntry("data.crc");
                    ZipStream.PutNextEntry(ze);
                    CompressionHandler.CompressFiles(ops.DataFiles, ops.DataFilePaths, out DataCompressed, out DataInfo,
                        ops.CompressionType, ops.DataFileCompresionLevel);
                    WriteStreamToZip(omodStream, DataInfo);
                    omodStream.Flush();
                    ZipStream.SetLevel(0);
                    ze=new ZipEntry("data");
                    ZipStream.PutNextEntry(ze);
                    WriteStreamToZip(omodStream, DataCompressed);
                    omodStream.Flush();
                    ZipStream.SetLevel(ZipHandler.GetCompressionLevel(ops.omodCompressionLevel));
                    DataCompressed.Close();
                    DataInfo.Close();
                }
                ZipStream.Finish();
            } finally {
                omodStream.Close();
            }
            //Cleanup
            omod o=Program.LoadNewOmod(omodFileName);
            o.group=groups;
            Conflicts.UpdateConflict(o);
            Program.ClearTempFiles();
            GC.Collect();
        }

        internal static void Remove(string filename) {
            filename=filename.ToLower();
            for(int i=0;i<Program.Data.omods.Count;i++) {
                omod o=Program.Data.omods[i];
                if(o.LowerFileName==filename) {
                    if(o.Conflict==ConflictLevel.Active) {
                        o.DeletionDeactivate();
                    }
                    o.Close();
                    Program.Data.omods.RemoveAt(i--);
                }
            }
        }

        [NonSerialized]
        private PrivateData pd=new PrivateData();
        internal void RecreatePrivateData() { if(pd==null) pd=new PrivateData(); }

        internal readonly string FilePath;
        internal readonly string FileName;
        internal readonly string LowerFileName;
        internal readonly string ModName;
        internal readonly int MajorVersion;
        internal readonly int MinorVersion;
        internal readonly int BuildVersion;
        internal readonly string Description;
        internal readonly string Email;
        internal readonly string Website;
        internal readonly string Author;
        internal readonly DateTime CreationTime;
        internal readonly string[] AllPlugins;
        internal readonly DataFileInfo[] AllDataFiles;
        internal readonly uint CRC;
        internal readonly CompressionType CompType;
        private readonly byte FileVersion;
        private bool hidden=false;
        internal bool Hidden { get { return hidden; } }

        internal ulong group;
        internal string Version {
            get { return ""+MajorVersion+((MinorVersion!=-1)?("."+MinorVersion+((BuildVersion!=-1)?("."+BuildVersion):"")):""); }
        }
        internal string FullFilePath {
            get { if(FilePath==null) return Settings.omodDir+FileName; else return FilePath+FileName; }
        }

        internal string[] Plugins;
        internal DataFileInfo[] DataFiles;
        internal string[] BSAs;
        internal List<INIEditInfo> INIEdits;
        internal List<SDPEditInfo> SDPEdits;

        internal ConflictLevel Conflict=ConflictLevel.NoConflict;

        internal readonly List<ConflictData> ConflictsWith=new List<ConflictData>();
        internal readonly List<ConflictData> DependsOn=new List<ConflictData>();

        private ZipFile ModFile {
            get {
                if(pd.modFile!=null) return pd.modFile;
                pd.modFile=new ZipFile(FullFilePath);
                return pd.modFile;
            }
        }
        internal System.Drawing.Image image {
            get {
                if(pd.image!=null) return pd.image;
                Stream s=ExtractWholeFile("image");
                if(s==null) return null;
                pd.image=System.Drawing.Image.FromStream(s);
                s.Close();
                if(Program.IsImageAnimated(pd.image)) {
                    MessageBox.Show("Animated or multi-resolution images are not supported", "Error");
                    pd.image=null;
                }
                return pd.image;
            }
        }

        internal fomod(string path, bool InOmodDir) {
            BinaryReader br=null;
            try {
                if(InOmodDir) FilePath=null;
                else FilePath=Path.GetDirectoryName(path)+"\\";
                FileName=Path.GetFileName(path);
                LowerFileName=FileName.ToLower();
                Stream Config=ExtractWholeFile("config");
                if(Config==null) throw new obmmException("Could not find omod configuration data");
                br=new BinaryReader(Config);
                byte version=br.ReadByte();
                FileVersion=version;
                if(version>Program.CurrentOmodVersion) {
                    throw new obmmException(FileName+" was created with a newer version of obmm and could not be loaded");
                }
                ModName=br.ReadString();
                MajorVersion=br.ReadInt32();
                MinorVersion=br.ReadInt32();
                Author=br.ReadString();
                Email=br.ReadString();
                Website=br.ReadString();
                Description=br.ReadString();
                if(version>=2) {
                    CreationTime=DateTime.FromBinary(br.ReadInt64());
                } else {
                    string sCreationTime=br.ReadString();
                    if(!DateTime.TryParseExact(sCreationTime, "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out CreationTime)) {
                        CreationTime=new DateTime(2006, 1, 1);
                    }
                }
                if(Description=="") Description="No description";
                CompType=(CompressionType)br.ReadByte();
                if(version>=1) {
                    BuildVersion=br.ReadInt32();
                } else BuildVersion=-1;

                AllPlugins=GetPluginList();
                AllDataFiles=GetDataList();
                foreach(string s2 in AllPlugins) {
                    if(!Program.IsSafeFileName(s2))
                        throw new obmmException("File "+FileName+" appears to have been modified in a way which could cause a security risk."+
                            Environment.NewLine+"obmm will not load it.");
                }
                foreach(DataFileInfo dfi in AllDataFiles) {
                    if(!Program.IsSafeFileName(dfi.FileName))
                        throw new obmmException("File "+FileName+" appears to have been modified in a way which could cause a security risk."+
                            Environment.NewLine+"obmm will not load it.");
                }
                CRC=CompressionHandler.CRC(FullFilePath);
            } finally {
                if(br!=null) br.Close();
                Close();
            }
        }

        internal void Close() {
            if(pd.modFile!=null) {
                pd.modFile.Close();
                pd.modFile=null;
            }
            if(pd.image!=null) {
                pd.image=null;
            }
        }

        private void CreateDirectoryStructure() {
            for(int x=0;x<DataFiles.Length;x++) {
                string s=Path.GetDirectoryName(DataFiles[x].FileName);
                if(!Directory.Exists("data\\"+s)) Directory.CreateDirectory("data\\"+s);
            }
        }

        private ScriptExecutationData ExecuteScript(string plugins, string data) {
            //Execute script
            ScriptReturnData srd=Scripting.ScriptRunner.ExecuteScript(GetScript(), data, plugins);
            bool HasClickedYesToAll;
            bool HasClickedNoToAll;
            //return on fatal error
            if(srd.CancelInstall) return null;
            //Check that any required mods are already active
            foreach(ConflictData cd in srd.DependsOn) {
                if(!Program.Data.DoesModExist(cd, true)) {
                    string s="This mod depends on "+cd.File+", which is either not active or the wrong version";
                    if(cd.Comment!=null) s+="\n"+cd.Comment;
                    MessageBox.Show(s, "Error");
                    return null;
                }
            }
            HasClickedYesToAll=false;
            foreach(ConflictData cd in srd.ConflictsWith) {
                if(Program.Data.DoesModExist(cd, true)) {
                    string s;
                    if(cd.Comment!=null) s="\n"+cd.Comment; else s="";
                    switch(cd.level) {
                    case ConflictLevel.Unusable:
                        MessageBox.Show("This mod has a fatal script defined conflict with "+cd.File+" and cant be activated."+s, "Error");
                        return null;
                    case ConflictLevel.MajorConflict:
                        s="This mod has a script defined major conflict with "+cd.File+s+"\nAre you sure you wish to activate it?";
                        break;
                    case ConflictLevel.MinorConflict:
                        s="This mod has a script defined minor conflict with "+cd.File+s+"\nAre you sure you wish to activate it?";
                        break;
                    }
                    if(!HasClickedYesToAll&&MessageBox.Show(s, "Warning", MessageBoxButtons.YesNo)!=DialogResult.Yes) return null;
                    else {
                        if((System.Windows.Forms.Control.ModifierKeys&Keys.Control)>0) HasClickedYesToAll=true;
                    }
                }
            }
            //Update conflicts and dependencies
            ConflictsWith.Clear();
            ConflictsWith.AddRange(srd.ConflictsWith);
            DependsOn.Clear();
            DependsOn.AddRange(srd.DependsOn);
            //Create some variables
            List<string> strtemp1=new List<string>();
            //Fill the Plugins array with the plugins that need to be installed
            if(srd.InstallAllPlugins) {
                foreach(string s in AllPlugins) if(!s.Contains("\\")) strtemp1.Add(s);
            }
            foreach(string s in srd.InstallPlugins) { if(!Program.strArrayContains(strtemp1, s)) strtemp1.Add(s); }
            foreach(string s in srd.IgnorePlugins) { Program.strArrayRemove(strtemp1, s); }
            foreach(ScriptCopyDataFile scd in srd.CopyPlugins) {
                if(!File.Exists(plugins+scd.CopyFrom)) {
                    MessageBox.Show("The script attempted to copy the plugin '"+scd.hCopyFrom+"', but it did not exist", "Warning");
                } else {
                    if(scd.CopyFrom!=scd.CopyTo) {
                        if(File.Exists(plugins+scd.CopyTo)) File.Delete(plugins+scd.CopyTo);
                        File.Copy(plugins+scd.CopyFrom, plugins+scd.hCopyTo);
                    }
                    if(!Program.strArrayContains(strtemp1, scd.CopyTo)) strtemp1.Add(scd.hCopyTo);
                }
            }
            for(int i=0;i<strtemp1.Count;i++) if(!File.Exists(plugins+strtemp1[i])) strtemp1.RemoveAt(i--);
            Plugins=strtemp1.ToArray();
            strtemp1.Clear();
            //Fill the data file array with data files to be installed
            if(srd.InstallAllData) {
                for(int i=0;i<AllDataFiles.Length;i++) strtemp1.Add(AllDataFiles[i].FileName);
            }
            foreach(string s in srd.InstallData) { if(!Program.strArrayContains(strtemp1, s)) strtemp1.Add(s); }
            foreach(string s in srd.IgnoreData) { Program.strArrayRemove(strtemp1, s); }
            foreach(ScriptCopyDataFile scd in srd.CopyDataFiles) {
                if(!File.Exists(data+scd.CopyFrom)) {
                    MessageBox.Show("The script attempted to copy the data file '"+scd.hCopyFrom+"', but it did not exist", "Warning");
                } else {
                    if(scd.CopyFrom!=scd.CopyTo) {
                        if(!Directory.Exists(Path.GetDirectoryName(data+scd.CopyTo))) Directory.CreateDirectory(Path.GetDirectoryName(data+scd.hCopyTo));
                        if(File.Exists(data+scd.CopyTo)) File.Delete(data+scd.CopyTo);
                        File.Copy(data+scd.CopyFrom, data+scd.hCopyTo);
                    }
                    if(!Program.strArrayContains(strtemp1, scd.CopyTo)) strtemp1.Add(scd.hCopyTo);
                }
            }
            for(int i=0;i<strtemp1.Count;i++) if(!File.Exists(data+strtemp1[i])) strtemp1.RemoveAt(i--);
            List<DataFileInfo> dtemp1 = new List<DataFileInfo>();
            foreach(string s in strtemp1) {
                DataFileInfo dfi;//=Program.Data.GetDataFile(s);
                dfi=Program.strArrayGet(AllDataFiles, s);
                if(dfi!=null) dtemp1.Add(new DataFileInfo(dfi));
                else dtemp1.Add(new DataFileInfo(s, CompressionHandler.CRC(data+s)));
            }
            DataFiles=dtemp1.ToArray();
            strtemp1.Clear();
            dtemp1.Clear();
            //Register BSAs
            foreach(string s in srd.RegisterBSAList) {
                strtemp1.Add(s);
                BSA b=Program.Data.GetBSA(s);
                if(b==null) {
                    OblivionBSA.RegisterBSA(s);
                    Program.Data.BSAs.Add(new BSA(s, FileName));
                } else {
                    b.UsedBy.Add(FileName);
                }
            }
            BSAs=strtemp1.ToArray();
            //Edit oblivion.ini files
            INIEdits=new List<INIEditInfo>();
            HasClickedNoToAll=false;
            HasClickedYesToAll=false;
            foreach(INIEditInfo iei in srd.INIEdits) {
                if(HasClickedNoToAll||(!HasClickedYesToAll&&Settings.WarnOnINIEdit&&MessageBox.Show(FileName+" wants to edit oblivion.ini.\n"+
                    "Section: "+iei.Section+"\n"+
                    "Key: "+iei.Name+"\n"+
                    "New value: "+iei.NewValue+"\n"+
                    "Do you want to allow it?", "Warning", MessageBoxButtons.YesNo)!=DialogResult.Yes)) {
                    if((System.Windows.Forms.Control.ModifierKeys&Keys.Control)>0) HasClickedNoToAll=true;
                    continue;
                } else {
                    if((System.Windows.Forms.Control.ModifierKeys&Keys.Control)>0) HasClickedYesToAll=true;
                }
                if(Program.Data.INIEdits.Contains(iei)) {
                    INIEditInfo edit=Program.Data.INIEdits[Program.Data.INIEdits.IndexOf(iei)];
                    if(HasClickedNoToAll||(!HasClickedYesToAll&&MessageBox.Show("The omod '"+edit.Plugin.FileName+"' has already edited '"+iei.Section+" "+iei.Name+"'\n"+
                        "Do you want to overwrite this change?\n"+
                        "Original value: "+edit.OldValue+"\n"+
                        "Current value: "+edit.NewValue+"\n"+
                        "New value: "+iei.NewValue, "Warning", MessageBoxButtons.YesNo)!=DialogResult.Yes)) {
                        if((System.Windows.Forms.Control.ModifierKeys&Keys.Control)>0) HasClickedNoToAll=true;
                        continue;
                    } else {
                        if((System.Windows.Forms.Control.ModifierKeys&Keys.Control)>0) HasClickedYesToAll=true;
                    }
                    iei.OldValue=edit.OldValue;
                    if(edit.Plugin.INIEdits!=null) edit.Plugin.INIEdits.Remove(edit);
                    Program.Data.INIEdits.Remove(edit);
                } else {
                    iei.OldValue=OblivionINI.GetINIValue(iei.Section, iei.Name);
                }
                iei.Plugin=this;
                OblivionINI.WriteINIValue(iei.Section, iei.Name, iei.NewValue);
                Program.Data.INIEdits.Add(iei);
                INIEdits.Add(iei);
            }

            if(INIEdits.Count==0) INIEdits=null;
            //Edit shader files
            SDPEdits=new List<SDPEditInfo>();
            foreach(SDPEditInfo sei in srd.SDPEdits) {
                if(Classes.OblivionSDP.EditShader(sei.Package, sei.Shader, sei.BinaryObject, FileName)) SDPEdits.Add(sei);
                sei.BinaryObject=null;
            }
            if(SDPEdits.Count==0) SDPEdits=null;
            //return
            ScriptExecutationData sed=new ScriptExecutationData();
            sed.PluginOrder=srd.LoadOrderList.ToArray();
            sed.UncheckedPlugins=srd.UncheckedPlugins.ToArray();
            sed.EspDeactivationWarning=srd.EspDeactivation.ToArray();
            sed.EspEdits=srd.EspEdits.ToArray();
            sed.EarlyPlugins=srd.EarlyPlugins.ToArray();
            return sed;
        }

        internal void Activate(bool warn) {
            hidden=false;
            bool HasClickedYesToAll;
            bool HasClickedNoToAll;
            //Extract plugins and data files
            string PluginsPath=GetPlugins();
            string DataPath=GetDataFiles();
            //Run the attached script
            ScriptExecutationData sed=ExecuteScript(PluginsPath, DataPath);
            if(sed==null) return;
            //Final check for serious conflicts
            HasClickedYesToAll=false;
            HasClickedNoToAll=false;
            for(int i=0;i<Plugins.Length;i++) {
                if(Program.Data.DoesEspExist(Plugins[i])||File.Exists("data\\"+Plugins[i])) {
                    if(Array.IndexOf<string>(Program.BannedFiles, Plugins[i].ToLower())!=-1) {
                        if(warn) MessageBox.Show(Plugins[i]+" is a protected oblivion base file and cannot be overwritten by mods", "Error");
                        Program.ArrayRemoveAt<string>(ref Plugins, i--);
                        continue;
                    } else if(HasClickedNoToAll||(!HasClickedYesToAll&&warn&&MessageBox.Show(Plugins[i]+" already exists!\n"+
                            "Overwrite?", "Warning", MessageBoxButtons.YesNo)!=DialogResult.Yes)) {
                        if((System.Windows.Forms.Control.ModifierKeys&Keys.Control)>0) HasClickedNoToAll=true;
                        Program.ArrayRemoveAt<string>(ref Plugins, i--);
                        continue;
                    } else {
                        if((System.Windows.Forms.Control.ModifierKeys&Keys.Control)>0) HasClickedYesToAll=true;
                        EspInfo ei=Program.Data.GetEsp(Plugins[i]);
                        if(ei!=null) {
                            if(ei.Parent!=null) ei.Parent.UnlinkPlugin(ei);
                            Program.Data.Esps.Remove(ei);
                            File.Delete("data\\"+ei.FileName);
                        } else File.Delete("data\\"+Plugins[i]);
                    }
                }
            }
            //copy plugins
            for(int i=0;i<Plugins.Length;i++) {
                File.Move(PluginsPath+Plugins[i], "data\\"+Plugins[i]);
                EspInfo ei=new EspInfo(Plugins[i], this);
                Program.Data.InsertESP(ei, sed.PluginOrder, Array.IndexOf<string>(sed.EarlyPlugins, ei.LowerFileName)!=-1);
                foreach(string s in sed.UncheckedPlugins) {
                    if(s==Plugins[i].ToLower()) ei.Active=false;
                }
                foreach(ScriptEspWarnAgainst s in sed.EspDeactivationWarning) {
                    if(s.Plugin==Plugins[i].ToLower()) ei.Deactivatable=s.Status;
                }
                foreach(ScriptEspEdit see in sed.EspEdits) {
                    if(see.Plugin!=Plugins[i].ToLower()) continue;
                    try {
                        if(see.IsGMST) ConflictDetector.TesFile.SetGMST("data\\"+see.Plugin, see.EDID, see.Value);
                        else ConflictDetector.TesFile.SetGLOB("data\\"+see.Plugin, see.EDID, see.Value);
                    } catch(Exception ex) {
                        MessageBox.Show("An error occured editing plugin "+see.Plugin+"\n"+
                        ex.Message, "Error");
                    }
                }
            }
            //copy data files
            HasClickedYesToAll=false;
            HasClickedNoToAll=false;
            CreateDirectoryStructure();
            for(int i=0;i<DataFiles.Length;i++) {
                DataFileInfo dfi=null;
                if(Program.Data.DoesDataFileExist(DataFiles[i]) || File.Exists("data\\"+DataFiles[i].FileName)) {
                    if(Array.IndexOf<string>(Program.BannedFiles, DataFiles[i].LowerFileName)!=-1) {
                        if(warn) MessageBox.Show(DataFiles[i].FileName+" is a protected oblivion base file and cannot be overwritten by mods", "Error");
                        Program.ArrayRemoveAt<DataFileInfo>(ref DataFiles, i--);
                        continue;
                    }
                    dfi=Program.Data.GetDataFile(DataFiles[i]);
                    uint CRC=DataFiles[i].CRC;
                    string owners=null;
                    if(dfi!=null) {
                        owners=dfi.Owners;
                        dfi.AddOwner(this);
                        DataFiles[i]=dfi;
                    }
                    if(dfi==null||dfi.CRC!=CRC) {
                        string msg;
                        if(dfi==null) {
                            msg=DataFiles[i].FileName+" already exists, and is not associated with an omod.\n"+
                                "Overwrite?";
                        } else {
                            msg=DataFiles[i].FileName+" already exists, and CRCs do not match.\n"+
                                "Currently associated omods: "+owners+"\n"+
                                "Overwrite?";
                        }
                        if(HasClickedNoToAll||(!HasClickedYesToAll&&warn&&MessageBox.Show(msg, "Warning", MessageBoxButtons.YesNo)!=DialogResult.Yes)) {
                            if((System.Windows.Forms.Control.ModifierKeys&Keys.Control)>0) HasClickedNoToAll=true;
                            if(dfi==null) {
                                Program.ArrayRemoveAt<DataFileInfo>(ref DataFiles, i--);
                            }
                            continue;
                        } else {
                            if((System.Windows.Forms.Control.ModifierKeys&Keys.Control)>0) HasClickedYesToAll=true;
                            if(dfi!=null) dfi.CRC=CRC;
                        }
                    }
                    File.Delete("data\\"+DataFiles[i].FileName);
                } else {
                    Program.Data.DataFiles.Add(DataFiles[i]);
                }
                DataFiles[i].AddOwner(this);
                File.Move(DataPath+DataFiles[i].FileName, "data\\"+DataFiles[i].FileName);
            }
            //mark the mod as active, update other mods conflicts and return
            Conflict=ConflictLevel.Active;
        }

        internal void AquisitionActivate(bool force) {
            List<DataFileInfo> dataFiles=new List<DataFileInfo>();
            for(int i=0;i<AllDataFiles.Length;i++) {
                if(File.Exists("Data\\"+AllDataFiles[i].FileName)) {
                    DataFileInfo dfi=Program.Data.GetDataFile(AllDataFiles[i]);
                    if(dfi==null) {
                        dfi=new DataFileInfo(AllDataFiles[i]);
                        Program.Data.DataFiles.Add(dfi);
                    }
                    dfi.AddOwner(this);
                    dataFiles.Add(dfi);
                }
            }
            DataFiles=dataFiles.ToArray();

            List<string> plugins=new List<string>();
            for(int i=0;i<AllPlugins.Length;i++) {
                EspInfo ei=Program.Data.GetEsp(AllPlugins[i]);
                if(ei!=null&&ei.Parent==null) {
                    ei.Parent=this;
                    ei.BelongsTo=FileName;
                    plugins.Add(ei.LowerFileName);
                }
            }
            Plugins=plugins.ToArray();

            if(force||dataFiles.Count>0||plugins.Count>0) {
                hidden=false;
                Conflict=ConflictLevel.Active;
            }

            BSAs=new string[0];
            INIEdits=new List<INIEditInfo>();
            SDPEdits=new List<SDPEditInfo>();
        }

        internal bool Deactivate(bool Force) {
            if(Conflict!=ConflictLevel.Active) return true;
            //Check for any dependent mods that are still active
            foreach(omod o in Program.Data.omods) {
                if(o.Conflict!=ConflictLevel.Active||o==this) continue;
                foreach(ConflictData cd in o.DependsOn) {
                    if(cd==this) {
                        if(Force) {
                            o.Deactivate(true);
                            break;
                        } else {
                            if(MessageBox.Show("Active mod "+o.FileName+" depends on "+FileName+" and must also be deactivated.\n"+
                            "Deactivate "+o.FileName+"?", "Question", MessageBoxButtons.YesNo)==DialogResult.Yes) {
                                if(!o.Deactivate(false)) return false; else break;
                            } else return false;
                        }
                    }
                }
            }
            try {
                //Deactivate the mod
                Conflict=ConflictLevel.NoConflict;
                ConflictsWith.Clear();
                DependsOn.Clear();
                //Undo any ini or shader edits
                if(INIEdits!=null) {
                    foreach(INIEditInfo iei in INIEdits) {
                        OblivionINI.WriteINIValue(iei.Section, iei.Name, iei.OldValue);
                        Program.Data.INIEdits.Remove(iei);
                    }
                    INIEdits=null;
                }
                if(SDPEdits!=null) {
                    foreach(SDPEditInfo sei in SDPEdits) {
                        Classes.OblivionSDP.RestoreShader(sei.Package, sei.Shader);
                    }
                    SDPEdits=null;
                }
                //Clear out the plugins
                foreach(string s in Plugins) {
                    EspInfo ei=Program.Data.GetEsp(s);
                    if(ei!=null) Program.Data.Esps.Remove(ei);
                    File.Delete("data\\"+s);
                }
                //Clear out the data files
                foreach(DataFileInfo dfi in DataFiles) dfi.RemoveOwner(this);
                DataFiles=new DataFileInfo[0];
                //Unregister BSAs
                for(int i=0;i<Program.Data.BSAs.Count;i++) {
                    BSA b=(BSA)Program.Data.BSAs[i];
                    if(b.UsedBy.Contains(FileName)) b.UsedBy.Remove(FileName);
                    if(b.UsedBy.Count==0) {
                        OblivionBSA.UnregisterBSA(b.FileName);
                        Program.Data.BSAs.RemoveAt(i--);
                    }
                }
            } catch(Exception ex) {
                if(Force) return true;
                MessageBox.Show("An error occured trying to deactivate "+FileName+".\n"+ex.Message, "Error");
                return false;
            }
            return true;
        }

        internal void DeletionDeactivate() {
            //Undo any ini or shader edits
            if(INIEdits!=null) {
                foreach(INIEditInfo iei in INIEdits) {
                    OblivionINI.WriteINIValue(iei.Section, iei.Name, iei.OldValue);
                    Program.Data.INIEdits.Remove(iei);
                }
                INIEdits=null;
            }
            if(SDPEdits!=null) {
                foreach(SDPEditInfo sei in SDPEdits) {
                    Classes.OblivionSDP.RestoreShader(sei.Package, sei.Shader);
                }
                SDPEdits=null;
            }
            //Clear out the plugins
            foreach(string s in Plugins) {
                EspInfo ei=Program.Data.GetEsp(s);
                if(ei!=null) Program.Data.Esps.Remove(ei);
                if(File.Exists(Path.Combine("data", s))) File.Delete(Path.Combine("data", s));
            }
            //Clear out the data files
            foreach(DataFileInfo dfi in DataFiles) dfi.RemoveOwner(this);

            //Clear some internal data
            ConflictsWith.Clear();
            DependsOn.Clear();
            Plugins=null;
            DataFiles=null;

            //make sure this mod doesn't exist in the main data list
            if(Program.Data.omods.Contains(this)) Program.Data.omods.Remove(this);
        }

        internal void Clean() {
            int a=0, b=0, c=0;
            Clean(ref a, ref b, ref c);
        }
        internal void Clean(ref int DeletedCount, ref int SkippedCount, ref int NotFoundCount) {
            //scan for ghost data file links
            //Not required anymore?
            //for(int i=0;i<Program.Data.DataFiles.Count;i++) {
            //    DataFileInfo dfi=Program.Data.DataFiles[i];
            //    if(dfi.UsedBy.Contains(LowerFileName)) dfi.UsedBy.Remove(LowerFileName);
            //    if(dfi.UsedBy.Count==0) Program.Data.DataFiles.RemoveAt(i--);
            //}
            //delete plugins
            foreach(string s in AllPlugins) {
                if(!File.Exists("data\\"+s)) {
                    NotFoundCount++;
                    continue;
                }
                if(Array.IndexOf<string>(Program.BannedFiles, s.ToLower())!=-1) {
                    SkippedCount++;
                    continue;
                }
                EspInfo ei=Program.Data.GetEsp(s);
                if(ei==null||ei.BelongsTo==EspInfo.UnknownOwner) {
                    File.Delete("data\\"+s);
                    if(ei!=null) Program.Data.Esps.Remove(ei);
                    DeletedCount++;
                } else SkippedCount++;
            }
            //delete data files
            for(int i=0;i<AllDataFiles.Length;i++) {
                string s=AllDataFiles[i].FileName;
                if(!File.Exists("data\\"+s)) {
                    NotFoundCount++;
                    continue;
                }
                if(Array.IndexOf<string>(Program.BannedFiles, s.ToLower())!=-1) {
                    SkippedCount++;
                    continue;
                }
                if(!Program.Data.DoesDataFileExist(s)) {
                    File.Delete("data\\"+s);
                    DeletedCount++;
                } else SkippedCount++;
            }
        }

        private string[] GetPluginList() {
            Stream TempStream=ExtractWholeFile("plugins.crc");
            if(TempStream==null) return new string[0];
            BinaryReader br=new BinaryReader(TempStream);
            List<string> ar=new List<string>();
            while(br.PeekChar()!=-1) {
                ar.Add(br.ReadString());
                br.ReadInt32();
                br.ReadInt64();
            }
            br.Close();
            return ar.ToArray();
        }

        private DataFileInfo[] GetDataList() {
            Stream TempStream=ExtractWholeFile("data.crc");
            if(TempStream==null) return new DataFileInfo[0];
            BinaryReader br=new BinaryReader(TempStream);
            List<DataFileInfo> ar=new List<DataFileInfo>();
            while(br.PeekChar()!=-1) {
                string s=br.ReadString();
                ar.Add(new DataFileInfo(s, br.ReadUInt32()));
                br.ReadInt64();
            }
            br.Close();
            return ar.ToArray();
        }

        internal string GetPlugins() {
            return ParseCompressedStream("plugins.crc", "plugins");
        }

        internal string GetDataFiles() {
            return ParseCompressedStream("data.crc", "data");
        }

        internal string GetReadme() {
            Stream s=ExtractWholeFile("readme");
            if(s==null) return null;
            BinaryReader br=new BinaryReader(s);
            string readme=br.ReadString();
            br.Close();
            return readme;
        }

        internal string GetScript() {
            Stream s=ExtractWholeFile("script");
            if(s==null) return null;
            BinaryReader br=new BinaryReader(s);
            string script=br.ReadString();
            br.Close();
            return script;
        }

        internal string GetImage() {
            string BitmapPath="";
            Stream s=ExtractWholeFile("image", ref BitmapPath);
            if(s==null) BitmapPath=null;
            else s.Close();
            return BitmapPath;
        }

        internal string GetInfo() {
            string n=Environment.NewLine;
            string report=FileName+n+n+"[basic info]"+n+"Name: "+ModName+n;
            report+=n+"Author: "+Author+n+"version: "+Version+
            	n+"Contact: "+Email+n+"Website: "+Website+n+n+Description+n;
            report+=n+"Date this omod was compiled: "+CreationTime.ToString();
            report+=n+"Contains readme: ";
            if(DoesReadmeExist()) report+="yes"; else report+="no";
            report+=n+"Contains script: ";
            if(DoesScriptExist()) report+="yes"; else report+="no";
            report+=n+n+"[omod file information]"+n;
            FileInfo fi;
            fi=new FileInfo(FullFilePath);
            if(fi.Length<8192) { //8kb
                report+="File size: "+fi.Length.ToString()+" bytes"+n;
            } else if(fi.Length<8388608) { //8mb
                report+="File size: "+(fi.Length/1024).ToString()+" kilobytes"+n;
            } else {
                report+="File size: "+(fi.Length/1048576).ToString()+" megabytes"+n;
            }
            report+="Internal omod file version: "+FileVersion+n;
            report+="CRC: "+CRC.ToString("x").ToUpper().PadLeft(8, '0')+n;
            report+="Created or installed: "+fi.CreationTime.ToShortDateString()+" "+fi.CreationTime.ToShortTimeString()+n;
            report+="Last modified: "+fi.LastWriteTime.ToShortDateString()+" "+fi.LastWriteTime.ToShortTimeString()+n;
            if(AllPlugins!=null&&AllPlugins.Length>0) {
                report+=n+"[Complete plugin list]"+n;
                foreach(string s in AllPlugins) report+=s+n;
            }
            if(AllDataFiles!=null&&AllDataFiles.Length>0) {
                report+=n+"[Complete data file list]"+n;
                foreach(DataFileInfo df in AllDataFiles)
                    report+=df.FileName.PadRight(80)+" ("+df.CRC.ToString("x").ToUpper().PadLeft(8, '0')+")"+n;
            }
            if(Conflict==ConflictLevel.Active) {
                if(Plugins!=null&&Plugins.Length>0) {
                    report+=n+"[Currently installed plugin list]"+n;
                    foreach(string s in Plugins) report+=s+n;
                }
                if(DataFiles!=null&&DataFiles.Length>0) {
                    report+=n+"[Currently installed data files]"+n;
                    foreach(DataFileInfo dfi in DataFiles) report+=dfi.FileName+n;
                }
                if(BSAs!=null&&BSAs.Length>0) {
                    report+=n+"[Registered BSAs]"+n;
                    foreach(string s in BSAs) report+=s+n;
                }
                if(INIEdits!=null&&INIEdits.Count>0) {
                    report+=n+"[Oblivion.ini edits]"+n;
                    foreach(INIEditInfo iei in INIEdits) report+=iei.Section+" "+iei.Name+": "+iei.NewValue+" (Changed from "+iei.OldValue+")"+n;
                }
                if(SDPEdits!=null&&SDPEdits.Count>0) {
                    report+=n+"[Shader package edits]"+n;
                    foreach(SDPEditInfo sei in SDPEdits) report+="Shader '"+sei.Shader+"' in package "+sei.Package+n;
                }
            }
            return report;
        }

        internal bool DoesReadmeExist() {
            ZipEntry ze=ModFile.GetEntry("readme");
            return (ze!=null);
        }

        internal bool DoesScriptExist() {
            ZipEntry ze=ModFile.GetEntry("script");
            return (ze!=null);
        }

        private string ParseCompressedStream(string fileList, string compressedStream) {
            string path;
            Stream FileList=ExtractWholeFile(fileList);
            if(FileList==null) return null;
            Stream CompressedStream=ExtractWholeFile(compressedStream);
            path=CompressionHandler.DecompressFiles(FileList, CompressedStream, CompType);
            FileList.Close();
            CompressedStream.Close();
            return path;
        }

        internal string GetConfig() {
            string s="";
            Stream st=ExtractWholeFile("config", ref s);
            st.Close();
            return s;
        }

        private Stream ExtractWholeFile(string s) {
            string s2=null;
            return ExtractWholeFile(s, ref s2);
        }
        private Stream ExtractWholeFile(string s, ref string path) {
            ZipEntry ze=ModFile.GetEntry(s);
            if(ze==null) return null;
            return ExtractWholeFile(ze, ref path);
        }
        private Stream ExtractWholeFile(ZipEntry ze, ref string path) {
            Stream file=ModFile.GetInputStream(ze);
            Stream TempStream;
            if(path!=null||ze.Size>Settings.MaxMemoryStreamSize) {
                TempStream=Program.CreateTempFile(out path);
            } else {
                TempStream=new MemoryStream((int)ze.Size);
            }
            byte[] buffer=new byte[4096];
            int i;
            while((i=file.Read(buffer, 0, 4096))>0) {
                TempStream.Write(buffer, 0, i);
            }
            TempStream.Position=0;
            return TempStream;
        }

        private void ReplaceFileInOmod(string file, string contents) {
            Close();
            FastZip fz=new FastZip();
            string dir=Program.CreateTempDirectory();
            fz.ExtractZip(Settings.omodDir+FileName, dir, null);
            if(contents==null||contents=="") {
                File.Delete(dir+file);
            } else {
                BinaryWriter bw=new BinaryWriter(File.Open(dir+file, FileMode.Create));
                bw.Write(contents);
                bw.Close();
            }
            string dir2=Program.CreateTempDirectory();
            fz.CreateZip(dir2+FileName, dir, false, null);
            File.Delete(Settings.omodDir+FileName);
            File.Move(dir2+FileName, Settings.omodDir+FileName);
        }

        internal void ReplaceReadme(string readme) {
            ReplaceFileInOmod("readme", readme);
        }

        internal void ReplaceScript(string script) {
            ReplaceFileInOmod("script", script.Length>1?script:null);
        }

        internal void Hide() {
            if(pd.image!=null) {
                pd.image.Dispose();
                pd.image=null;
            }
            hidden=true;
        }
        internal void Show() {
            hidden=false;
        }

        internal void UnlinkPlugin(EspInfo ei) {
            if(Plugins!=null) {
                List<string> esps=new List<string>(Plugins);
                Program.strArrayRemove(esps, ei.LowerFileName);
                Plugins=esps.ToArray();
            }
        }

        internal void UnlinkDataFile(DataFileInfo dfi) {
            if(DataFiles!=null) {
                List<DataFileInfo> files=new List<DataFileInfo>(DataFiles);
                Program.strArrayRemove(files, dfi.LowerFileName);
                DataFiles=files.ToArray();
            }
        }

    }
}