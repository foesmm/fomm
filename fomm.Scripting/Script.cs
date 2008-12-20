using System;

namespace fomm.Scripting {
    public abstract class Script : fomm.PackageManager.BaseScript {
        public new string[] GetFileList() { return base.GetFileList(); }
    }
}
