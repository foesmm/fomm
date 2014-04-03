using System;
using ICSharpCode.TextEditor;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using System.Xml.Schema;
using System.Xml;
using ICSharpCode.TextEditor.Document;
using System.IO;
using System.Text;

namespace Fomm.Controls
{
  /// <summary>
  /// The event arguments for events that allow extending the code completion list, on regenerating
  /// the list on the next key press.
  /// </summary>
  public class RegeneratableAutoCompleteListEventArgs : AutoCompleteListEventArgs
  {
    #region Properties

    /// <summary>
    /// Gets or sets whether the next keys press should cause the code completion list to regenerate.
    /// </summary>
    /// <value>Whether the next keys press should cause the code completion list to regenerate.</value>
    public bool GenerateOnNextKey { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// A copy constructor.
    /// </summary>
    /// <remarks>
    /// This constructor creates a <see cref="RegeneratableAutoCompleteListEventArgs"/> based on the given
    /// <see cref="AutoCompleteListEventArgs"/>.
    /// </remarks>
    /// <param name="p_acaArgs">The <see cref="AutoCompleteListEventArgs"/> on which to base
    /// this object.</param>
    public RegeneratableAutoCompleteListEventArgs(AutoCompleteListEventArgs p_acaArgs)
      : base(
        p_acaArgs.AutoCompleteList, p_acaArgs.ElementPath, p_acaArgs.Siblings, p_acaArgs.AutoCompleteType,
        p_acaArgs.LastWord)
    {
    }

    #endregion
  }

  /// <summary>
  /// An XML text editor.
  /// </summary>
  /// <remarks>
  /// This editor provides highlighting, code copmletion, autoindenting, and code folding.
  /// </remarks>
  public class XmlEditor : TextEditorControl
  {
    /// <summary>
    /// Raised when the code completion options have been retrieved.
    /// </summary>
    /// <remarks>
    /// Handling this event allows the addition/removal of code completion items.
    /// </remarks>
    public event EventHandler<RegeneratableAutoCompleteListEventArgs> GotAutoCompleteList;

    private static Regex rgxTagContents = new Regex("<([^!>][^>]*)>?", RegexOptions.Singleline);

    private Timer m_tmrFoldUpdater = new Timer();
    private Timer m_tmrValidator = new Timer();
    private XmlCompletionProvider m_cdpXmlCompletionProvider;
    private CodeCompletionWindow m_ccwCodeCompletionWindow;
    private XmlSchema m_xshSchema;
    private bool m_booMalformedXml;
    private XmlReaderSettings m_xrsSettings;
    private bool m_booFormatOnce;
    private bool m_booGenerateOnNextKey;
    private char m_chrLastChar = '\0';

    #region Properties

    /// <summary>
    /// Sets the XML Schema used to validate the editor content.
    /// </summary>
    /// <value>The XML Schema used to validate the editor content.</value>
    public XmlSchema Schema
    {
      set
      {
        if (m_xshSchema == value)
        {
          return;
        }
        if (m_xrsSettings == null)
        {
          m_xrsSettings = new XmlReaderSettings();
          m_xrsSettings.ConformanceLevel = ConformanceLevel.Document;
          m_xrsSettings.ValidationType = ValidationType.Schema;
          m_xrsSettings.ValidationEventHandler += HighlightValidationErrors;
        }
        if (m_xshSchema != null)
        {
          m_xrsSettings.Schemas.RemoveRecursive(m_xshSchema);
        }
        m_xshSchema = value;
        if (m_xshSchema != null)
        {
          m_xrsSettings.Schemas.Add(m_xshSchema);
        }

        m_cdpXmlCompletionProvider.Schema = value;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// The default constructor.
    /// </summary>
    public XmlEditor()
    {
      m_cdpXmlCompletionProvider = new XmlCompletionProvider(this);

      SetHighlighting("XML");
      ActiveTextAreaControl.TextArea.KeyEventHandler +=
        TextArea_KeyEventHandler;
      Disposed += DisposeCodeCompletionWindow;

      Document.FoldingManager.FoldingStrategy = new XmlFoldingStrategy();
      Document.FormattingStrategy = new XmlFormattingStrategy();

      m_tmrFoldUpdater.Tick += UpdateFolds;
      m_tmrFoldUpdater.Interval = 2000;

      m_tmrValidator.Tick += ValidateOnTimer;
      m_tmrValidator.Interval = 2000;

      m_cdpXmlCompletionProvider.GotAutoCompleteList +=
        m_cdpXmlCompletionProvider_GotAutoCompleteList;
    }

    #endregion

    #region Code Completion

    /// <summary>
    /// Handles the <see cref="XmlCompletionProvider.GotAutoCompleteList"/> event of the
    /// completion provider.
    /// </summary>
    /// <remarks>
    /// This raises the editor's <see cref="GotAutoCompleteList"/> event.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="AutoCompleteListEventArgs"/> describing the event arguments.</param>
    private void m_cdpXmlCompletionProvider_GotAutoCompleteList(object sender, AutoCompleteListEventArgs e)
    {
      if (GotAutoCompleteList != null)
      {
        var raaArgs = new RegeneratableAutoCompleteListEventArgs(e);
        GotAutoCompleteList(this, raaArgs);
        m_booGenerateOnNextKey = raaArgs.GenerateOnNextKey;
        e.ExtraInsertionCharacters.AddRange(raaArgs.ExtraInsertionCharacters);
      }
    }

    /// <summary>
    /// Called whenever a character is about to be added to the document.
    /// </summary>
    /// <param name="p_chrChar">The character about to be added.</param>
    /// <returns><lang langref="true"/> if the character has been handled; <lang langref="false"/> otherwise.</returns>
    private bool TextArea_KeyEventHandler(char p_chrChar)
    {
      if ((m_ccwCodeCompletionWindow != null) && m_ccwCodeCompletionWindow.ProcessKeyEvent(p_chrChar))
      {
        return true;
      }
      m_chrLastChar = p_chrChar;
      if (p_chrChar.Equals('<') || p_chrChar.Equals(' ') || m_booGenerateOnNextKey)
      {
        m_booGenerateOnNextKey = false;
        ShowCodeCompletionWindow(p_chrChar);
      }
      return false;
    }

    /// <summary>
    /// Displays the code completion window.
    /// </summary>
    /// <param name="p_chrChar">The character that was typed that caused the code window to display.</param>
    public void ShowCodeCompletionWindow(char p_chrChar)
    {
      m_ccwCodeCompletionWindow = CodeCompletionWindow.ShowCompletionWindow(FindForm(), this, null,
                                                                            m_cdpXmlCompletionProvider, p_chrChar, true,
                                                                            false);
      //m_ccwCodeCompletionWindow is null if there are no valid completions
      if (m_ccwCodeCompletionWindow != null)
      {
        m_ccwCodeCompletionWindow.Closed += DisposeCodeCompletionWindow;
      }
    }

    /// <summary>
    /// Disposes of the code completion window when it closes.
    /// </summary>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    private void DisposeCodeCompletionWindow(object sender, EventArgs e)
    {
      if (m_ccwCodeCompletionWindow != null)
      {
        m_ccwCodeCompletionWindow.Closed -= DisposeCodeCompletionWindow;
        m_ccwCodeCompletionWindow.Dispose();
        m_ccwCodeCompletionWindow = null;
      }
    }

    /// <summary>
    /// Starts the timers to update the code folds and validate the XML.
    /// </summary>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    protected override void OnTextChanged(EventArgs e)
    {
      m_tmrFoldUpdater.Stop();
      m_tmrFoldUpdater.Start();
      if (m_xshSchema != null)
      {
        m_tmrValidator.Stop();
        m_tmrValidator.Start();
      }
      var intCaretOffset = ActiveTextAreaControl.Caret.Offset;
      if (!m_booFormatOnce &&
          (intCaretOffset > 0) &&
          (intCaretOffset < Document.TextLength) &&
          (Document.GetCharAt(intCaretOffset) == '>') &&
          (Document.TextContent.LastIndexOf("</", intCaretOffset) >
           Document.TextContent.LastIndexOf(">", intCaretOffset - 1)))
      {
        m_booFormatOnce = true;
        Document.FormattingStrategy.IndentLine(ActiveTextAreaControl.TextArea, ActiveTextAreaControl.Caret.Position.Line);
      }
      m_booFormatOnce = false;

      base.OnTextChanged(e);

      if (m_chrLastChar.Equals('=') && XmlParser.IsInsideTag(ActiveTextAreaControl.TextArea))
      {
        m_chrLastChar = '"';
        var caret = ActiveTextAreaControl.Caret;
        Document.Insert(caret.Offset + 1, "\"\"");
        caret.Position = Document.OffsetToPosition(caret.Offset + 1);
        ShowCodeCompletionWindow('=');
      }
    }

    #endregion

    /// <summary>
    /// Updates the code folds.
    /// </summary>
    /// <remarks>
    /// This method is called by a timer after a set span after the text in the editor was last changed.
    /// </remarks>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    private void UpdateFolds(object sender, EventArgs e)
    {
      try
      {
        Document.FoldingManager.UpdateFoldings(null, null);
        Document.FoldingManager.NotifyFoldingsChanged(null);
      }
      catch
      {
      }
      m_tmrFoldUpdater.Stop();
    }

    #region Validation

    /// <summary>
    /// Validates the XML against the schema, after a given period has elapsed.
    /// </summary>
    /// <remarks>
    /// This method is called by a timer after a set span after the text in the editor was last changed.
    /// </remarks>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    private void ValidateOnTimer(object sender, EventArgs e) //ElapsedEventArgs e)
    {
      ValidateXml();
      ActiveTextAreaControl.TextArea.Invalidate();
    }

    /// <summary>
    /// Validates the XML against the schema.
    /// </summary>
    /// <returns><lang langref="true"/> if the XML is valid; <lang langref="false"/> otherwise.</returns>
    public bool ValidateXml()
    {
      m_tmrValidator.Stop();

      var docDocument = ActiveTextAreaControl.TextArea.Document;
      docDocument.MarkerStrategy.RemoveAll(x =>
      {
        return (x.TextMarkerType == TextMarkerType.WaveLine);
      });
      m_booMalformedXml = false;

      if (docDocument.TextLength == 0)
      {
        return true;
      }

      var stkBadTags = XmlParser.ParseTags(docDocument, docDocument.TotalNumberOfLines - 1, null,
                                                          HighlightMalformedTag);
      //this deals with extra tags at beginning of file
      if ((stkBadTags.Count > 0) || m_booMalformedXml)
      {
        while (stkBadTags.Count > 0)
        {
          var tpsTag = stkBadTags.Pop();
          var tlcStart = new TextLocation(tpsTag.Column, tpsTag.LineNumber);
          var tlcEnd = new TextLocation(tpsTag.Column + tpsTag.Name.Length, tpsTag.LineNumber);
          HighlightMalformedTag(docDocument, tpsTag.Name, tlcStart, tlcEnd);
        }
        return false;
      }

      var intBadLineNum = Int32.MaxValue;
      using (var xrdValidator = XmlReader.Create(new StringReader(Text), m_xrsSettings))
      {
        try
        {
          while (xrdValidator.Read())
          {
          }
          if (m_booMalformedXml)
          {
            return false;
          }
        }
        catch (XmlException err2)
        {
          intBadLineNum = err2.LineNumber;
          HighlightValidationErrors(err2.Message, new TextLocation(err2.LinePosition - 1, err2.LineNumber - 1));
        }
        finally
        {
          xrdValidator.Close();
        }
      }
      for (var i = intBadLineNum; i < docDocument.TotalNumberOfLines; i++)
      {
        var strLine = docDocument.GetText(docDocument.GetLineSegment(i));
        var intLastOpenPos = strLine.LastIndexOf('<');
        if (intLastOpenPos < 0)
        {
          continue;
        }
        var intLastClosePos = strLine.LastIndexOf('>');
        if ((intLastClosePos > -1) && (intLastOpenPos > intLastClosePos))
        {
          var stbLines = new StringBuilder(strLine);
          //there is an open tag on this line - read lines until it is closed.
          for (; i < docDocument.TotalNumberOfLines; i++)
          {
            var strNextLine = docDocument.GetText(docDocument.GetLineSegment(i));
            intLastClosePos = strLine.LastIndexOf('>');
            stbLines.Append(strNextLine);
            if (intLastClosePos < 0)
            {
              i--;
              break;
            }
          }
          strLine = stbLines.ToString();
        }

        var mclLineTags = rgxTagContents.Matches(strLine);
        foreach (Match mtcTag in mclLineTags)
        {
          var strTag = mtcTag.Groups[1].Value.Trim();
          if (strTag.StartsWith("/"))
          {
            HighlightValidationErrors("Unexpected end tag.", new TextLocation(mtcTag.Groups[1].Index + 1, i));
          }
          else
          {
            HighlightValidationErrors("Invalid tag.", new TextLocation(mtcTag.Groups[1].Index, i));
          }
        }
      }
      return (intBadLineNum == Int32.MaxValue);
    }

    /// <summary>
    /// Highlights any malformed XML tags that are found.
    /// </summary>
    /// <param name="p_docDocument">The document being validated.</param>
    /// <param name="p_strTagName">The name of the malformed tag.</param>
    /// <param name="p_tlcStart">The start of the malformed tag.</param>
    /// <param name="p_tlcEnd">The end of the malformed tag.</param>
    protected void HighlightMalformedTag(IDocument p_docDocument, string p_strTagName, TextLocation p_tlcStart,
                                         TextLocation p_tlcEnd)
    {
      m_booMalformedXml = true;
      HighlightValidationErrors("Tag was not closed.", p_tlcStart);
    }

    /// <summary>
    /// Highlights any XML validation errors that are found.
    /// </summary>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">A <see cref="ValidationEventArgs"/> describing the event arguments.</param>
    private void HighlightValidationErrors(object sender, ValidationEventArgs e)
    {
      m_booMalformedXml = true;
      HighlightValidationErrors(e.Message, new TextLocation(e.Exception.LinePosition - 1, e.Exception.LineNumber - 1));
    }

    /// <summary>
    /// Highlights any XML validation errors that are found.
    /// </summary>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">A <see cref="ValidationEventArgs"/> describing the event arguments.</param>
    private void HighlightValidationErrors(string p_strMessage, TextLocation p_tlcStart)
    {
      var docDocument = ActiveTextAreaControl.TextArea.Document;
      var twdWord = docDocument.GetLineSegment(p_tlcStart.Line).GetWord(p_tlcStart.Column);
      var intWordOffest = docDocument.PositionToOffset(p_tlcStart);
      var tmkError = new TextMarker(intWordOffest, (twdWord == null) ? 1 : twdWord.Length,
                                           TextMarkerType.WaveLine);
      tmkError.ToolTip = p_strMessage;
      docDocument.MarkerStrategy.AddMarker(tmkError);
    }

    #endregion

    /// <summary>
    /// Disposes of resources used by the editor.
    /// </summary>
    /// <remarks>
    /// This makes sure that the code completion windows is closed.
    /// </remarks>
    /// <param name="disposing">Whether or not the object is being disposed.</param>
    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (m_ccwCodeCompletionWindow != null)
      {
        m_ccwCodeCompletionWindow.Close();
        m_ccwCodeCompletionWindow.Dispose();
      }
    }
  }
}