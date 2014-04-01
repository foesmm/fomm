using System.Collections.Generic;
using ICSharpCode.TextEditor.Document;

namespace Fomm.Controls
{
  /// <summary>
  /// Handles the folding of programming languages that use { }.
  /// </summary>
  public class CodeFoldingStrategy : IFoldingStrategy
  {
    /// <summary>
    /// Generates the list of markers indicating where the XML should be folded.
    /// </summary>
    /// <param name="document">The document to fold.</param>
    /// <param name="fileName">The file name of the document to fold.</param>
    /// <param name="parseInformation">User-supplied parse information.</param>
    /// <returns>The list of markers indicating where the code should be folded.</returns>
    public List<FoldMarker> GenerateFoldMarkers(IDocument document, string fileName, object parseInformation)
    {
      List<FoldMarker> list = new List<FoldMarker>();

      Stack<int> stack = new Stack<int>();
      //bool InComment;

      for (int i = 0; i < document.TotalNumberOfLines; i++)
      {
        string text = document.GetText(document.GetLineSegment(i)).Trim();
        if (text.StartsWith("}") && stack.Count > 0)
        {
          int pos = stack.Pop();
          list.Add(new FoldMarker(document, pos, document.GetLineSegment(pos).Length, i, 1));
        }
        if (text.EndsWith("{"))
        {
          stack.Push(i);
        }
      }

      return list;
    }
  }
}