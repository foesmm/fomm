using System;
using System.Collections.Generic;

namespace fomm.PackageManager {
    public class BaseScript {
        public virtual void OnInstall() {

        }

        public string[] GetFileList() {
            return new string[0];
            //List<string> files=
        }
    }
}
