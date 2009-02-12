using System;
using System.Windows.Forms;

namespace Fomm.InstallTweaker {
    partial class InstallationTweaker : Form {
        public InstallationTweaker() {
            InitializeComponent();
        }

        private void cbDisableLive_CheckedChanged(object sender, EventArgs e) {
            tbDescription.Text="Disable windows live\nPrevents fallout from loading xlive.dll at all\n"+
                "Improves program startup time\nDo not use if you use any of the windows live features";
        }

        private void cbShrinkTextures_CheckedChanged(object sender, EventArgs e) {
            tbDescription.Text="Repacks the textures bsa after stripping the top mipmap from all non-interface textures\n"+
                "Improves loading times\nDo not use if you normally have texture size set to large\n"+
                "After checking this, change textures to medium if you normally use small or large if you normally use medium to keep the same visual quality.";
        }

        private void cbRemoveClutter_CheckedChanged(object sender, EventArgs e) {
            tbDescription.Text="Removes all references to some types of unused clutter from fallout3.esm\n"+
                "Improves loading times and fps in any affected cells";
        }

        private void cbStripGeck_CheckedChanged(object sender, EventArgs e) {
            tbDescription.Text="Strips some data out of fallout3.esm that is only used by the geck\n"+
                "Slightly improves startup time and loading times\n"+
                "Do not check this option if you plan to use the geck or other tools to create your own mods\n"+
                "Does not effect your use of any third party mods";
        }

        private void bCancel_Click(object sender, EventArgs e) {
            Close();
        }

        private void bApply_Click(object sender, EventArgs e) {

        }
    }
}
