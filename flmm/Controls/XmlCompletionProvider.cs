using System;
using System.Collections.Generic;
using Fomm.Properties;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using System.Text.RegularExpressions;
using System.Xml.Schema;
using System.Windows.Forms;
using ICSharpCode.TextEditor;
using System.Text;
using System.Drawing;
using System.IO;

namespace Fomm.Controls
{
  /// <summary>
  /// The possible types of completion selections.
  /// </summary>
  public enum AutoCompleteType
  {
    /// <summary>
    /// Indicates that the selections are XML elements.
    /// </summary>
    Element,

    /// <summary>
    /// Indicates that the selections are XML attributes.
    /// </summary>
    Attribute,

    /// <summary>
    /// Indicates that the selections are values for an XML attribute.
    /// </summary>
    AttributeValues
  }

  /// <summary>
  /// The event arguments for events that allow extending the code completion list.
  /// </summary>
  public class AutoCompleteListEventArgs : EventArgs
  {
    private AutoCompleteType m_actType = AutoCompleteType.Element;
    private List<char> m_lstExtraInsertionCharacters = new List<char>();

    #region Properties

    /// <summary>
    /// Gets the list of code completions.
    /// </summary>
    /// <remarks>
    /// This property can be used to add or remove code completions.
    /// </remarks>
    /// <value>The list of code completions.</value>
    public List<XmlCompletionData> AutoCompleteList { get; private set; }

    /// <summary>
    /// Gets the path to the current element in the XML.
    /// </summary>
    /// <value>The path to the current element in the XML.</value>
    public string ElementPath { get; private set; }

    /// <summary>
    /// Gets the siblings of the current XML object being completed.
    /// </summary>
    /// <remarks>
    /// If the current object being completed is an element, than the siblings are sibling elements.
    /// If the current object is an attribute, than the siblings are the other attributes in the current tag.
    /// If the current object is an attribute value, there is only one sibling: to attribute whose value
    /// is being completed.
    /// </remarks>
    /// <value>The siblings of the current XML object being completed.</value>
    public string[] Siblings { get; private set; }

    /// <summary>
    /// Gets the type of object being completed.
    /// </summary>
    /// <value>The type of object being completed.</value>
    public AutoCompleteType AutoCompleteType
    {
      get
      {
        return m_actType;
      }
    }

    /// <summary>
    /// Gets the word that has been entered thus far for the autocompletion string.
    /// </summary>
    /// <value>The word that has been entered thus far for the autocompletion string.</value>
    public string LastWord { get; private set; }

    /// <summary>
    /// Gets a list of extra characters that should be treated as insertion characters.
    /// </summary>
    /// <value>A list of extra characters that should be treated as insertion characters.</value>
    public List<char> ExtraInsertionCharacters
    {
      get
      {
        return m_lstExtraInsertionCharacters;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// A simple constructor that initializes the object with the given values.
    /// </summary>
    /// <param name="p_lstAutoCompleteList">The list of code completions.</param>
    /// <param name="p_strElementPath">The path to the current element in the XML.</param>
    /// <param name="p_strSiblings">The siblings of the current XML object being completed.</param>
    /// <param name="p_actType">The type of object being completed.</param>
    /// <param name="p_strLastWord">The word that has been entered thus far for the autocompletion string.</param>
    public AutoCompleteListEventArgs(List<XmlCompletionData> p_lstAutoCompleteList, string p_strElementPath,
                                     string[] p_strSiblings, AutoCompleteType p_actType, string p_strLastWord)
    {
      AutoCompleteList = p_lstAutoCompleteList;
      ElementPath = p_strElementPath;
      Siblings = p_strSiblings;
      m_actType = p_actType;
      LastWord = p_strLastWord;
    }

    #endregion
  }

  /// <summary>
  /// Provides the code completion selections for an XML base on a specified schema.
  /// </summary>
  public class XmlCompletionProvider : ICompletionDataProvider
  {
    /// <summary>
    /// Raised when the code completion option6s have been retrieved.
    /// </summary>
    /// <remarks>
    /// Handling this event allows the addition/removal of code completion items.
    /// </remarks>
    public event EventHandler<AutoCompleteListEventArgs> GotAutoCompleteList;

    private Regex rgxTagContents = new Regex("<([^!>][^>]*)>?", RegexOptions.Singleline);
    private Regex rgxTagName = new Regex(@"([^!/\s][^/\s]*)");
    private Regex rgxLastAttribute = new Regex(".*\\s([^=]+)=?", RegexOptions.Singleline);
    private Regex rgxAttribute = new Regex(@"(\S+)=");

    private XmlSchemaSet m_xstSchema;
    private XmlSchema m_xshSchema;
    private AutoCompleteType m_actCompleteType = AutoCompleteType.Element;

    private Dictionary<AutoCompleteType, List<char>> m_dicExtraCompletionCharacters =
      new Dictionary<AutoCompleteType, List<char>>();

    #region Properties

    /// <summary>
    /// Sets the XML Schema used to get the code completion selections.
    /// </summary>
    /// <value>The XML Schema used to get the code completion selections.</value>
    public XmlSchema Schema
    {
      set
      {
        if (m_xshSchema == value)
        {
          return;
        }
        if (m_xstSchema == null)
        {
          m_xstSchema = new XmlSchemaSet();
        }
        if (m_xshSchema != null)
        {
          m_xstSchema.RemoveRecursive(m_xshSchema);
        }
        if (value != null)
        {
          m_xstSchema.Add(value);
        }
        m_xshSchema = value;
        m_xstSchema.Compile();
      }
    }

    #endregion

    #region XML Schema Parsing

    /// <summary>
    /// Finds the element specified in the given stack, starting from the given XML particle.
    /// </summary>
    /// <param name="p_xspParticle">The particle at which to being the search.</param>
    /// <param name="p_stkCode">A stack describing a path to an XML element.</param>
    /// <returns>The element specified by the given stack, or <lang langref="null"/> if no such
    /// element could be found.</returns>
    private XmlSchemaElement findElement(XmlSchemaParticle p_xspParticle, Stack<string> p_stkCode)
    {
      if (p_xspParticle is XmlSchemaElement)
      {
        var xseElement = (XmlSchemaElement) p_xspParticle;
        if (p_stkCode.Peek().Equals(xseElement.Name))
        {
          p_stkCode.Pop();
          if (p_stkCode.Count == 0)
          {
            return xseElement;
          }

          if (xseElement.ElementSchemaType is XmlSchemaComplexType)
          {
            var xctElement = (XmlSchemaComplexType) xseElement.ElementSchemaType;
            if (xctElement.ContentTypeParticle != null)
            {
              return findElement(xctElement.ContentTypeParticle, p_stkCode);
            }
          }
          //if this isn't a complex type, then this element has no children,
          // so if this isn't the element we are looking for, we are looking
          // in the wrong part of the schema tree.
        }
      }
      else if (p_xspParticle is XmlSchemaGroupBase)
      {
        //this handle sequences, choices, and all
        var xbsGroup = (XmlSchemaGroupBase) p_xspParticle;
        foreach (XmlSchemaParticle xspParticle in xbsGroup.Items)
        {
          var xseElement = findElement(xspParticle, p_stkCode);
          if (xseElement != null)
          {
            return xseElement;
          }
        }
      }

      return null;
    }

    /// <summary>
    /// Gets the documentation associated with the given XML element.
    /// </summary>
    /// <param name="p_xsaAnnotatedElement">The element for which to retrieve the documentations.</param>
    /// <returns>The documentation associated with the given XML element, or <lang langref="null"/>
    /// if there is no documentation in the schema.</returns>
    private string GetDocumentation(XmlSchemaAnnotated p_xsaAnnotatedElement)
    {
      var xsaAnnotation = p_xsaAnnotatedElement.Annotation;
      if (xsaAnnotation != null)
      {
        var stbDescriptions = new StringBuilder();
        foreach (var xmoObject in xsaAnnotation.Items)
        {
          if (xmoObject is XmlSchemaDocumentation)
          {
            var xsdDocumentation = (XmlSchemaDocumentation) xmoObject;
            foreach (var xndNode in xsdDocumentation.Markup)
            {
              stbDescriptions.AppendLine(xndNode.Value);
            }
          }
        }
        return stbDescriptions.ToString();
      }
      return null;
    }

    /// <summary>
    /// Gets the child elements of the given XML particle that are eligible to
    /// be the next element in the XML document.
    /// </summary>
    /// <param name="p_xspParticle">The particle whoe children are to be retrieved.</param>
    /// <returns>A list of the child elements of the given XML particle that are eligible to
    /// be the next element in the XML document.</returns>
    private List<KeyValuePair<string, string>> GetChildrenElements(XmlSchemaParticle p_xspParticle)
    {
      if (p_xspParticle is XmlSchemaElement)
      {
        var xseElement = (XmlSchemaElement) p_xspParticle;
        return new List<KeyValuePair<string, string>>
        {
          new KeyValuePair<string, string>(xseElement.Name, GetDocumentation(xseElement))
        };
      }
      if (p_xspParticle is XmlSchemaSequence)
      {
        var xssSequence = (XmlSchemaSequence) p_xspParticle;
        var lstChoices = new List<KeyValuePair<string, string>>();
        foreach (XmlSchemaParticle xspParticle in xssSequence.Items)
        {
          lstChoices.AddRange(GetChildrenElements(xspParticle));
          if (xspParticle.MinOccurs > 0)
          {
            break;
          }
        }
        return lstChoices;
      }
      if (p_xspParticle is XmlSchemaChoice)
      {
        var xscChoice = (XmlSchemaChoice) p_xspParticle;
        var lstChoices = new List<KeyValuePair<string, string>>();
        foreach (XmlSchemaParticle xspParticle in xscChoice.Items)
        {
          lstChoices.AddRange(GetChildrenElements(xspParticle));
        }
        return lstChoices;
      }
      if (p_xspParticle is XmlSchemaAll)
      {
        var xsaAll = (XmlSchemaAll) p_xspParticle;
        var lstChoices = new List<KeyValuePair<string, string>>();
        foreach (XmlSchemaParticle xspParticle in xsaAll.Items)
        {
          lstChoices.AddRange(GetChildrenElements(xspParticle));
        }
        return lstChoices;
      }
      return null;
    }

    /// <summary>
    /// Determines if the given XML particle contains the given sibling elements, and populates
    /// the list of elements that are eligible to be the next element in the XML document.
    /// </summary>
    /// <remarks>
    /// A particle contains the siblings if the siblings are children of the given particle.
    /// </remarks>
    /// <param name="p_xspParticle">That XML particle for which it is to be determined if it
    /// contains the given sibling element.</param>
    /// <param name="p_lstChoices">The list of elements that are eligible to be the next element in the XML document.</param>
    /// <param name="p_lstSiblings">A list of sibling elements.</param>
    /// <returns><lang langref="true"/> if the given siblings are contained by the given particle.</returns>
    private bool ContainsSiblings(XmlSchemaParticle p_xspParticle, ref List<KeyValuePair<string, string>> p_lstChoices,
                                  List<string> p_lstSiblings)
    {
      if (p_xspParticle is XmlSchemaElement)
      {
        var xseElement = (XmlSchemaElement) p_xspParticle;
        string strLastSibling = null;
        if (p_lstSiblings.Count > 0)
        {
          strLastSibling = p_lstSiblings[p_lstSiblings.Count - 1];
        }
        return xseElement.Name.Equals(strLastSibling);
      }
      if (p_xspParticle is XmlSchemaSequence)
      {
        var xssSequence = (XmlSchemaSequence) p_xspParticle;
        var booFound = false;
        int i;
        XmlSchemaParticle xspParticle = null;
        for (i = 0; i < xssSequence.Items.Count; i++)
        {
          xspParticle = (XmlSchemaParticle) xssSequence.Items[i];
          if (ContainsSiblings(xspParticle, ref p_lstChoices, p_lstSiblings))
          {
            booFound = true;
            break;
          }
        }
        if (booFound)
        {
          if (p_lstChoices == null)
          {
            int intLastSiblingCount;
            for (intLastSiblingCount = p_lstSiblings.Count - 1;
                 (intLastSiblingCount > -1) &&
                 p_lstSiblings[intLastSiblingCount].Equals(p_lstSiblings[p_lstSiblings.Count - 1]);
                 intLastSiblingCount--)
            {
            }
            intLastSiblingCount = p_lstSiblings.Count - intLastSiblingCount - 1;

            var lstChoices = new List<KeyValuePair<string, string>>();
            if (intLastSiblingCount < xspParticle.MaxOccurs)
            {
              lstChoices.AddRange(GetChildrenElements(xspParticle));
            }
            for (i++; i < xssSequence.Items.Count; i++)
            {
              xspParticle = (XmlSchemaParticle) xssSequence.Items[i];
              lstChoices.AddRange(GetChildrenElements(xspParticle));
              if (xspParticle.MinOccurs > 0)
              {
                break;
              }
            }
            if (lstChoices.Count > 0)
            {
              p_lstChoices = lstChoices;
            }
          }
          return true;
        }
      }
      else if (p_xspParticle is XmlSchemaChoice)
      {
        var xscChoice = (XmlSchemaChoice) p_xspParticle;
        foreach (XmlSchemaParticle xspParticle in xscChoice.Items)
        {
          if (ContainsSiblings(xspParticle, ref p_lstChoices, p_lstSiblings))
          {
            return true;
          }
        }
      }
      else if (p_xspParticle is XmlSchemaAll)
      {
        var xsaAll = (XmlSchemaAll) p_xspParticle;
        var booFound = false;
        foreach (XmlSchemaParticle xspParticle in xsaAll.Items)
        {
          if (ContainsSiblings(xspParticle, ref p_lstChoices, p_lstSiblings))
          {
            booFound = true;
            break;
          }
        }
        if (booFound)
        {
          if (p_lstChoices == null)
          {
            var lstChoices = new List<KeyValuePair<string, string>>();
            var lstSibling = new List<string>();
            var intFoundCount = 0;
            foreach (XmlSchemaParticle xspParticle in xsaAll.Items)
            {
              booFound = false;
              foreach (var strSibling in p_lstSiblings)
              {
                lstSibling.Clear();
                lstSibling.Add(strSibling);
                if (ContainsSiblings(xspParticle, ref lstChoices, lstSibling))
                {
                  intFoundCount++;
                  booFound = true;
                  break;
                }
              }
              if (!booFound)
              {
                lstChoices.AddRange(GetChildrenElements(xspParticle));
              }
            }
            if (intFoundCount < xsaAll.Items.Count)
            {
              p_lstChoices = lstChoices;
            }
          }
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Gets the list of possible next values.
    /// </summary>
    /// <param name="p_xseElement">The current elment in the XML document.</param>
    /// <param name="p_lstSiblings">The siblings of the current XML element.</param>
    /// <param name="p_rtvReturnType">The type of autocomplete items we want.</param>
    /// <returns>The list of possible next values.</returns>
    private List<KeyValuePair<string, string>> GetAutoCompleteList(XmlSchemaElement p_xseElement,
                                                                   List<string> p_lstSiblings,
                                                                   AutoCompleteType p_rtvReturnType)
    {
      List<KeyValuePair<string, string>> lstCompleteList = null;
      switch (p_rtvReturnType)
      {
        case AutoCompleteType.Element:
          if (p_xseElement.ElementSchemaType is XmlSchemaComplexType)
          {
            var xctElement = (XmlSchemaComplexType) p_xseElement.ElementSchemaType;
            if (xctElement.ContentTypeParticle != null)
            {
              if (!ContainsSiblings(xctElement.ContentTypeParticle, ref lstCompleteList, p_lstSiblings))
              {
                lstCompleteList = GetChildrenElements(xctElement.ContentTypeParticle);
              }
            }
          }
          else
          {
            var xstElement = (XmlSchemaSimpleType) p_xseElement.ElementSchemaType;
            switch (xstElement.TypeCode)
            {
              case XmlTypeCode.String:
                lstCompleteList = new List<KeyValuePair<string, string>>();
                lstCompleteList.Add(new KeyValuePair<string, string>("![CDATA[", "Character data."));
                break;
            }
          }
          if (lstCompleteList == null)
          {
            lstCompleteList = new List<KeyValuePair<string, string>>();
          }
          lstCompleteList.Add(new KeyValuePair<string, string>("/" + p_xseElement.Name + ">", "Closing tag."));
          break;
        case AutoCompleteType.Attribute:
          lstCompleteList = new List<KeyValuePair<string, string>>();
          if (p_xseElement.ElementSchemaType is XmlSchemaComplexType)
          {
            var xctElement = (XmlSchemaComplexType) p_xseElement.ElementSchemaType;
            foreach (var xsoAttribute in xctElement.Attributes)
            {
              if (xsoAttribute is XmlSchemaAttribute)
              {
                if (!p_lstSiblings.Contains(((XmlSchemaAttribute) xsoAttribute).Name))
                {
                  lstCompleteList.Add(new KeyValuePair<string, string>(((XmlSchemaAttribute) xsoAttribute).Name,
                                                                       GetDocumentation(
                                                                         (XmlSchemaAnnotated) xsoAttribute)));
                }
              }
              else if (xsoAttribute.ToString() == "System.Xml.Schema.XmlSchemaAttributeGroupRef")
              {
              }
            }
          }
          break;
        case AutoCompleteType.AttributeValues:
          lstCompleteList = new List<KeyValuePair<string, string>>();
          if (p_xseElement.ElementSchemaType is XmlSchemaComplexType)
          {
            var xctElement = (XmlSchemaComplexType) p_xseElement.ElementSchemaType;
            XmlSchemaAttribute xsaAttribute = null;
            foreach (var attribute in xctElement.Attributes)
            {
              if (attribute is XmlSchemaAttribute)
              {
                xsaAttribute = (XmlSchemaAttribute) attribute;
                if (xsaAttribute.Name == p_lstSiblings[p_lstSiblings.Count - 1])
                {
                  break;
                }
              }
              xsaAttribute = null;
            }
            if (xsaAttribute != null)
            {
              XmlSchemaSimpleType xssSimpleType;
              if (xsaAttribute.SchemaType != null)
              {
                xssSimpleType = xsaAttribute.SchemaType;
              }
              else
              {
                xssSimpleType = (XmlSchemaSimpleType) m_xstSchema.GlobalTypes[xsaAttribute.SchemaTypeName];
              }
              if (xssSimpleType == null)
              {
                switch (xsaAttribute.AttributeSchemaType.TypeCode)
                {
                  case XmlTypeCode.Boolean:
                    lstCompleteList.Add(new KeyValuePair<string, string>("0", null));
                    lstCompleteList.Add(new KeyValuePair<string, string>("1", null));
                    lstCompleteList.Add(new KeyValuePair<string, string>("true", null));
                    lstCompleteList.Add(new KeyValuePair<string, string>("false", null));
                    break;
                }
              }
              else if (xssSimpleType.Content.ToString() == "System.Xml.Schema.XmlSchemaSimpleTypeRestriction")
              {
                foreach (
                  XmlSchemaEnumerationFacet sefEnumValue in
                    ((XmlSchemaSimpleTypeRestriction) xssSimpleType.Content).Facets)
                {
                  lstCompleteList.Add(new KeyValuePair<string, string>(sefEnumValue.Value,
                                                                       GetDocumentation(sefEnumValue)));
                }
              }
            }
          }
          break;
      }

      return lstCompleteList;
    }

    /// <summary>
    /// Parse the current XML Schema to generate a list of autocomplete values.
    /// </summary>
    /// <param name="p_stkCode">A stack describing the path to the current XML element in the document.</param>
    /// <param name="p_lstSiblings">The siblings of the current XML element.</param>
    /// <param name="p_rtvReturnType">The type of autocomplete items we want.</param>
    /// <returns>The list of possible autocomplete values.</returns>
    private List<KeyValuePair<string, string>> ParseSchema(Stack<string> p_stkCode, List<string> p_lstSiblings,
                                                           AutoCompleteType p_rtvReturnType)
    {
      var lstElements = new List<KeyValuePair<string, string>>();
      if (m_xstSchema == null)
      {
        return lstElements;
      }

      XmlSchemaElement xseCurrentElement = null;
      foreach (XmlSchemaElement parentElement in m_xstSchema.GlobalElements.Values)
      {
        if (p_stkCode.Count == 0)
        {
          lstElements.Add(new KeyValuePair<string, string>(parentElement.Name, GetDocumentation(parentElement)));
        }
        else if (p_stkCode.Peek().Equals(parentElement.Name))
        {
          xseCurrentElement = findElement(parentElement, p_stkCode);
          break;
        }
      }
      if (xseCurrentElement != null)
      {
        lstElements.AddRange(GetAutoCompleteList(xseCurrentElement, p_lstSiblings, p_rtvReturnType));
      }

      return lstElements;
    }

    #endregion

    #region Constructors

    /// <summary>
    /// The default constructor.
    /// </summary>
    public XmlCompletionProvider(XmlEditor p_xedEditor)
    {
      ImageList = new ImageList();
      ImageList.TransparentColor = Color.Magenta;
      ImageList.Images.Add(Resources.xml);
      ImageList.Images.Add(Resources.Properties);
      ImageList.Images.Add(Resources.EnumItem);
    }

    #endregion

    #region ICompletionDataProvider Members

    #region Properties

    /// <summary>
    /// Gets the index of the default image to use for aucompletion items.
    /// </summary>
    /// <value>The index of the default image to use for aucompletion items.</value>
    public int DefaultIndex
    {
      get
      {
        return 0;
      }
    }

    /// <summary>
    /// Gets the image list to use for the autocompletion window.
    /// </summary>
    /// <value>The image list to use for the autocompletion window.</value>
    public ImageList ImageList { get; private set; }

    /// <summary>
    /// Gets the preselection.
    /// </summary>
    /// <value>The preselection.</value>
    public string PreSelection { get; private set; }

    #endregion

    /// <summary>
    /// Generate the list of possible code copmletion values.
    /// </summary>
    /// <param name="p_strFileName">The name of the file being edited.</param>
    /// <param name="p_txaTextArea">The area containing the document being edited.</param>
    /// <param name="p_chrCharTyped">The character that was typed that triggered the request for
    /// the code completion list.</param>
    /// <returns>The list of possible code copmletion values.</returns>
    public ICompletionData[] GenerateCompletionData(string p_strFileName, TextArea p_txaTextArea, char p_chrCharTyped)
    {
      var strText = p_txaTextArea.Document.TextContent.Substring(0, p_txaTextArea.Caret.Offset);
      var intOpenTagPos = strText.LastIndexOf('<');
      var booInsideTag = intOpenTagPos > strText.LastIndexOf('>');
      var booInsideValue = false;

      if (!booInsideTag && (p_chrCharTyped != '<'))
      {
        return null;
      }

      if (booInsideTag)
      {
        var intQuoteCount = 0;
        for (var intStartPos = intOpenTagPos;
             (intStartPos = strText.IndexOf('"', intStartPos + 1)) > -1;
             intQuoteCount++)
        {
        }
        booInsideValue = (intQuoteCount%2 == 1);
      }

      PreSelection = null;

      //parse the buffer
      var mclTags = rgxTagContents.Matches(strText);
      var lstTagAncestors = new LinkedList<KeyValuePair<string, string>>();
      var dicSiblings = new Dictionary<int, List<string>>();
      var intDepth = -1;
      foreach (Match mtcTag in mclTags)
      {
        var strTag = mtcTag.Groups[1].Value.Trim();
        var strTagName = rgxTagName.Match(strTag).Groups[1].Value;
        if (strTag.StartsWith("/"))
        {
          var intAcestorCount = lstTagAncestors.Count;
          var intLastIndex = intAcestorCount - 1;
          var lndItem = lstTagAncestors.Last;
          while ((lndItem != null) && !lndItem.Value.Key.Equals(strTagName))
          {
            intLastIndex--;
            lndItem = lndItem.Previous;
          }
          if (intLastIndex > -1)
          {
            while ((lstTagAncestors.Last != null) && !lstTagAncestors.Last.Value.Key.Equals(strTagName))
            {
              lstTagAncestors.RemoveLast();
            }
            lstTagAncestors.RemoveLast();
            var intOldDepth = intDepth;
            intDepth -= intAcestorCount - intLastIndex;
            for (var i = intOldDepth + 1; i > intDepth + 1; i--)
            {
              if (dicSiblings.ContainsKey(i))
              {
                dicSiblings[i].Clear();
              }
            }
          }
        }
        else
        {
          intDepth++;
          if (!dicSiblings.ContainsKey(intDepth))
          {
            dicSiblings[intDepth] = new List<string>();
          }
          dicSiblings[intDepth].Add(strTagName);
          if (!strTag.EndsWith("/"))
          {
            lstTagAncestors.AddLast(new KeyValuePair<string, string>(strTagName, strTag));
          }
          else
          {
            intDepth--;
          }
        }
      }
      intDepth++;
      if (!dicSiblings.ContainsKey(intDepth))
      {
        dicSiblings[intDepth] = new List<string>();
      }
      var stkAncestors = new Stack<string>();
      for (var llnLast = lstTagAncestors.Last;
           llnLast != null;
           llnLast = llnLast.Previous)
      {
        stkAncestors.Push(llnLast.Value.Key);
      }

      var lstSiblings = dicSiblings[intDepth];
      m_actCompleteType = AutoCompleteType.Element;

      if (booInsideValue || (booInsideTag && p_chrCharTyped.Equals('=')))
      {
        var strOutsideText = strText;
        if (booInsideValue)
        {
          var intValueStart = strText.LastIndexOf('"');
          strOutsideText = strText.Substring(0, intValueStart);
        }
        lstSiblings = new List<string>();
        if (rgxLastAttribute.IsMatch(strOutsideText))
        {
          lstSiblings.Add(rgxLastAttribute.Match(strOutsideText).Groups[1].Value);
        }

        m_actCompleteType = AutoCompleteType.AttributeValues;
      }
      else if (booInsideTag && p_chrCharTyped.Equals(' '))
      {
        var strTagContents = lstTagAncestors.Last.Value.Value;
        lstSiblings = new List<string>();
        foreach (Match mtcAttribute in rgxAttribute.Matches(strTagContents))
        {
          lstSiblings.Add(mtcAttribute.Groups[1].Value);
        }
        m_actCompleteType = AutoCompleteType.Attribute;
      }
      var lstComplete = ParseSchema(stkAncestors, lstSiblings, m_actCompleteType);

      var k = new List<XmlCompletionData>();
      if (lstComplete.Count > 0)
      {
        foreach (var kvpCompletion in lstComplete)
        {
          k.Add(new XmlCompletionData(m_actCompleteType, kvpCompletion.Key, kvpCompletion.Value));
        }
      }

      m_dicExtraCompletionCharacters.Clear();
      if (GotAutoCompleteList != null)
      {
        var stbPath = new StringBuilder();
        for (var llnFirst = lstTagAncestors.First;
             llnFirst != null;
             llnFirst = llnFirst.Next)
        {
          stbPath.Append(llnFirst.Value.Key);
          if (llnFirst.Next != null)
          {
            stbPath.Append(Path.DirectorySeparatorChar);
          }
        }

        var strLastWord = "";
        if (ProcessKey(p_chrCharTyped) == CompletionDataProviderKeyResult.NormalKey)
        {
          var twdLastWord =
            p_txaTextArea.Document.GetLineSegment(p_txaTextArea.Caret.Line).GetWord(p_txaTextArea.Caret.Column - 1);
          if (booInsideValue)
          {
            var intValueStart = strText.LastIndexOf('"') + 1;
            if (intValueStart < strText.Length)
            {
              strLastWord = strText.Substring(intValueStart) + p_chrCharTyped;
            }
            else
            {
              strLastWord = p_chrCharTyped.ToString();
            }
          }
          else
          {
            if (!twdLastWord.Word.Equals("\""))
            {
              if (!twdLastWord.Word.Equals("="))
              {
                strLastWord = twdLastWord.Word + p_chrCharTyped;
              }
            }
            else
            {
              strLastWord = p_chrCharTyped.ToString();
            }
          }
          PreSelection = String.IsNullOrEmpty(strLastWord)
            ? null
            : strLastWord.Substring(0, strLastWord.Length - 1);
        }
        var aclArgs = new AutoCompleteListEventArgs(k, stbPath.ToString(), lstSiblings.ToArray(),
                                                                          m_actCompleteType, strLastWord);
        GotAutoCompleteList(this, aclArgs);
        m_dicExtraCompletionCharacters[m_actCompleteType] = aclArgs.ExtraInsertionCharacters;
      }

      return k.ToArray();
    }

    /// <summary>
    /// Inserts the selected completion value.
    /// </summary>
    /// <remarks>
    /// If we are closing a tag, we request a reformatting of the line.
    /// </remarks>
    /// <param name="p_cdtData">The code completion selection that was chosen.</param>
    /// <param name="p_txaTextArea">The area containing the document being edited.</param>
    /// <param name="p_intInsertionOffset">Where the selection should be inserted into the document.</param>
    /// <param name="p_chrKey">The character that was used to choose this completion selection.</param>
    /// <returns><lang langref="true"/> if the insertion of <paramref name="p_chrKey"/> was handled;
    /// <lang langref="false"/> otherwise.</returns>
    public bool InsertAction(ICompletionData p_cdtData, TextArea p_txaTextArea, int p_intInsertionOffset, char p_chrKey)
    {
      p_txaTextArea.Caret.Position = p_txaTextArea.Document.OffsetToPosition(p_intInsertionOffset);
      var booInserted = p_cdtData.InsertAction(p_txaTextArea, p_chrKey);

      if (p_cdtData.Text.StartsWith("/"))
      {
        p_txaTextArea.Document.FormattingStrategy.IndentLine(p_txaTextArea, p_txaTextArea.Caret.Position.Line);
        if (p_chrKey == '/')
        {
          return true;
        }
      }
      return booInserted;
    }

    /// <summary>
    /// Determines if the given character should trigger selection of the current code completion
    /// item in the code completion window.
    /// </summary>
    /// <param name="p_chrKey">The key for which it is to be determined if it should trigger selection of the current code completion
    /// item in the code completion window.</param>
    /// <returns>The type of key for the given character.</returns>
    public CompletionDataProviderKeyResult ProcessKey(char p_chrKey)
    {
      List<char> lstExtraChars;
      if (!m_dicExtraCompletionCharacters.TryGetValue(m_actCompleteType, out lstExtraChars))
      {
        lstExtraChars = new List<char>();
      }
      switch (m_actCompleteType)
      {
        case AutoCompleteType.Attribute:
          if (lstExtraChars.Contains(p_chrKey) || p_chrKey.Equals('=') || p_chrKey.Equals('\n') || p_chrKey.Equals('\t'))
          {
            return CompletionDataProviderKeyResult.InsertionKey;
          }
          if (p_chrKey.Equals(' '))
          {
            return CompletionDataProviderKeyResult.BeforeStartKey;
          }
          return CompletionDataProviderKeyResult.NormalKey;
        case AutoCompleteType.AttributeValues:
          if (lstExtraChars.Contains(p_chrKey) || p_chrKey.Equals('\n') || p_chrKey.Equals('\t'))
          {
            return CompletionDataProviderKeyResult.InsertionKey;
          }
          return CompletionDataProviderKeyResult.NormalKey;
        case AutoCompleteType.Element:
          if (lstExtraChars.Contains(p_chrKey))
          {
            return CompletionDataProviderKeyResult.InsertionKey;
          }
          if (char.IsLetterOrDigit(p_chrKey) || p_chrKey == '_')
          {
            return CompletionDataProviderKeyResult.NormalKey;
          }
          return CompletionDataProviderKeyResult.InsertionKey;
        default:
          throw new InvalidOperationException("Unrecognized value for AutoCompleteType enum.");
      }
    }

    #endregion
  }
}