using System;
using System.Windows.Forms;
using System.IO;
using System.Xml.Schema;
using System.Text.RegularExpressions;
using Fomm.Controls;
using Fomm.PackageManager.XmlConfiguredInstall.Parsers;
using System.Xml;

namespace Fomm.PackageManager.Controls
{
  public partial class FomodScriptEditor : UserControl
  {
    /// <summary>
    /// Raised when the code completion options for the XML config script have been retrieved.
    /// </summary>
    /// <remarks>
    /// Handling this event allows the addition/removal of code completion items.
    /// </remarks>
    public event EventHandler<RegeneratableAutoCompleteListEventArgs> GotXMLAutoCompleteList;

    #region Properties

    /// <summary>
    /// Gets or sets the <see cref="FomodScript"/> being edited.
    /// </summary>
    /// <value>The <see cref="FomodScript"/> being edited.</value>
    public FomodScript Script
    {
      get
      {
        var fscScript = new FomodScript(FomodScriptType.CSharp, null);
        if (ddtScript.SelectedTabPage == dtpCSharp)
        {
          fscScript.Type = FomodScriptType.CSharp;
          fscScript.Text = sedScript.Text;
        }
        else
        {
          if (!String.IsNullOrEmpty(xedScript.Text))
          {
            fscScript.Type = FomodScriptType.XMLConfig;
            var strHeader = "<?xml version=\"1.0\" encoding=\"UTF-16\" ?>" + Environment.NewLine +
                               "<config xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"http://qconsulting.ca/fo3/ModConfig{0}.xsd\">";
            strHeader = String.Format(strHeader, cbxVersion.SelectedItem);
            fscScript.Text = xedScript.Text.Replace("<config>", strHeader);
          }
        }
        return String.IsNullOrEmpty(fscScript.Text) ? null : fscScript;
      }
      set
      {
        if (value == null)
        {
          ddtScript.SelectedTabPage = dtpCSharp;
          sedScript.Text = null;
        }
        else
        {
          switch (value.Type)
          {
            case FomodScriptType.XMLConfig:
              switch (Parser.GetConfigVersion(value.Text))
              {
                case "1.0":
                  cbxVersion.SelectedIndex = 0;
                  break;
                case "2.0":
                  cbxVersion.SelectedIndex = 1;
                  break;
                case "3.0":
                  cbxVersion.SelectedIndex = 2;
                  break;
                case "4.0":
                  cbxVersion.SelectedIndex = 3;
                  break;
                case "5.0":
                  cbxVersion.SelectedIndex = 4;
                  break;
                default:
                  ddtScript.SelectedTabPage = dtpCSharp;
                  sedScript.Text = null;
                  return;
              }
              var rgxXMLConfigCleanup = new Regex(@"<\?xml[^>]+\?>.*?<config[^>]*>", RegexOptions.Singleline);
              ddtScript.SelectedTabPage = dtpXML;
              xedScript.Text = rgxXMLConfigCleanup.Replace(value.Text, "<config>");
              break;
            case FomodScriptType.CSharp:
              ddtScript.SelectedTabPage = dtpCSharp;
              sedScript.Text = value.Text;
              break;
            default:
              throw new Exception("Unrecognized value for FomodScriptType enum.");
          }
        }
      }
    }

    /// <summary>
    /// Gets whether or not the script being edited is valid.
    /// </summary>
    /// <value>Whether or not the script being edited is valid.</value>
    public bool IsValid
    {
      get
      {
        if (((ddtScript.SelectedTabPage == dtpCSharp) && !sedScript.ValidateSyntax()) ||
            ((ddtScript.SelectedTabPage == dtpXML) && !xedScript.ValidateXml()))
        {
          return false;
        }
        return true;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// The default constructor.
    /// </summary>
    public FomodScriptEditor()
    {
      InitializeComponent();

      cbxVersion.Items.Add("1.0");
      cbxVersion.Items.Add("2.0");
      cbxVersion.Items.Add("3.0");
      cbxVersion.Items.Add("4.0");
      cbxVersion.Items.Add("5.0");

      cbxVersion.SelectedIndex = 4;
      LoadConfigSchema();
    }

    #endregion

    /// <summary>
    /// Handles the <see cref="ComboBox.SelectedIndexChanged"/> event of the XML config version drop down list.
    /// </summary>
    /// <param name="sender">The object the raised the event.</param>
    /// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
    private void cbxVersion_SelectedIndexChanged(object sender, EventArgs e)
    {
      LoadConfigSchema();
    }

    /// <summary>
    /// This loads the selected version of the XML configuration script's schema into the XML editor.
    /// </summary>
    /// <remarks>
    /// Loading the schema into the editor allows for validation and auto-completion.
    /// </remarks>
    protected void LoadConfigSchema()
    {
      if (Program.GameMode != null)
      {
        var strSchemaPath = Program.GameMode.GetXMLConfigSchemaPath(cbxVersion.SelectedItem.ToString());
        if (File.Exists(strSchemaPath))
        {
          var xrsSettings = new XmlReaderSettings();
          xrsSettings.IgnoreComments = true;
          xrsSettings.IgnoreWhitespace = true;
          using (var xrdSchemaReader = XmlReader.Create(strSchemaPath, xrsSettings))
          {
            xedScript.Schema = XmlSchema.Read(xrdSchemaReader, delegate(object sender, ValidationEventArgs e)
            {
              throw e.Exception;
            });
          }
        }
      }
    }

    /// <summary>
    /// Handles the <see cref="XmlCompletionProvider.GotAutoCompleteList"/> event of the
    /// xml config editor.
    /// </summary>
    /// <remarks>
    /// This raises the editor's <see cref="GotXMLAutoCompleteList"/> event.
    /// </remarks>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">An <see cref="RegeneratableAutoCompleteListEventArgs"/> describing the event arguments.</param>
    private void xedScript_GotAutoCompleteList(object sender, RegeneratableAutoCompleteListEventArgs e)
    {
      if (GotXMLAutoCompleteList != null)
      {
        GotXMLAutoCompleteList(this, e);
      }
    }
  }
}