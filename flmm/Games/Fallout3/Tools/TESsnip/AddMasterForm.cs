using System;
using System.IO;
using System.Windows.Forms;

namespace Fomm.Games.Fallout3.Tools.TESsnip
{
  public partial class AddMasterForm : Form
  {
    public string MasterName
    {
      get
      {
        return tbxMaster.Text;
      }
    }

    public AddMasterForm()
    {
      InitializeComponent();
      ofdChooseMaster.InitialDirectory = Program.GameMode.PluginsPath;
    }

    private void butOK_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
    }

    private void butChooseMaster_Click(object sender, EventArgs e)
    {
      if (ofdChooseMaster.ShowDialog() == DialogResult.OK)
      {
        tbxMaster.Text = Path.GetFileName(ofdChooseMaster.FileName);
      }
    }
  }
}