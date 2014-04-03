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

    private bool debug;

    public string Profile { get; private set; }

    public string EntryPoint { get; private set; }

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
          Profile = c.Text;
          break;
        }
      }
      EntryPoint = tbEntry.Text;
      debug = cbDebug.Checked;
      Close();
    }
  }
}