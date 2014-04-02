using System;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using Fomm.Properties;

namespace Fomm.Games.Fallout3.Tools.TESsnip
{
  internal partial class MediumLevelRecordEditor : Form
  {
    private SubRecord sr;
    private SubrecordStructure ss;
    private List<TextBox> boxes;
    private List<ElementValueType> valueTypes;
    private List<Panel> elements;
    private dFormIDLookupS formIDLookup;
    private dFormIDScan formIDScan;

    private Dictionary<int, string> removedStrings = new Dictionary<int, string>();

    private int repeatcount;

    private Dictionary<string, string[]> cachedFormIDs = new Dictionary<string, string[]>();

    private class cbTag
    {
      public readonly int group;
      public readonly TextBox textBox;

      public cbTag(int group, TextBox textBox)
      {
        this.group = group;
        this.textBox = textBox;
      }
    }

    private class bTag
    {
      public readonly TextBox formID;
      public readonly TextBox edid;

      public bTag(TextBox form, TextBox edid)
      {
        formID = form;
        this.edid = edid;
      }
    }

    private class comboBoxItem
    {
      public readonly string name;
      public readonly string value;

      public comboBoxItem(string name, string value)
      {
        this.name = name;
        this.value = value;
      }

      public override string ToString()
      {
        return name;
      }
    }

    private class repeatCbTag
    {
      public readonly TextBox tb;
      public readonly int panel;

      public repeatCbTag(TextBox tb, int panel)
      {
        this.tb = tb;
        this.panel = panel;
      }
    }

    private void AddElement(ElementStructure es)
    {
      int a = -1, b = 0, c = 0;
      AddElement(es, ref a, null, ref b, ref c);
    }

    private void AddElement(ElementStructure es, ref int offset, byte[] data, ref int groupOffset, ref int CurrentGroup)
    {
      var panel1 = new Panel();
      panel1.AutoSize = true;
      panel1.Width = fpanel1.Width;
      panel1.Height = 1;
      var ypos = 0;

      var tb = new TextBox();
      boxes.Add(tb);
      if (es.group != 0)
      {
        var cb = new CheckBox();
        cb.Text = "Use this value?";
        panel1.Controls.Add(cb);
        cb.Location = new Point(10, ypos);
        ypos += 24;
        cb.Tag = new cbTag(es.group, tb);
        if (CurrentGroup != es.group)
        {
          cb.Checked = true;
        }
        else
        {
          tb.Enabled = false;
        }
        cb.CheckedChanged += CheckBox_CheckedChanged;
      }
      if (es.optional || es.repeat && repeatcount > 0)
      {
        var cb = new CheckBox();
        cb.Text = "Use this value?";
        panel1.Controls.Add(cb);
        cb.Location = new Point(10, ypos);
        ypos += 24;
        cb.Tag = new repeatCbTag(tb, elements.Count);
        if (data == null)
        {
          tb.Enabled = false;
        }
        else
        {
          cb.Checked = true;
        }
        cb.CheckedChanged += RepeatCheckBox_CheckedChanged;
      }
      if ((CurrentGroup == 0 && es.group != 0) || (CurrentGroup != 0 && es.group != 0 && CurrentGroup != es.group))
      {
        CurrentGroup = es.group;
        groupOffset = offset;
      }
      else if (CurrentGroup != 0 && es.group == 0)
      {
        CurrentGroup = 0;
      }
      else if (CurrentGroup != 0 && CurrentGroup == es.group)
      {
        offset = groupOffset;
      }
      valueTypes.Add(es.type);
      if (data != null)
      {
        switch (es.type)
        {
          case ElementValueType.Int:
            tb.Text = TypeConverter.h2si(data[offset], data[offset + 1], data[offset + 2], data[offset + 3]).ToString();
            offset += 4;
            break;
          case ElementValueType.FormID:
            tb.Text =
              TypeConverter.h2i(data[offset], data[offset + 1], data[offset + 2], data[offset + 3]).ToString("X8");
            offset += 4;
            break;
          case ElementValueType.Float:
            tb.Text = TypeConverter.h2f(data[offset], data[offset + 1], data[offset + 2], data[offset + 3]).ToString();
            offset += 4;
            break;
          case ElementValueType.Short:
            tb.Text = TypeConverter.h2ss(data[offset], data[offset + 1]).ToString();
            offset += 2;
            break;
          case ElementValueType.Byte:
            tb.Text = data[offset].ToString();
            offset++;
            break;
          case ElementValueType.String:
            var s = "";
            while (data[offset] != 0)
            {
              s += (char) data[offset++];
            }
            offset++;
            tb.Text = s;
            tb.Width += 200;
            break;
          case ElementValueType.fstring:
            tb.Text = sr.GetStrData();
            tb.Width += 200;
            break;
          default:
            throw new ApplicationException();
        }
      }
      else
      {
        if (es.type == ElementValueType.String || es.type == ElementValueType.fstring)
        {
          tb.Width += 200;
        }
        if (removedStrings.ContainsKey(boxes.Count - 1))
        {
          tb.Text = removedStrings[boxes.Count - 1];
        }
      }
      var l = new Label();
      l.AutoSize = true;
      var tmp = es.type.ToString();
      l.Text = tmp + ": " + es.name + (es.desc != null ? (" (" + es.desc + ")") : "");
      panel1.Controls.Add(tb);
      tb.Location = new Point(10, ypos);
      if (es.multiline)
      {
        tb.Multiline = true;
        ypos += tb.Height*5;
        tb.Height *= 6;
      }
      panel1.Controls.Add(l);
      l.Location = new Point(tb.Right + 10, ypos + 3);
      string[] options = null;
      if (es.type == ElementValueType.FormID)
      {
        ypos += 28;
        var b = new Button();
        b.Text = "FormID lookup";
        b.Click += LookupFormID_Click;
        panel1.Controls.Add(b);
        b.Location = new Point(20, ypos);
        var tb2 = new TextBox();
        tb2.Width += 200;
        tb2.ReadOnly = true;
        panel1.Controls.Add(tb2);
        tb2.Location = new Point(b.Right + 10, ypos);
        b.Tag = new bTag(tb, tb2);
        if (es.FormIDType != null)
        {
          if (cachedFormIDs.ContainsKey(es.FormIDType))
          {
            options = cachedFormIDs[es.FormIDType];
          }
          else
          {
            options = formIDScan(es.FormIDType);
            cachedFormIDs[es.FormIDType] = options;
          }
        }
      }
      else if (es.options != null)
      {
        options = es.options;
      }
      if (options != null)
      {
        ypos += 28;
        var cmb = new ComboBox();
        cmb.Tag = tb;
        cmb.Width += 200;
        for (var j = 0; j < options.Length; j += 2)
        {
          cmb.Items.Add(new comboBoxItem(options[j], options[j + 1]));
        }
        cmb.KeyPress += cb_KeyPress;
        cmb.ContextMenu = new ContextMenu();
        cmb.SelectedIndexChanged += cb_SelectedIndexChanged;
        panel1.Controls.Add(cmb);
        cmb.Location = new Point(20, ypos);
      }

      fpanel1.Controls.Add(panel1);
      elements.Add(panel1);
    }

    public MediumLevelRecordEditor(SubRecord sr, SubrecordStructure ss, dFormIDLookupS formIDLookup,
                                   dFormIDScan formIDScan)
    {
      InitializeComponent();
      Icon = Resources.fomm02;
      SuspendLayout();
      this.sr = sr;
      this.ss = ss;
      this.formIDLookup = formIDLookup;
      this.formIDScan = formIDScan;

      var offset = 0;
      var data = sr.GetReadonlyData();
      boxes = new List<TextBox>(ss.elements.Length);
      valueTypes = new List<ElementValueType>(ss.elements.Length);
      elements = new List<Panel>();
      var groupOffset = 0;
      var CurrentGroup = 0;
      try
      {
        for (var i = 0; i < ss.elements.Length; i++)
        {
          if (ss.elements[i].optional && offset == data.Length)
          {
            AddElement(ss.elements[i]);
          }
          else
          {
            AddElement(ss.elements[i], ref offset, data, ref groupOffset, ref CurrentGroup);
            if (ss.elements[i].repeat)
            {
              repeatcount++;
              if (offset < data.Length)
              {
                i--;
              }
            }
          }
        }
        if (ss.elements[ss.elements.Length - 1].repeat && repeatcount > 0)
        {
          AddElement(ss.elements[ss.elements.Length - 1]);
        }
      }
      catch
      {
        MessageBox.Show("The subrecord doesn't appear to conform to the expected structure.\n" +
                        "Saving is disabled, and the formatted information may be incorrect", "Warning");
        bSave.Enabled = false;
      }
      ResumeLayout();
    }

    private void cb_SelectedIndexChanged(object sender, EventArgs e)
    {
      var cmb = (ComboBox) sender;
      var cbi = (comboBoxItem) cmb.SelectedItem;
      ((TextBox) cmb.Tag).Text = cbi.value;
    }

    private void cb_KeyPress(object sender, KeyPressEventArgs e)
    {
      e.Handled = true;
    }

    private bool CheckingChange;
    private bool IgnoreChange;

    private void CheckBox_CheckedChanged(object sender, EventArgs e)
    {
      if (IgnoreChange)
      {
        return;
      }
      var changed = (CheckBox) sender;
      if (changed.Checked == false)
      {
        if (!CheckingChange)
        {
          IgnoreChange = true;
          changed.Checked = true;
          IgnoreChange = false;
          return;
        }
        else
        {
          var toDisable = ((cbTag) changed.Tag).textBox;
          foreach (Control c in fpanel1.Controls)
          {
            var tb = c as TextBox;
            if (tb == toDisable)
            {
              toDisable.Enabled = false;
              return;
            }
          }
          throw new ApplicationException();
        }
      }
      var Group = ((cbTag) changed.Tag).group;
      var toEnable = ((cbTag) changed.Tag).textBox;
      CheckingChange = true;
      foreach (Panel p in fpanel1.Controls)
      {
        foreach (Control c in p.Controls)
        {
          var cb = c as CheckBox;
          if (cb != null && cb != changed)
          {
            if (((cbTag) changed.Tag).group == Group)
            {
              cb.Checked = false;
            }
          }
          var tb = c as TextBox;
          if (tb == toEnable)
          {
            toEnable.Enabled = true;
          }
        }
      }
      CheckingChange = false;
    }

    private void RepeatCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      var cb = (CheckBox) sender;
      var tag = (repeatCbTag) cb.Tag;
      tag.tb.Enabled = cb.Checked;
      if (cb.Checked)
      {
        if (ss.elements[ss.elements.Length - 1].repeat)
        {
          AddElement(ss.elements[ss.elements.Length - 1]);
        }
      }
      else
      {
        for (var i = tag.panel + 1; i < elements.Count; i++)
        {
          removedStrings[i] = boxes[i].Text;
          fpanel1.Controls.Remove(elements[i]);
        }
        boxes.RemoveRange(tag.panel + 1, elements.Count - (tag.panel + 1));
        valueTypes.RemoveRange(tag.panel + 1, elements.Count - (tag.panel + 1));
        elements.RemoveRange(tag.panel + 1, elements.Count - (tag.panel + 1));
      }
    }

    private void bCancel_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void bSave_Click(object sender, EventArgs e)
    {
      var bytes = new List<byte>();
      for (var j = 0; j < boxes.Count; j++)
      {
        if (!boxes[j].Enabled)
        {
          continue;
        }
        switch (valueTypes[j])
        {
          case ElementValueType.Byte:
          {
            byte b;
            if (!byte.TryParse(boxes[j].Text, out b))
            {
              MessageBox.Show("Invalid byte: " + boxes[j].Text, "Error");
              return;
            }
            bytes.Add(b);
            break;
          }
          case ElementValueType.Short:
          {
            short s;
            if (!short.TryParse(boxes[j].Text, out s))
            {
              MessageBox.Show("Invalid short: " + boxes[j].Text, "Error");
              return;
            }
            var conv = TypeConverter.ss2h(s);
            bytes.Add(conv[0]);
            bytes.Add(conv[1]);
            break;
          }
          case ElementValueType.Int:
          {
            int i;
            if (!int.TryParse(boxes[j].Text, out i))
            {
              MessageBox.Show("Invalid int: " + boxes[j].Text, "Error");
              return;
            }
            var conv = TypeConverter.si2h(i);
            bytes.AddRange(conv);
            break;
          }
          case ElementValueType.Float:
          {
            float f;
            if (!float.TryParse(boxes[j].Text, out f))
            {
              MessageBox.Show("Invalid float: " + boxes[j].Text, "Error");
              return;
            }
            var conv = TypeConverter.f2h(f);
            bytes.AddRange(conv);
            break;
          }
          case ElementValueType.FormID:
          {
            uint i;
            if (!uint.TryParse(boxes[j].Text, NumberStyles.AllowHexSpecifier, null, out i))
            {
              MessageBox.Show("Invalid formID: " + boxes[j].Text, "Error");
              return;
            }
            var conv = TypeConverter.i2h(i);
            bytes.AddRange(conv);
            break;
          }
          case ElementValueType.String:
          {
            var conv = Encoding.Default.GetBytes(boxes[j].Text);
            bytes.AddRange(conv);
            bytes.Add(0);
            break;
          }
          case ElementValueType.fstring:
          {
            var conv = Encoding.Default.GetBytes(boxes[j].Text);
            bytes.AddRange(conv);
            break;
          }
          default:
            throw new ApplicationException();
        }
      }
      sr.SetData(bytes.ToArray());
      Close();
    }

    private void LookupFormID_Click(object sender, EventArgs e)
    {
      var tag = (bTag) ((Button) sender).Tag;
      tag.edid.Text = formIDLookup(tag.formID.Text);
    }
  }
}