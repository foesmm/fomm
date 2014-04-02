using System;
using ICSharpCode.TextEditor.Document;
using System.Text;
using ICSharpCode.TextEditor;

namespace Fomm.Controls
{
  /// <summary>
  /// Handles the smart indenting of XML.
  /// </summary>
  public class XmlFormattingStrategy : DefaultFormattingStrategy
  {
    /// <summary>
    /// Indents the specified line based on the current depth of the XML hierarchy.
    /// </summary>
    /// <param name="p_txaTextArea">The text area containing the line to indent.</param>
    /// <param name="p_intLineNumber">The line number of the line to indent.</param>
    /// <returns>The indent depth of the specified line.</returns>
    protected override int AutoIndentLine(TextArea p_txaTextArea, int p_intLineNumber)
    {
      var stkTags = XmlParser.ParseTags(p_txaTextArea.Document, p_intLineNumber, null, null);
      var intDepth = 0;
      var intLastLineNum = -1;
      while (stkTags.Count > 0)
      {
        if (stkTags.Peek().LineNumber != intLastLineNum)
        {
          intLastLineNum = stkTags.Peek().LineNumber;
          intDepth++;
        }
        stkTags.Pop();
      }

      var stbLineWithIndent = new StringBuilder();
      for (var i = 0; i < intDepth; i++)
      {
        stbLineWithIndent.Append("\t");
      }
      stbLineWithIndent.Append(TextUtilities.GetLineAsString(p_txaTextArea.Document, p_intLineNumber).Trim());
      var oldLine = p_txaTextArea.Document.GetLineSegment(p_intLineNumber);
      var intCaretOffset = stbLineWithIndent.Length - oldLine.Length;
      SmartReplaceLine(p_txaTextArea.Document, oldLine, stbLineWithIndent.ToString());
      p_txaTextArea.Caret.Column += intCaretOffset;

      return intDepth;
    }
  }
}