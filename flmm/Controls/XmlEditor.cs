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
using System.Collections.Generic;

namespace Fomm.Controls
{
	/// <summary>
	/// An XML text editor.
	/// </summary>
	/// <remarks>
	/// This editor provides highlighting, code copmletion, autoindenting, and code folding.
	/// </remarks>
	public class XmlEditor : TextEditorControl
	{
		public event EventHandler<AutoCompleteListEventArgs> GotAutoCompleteList
		{
			add
			{
				m_cdpXmlCompletionProvider.GotAutoCompleteList += value;
			}
			remove
			{
				m_cdpXmlCompletionProvider.GotAutoCompleteList -= value;
			}
		}

		private static Regex rgxTagContents = new Regex("<([^!>][^>]*)>?", RegexOptions.Singleline);

		private readonly List<char> COMPLETION_CHARS = new List<char> { '<', ' ', '=' };
		private Timer m_tmrFoldUpdater = new Timer();
		private Timer m_tmrValidator = new Timer();
		private XmlCompletionProvider m_cdpXmlCompletionProvider = null;
		private CodeCompletionWindow m_ccwCodeCompletionWindow = null;
		private XmlSchema m_xshSchema = null;
		private bool m_booMalformedXml = false;
		private XmlReaderSettings m_xrsSettings = null;
		bool m_booFormatOnce = false;

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
					return;
				if (m_xrsSettings == null)
				{
					m_xrsSettings = new XmlReaderSettings();
					m_xrsSettings.ConformanceLevel = ConformanceLevel.Document;
					m_xrsSettings.ValidationType = ValidationType.Schema;
					m_xrsSettings.ValidationEventHandler += new ValidationEventHandler(HighlightValidationErrors);
				}
				if (m_xshSchema != null)
					m_xrsSettings.Schemas.RemoveRecursive(m_xshSchema);
				m_xshSchema = value;
				if (m_xshSchema != null)
					m_xrsSettings.Schemas.Add(m_xshSchema);

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
			m_cdpXmlCompletionProvider = new XmlCompletionProvider();

			SetHighlighting("XML");
			ActiveTextAreaControl.TextArea.KeyEventHandler += new ICSharpCode.TextEditor.KeyEventHandler(TextArea_KeyEventHandler);
			this.Disposed += DisposeCodeCompletionWindow;

			Document.FoldingManager.FoldingStrategy = new XmlFoldingStrategy();
			Document.FormattingStrategy = new XmlFormattingStrategy();

			m_tmrFoldUpdater.Tick += UpdateFolds;
			m_tmrFoldUpdater.Interval = 2000;

			m_tmrValidator.Tick += ValidateOnTimer;
			m_tmrValidator.Interval = 2000;
		}

		#endregion

		/// <summary>
		/// Raises the <see cref="Control.Load"/> event.
		/// </summary>
		/// <remarks>
		/// This sets the synchronizing object on the timers to our form. Doing so allows the timers
		/// to update the UI.
		/// </remarks>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		protected override void OnLoad(EventArgs e)
		{
			/*m_tmrFoldUpdater.SynchronizingObject = this.FindForm();
			m_tmrValidator.SynchronizingObject = this.FindForm();*/
			base.OnLoad(e);
		}

		/// <summary>
		/// Called whenever a character is about to be added to the document.
		/// </summary>
		/// <param name="p_chrChar">The character about to be added.</param>
		/// <returns><lang cref="true"/> if the character has been handled; <lang cref="false"/> otherwise.</returns>
		private bool TextArea_KeyEventHandler(char p_chrChar)
		{
			if ((m_ccwCodeCompletionWindow != null) && m_ccwCodeCompletionWindow.ProcessKeyEvent(p_chrChar))
				return true;
			if (COMPLETION_CHARS.Contains(p_chrChar))
			{
				m_ccwCodeCompletionWindow = CodeCompletionWindow.ShowCompletionWindow(this.FindForm(), this, null, m_cdpXmlCompletionProvider, p_chrChar);
				//m_ccwCodeCompletionWindow is null if there are no valid completions
				if (m_ccwCodeCompletionWindow != null)
					m_ccwCodeCompletionWindow.Closed += new EventHandler(DisposeCodeCompletionWindow);
			}

			return false;
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
				m_ccwCodeCompletionWindow.Closed -= new EventHandler(DisposeCodeCompletionWindow);
				m_ccwCodeCompletionWindow.Dispose();
				m_ccwCodeCompletionWindow = null;
			}
		}

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
			Document.FoldingManager.UpdateFoldings(null, null);
			Document.FoldingManager.NotifyFoldingsChanged(null);
			m_tmrFoldUpdater.Stop();
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
			Int32 intCaretOffset = ActiveTextAreaControl.Caret.Offset;
			if (!m_booFormatOnce &&
				(intCaretOffset > 0) &&
				(intCaretOffset < Document.TextLength) &&
				(Document.GetCharAt(intCaretOffset) == '>') &&
				(Document.TextContent.LastIndexOf("</", intCaretOffset) > Document.TextContent.LastIndexOf(">", intCaretOffset - 1)))
			{
				m_booFormatOnce = true;
				Document.FormattingStrategy.IndentLine(ActiveTextAreaControl.TextArea, ActiveTextAreaControl.Caret.Position.Line);
			}
			m_booFormatOnce = false;

			base.OnTextChanged(e);
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
		void ValidateOnTimer(object sender, EventArgs e) //ElapsedEventArgs e)
		{
			ValidateXml();
			ActiveTextAreaControl.TextArea.Invalidate();
		}

		/// <summary>
		/// Validates the XML against the schema.
		/// </summary>
		public void ValidateXml()
		{
			m_tmrValidator.Stop();

			IDocument docDocument = ActiveTextAreaControl.TextArea.Document;
			docDocument.MarkerStrategy.RemoveAll(x => { return (x.TextMarkerType == TextMarkerType.WaveLine); });
			m_booMalformedXml = false;

			if (docDocument.TextLength == 0)
				return;

			XmlParser.TagStack stkBadTags = XmlParser.ParseTags(docDocument, docDocument.TotalNumberOfLines - 1, null, HighlightMalformedTag);
			//this deals with extra tags at beginning of file
			if ((stkBadTags.Count > 0) || m_booMalformedXml)
			{
				while (stkBadTags.Count > 0)
				{
					XmlParser.TagStack.TagPosition tpsTag = stkBadTags.Pop();
					TextLocation tlcStart = new TextLocation(tpsTag.Column, tpsTag.LineNumber);
					TextLocation tlcEnd = new TextLocation(tpsTag.Column + tpsTag.Name.Length, tpsTag.LineNumber);
					HighlightMalformedTag(docDocument, tpsTag.Name, tlcStart, tlcEnd);
				}
				return;
			}

			Int32 intBadLineNum = Int32.MaxValue;
			using (XmlReader xrdValidator = XmlReader.Create(new StringReader(this.Text), m_xrsSettings))
			{
				try
				{
					while (xrdValidator.Read()) ;
				}
				catch (XmlException err2)
				{
					intBadLineNum = err2.LineNumber;
					HighlightValidationErrors(err2.Message, new TextLocation(err2.LinePosition - 1, err2.LineNumber - 1));
				}
				xrdValidator.Close();
			}
			for (Int32 i = intBadLineNum; i < docDocument.TotalNumberOfLines; i++)
			{
				string strLine = docDocument.GetText(docDocument.GetLineSegment(i));
				Int32 intLineNum = i;
				Int32 intLastOpenPos = strLine.LastIndexOf('<');
				if (intLastOpenPos < 0)
					continue;
				Int32 intLastClosePos = strLine.LastIndexOf('>');
				if ((intLastClosePos > -1) && (intLastOpenPos > intLastClosePos))
				{
					string strNextLine = null;
					StringBuilder stbLines = new StringBuilder(strLine);
					//there is an open tag on this line - read lines until it is closed.
					for (; i < docDocument.TotalNumberOfLines; i++)
					{
						strNextLine = docDocument.GetText(docDocument.GetLineSegment(i));
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

				MatchCollection mclLineTags = rgxTagContents.Matches(strLine);
				foreach (Match mtcTag in mclLineTags)
				{
					string strTag = mtcTag.Groups[1].Value.Trim();
					if (strTag.StartsWith("/"))
						HighlightValidationErrors("Unexpected end tag.", new TextLocation(mtcTag.Groups[1].Index + 1, i));
					else
						HighlightValidationErrors("Invalid tag.", new TextLocation(mtcTag.Groups[1].Index, i));
				}
			}
		}

		/// <summary>
		/// Highlights any malformed XML tags that are found.
		/// </summary>
		/// <param name="p_docDocument">The document being validated.</param>
		/// <param name="p_strTagName">The name of the malformed tag.</param>
		/// <param name="p_tlcStart">The start of the malformed tag.</param>
		/// <param name="p_tlcEnd">The end of the malformed tag.</param>
		protected void HighlightMalformedTag(IDocument p_docDocument, string p_strTagName, TextLocation p_tlcStart, TextLocation p_tlcEnd)
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
			HighlightValidationErrors(e.Message, new TextLocation(e.Exception.LinePosition - 1, e.Exception.LineNumber - 1));
		}

		/// <summary>
		/// Highlights any XML validation errors that are found.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">A <see cref="ValidationEventArgs"/> describing the event arguments.</param>
		private void HighlightValidationErrors(string p_strMessage, TextLocation p_tlcStart)
		{
			IDocument docDocument = ActiveTextAreaControl.TextArea.Document;
			TextWord twdWord = docDocument.GetLineSegment(p_tlcStart.Line).GetWord(p_tlcStart.Column);
			Int32 intWordOffest = docDocument.PositionToOffset(p_tlcStart);
			TextMarker tmkError = new TextMarker(intWordOffest, (twdWord == null) ? 1 : twdWord.Length, TextMarkerType.WaveLine);
			tmkError.ToolTip = p_strMessage;
			docDocument.MarkerStrategy.AddMarker(tmkError);
		}

		#endregion
	}
}
