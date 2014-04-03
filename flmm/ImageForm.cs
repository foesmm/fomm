using System;
using System.Drawing;
using System.Windows.Forms;
using Fomm.Properties;

namespace Fomm
{
  internal partial class ImageForm : Form
  {
    internal ImageForm(Image i)
    {
      InitializeComponent();
      Icon = Resources.fomm02;
      pictureBox1.Image = i;
    }

    private void pictureBox1_Click(object sender, EventArgs e)
    {
      Close();
    }
  }
}