using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Fomm.Properties;

namespace Fomm.PackageManager
{
  internal partial class SelectForm : Form
  {
    internal SelectForm(string[] items, string title, bool multi, Image[] previews, string[] tooltips)
    {
      InitializeComponent();
      Icon = Resources.fomm02;
      Settings.Default.windowPositions.GetWindowPosition("PackageManagerSelectForm", this);
      Text = title;
      toolTips = tooltips;
      Multi = multi;
      List<int> lSelected = new List<int>();
      for (int i = 0; i < items.Length; i++)
      {
        if (items[i].Length == 0)
        {
          continue;
        }
        if (items[i][0] == '|')
        {
          items[i] = items[i].Substring(1);
          lSelected.Add(i);
          if (!Multi)
          {
            break;
          }
        }
      }
      lbSelect.Items.AddRange(items);
      if (Multi)
      {
        lbSelect.SelectionMode = SelectionMode.MultiExtended;
        label1.Text = "Select any number of options (Use ctrl and shift for multiselect)";
        if (lSelected.Count > 0)
        {
          foreach (int i in lSelected)
          {
            lbSelect.SetSelected(i, true);
          }
        }
      }
      else
      {
        lbSelect.SelectedIndex = lSelected.Count == 0 ? 0 : lSelected[0];
      }
      if (toolTips == null)
      {
        bDescription.Visible = false;
      }
      if (previews != null)
      {
        Previews = previews;
      }
      else
      {
        bPreview.Visible = false;
      }
      lbSelect_SelectedIndexChanged(null, null);
    }

    private bool blockClose = true;

    internal int[] SelectedIndex =
    {
      0
    };

    private Image[] Previews;
    private string[] toolTips;
    private bool ShowingDesc;
    private bool Multi;

    private int selectedIndex;
    private List<int> selected = new List<int>();

    private void bOK_Click(object sender, EventArgs e)
    {
      blockClose = false;
      SelectedIndex = new int[lbSelect.SelectedIndices.Count];
      for (int i = 0; i < SelectedIndex.Length; i++)
      {
        SelectedIndex[i] = lbSelect.SelectedIndices[i];
      }
      if (Previews != null)
      {
        foreach (Image i in Previews)
        {
          if (i != null)
          {
            i.Dispose();
          }
        }
      }
      Close();
    }

    private void SelectForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      Settings.Default.windowPositions.SetWindowPosition("PackageManagerSelectForm", this);
      Settings.Default.Save();
      e.Cancel = blockClose;
    }

    private void lbSelect_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (lbSelect.SelectedItems.Count == 0)
      {
        selected.Clear();
        selectedIndex = -1;
        bDescription.Enabled = false;
        bPreview.Enabled = false;
        return;
      }
      if (selectedIndex != -1 && !lbSelect.SelectedIndices.Contains(selectedIndex))
      {
        selectedIndex = -1;
      }
      for (int i = 0; i < selected.Count; i++)
      {
        if (!lbSelect.SelectedIndices.Contains(selected[i]))
        {
          selected.RemoveAt(i--);
        }
      }
      for (int i = 0; i < lbSelect.SelectedIndices.Count; i++)
      {
        if (!selected.Contains(lbSelect.SelectedIndices[i]))
        {
          selectedIndex = lbSelect.SelectedIndices[i];
          selected.Add(lbSelect.SelectedIndices[i]);
        }
      }
      if (toolTips != null)
      {
        if (selectedIndex != -1 && toolTips[selectedIndex] != null)
        {
          bDescription.Enabled = true;
        }
        else
        {
          bDescription.Enabled = false;
        }
      }
      if (Previews != null)
      {
        if (selectedIndex != -1 && Previews[selectedIndex] != null)
        {
          bPreview.Enabled = true;
        }
        else
        {
          bPreview.Enabled = false;
        }
      }
    }

    private void bPreview_Click(object sender, EventArgs e)
    {
      (new ImageForm(Previews[selectedIndex], (string) lbSelect.SelectedItem)).ShowDialog();
    }

    private void bDescription_Click(object sender, EventArgs e)
    {
      if (ShowingDesc)
      {
        lbSelect.Visible = true;
        tbDesc.Visible = false;
        bDescription.Text = "Description";
        ShowingDesc = false;
      }
      else if (toolTips[selectedIndex] != null)
      {
        tbDesc.Text = toolTips[selectedIndex];
        tbDesc.Select(0, 0);
        tbDesc.ScrollToCaret();
        tbDesc.Visible = true;
        lbSelect.Visible = false;
        bDescription.Text = "Options";
        ShowingDesc = true;
      }
      else
      {
        bDescription.Enabled = false;
      }
    }

    private void lbSelect_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Control && e.KeyCode == Keys.A && Multi)
      {
        //No function to do this all in one go?
        SuspendLayout();
        lbSelect.SelectedIndices.Clear();
        for (int i = 0; i < lbSelect.Items.Count; i++)
        {
          lbSelect.SelectedIndices.Add(i);
        }
        ResumeLayout();
      }
    }
  }
}