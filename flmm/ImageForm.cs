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
      /*if(Program.IsImageAnimated(i)) {
                MessageBox.Show("Animated or multi-resolution images are not supported", "Error");
                pictureBox1.Image=null;
            } else */
      pictureBox1.Image = i;
    }

    internal ImageForm(Image i, string text) : this(i)
    {
      Text = text;
    }

    private void pictureBox1_Click(object sender, EventArgs e)
    {
      Close();
    }
  }
}