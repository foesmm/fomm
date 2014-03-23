using System;
using System.Windows.Forms;

namespace Fomm.PackageManager
{
  enum OverwriteResult { YesToAll = 1, YesToFolder = 2, Yes = 3, NoToAll = 4, NoToFolder = 5, No = 6, YesToMod = 7, NoToMod = 8 }

  partial class Overwriteform : Form
  {
    private Overwriteform(string msg, bool allowFolder, bool allowMod)
    {
      InitializeComponent();
      this.Icon = Fomm.Properties.Resources.fomm02;
      Properties.Settings.Default.windowPositions.GetWindowPosition("OverwriteForm", this);
      label1.Text = msg;
      if (!allowFolder)
      {
        bYesToFolder.Enabled = false;
        bNoToFolder.Enabled = false;
      }
      if (!allowMod)
      {
        bYesToMod.Enabled = false;
        bNoToMod.Enabled = false;
      }
    }

    private OverwriteResult result;

    public static OverwriteResult ShowDialog(string msg, bool allowFolder, bool allowMod)
    {
      Overwriteform of = new Overwriteform(msg, allowFolder, allowMod);
      of.ShowDialog();
      return of.result;
    }

    private void bYesToAll_Click(object sender, EventArgs e)
    {
      result = OverwriteResult.YesToAll;
      Close();
    }

    private void bYesToFolder_Click(object sender, EventArgs e)
    {
      result = OverwriteResult.YesToFolder;
      Close();
    }

    private void bYes_Click(object sender, EventArgs e)
    {
      result = OverwriteResult.Yes;
      Close();
    }

    private void bNoToAll_Click(object sender, EventArgs e)
    {
      result = OverwriteResult.NoToAll;
      Close();
    }

    private void bNoToFolder_Click(object sender, EventArgs e)
    {
      result = OverwriteResult.NoToFolder;
      Close();
    }

    private void bNo_Click(object sender, EventArgs e)
    {
      result = OverwriteResult.No;
      Close();
    }

    private void bYesToMod_Click(object sender, EventArgs e)
    {
      result = OverwriteResult.YesToMod;
      Close();
    }

    private void bNoToMod_Click(object sender, EventArgs e)
    {
      result = OverwriteResult.NoToMod;
      Close();
    }

    private void Overwriteform_FormClosing(object sender, FormClosingEventArgs e)
    {
      Properties.Settings.Default.windowPositions.SetWindowPosition("OverwriteForm", this);
      Properties.Settings.Default.Save();
    }

    private void panel1_Layout(object sender, LayoutEventArgs e)
    {
      panel6.Width = panel1.Width / 4;
      panel5.Width = panel1.Width / 4;
      panel4.Width = panel1.Width / 4;
    }
  }
}