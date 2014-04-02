using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Fomm.Games.Fallout3.Tools.TESsnip
{
  internal class RecordXmlException : Exception
  {
    public RecordXmlException(string msg) : base(msg)
    {
    }
  }

  internal enum ElementValueType
  {
    String,
    Float,
    Int,
    Short,
    Byte,
    FormID,
    fstring,
    Blob
  }

  internal enum CondType
  {
    None,
    Equal,
    Not,
    Greater,
    Less,
    GreaterEqual,
    LessEqual,
    StartsWith,
    EndsWith,
    Contains,
    Exists,
    Missing
  }

  internal struct SubrecordStructure
  {
    public readonly string name;
    public readonly int repeat;
    public readonly int optional;
    public readonly string desc;
    public readonly ElementStructure[] elements;
    public readonly bool notininfo;
    public readonly int size;

    public readonly CondType Condition;
    public readonly int CondID;
    public readonly string CondOperand;
    public readonly bool ContaintsConditionals;
    public readonly bool UseHexEditor;

    public SubrecordStructure(XmlNode node)
    {
      if (node.Name != "Subrecord")
      {
        throw new RecordXmlException("Invalid node");
      }

      name = node.Attributes.GetNamedItem("name").Value;
      desc = node.Attributes.GetNamedItem("desc").Value;
      var node2 = node.Attributes.GetNamedItem("repeat");
      repeat = node2 != null ? int.Parse(node2.Value) : 0;
      node2 = node.Attributes.GetNamedItem("optional");
      optional = node2 != null ? int.Parse(node2.Value) : 0;
      node2 = node.Attributes.GetNamedItem("size");
      size = node2 != null ? int.Parse(node2.Value) : 0;
      node2 = node.Attributes.GetNamedItem("notininfo");
      if (node2 != null && node2.Value == "true")
      {
        notininfo = true;
      }
      else
      {
        notininfo = false;
      }
      node2 = node.Attributes.GetNamedItem("usehexeditor");
      if (node2 != null && node2.Value == "true")
      {
        UseHexEditor = true;
      }
      else
      {
        UseHexEditor = false;
      }

      if (optional != 0 && repeat != 0 && optional != repeat)
      {
        throw new RecordXmlException("repeat and optional must both have the same value if they are non zero");
      }

      node2 = node.Attributes.GetNamedItem("condition");
      if (node2 != null)
      {
        switch (node2.Value)
        {
          case "equal":
            Condition = CondType.Equal;
            break;
          case "not":
            Condition = CondType.Not;
            break;
          case "greater":
            Condition = CondType.Greater;
            break;
          case "less":
            Condition = CondType.Less;
            break;
          case "greaterequal":
            Condition = CondType.GreaterEqual;
            break;
          case "lessequal":
            Condition = CondType.LessEqual;
            break;
          case "startswith":
            Condition = CondType.StartsWith;
            break;
          case "endswith":
            Condition = CondType.EndsWith;
            break;
          case "contains":
            Condition = CondType.Contains;
            break;
          case "exists":
            Condition = CondType.Exists;
            break;
          case "missing":
            Condition = CondType.Missing;
            break;
          default:
            throw new RecordXmlException("Invalid condition");
        }
        CondID = int.Parse(node.Attributes.GetNamedItem("condid").Value);
        CondOperand = node.Attributes.GetNamedItem("condvalue").Value;
      }
      else
      {
        Condition = CondType.None;
        CondID = 0;
        CondOperand = null;
      }

      var elements = new List<ElementStructure>();
      foreach (XmlNode n in node.ChildNodes)
      {
        if (n.NodeType == XmlNodeType.Comment)
        {
          continue;
        }
        elements.Add(new ElementStructure(n));
      }
      this.elements = elements.ToArray();
      ContaintsConditionals = false;
      var containsBlob = false;
      foreach (var es in this.elements)
      {
        if (es.CondID != 0)
        {
          ContaintsConditionals = true;
        }
        if (es.type == ElementValueType.fstring || es.type == ElementValueType.Blob)
        {
          containsBlob = true;
        }
      }
      if (containsBlob && elements.Count > 1)
      {
        throw new RecordXmlException("A subrecord containing a blorb or fstring may only contain one element");
      }
      for (var i = 0; i < this.elements.Length - 1; i++)
      {
        if (this.elements[i].repeat || this.elements[i].optional)
        {
          throw new RecordXmlException(
            "Repeat and optional attributes are only valid on the final element of a subrecord");
        }
      }
    }
  }

  internal struct ElementStructure
  {
    public readonly string name;
    public readonly string desc;
    public readonly int group;
    public readonly ElementValueType type;
    public readonly string FormIDType;
    public readonly string[] options;
    public readonly int CondID;
    public readonly bool notininfo;
    public readonly bool multiline;
    public readonly bool repeat;
    public readonly bool optional;
    public readonly bool hexview;
    public readonly string[] flags;

    public ElementStructure(XmlNode node)
    {
      if (node.Name != "Element")
      {
        throw new RecordXmlException("Invalid node");
      }

      name = node.Attributes.GetNamedItem("name").Value;
      var node2 = node.Attributes.GetNamedItem("group");
      @group = node2 != null ? int.Parse(node2.Value) : 0;
      node2 = node.Attributes.GetNamedItem("condid");
      CondID = node2 != null ? int.Parse(node2.Value) : 0;
      node2 = node.Attributes.GetNamedItem("notininfo");
      if (node2 != null && node2.Value == "true")
      {
        notininfo = true;
      }
      else
      {
        notininfo = false;
      }
      node2 = node.Attributes.GetNamedItem("desc");
      desc = node2 != null ? node2.Value : null;
      node2 = node.Attributes.GetNamedItem("hexview");
      if (node2 != null && node2.Value == "true")
      {
        hexview = true;
      }
      else
      {
        hexview = false;
      }
      node2 = node.Attributes.GetNamedItem("flags");
      flags = node2 != null ? node2.Value.Split(';') : null;

      node2 = node.Attributes.GetNamedItem("options");
      options = node2 != null ? node2.Value.Split(';') : null;
      node2 = node.Attributes.GetNamedItem("repeat");
      if (node2 != null && node2.Value == "1")
      {
        repeat = true;
      }
      else
      {
        repeat = false;
      }
      node2 = node.Attributes.GetNamedItem("optional");
      if (node2 != null && node2.Value == "1")
      {
        optional = true;
      }
      else
      {
        optional = false;
      }

      if (optional || repeat)
      {
        if (group != 0)
        {
          throw new RecordXmlException("Elements with a group attribute cant be marked optional or repeat");
        }
      }

      FormIDType = null;
      multiline = false;
      switch (node.Attributes.GetNamedItem("type").Value)
      {
        case "float":
          type = ElementValueType.Float;
          break;
        case "int":
          type = ElementValueType.Int;
          break;
        case "short":
          type = ElementValueType.Short;
          break;
        case "byte":
          type = ElementValueType.Byte;
          break;
        case "formid":
          type = ElementValueType.FormID;
          node2 = node.Attributes.GetNamedItem("reftype");
          if (node2 != null)
          {
            FormIDType = node2.Value;
          }
          break;
        case "string":
          type = ElementValueType.String;
          node2 = node.Attributes.GetNamedItem("multiline");
          if (node2 != null && node2.Value == "true")
          {
            multiline = true;
          }
          break;
        case "blob":
          if (repeat || optional)
          {
            throw new RecordXmlException("blob or fstring type elements can't be marked with repeat or optional");
          }
          type = ElementValueType.Blob;
          break;
        case "fstring":
          if (repeat || optional)
          {
            throw new RecordXmlException("blob or fstring type elements can't be marked with repeat or optional");
          }
          type = ElementValueType.fstring;
          node2 = node.Attributes.GetNamedItem("multiline");
          if (node2 != null && node2.Value == "true")
          {
            multiline = true;
          }
          break;
        default:
          throw new RecordXmlException("Invalid element type");
      }
    }
  }

  internal class RecordStructure
  {
    #region Static

    private static bool loaded;

    public static bool Loaded
    {
      get
      {
        return loaded;
      }
    }

    public static Dictionary<string, RecordStructure> Records;

    private static string xmlPath = Path.Combine(Program.ProgrammeInfoDirectory,
                                                           "Fallout3\\TESsnip\\RecordStructure.xml");

    public static void Load()
    {
      if (loaded)
      {
        Records.Clear();
      }
      else
      {
        loaded = true;
      }
      var doc = new XmlDocument();
      doc.LoadXml(File.ReadAllText(xmlPath));

      XmlNode root = null;
      foreach (XmlNode n in doc.ChildNodes)
      {
        if (n.NodeType == XmlNodeType.Element)
        {
          root = n;
          break;
        }
      }
      if (root == null)
      {
        throw new RecordXmlException("Root node was missing");
      }
      var records = new Dictionary<string, RecordStructure>();
      var groups = new Dictionary<uint, SubrecordStructure[]>();
      foreach (XmlNode n in root.ChildNodes)
      {
        var err = n.OuterXml.Remove(n.OuterXml.IndexOf('>') + 1);
        ;
        try
        {
          if (n.NodeType == XmlNodeType.Comment)
          {
            continue;
          }
          if (n.Name == "Record")
          {
            records.Add(n.Attributes.GetNamedItem("name").Value, new RecordStructure(n, groups));
          }
          else if (n.Name == "Group")
          {
            var subs = new List<SubrecordStructure>();
            foreach (XmlNode n2 in n.ChildNodes)
            {
              if (n2.NodeType == XmlNodeType.Comment)
              {
                continue;
              }
              if (n2.Name == "Subrecord")
              {
                subs.Add(new SubrecordStructure(n2));
              }
              else if (n2.Name == "Group")
              {
                foreach (var ss in groups[uint.Parse(n2.Attributes.GetNamedItem("id").Value)])
                {
                  subs.Add(ss);
                }
              }
              else
              {
                throw new RecordXmlException("Was expecting 'Subrecord' or 'Group'");
              }
            }
            groups[uint.Parse(n.Attributes.GetNamedItem("id").Value)] = subs.ToArray();
          }
          else
          {
            throw new RecordXmlException("Was expecting 'Record' or 'Group'");
          }
        }
        catch (Exception ex)
        {
          throw new RecordXmlException(ex.Message + Environment.NewLine + "Last encountered record: " + err);
        }
      }
      Records = records;
    }

    #endregion

    public readonly SubrecordStructure[] subrecords;
    public readonly string description;

    private RecordStructure(XmlNode node, Dictionary<uint, SubrecordStructure[]> groups)
    {
      if (node.Name != "Record")
      {
        throw new RecordXmlException("Invalid node");
      }
      var subrecords = new List<SubrecordStructure>();
      description = node.Attributes.GetNamedItem("desc").Value;
      foreach (XmlNode n in node.ChildNodes)
      {
        if (n.NodeType == XmlNodeType.Comment)
        {
          continue;
        }
        if (n.Name == "Subrecord")
        {
          subrecords.Add(new SubrecordStructure(n));
        }
        else if (n.Name == "Group")
        {
          foreach (var ss in groups[uint.Parse(n.Attributes.GetNamedItem("id").Value)])
          {
            subrecords.Add(ss);
          }
        }
        else
        {
          throw new RecordXmlException("Was expecting 'Subrecord' or 'Group'");
        }
      }
      this.subrecords = subrecords.ToArray();
      var ends = new Stack<int>();
      for (var i = 0; i < this.subrecords.Length; i++)
      {
        while (ends.Count > 0 && ends.Peek() < i)
        {
          ends.Pop();
        }
        var j = Math.Max(this.subrecords[i].optional, this.subrecords[i].repeat) + i - 1;
        if (ends.Count > 0 && ends.Peek() < j)
        {
          throw new RecordXmlException("Overlapping subrecord blocks");
        }
        if (j != i)
        {
          ends.Push(j);
        }
      }
    }
  }
}