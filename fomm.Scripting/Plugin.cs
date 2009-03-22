using System;
using System.IO;
using System.Collections.Generic;

namespace fomm.Scripting {
    public abstract class BaseRecord {
        protected internal Fomm.TESsnip.BaseRecord Base;

        public string Name { get { return Base.Name; } set { Base.Name=value; } }

        public long Size { get { return Base.Size; } }
        public long Size2 { get { return Base.Size2; } }

        public abstract void DeleteRecord(BaseRecord br);
        public abstract void AddRecord(BaseRecord br);

        public abstract BaseRecord Clone();
    }

    public sealed class Plugin : BaseRecord {
        internal new readonly Fomm.TESsnip.Plugin Base;

        public readonly List<Rec> Records=new List<Rec>();

        public override void DeleteRecord(BaseRecord br) {
            Base.DeleteRecord(br.Base);
            Records.Remove((Rec)br);
        }
        public override void AddRecord(BaseRecord br) {
            Base.AddRecord(br.Base);
            Records.Add((Rec)br);
        }

        public Plugin(byte[] data) : this(data, "") { }
        public Plugin(byte[] data, string name) {
            Base=new Fomm.TESsnip.Plugin(data, name);
            base.Base=Base;
            for(int i=0;i<Base.Records.Count;i++) {
                if(Base.Records[i] is Fomm.TESsnip.GroupRecord) Records.Add(new GroupRecord((Fomm.TESsnip.GroupRecord)Base.Records[i]));
                else Records.Add(new Record((Fomm.TESsnip.Record)Base.Records[i]));
            }
        }
        public Plugin() { Base=new Fomm.TESsnip.Plugin(); base.Base=Base; }

        public byte[] Save() { return Base.Save(); }

        public override BaseRecord Clone() {
            throw new NotImplementedException("The method or operation is not implemented.");
        }
    }

    public abstract class Rec : BaseRecord { }

    public sealed class GroupRecord : Rec {
        internal new readonly Fomm.TESsnip.GroupRecord Base;

        public readonly List<Rec> Records=new List<Rec>();
        public uint groupType { get { return Base.groupType; } set { Base.groupType=value; } }
        public uint dateStamp { get { return Base.dateStamp; } set { Base.dateStamp=value; } }
        public uint flags { get { return Base.flags; } set { Base.flags=value; } }

        public string ContentsType { get { return Base.ContentsType; } }

        public override void DeleteRecord(BaseRecord br) {
            Base.DeleteRecord(br.Base);
            Records.Remove((Rec)br);
        }
        public override void AddRecord(BaseRecord br) {
            Base.AddRecord(br.Base);
            Records.Add((Rec)br);
        }

        public GroupRecord(string data) { Base=new Fomm.TESsnip.GroupRecord(data); base.Base=Base; }

        internal GroupRecord(Fomm.TESsnip.GroupRecord gr) {
            Base=gr;
            base.Base=gr;
            for(int i=0;i<Base.Records.Count;i++) {
                if(Base.Records[i] is Fomm.TESsnip.GroupRecord) Records.Add(new GroupRecord((Fomm.TESsnip.GroupRecord)Base.Records[i]));
                else Records.Add(new Record((Fomm.TESsnip.Record)Base.Records[i]));
            }
        }

        public override BaseRecord Clone() { return new GroupRecord((Fomm.TESsnip.GroupRecord)Base.Clone()); }

        public byte[] GetData() { return Base.GetData(); }
        public void SetData(byte[] data) { Base.SetData(data); }
    }

    public sealed class Record : Rec {
        internal new readonly Fomm.TESsnip.Record Base;

        public readonly List<SubRecord> SubRecords=new List<SubRecord>();
        public uint Flags1 { get { return Base.Flags1; } set { Base.Flags1=value; } }
        public uint Flags2 { get { return Base.Flags2; } set { Base.Flags2=value; } }
        public uint Flags3 { get { return Base.Flags3; } set { Base.Flags3=value; } }
        public uint FormID { get { return Base.FormID; } set { Base.FormID=value; } }

        public override void DeleteRecord(BaseRecord br) {
            Base.DeleteRecord(br.Base);
            SubRecords.Remove((SubRecord)br);
        }
        public override void AddRecord(BaseRecord br) {
            Base.AddRecord(br.Base);
            SubRecords.Add((SubRecord)br);
        }

        internal Record(Fomm.TESsnip.Record r) {
            Base=r;
            base.Base=r;
            for(int i=0;i<Base.SubRecords.Count;i++) {
                SubRecords.Add(new SubRecord((Fomm.TESsnip.SubRecord)Base.SubRecords[i]));
            }
        }

        public Record() { Base=new Fomm.TESsnip.Record(); base.Base=Base; }

        public override BaseRecord Clone() { return new Record((Fomm.TESsnip.Record)Base.Clone()); }
    }

    public sealed class SubRecord : BaseRecord {
        internal new readonly Fomm.TESsnip.SubRecord Base;

        public byte[] GetData() { return Base.GetData(); }
        public void SetData(byte[] data) { Base.SetData(data); }
        public void SetStrData(string s, bool nullTerminate) { Base.SetStrData(s, nullTerminate); }

        internal SubRecord(Fomm.TESsnip.SubRecord sr) { Base=sr; base.Base=Base; }

        public override BaseRecord Clone() { return new SubRecord((Fomm.TESsnip.SubRecord)Base.Clone()); }

        public SubRecord() { Base=new Fomm.TESsnip.SubRecord(); base.Base=Base; }

        public override void DeleteRecord(BaseRecord br) { }
        public override void AddRecord(BaseRecord br) { }
    }
}
