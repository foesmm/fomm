using System;
using System.Windows.Forms;
using Fomm.Properties;

namespace Fomm.Games.Fallout3.Tools.ShaderEdit
{
  partial class HLSLImporter : Form
  {
    public HLSLImporter()
    {
      InitializeComponent();
      Icon = Resources.fomm02;
      DialogResult = DialogResult.Cancel;
    }

    private string profile;
    private string entryPoint;
    private bool debug;

    public string Profile
    {
      get
      {
        return profile;
      }
    }

    public string EntryPoint
    {
      get
      {
        return entryPoint;
      }
    }

    public byte Debug
    {
      get
      {
        return (byte) (debug ? 1 : 0);
      }
    }

    private void bCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void bImport_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
      foreach (Control c in Controls)
      {
        if (c is RadioButton && ((RadioButton) c).Checked)
        {
          profile = c.Text;
          break;
        }
      }
      entryPoint = tbEntry.Text;
      debug = cbDebug.Checked;
      Close();
    }
  }
}