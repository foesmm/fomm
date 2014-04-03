using System;
using System.Collections.Generic;
using Fomm.PackageManager;
using MessageBox = System.Windows.Forms.MessageBox;
using mbButtons = System.Windows.Forms.MessageBoxButtons;
using DialogResult = System.Windows.Forms.DialogResult;
using Path = System.IO.Path;
using Fomm.Games.Fallout3.Script;

namespace fomm.Scripting
{
  /// <summary>
  /// The class that handles the old scripting language.
  /// </summary>
  public static class fommScript
  {
    private class FlowControlStruct
    {
      public readonly int line;
      public readonly byte type;
      public readonly string[] values;
      public readonly string var;
      public bool active;
      public bool hitCase;
      public int forCount;

      //Inactive
      public FlowControlStruct(byte type)
      {
        line = -1;
        this.type = type;
      }

      //If
      public FlowControlStruct(int line, bool active)
      {
        this.line = line;
        this.active = active;
      }

      //Select
      public FlowControlStruct(int line, string[] values)
      {
        this.line = line;
        type = 1;
        this.values = values;
      }

      //For
      public FlowControlStruct(string[] values, string var, int line)
      {
        this.line = line;
        type = 2;
        this.values = values;
        this.var = var;
      }
    }

    private static Dictionary<string, string> variables;
    private static ModInstaller m_midInstaller;

    private static Fallout3ModInstallScript Script
    {
      get
      {
        return (Fallout3ModInstallScript) m_midInstaller.Script;
      }
    }

    #region Method Execution

    private delegate void GenereicVoidMethodDelegate();

    private delegate object GenereicReturnMethodDelegate();

    /// <summary>
    /// Executes the given void method.
    /// </summary>
    /// <remarks>
    /// This method is used to execute all void method calls the script needs to make.
    /// This allows for centralized error handling.
    /// 
    /// It should be noted that using delegates does engender a very slight performance hit,
    /// but given the nature of this application (more precisely, that this is a single-user
    /// application) there should not be any noticable difference.
    /// </remarks>
    /// <param name="p_gmdMethod">The method to execute.</param>
    /// <see cref="ExecuteMethod(GenereicReturnMethodDelegate)"/>
    private static void ExecuteMethod(GenereicVoidMethodDelegate p_gmdMethod)
    {
      try
      {
        p_gmdMethod();
      }
      catch (Exception e)
      {
        var strError = e.Message;
        if (e.InnerException != null)
        {
          strError += "\n" + e.InnerException.Message;
        }
        Warn(strError);
      }
    }

    /// <summary>
    /// Executes the given method with a return value.
    /// </summary>
    /// <remarks>
    /// This method is used to execute all method calls that return a value that
    /// the script needs to make. This allows for centralized error handling.
    /// 
    /// It should be noted that using delegates does engender a very slight performance hit,
    /// but given the nature of this application (more precisely, that this is a single-user
    /// application) there should not be any noticable difference.
    /// </remarks>
    /// <param name="p_gmdMethod">The method to execute.</param>
    /// <see cref="ExecuteMethod(GenereicVoidMethodDelegate)"/>
    private static object ExecuteMethod(GenereicReturnMethodDelegate p_gmdMethod)
    {
      try
      {
        return p_gmdMethod();
      }
      catch (Exception e)
      {
        var strError = e.Message;
        if (e.InnerException != null)
        {
          strError += "\n" + e.InnerException.Message;
        }
        Warn(strError);
      }
      return null;
    }

    #endregion

    private static string cLine = "0";

    private static string[] SplitLine(string s)
    {
      var temp = new List<string>();
      var WasLastSpace = false;
      var InQuotes = false;
      var WasLastEscape = false;
      var DoubleBreak = false;
      var InVar = false;
      var CurrentWord = "";
      var CurrentVar = "";

      if (s.Length == 0)
      {
        return new string[0];
      }
      s += " ";
      for (var i = 0; i < s.Length; i++)
      {
        switch (s[i])
        {
          case '%':
            WasLastSpace = false;
            if (InVar)
            {
              if (variables.ContainsKey(CurrentWord))
              {
                CurrentWord = CurrentVar + variables[CurrentWord];
              }
              else
              {
                CurrentWord = CurrentVar + "%" + CurrentWord + "%";
              }
              CurrentVar = "";
              InVar = false;
            }
            else
            {
              if (InQuotes && WasLastEscape)
              {
                CurrentWord += "%";
              }
              else
              {
                InVar = true;
                CurrentVar = CurrentWord;
                CurrentWord = "";
              }
            }
            WasLastEscape = false;
            break;
          case ',':
          case ' ':
            WasLastEscape = false;
            if (InVar)
            {
              CurrentWord = CurrentVar + "%" + CurrentWord;
              CurrentVar = "";
              InVar = false;
            }

            if (InQuotes)
            {
              CurrentWord += s[i];
            }
            else if (!WasLastSpace)
            {
              temp.Add(CurrentWord);
              CurrentWord = "";
              WasLastSpace = true;
            }
            break;
          case ';':
            WasLastEscape = false;
            if (!InQuotes)
            {
              DoubleBreak = true;
            }
            else
            {
              CurrentWord += s[i];
            }
            break;
          case '"':
            if (InQuotes && WasLastEscape)
            {
              CurrentWord += s[i];
            }
            else
            {
              if (InVar)
              {
                Warn("String marker found in the middle of a variable name");
              }
              InQuotes = !InQuotes;
            }
            WasLastSpace = false;
            WasLastEscape = false;
            break;
          case '\\':
            if (InQuotes && WasLastEscape)
            {
              CurrentWord += s[i];
              WasLastEscape = false;
            }
            else if (InQuotes)
            {
              WasLastEscape = true;
            }
            else
            {
              CurrentWord += s[i];
            }
            WasLastSpace = false;
            break;
          default:
            WasLastEscape = false;
            WasLastSpace = false;
            CurrentWord += s[i];
            break;
        }
        if (DoubleBreak)
        {
          break;
        }
      }
      if (InVar)
      {
        Warn("Unterminated variable");
      }
      if (InQuotes)
      {
        Warn("Unterminated quote");
      }
      return temp.ToArray();
    }

    private static void Warn(string Message)
    {
      //if(Settings.ShowScriptWarnings)
      MessageBox.Show(Message + " on line " + cLine, "Error in script");
    }

    private static bool FunctionIf(string[] line)
    {
      if (line.Length == 1)
      {
        Warn("Missing arguments to function 'If'");
        return false;
      }
      switch (line[1])
      {
        case "DialogYesNo":
          switch (line.Length)
          {
            case 2:
              Warn("Missing arguments to function 'If DialogYesNo'");
              return false;
            case 3:
              return MessageBox.Show(line[2], "", mbButtons.YesNo) == DialogResult.Yes;
            case 4:
              return MessageBox.Show(line[2], line[3], mbButtons.YesNo) == DialogResult.Yes;
            default:
              Warn("Unexpected arguments after function 'If DialogYesNo'");
              goto case 4;
          }
        case "DataFileExists":
          if (line.Length == 2)
          {
            Warn("Missing arguments to function 'If DataFileExists'");
            return false;
          }
          return
            ((string[])
              ExecuteMethod(
                () =>
                  FileManagement.GetExistingDataFileList(Path.GetDirectoryName(line[2]), Path.GetFileName(line[2]),
                                                         false))).Length == 1;
        case "FommNewerThan":
          if (line.Length == 2)
          {
            Warn("Missing arguments to function 'If FommNEwerThan'");
            return false;
          }
          try
          {
            return ((Version) ExecuteMethod(() => m_midInstaller.Script.GetGameVersion())) >=
                   new Version(line[2] + ".0");
          }
          catch
          {
            Warn("Invalid argument to function 'If VersionGreaterThan'");
            return false;
          }
        case "ScriptExtenderPresent":
          if (line.Length > 2)
          {
            Warn("Unexpected arguments to 'If ScriptExtenderPresent'");
          }
          return (bool) (ExecuteMethod(() => Script.ScriptExtenderPresent()) ?? false);
        case "ScriptExtenderNewerThan":
          if (line.Length == 2)
          {
            Warn("Missing arguments to function 'If ScriptExtenderNewerThan'");
            return false;
          }
          if (line.Length > 3)
          {
            Warn("Unexpected arguments to 'If ScriptExtenderNewerThan'");
          }
          if (!(bool) (ExecuteMethod(() => Script.ScriptExtenderPresent()) ?? false))
          {
            return false;
          }
          try
          {
            return ((Version) ExecuteMethod(() => Script.GetScriptExtenderVersion())) >= new Version(line[2] + ".0");
          }
          catch
          {
            Warn("Invalid argument to function 'If ScriptExtenderNewerThan'");
            return false;
          }
        case "FalloutNewerThan":
          if (line.Length == 2)
          {
            Warn("Missing arguments to function 'If FalloutNewerThan'");
            return false;
          }
          if (line.Length > 3)
          {
            Warn("Unexpected arguments to 'If FalloutNewerThan'");
          }
          try
          {
            return ((Version) ExecuteMethod(() => Script.GetGameVersion())) >= new Version(line[2] + ".0");
          }
          catch
          {
            Warn("Invalid argument to function 'If FalloutNewerThan'");
            return false;
          }
        case "Equal":
          if (line.Length < 4)
          {
            Warn("Missing arguments to function 'If Equal'");
            return false;
          }
          if (line.Length > 4)
          {
            Warn("Unexpected arguments to 'If Equal'");
          }
          return line[2] == line[3];
        case "GreaterEqual":
        case "GreaterThan":
        {
          if (line.Length < 4)
          {
            Warn("Missing arguments to function 'If Greater'");
            return false;
          }
          if (line.Length > 4)
          {
            Warn("Unexpected arguments to 'If Greater'");
          }
          int arg1, arg2;
          if (!int.TryParse(line[2], out arg1) || !int.TryParse(line[3], out arg2))
          {
            Warn("Invalid argument upplied to function 'If Greater'");
            return false;
          }
          if (line[1] == "GreaterEqual")
          {
            return arg1 >= arg2;
          }
          return arg1 > arg2;
        }
        case "fGreaterEqual":
        case "fGreaterThan":
        {
          if (line.Length < 4)
          {
            Warn("Missing arguments to function 'If fGreater'");
            return false;
          }
          if (line.Length > 4)
          {
            Warn("Unexpected arguments to 'If fGreater'");
          }
          double arg1, arg2;
          if (!double.TryParse(line[2], out arg1) || !double.TryParse(line[3], out arg2))
          {
            Warn("Invalid argument upplied to function 'If fGreater'");
            return false;
          }
          if (line[1] == "fGreaterEqual")
          {
            return arg1 >= arg2;
          }
          return arg1 > arg2;
        }
        default:
          Warn("Unknown argument '" + line[1] + "' supplied to 'If'");
          return false;
      }
    }

    private static string[] FunctionSelect(string[] line, bool many, bool Previews, bool Descriptions)
    {
      if (line.Length < 3)
      {
        Warn("Missing arguments to function 'Select'");
        return new string[0];
      }
      //variables
      var argsperoption = 1 + (Previews ? 1 : 0) + (Descriptions ? 1 : 0);
      //Remove first 2 arguments
      var title = line[1];
      string[] items = new string[line.Length - 2];
      Array.Copy(line, 2, items, 0, line.Length - 2);
      line = items;
      //Check for incorrect number of arguments
      if (line.Length%argsperoption != 0)
      {
        Warn("Unexpected extra arguments to 'Select'");
        Array.Resize(ref line, line.Length - line.Length%argsperoption);
      }
      //Create arrays to pass to the select form
      items = new string[line.Length/argsperoption];
      string[] previews = Previews ? new string[line.Length/argsperoption] : null;
      string[] descs = Descriptions ? new string[line.Length/argsperoption] : null;
      for (var i = 0; i < line.Length/argsperoption; i++)
      {
        items[i] = line[i*argsperoption];
        if (Previews)
        {
          previews[i] = line[i*argsperoption + 1];
          if (Descriptions)
          {
            descs[i] = line[i*argsperoption + 2];
          }
        }
        else
        {
          if (Descriptions)
          {
            descs[i] = line[i*argsperoption + 1];
          }
        }
      }
      //Check for previews
      if (previews != null)
      {
        for (var i = 0; i < previews.Length; i++)
        {
          if (previews[i] == "None")
          {
            previews[i] = null;
          }
        }
      }
      //Display select form
      var indices = (int[]) ExecuteMethod(() => m_midInstaller.Script.Select(items, previews, descs, title, many));
      var result = new string[indices.Length];
      for (var i = 0; i < indices.Length; i++)
      {
        result[i] = "Case " + items[indices[i]];
      }
      return result;
    }

    private static string[] FunctionSelectVar(string[] line, bool IsVariable)
    {
      string Func = IsVariable ? " to function 'SelectVar'" : "to function 'SelectString'";
      if (line.Length < 2)
      {
        Warn("Missing arguments" + Func);
        return new string[0];
      }
      if (line.Length > 2)
      {
        Warn("Unexpected arguments" + Func);
      }
      if (IsVariable)
      {
        if (!variables.ContainsKey(line[1]))
        {
          Warn("Invalid argument" + Func + "\nVariable '" + line[1] + "' does not exist");
          return new string[0];
        }
        return new[]
        {
          "Case " + variables[line[1]]
        };
      }
      return new[]
      {
        "Case " + line[1]
      };
    }

    private static FlowControlStruct FunctionFor(string[] line, int LineNo)
    {
      var NullLoop = new FlowControlStruct(2);
      if (line.Length < 3)
      {
        Warn("Missing arguments to function 'For'");
        return NullLoop;
      }
      if (line[1] == "Each")
      {
        line[1] = line[2];
      }
      switch (line[1])
      {
        case "Count":
        {
          if (line.Length < 5)
          {
            Warn("Missing arguments to function 'For Count'");
            return NullLoop;
          }
          if (line.Length > 6)
          {
            Warn("Unexpected extra arguments to 'For Count'");
          }
          int start, end, step = 1;
          if (!int.TryParse(line[3], out start) || !int.TryParse(line[4], out end) ||
              (line.Length >= 6 && !int.TryParse(line[5], out step)))
          {
            Warn("Invalid argument to 'For Count'");
            return NullLoop;
          }
          var steps = new List<string>();
          for (var i = start; i <= end; i += step)
          {
            steps.Add(i.ToString());
          }
          return new FlowControlStruct(steps.ToArray(), line[2], LineNo);
        }
        case "DataFile":
        {
          if (line.Length < 4)
          {
            Warn("Missing arguments to function 'For Each DataFile'");
            return NullLoop;
          }
          if (line.Length > 4)
          {
            Warn("Unexpected extra arguments to 'For Each DataFile'");
          }

          var strFiles = (string[]) ExecuteMethod(() => m_midInstaller.Fomod.GetFileList().ToArray());
          for (var i = strFiles.Length - 1; i >= 0; i--)
          {
            strFiles[i] = strFiles[i].Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
          }
          return new FlowControlStruct(strFiles, line[3], LineNo);
        }
      }
      return NullLoop;
    }

    private static void FunctionPerformBasicInstall(string[] line)
    {
      if (line.Length > 1)
      {
        Warn("Unexpected extra arguments to function PerformBasicInstall");
      }
      Fallout3BaseScript.PerformBasicInstall();
    }

    private static void FunctionMessage(string[] line)
    {
      switch (line.Length)
      {
        case 1:
          Warn("Missing arguments to function 'Message'");
          break;
        case 2:
          ExecuteMethod(() => Script.MessageBox(line[1]));
          break;
        case 3:
          ExecuteMethod(() => Script.MessageBox(line[1], line[2]));
          break;
        default:
          Warn("Unexpected arguments after 'Message'");
          goto case 3;
      }
    }

    private static void FunctionInstallDataFile(string[] line)
    {
      if (line.Length == 1)
      {
        Warn("Missing arguments to InstallDataFile");
        return;
      }
      if (line.Length > 2)
      {
        Warn("Unexpected arguments after InstallDataFile");
      }
      ExecuteMethod(() => Script.InstallFileFromFomod(line[1]));
    }

    private static void FunctionCopyDataFile(string[] line)
    {
      if (line.Length < 3)
      {
        Warn("Missing arguments to CopyDataFile");
      }
      else
      {
        if (line.Length > 3)
        {
          Warn("Unexpected extra arguments after CopyDataFile");
        }

        ExecuteMethod(() => Script.CopyDataFile(line[1], line[2]));
      }
    }

    private static void FunctionSetPluginActivation(string[] line)
    {
      if (line.Length < 3)
      {
        Warn("Missing arguments to function SetPluginActivation");
        return;
      }
      if (line.Length > 3)
      {
        Warn("Unexpected extra arguments to function SetPluginActivation");
      }
      if (line[2] == "True")
      {
        ExecuteMethod(() => Script.SetPluginActivation(line[1], true));
      }
      else if (line[2] == "False")
      {
        ExecuteMethod(() => Script.SetPluginActivation(line[1], false));
      }
      else
      {
        Warn("Expected 'True' or 'False', but got '" + line[2] + "'");
      }
    }

    private enum IniType
    {
      Fallout,
      FalloutPrefs,
      Geck,
      GeckPrefs
    }

    private static void FunctionEditINI(string[] line, IniType type)
    {
      if (line.Length < 4)
      {
        Warn("Missing arguments to EditINI");
        return;
      }
      if (line.Length > 4)
      {
        Warn("Unexpected arguments to EditINI");
      }
      switch (type)
      {
        case IniType.Fallout:
          ExecuteMethod(() => Script.EditFalloutINI(line[1], line[2], line[3], true));
          break;
        case IniType.FalloutPrefs:
          ExecuteMethod(() => Script.EditPrefsINI(line[1], line[2], line[3], true));
          break;
        case IniType.Geck:
          ExecuteMethod(() => Script.EditGeckINI(line[1], line[2], line[3], true));
          break;
        case IniType.GeckPrefs:
          ExecuteMethod(() => Script.EditGeckPrefsINI(line[1], line[2], line[3], true));
          break;
      }
    }

    private static void FunctionEditShader(string[] line)
    {
      if (line.Length < 4)
      {
        Warn("Missing arguments to 'EditShader'");
        return;
      }
      if (line.Length > 4)
      {
        Warn("Unexpected arguments to 'EditShader'");
      }
      int package;
      if (!int.TryParse(line[1], out package))
      {
        Warn("Invalid argument to function 'EditShader'\n'" + line[1] + "' is not a valid shader package ID");
        return;
      }
      var bteData = (byte[]) ExecuteMethod(() => m_midInstaller.Fomod.GetFile(line[3]));
      ExecuteMethod(() => Script.EditShader(package, line[2], bteData));
    }

    private static void FunctionSetVar(string[] line)
    {
      if (line.Length < 3)
      {
        Warn("Missing arguments to function SetVar");
        return;
      }
      if (line.Length > 3)
      {
        Warn("Unexpected extra arguments to function SetVar");
      }
      variables[line[1]] = line[2];
    }

    private static void FunctionGetDirectoryName(string[] line)
    {
      if (line.Length < 3)
      {
        Warn("Missing arguments to GetDirectoryName");
        return;
      }
      if (line.Length > 3)
      {
        Warn("Unexpected arguments to GetDirectoryName");
      }
      try
      {
        variables[line[1]] = Path.GetDirectoryName(line[2]);
      }
      catch
      {
        Warn("Invalid argument to GetDirectoryName");
      }
    }

    private static void FunctionGetFileName(string[] line)
    {
      if (line.Length < 3)
      {
        Warn("Missing arguments to GetFileName");
        return;
      }
      if (line.Length > 3)
      {
        Warn("Unexpected arguments to GetFileName");
      }
      try
      {
        variables[line[1]] = Path.GetFileName(line[2]);
      }
      catch
      {
        Warn("Invalid argument to GetFileName");
      }
    }

    private static void FunctionGetFileNameWithoutExtension(string[] line)
    {
      if (line.Length < 3)
      {
        Warn("Missing arguments to GetFileNameWithoutExtension");
        return;
      }
      if (line.Length > 3)
      {
        Warn("Unexpected arguments to GetFileNameWithoutExtension");
      }
      try
      {
        variables[line[1]] = Path.GetFileNameWithoutExtension(line[2]);
      }
      catch
      {
        Warn("Invalid argument to GetFileNameWithoutExtension");
      }
    }

    private static void FunctionCombinePaths(string[] line)
    {
      if (line.Length < 4)
      {
        Warn("Missing arguments to CombinePaths");
        return;
      }
      if (line.Length > 4)
      {
        Warn("Unexpected arguments to CombinePaths");
      }
      try
      {
        variables[line[1]] = Path.Combine(line[2], line[3]);
      }
      catch
      {
        Warn("Invalid argument to CombinePaths");
      }
    }

    private static void FunctionSubstring(string[] line)
    {
      if (line.Length < 4)
      {
        Warn("Missing arguments to Substring");
        return;
      }
      if (line.Length > 5)
      {
        Warn("Unexpected extra arguments to Substring");
      }
      if (line.Length == 4)
      {
        int start;
        if (!int.TryParse(line[3], out start))
        {
          Warn("Invalid argument to Substring");
          return;
        }
        variables[line[1]] = line[2].Substring(start);
      }
      else
      {
        int start, end;
        if (!int.TryParse(line[3], out start) || !int.TryParse(line[4], out end))
        {
          Warn("Invalid argument to Substring");
          return;
        }
        variables[line[1]] = line[2].Substring(start, end);
      }
    }

    private static void FunctionRemoveString(string[] line)
    {
      if (line.Length < 4)
      {
        Warn("Missing arguments to RemoveString");
        return;
      }
      if (line.Length > 5)
      {
        Warn("Unexpected extra arguments to RemoveString");
      }
      if (line.Length == 4)
      {
        int start;
        if (!int.TryParse(line[3], out start))
        {
          Warn("Invalid argument to RemoveString");
          return;
        }
        variables[line[1]] = line[2].Remove(start);
      }
      else
      {
        int start, end;
        if (!int.TryParse(line[3], out start) || !int.TryParse(line[4], out end))
        {
          Warn("Invalid argument to RemoveString");
          return;
        }
        variables[line[1]] = line[2].Remove(start, end);
      }
    }

    private static void FunctionStringLength(string[] line)
    {
      if (line.Length < 3)
      {
        Warn("Missing arguments to StringLength");
        return;
      }
      if (line.Length > 3)
      {
        Warn("Unexpected extra arguments to StringLength");
      }
      variables[line[1]] = line[2].Length.ToString();
    }

    private static void FunctionReadINI(string[] line, IniType type)
    {
      if (line.Length < 4)
      {
        Warn("Missing arguments to function ReadINI");
        return;
      }
      if (line.Length > 4)
      {
        Warn("Unexpected extra arguments to function ReadINI");
      }
      switch (type)
      {
        case IniType.Fallout:
          variables[line[1]] = (string) ExecuteMethod(() => Script.GetFalloutIniString(line[2], line[3]));
          break;
        case IniType.FalloutPrefs:
          variables[line[1]] = (string) ExecuteMethod(() => Script.GetPrefsIniString(line[2], line[3]));
          break;
        case IniType.Geck:
          variables[line[1]] = (string) ExecuteMethod(() => Script.GetGeckIniString(line[2], line[3]));
          break;
        case IniType.GeckPrefs:
          variables[line[1]] = (string) ExecuteMethod(() => Script.GetGeckPrefsIniString(line[2], line[3]));
          break;
      }
    }

    private static void FunctionReadRenderer(string[] line)
    {
      if (line.Length < 3)
      {
        Warn("Missing arguments to function 'ReadRendererInfo'");
        return;
      }
      if (line.Length > 3)
      {
        Warn("Unexpected extra arguments to function 'ReadRendererInfo'");
      }
      variables[line[1]] = (string) ExecuteMethod(() => Script.GetRendererInfo(line[2]));
    }

    private static void FunctionExecLines(string[] line, Queue<string> queue)
    {
      if (line.Length < 2)
      {
        Warn("Missing arguments to function 'ExecLines'");
        return;
      }
      if (line.Length > 2)
      {
        Warn("Unexpected extra arguments to function 'ExecLines'");
      }
      var lines = line[1].Split(new[]
      {
        Environment.NewLine
      }, StringSplitOptions.RemoveEmptyEntries);
      foreach (var s in lines)
      {
        queue.Enqueue(s);
      }
    }

    private class SetException : Exception
    {
      public SetException(string msg) : base(msg)
      {
      }
    }

    private static int iSet(List<string> func)
    {
      if (func.Count == 0)
      {
        throw new SetException("Empty iSet");
      }
      if (func.Count == 1)
      {
        return int.Parse(func[0]);
      }
      //check for brackets

      int index = func.IndexOf("(");
      while (index != -1)
      {
        var count = 1;
        var newfunc = new List<string>();
        for (var i = index + 1; i < func.Count; i++)
        {
          if (func[i] == "(")
          {
            count++;
          }
          else if (func[i] == ")")
          {
            count--;
          }
          if (count == 0)
          {
            func.RemoveRange(index, (i - index) + 1);
            func.Insert(index, iSet(newfunc).ToString());
            break;
          }
          newfunc.Add(func[i]);
        }
        if (count != 0)
        {
          throw new SetException("Mismatched brackets");
        }
        index = func.IndexOf("(");
      }

      //not
      index = func.IndexOf("not");
      while (index != -1)
      {
        var i = int.Parse(func[index + 1]);
        i = ~i;
        func[index + 1] = i.ToString();
        func.RemoveAt(index);
        index = func.IndexOf("not");
      }

      //and
      index = func.IndexOf("not");
      while (index != -1)
      {
        var i = int.Parse(func[index - 1]) & int.Parse(func[index + 1]);
        func[index + 1] = i.ToString();
        func.RemoveRange(index - 1, 2);
        index = func.IndexOf("not");
      }

      //or
      index = func.IndexOf("or");
      while (index != -1)
      {
        var i = int.Parse(func[index - 1]) | int.Parse(func[index + 1]);
        func[index + 1] = i.ToString();
        func.RemoveRange(index - 1, 2);
        index = func.IndexOf("or");
      }

      //xor
      index = func.IndexOf("xor");
      while (index != -1)
      {
        var i = int.Parse(func[index - 1]) ^ int.Parse(func[index + 1]);
        func[index + 1] = i.ToString();
        func.RemoveRange(index - 1, 2);
        index = func.IndexOf("xor");
      }

      //mod
      index = func.IndexOf("mod");
      while (index != -1)
      {
        var i = int.Parse(func[index - 1])%int.Parse(func[index + 1]);
        func[index + 1] = i.ToString();
        func.RemoveRange(index - 1, 2);
        index = func.IndexOf("mod");
      }

      //mod
      index = func.IndexOf("%");
      while (index != -1)
      {
        var i = int.Parse(func[index - 1])%int.Parse(func[index + 1]);
        func[index + 1] = i.ToString();
        func.RemoveRange(index - 1, 2);
        index = func.IndexOf("%");
      }

      //power
      index = func.IndexOf("^");
      while (index != -1)
      {
        var i = (int) Math.Pow(int.Parse(func[index - 1]), int.Parse(func[index + 1]));
        func[index + 1] = i.ToString();
        func.RemoveRange(index - 1, 2);
        index = func.IndexOf("^");
      }

      //division
      index = func.IndexOf("/");
      while (index != -1)
      {
        var i = int.Parse(func[index - 1])/int.Parse(func[index + 1]);
        func[index + 1] = i.ToString();
        func.RemoveRange(index - 1, 2);
        index = func.IndexOf("/");
      }

      //multiplication
      index = func.IndexOf("*");
      while (index != -1)
      {
        var i = int.Parse(func[index - 1])*int.Parse(func[index + 1]);
        func[index + 1] = i.ToString();
        func.RemoveRange(index - 1, 2);
        index = func.IndexOf("*");
      }

      //add
      index = func.IndexOf("+");
      while (index != -1)
      {
        var i = int.Parse(func[index - 1]) + int.Parse(func[index + 1]);
        func[index + 1] = i.ToString();
        func.RemoveRange(index - 1, 2);
        index = func.IndexOf("+");
      }

      //sub
      index = func.IndexOf("-");
      while (index != -1)
      {
        var i = int.Parse(func[index - 1]) - int.Parse(func[index + 1]);
        func[index + 1] = i.ToString();
        func.RemoveRange(index - 1, 2);
        index = func.IndexOf("-");
      }

      if (func.Count != 1)
      {
        throw new SetException("leftovers in function");
      }
      return int.Parse(func[0]);
    }

    private static double fSet(List<string> func)
    {
      if (func.Count == 0)
      {
        throw new SetException("Empty fSet");
      }
      if (func.Count == 1)
      {
        return int.Parse(func[0]);
      }
      //check for brackets

      int index = func.IndexOf("(");
      while (index != -1)
      {
        var count = 1;
        var newfunc = new List<string>();
        for (var i = index; i < func.Count; i++)
        {
          if (func[i] == "(")
          {
            count++;
          }
          else if (func[i] == ")")
          {
            count--;
          }
          if (count == 0)
          {
            func.RemoveRange(index, i - index);
            func.Insert(index, fSet(newfunc).ToString());
            break;
          }
          newfunc.Add(func[i]);
        }
        if (count != 0)
        {
          throw new SetException("Mismatched brackets");
        }
        index = func.IndexOf("(");
      }

      //sin
      index = func.IndexOf("sin");
      while (index != -1)
      {
        func[index + 1] = Math.Sin(double.Parse(func[index + 1])).ToString();
        func.RemoveAt(index);
        index = func.IndexOf("sin");
      }

      //cos
      index = func.IndexOf("cos");
      while (index != -1)
      {
        func[index + 1] = Math.Cos(double.Parse(func[index + 1])).ToString();
        func.RemoveAt(index);
        index = func.IndexOf("cos");
      }

      //tan
      index = func.IndexOf("tan");
      while (index != -1)
      {
        func[index + 1] = Math.Tan(double.Parse(func[index + 1])).ToString();
        func.RemoveAt(index);
        index = func.IndexOf("tan");
      }

      //sinh
      index = func.IndexOf("sinh");
      while (index != -1)
      {
        func[index + 1] = Math.Sinh(double.Parse(func[index + 1])).ToString();
        func.RemoveAt(index);
        index = func.IndexOf("sinh");
      }

      //cosh
      index = func.IndexOf("cosh");
      while (index != -1)
      {
        func[index + 1] = Math.Cosh(double.Parse(func[index + 1])).ToString();
        func.RemoveAt(index);
        index = func.IndexOf("cosh");
      }

      //tanh
      index = func.IndexOf("tanh");
      while (index != -1)
      {
        func[index + 1] = Math.Tanh(double.Parse(func[index + 1])).ToString();
        func.RemoveAt(index);
        index = func.IndexOf("tanh");
      }

      //exp
      index = func.IndexOf("exp");
      while (index != -1)
      {
        func[index + 1] = Math.Exp(double.Parse(func[index + 1])).ToString();
        func.RemoveAt(index);
        index = func.IndexOf("exp");
      }

      //log
      index = func.IndexOf("log");
      while (index != -1)
      {
        func[index + 1] = Math.Log10(double.Parse(func[index + 1])).ToString();
        func.RemoveAt(index);
        index = func.IndexOf("log");
      }

      //ln
      index = func.IndexOf("ln");
      while (index != -1)
      {
        func[index + 1] = Math.Log(double.Parse(func[index + 1])).ToString();
        func.RemoveAt(index);
        index = func.IndexOf("ln");
      }

      //mod
      index = func.IndexOf("mod");
      while (index != -1)
      {
        var i = double.Parse(func[index - 1])%double.Parse(func[index + 1]);
        func[index + 1] = i.ToString();
        func.RemoveRange(index - 1, 2);
        index = func.IndexOf("mod");
      }

      //mod2
      index = func.IndexOf("%");
      while (index != -1)
      {
        var i = double.Parse(func[index - 1])%double.Parse(func[index + 1]);
        func[index + 1] = i.ToString();
        func.RemoveRange(index - 1, 2);
        index = func.IndexOf("%");
      }

      //power
      index = func.IndexOf("^");
      while (index != -1)
      {
        var i = Math.Pow(double.Parse(func[index - 1]), double.Parse(func[index + 1]));
        func[index + 1] = i.ToString();
        func.RemoveRange(index - 1, 2);
        index = func.IndexOf("^");
      }

      //division
      index = func.IndexOf("/");
      while (index != -1)
      {
        var i = double.Parse(func[index - 1])/double.Parse(func[index + 1]);
        func[index + 1] = i.ToString();
        func.RemoveRange(index - 1, 2);
        index = func.IndexOf("/");
      }

      //multiplication
      index = func.IndexOf("*");
      while (index != -1)
      {
        var i = double.Parse(func[index - 1])*double.Parse(func[index + 1]);
        func[index + 1] = i.ToString();
        func.RemoveRange(index - 1, 2);
        index = func.IndexOf("*");
      }

      //add
      index = func.IndexOf("+");
      while (index != -1)
      {
        var i = double.Parse(func[index - 1]) + double.Parse(func[index + 1]);
        func[index + 1] = i.ToString();
        func.RemoveRange(index - 1, 2);
        index = func.IndexOf("+");
      }

      //sub
      index = func.IndexOf("-");
      while (index != -1)
      {
        var i = double.Parse(func[index - 1]) - double.Parse(func[index + 1]);
        func[index + 1] = i.ToString();
        func.RemoveRange(index - 1, 2);
        index = func.IndexOf("-");
      }

      if (func.Count != 1)
      {
        throw new SetException("leftovers in function");
      }
      return double.Parse(func[0]);
    }

    private static void FunctionSet(string[] line, bool integer)
    {
      if (line.Length < 3)
      {
        Warn("Missing arguments to function " + (integer ? "iSet" : "fSet"));
        return;
      }
      var func = new List<string>();
      for (var i = 2; i < line.Length; i++)
      {
        func.Add(line[i]);
      }
      try
      {
        string result;
        if (integer)
        {
          var i = iSet(func);
          result = i.ToString();
        }
        else
        {
          var f = (float) fSet(func);
          result = f.ToString();
        }
        variables[line[1]] = result;
      }
      catch
      {
        Warn("Invalid argument to function " + (integer ? "iSet" : "fSet"));
      }
    }

    public static bool Execute(string InputScript, ModInstaller p_midInstaller)
    {
      m_midInstaller = p_midInstaller;

      variables = new Dictionary<string, string>();
      var FlowControl = new Stack<FlowControlStruct>();
      var ExtraLines = new Queue<string>();
      variables["NewLine"] = Environment.NewLine;
      variables["Tab"] = "\t";
      var script = InputScript.Replace("\r", "").Split('\n');
      string SkipTo = null;
      var Break = false;
      var Fatal = false;
      for (var i = 1; i < script.Length || ExtraLines.Count > 0; i++)
      {
        string s;
        if (ExtraLines.Count > 0)
        {
          i--;
          s = ExtraLines.Dequeue().Replace('\t', ' ').Trim();
        }
        else
        {
          s = script[i].Replace('\t', ' ').Trim();
        }
        cLine = i.ToString();
        while (s.EndsWith("\\"))
        {
          s = s.Remove(s.Length - 1);
          if (ExtraLines.Count > 0)
          {
            s += ExtraLines.Dequeue().Replace('\t', ' ').Trim();
          }
          else
          {
            if (++i == script.Length)
            {
              Warn("Run-on line passed end of script");
            }
            else
            {
              s += script[i].Replace('\t', ' ').Trim();
            }
          }
        }

        if (SkipTo != null)
        {
          if (s == SkipTo)
          {
            SkipTo = null;
          }
          else
          {
            continue;
          }
        }

        var line = SplitLine(s);
        if (line.Length == 0)
        {
          continue;
        }

        if (FlowControl.Count != 0 && !FlowControl.Peek().active)
        {
          switch (line[0])
          {
            case "":
              Warn("Empty function");
              break;
            case "If":
            case "IfNot":
              FlowControl.Push(new FlowControlStruct(0));
              break;
            case "Else":
              if (FlowControl.Count != 0 && FlowControl.Peek().type == 0)
              {
                FlowControl.Peek().active = FlowControl.Peek().line != -1;
              }
              else
              {
                Warn("Unexpected Else");
              }
              break;
            case "EndIf":
              if (FlowControl.Count != 0 && FlowControl.Peek().type == 0)
              {
                FlowControl.Pop();
              }
              else
              {
                Warn("Unexpected EndIf");
              }
              break;
            case "Select":
            case "SelectMany":
            case "SelectWithPreview":
            case "SelectManyWithPreview":
            case "SelectWithDescriptions":
            case "SelectManyWithDescriptions":
            case "SelectWithDescriptionsAndPreviews":
            case "SelectManyWithDescriptionsAndPreviews":
            case "SelectVar":
            case "SelectString":
              FlowControl.Push(new FlowControlStruct(1));
              break;
            case "Case":
              if (FlowControl.Count != 0 && FlowControl.Peek().type == 1)
              {
                if (FlowControl.Peek().line != -1 && Array.IndexOf(FlowControl.Peek().values, s) != -1)
                {
                  FlowControl.Peek().active = true;
                  FlowControl.Peek().hitCase = true;
                }
              }
              else
              {
                Warn("Unexpected Break");
              }
              break;
            case "Default":
              if (FlowControl.Count != 0 && FlowControl.Peek().type == 1)
              {
                if (FlowControl.Peek().line != -1 && !FlowControl.Peek().hitCase)
                {
                  FlowControl.Peek().active = true;
                }
              }
              else
              {
                Warn("Unexpected Default");
              }
              break;
            case "EndSelect":
              if (FlowControl.Count != 0 && FlowControl.Peek().type == 1)
              {
                FlowControl.Pop();
              }
              else
              {
                Warn("Unexpected EndSelect");
              }
              break;
            case "For":
              FlowControl.Push(new FlowControlStruct(2));
              break;
            case "EndFor":
              if (FlowControl.Count != 0 && FlowControl.Peek().type == 2)
              {
                FlowControl.Pop();
              }
              else
              {
                Warn("Unexpected EndFor");
              }
              break;
            case "Break":
            case "Continue":
            case "Exit":
              break;
          }
        }
        else
        {
          switch (line[0])
          {
            case "":
              Warn("Empty function");
              break;
              //Control structures
            case "Goto":
              if (line.Length < 2)
              {
                Warn("Not enough arguments to function 'Goto'");
              }
              else
              {
                if (line.Length > 2)
                {
                  Warn("Unexpected extra arguments to function 'Goto'");
                }
                SkipTo = "Label " + line[1];
                FlowControl.Clear();
              }
              break;
            case "Label":
              break;
            case "If":
              FlowControl.Push(new FlowControlStruct(i, FunctionIf(line)));
              break;
            case "IfNot":
              FlowControl.Push(new FlowControlStruct(i, !FunctionIf(line)));
              break;
            case "Else":
              if (FlowControl.Count != 0 && FlowControl.Peek().type == 0)
              {
                FlowControl.Peek().active = false;
              }
              else
              {
                Warn("Unexpected Else");
              }
              break;
            case "EndIf":
              if (FlowControl.Count != 0 && FlowControl.Peek().type == 0)
              {
                FlowControl.Pop();
              }
              else
              {
                Warn("Unexpected EndIf");
              }
              break;
            case "Select":
              FlowControl.Push(new FlowControlStruct(i, FunctionSelect(line, false, false, false)));
              break;
            case "SelectMany":
              FlowControl.Push(new FlowControlStruct(i, FunctionSelect(line, true, false, false)));
              break;
            case "SelectWithPreview":
              FlowControl.Push(new FlowControlStruct(i, FunctionSelect(line, false, true, false)));
              break;
            case "SelectManyWithPreview":
              FlowControl.Push(new FlowControlStruct(i, FunctionSelect(line, true, true, false)));
              break;
            case "SelectWithDescriptions":
              FlowControl.Push(new FlowControlStruct(i, FunctionSelect(line, false, false, true)));
              break;
            case "SelectManyWithDescriptions":
              FlowControl.Push(new FlowControlStruct(i, FunctionSelect(line, true, false, true)));
              break;
            case "SelectWithDescriptionsAndPreviews":
              FlowControl.Push(new FlowControlStruct(i, FunctionSelect(line, false, true, true)));
              break;
            case "SelectManyWithDescriptionsAndPreviews":
              FlowControl.Push(new FlowControlStruct(i, FunctionSelect(line, true, true, true)));
              break;
            case "SelectVar":
              FlowControl.Push(new FlowControlStruct(i, FunctionSelectVar(line, true)));
              break;
            case "SelectString":
              FlowControl.Push(new FlowControlStruct(i, FunctionSelectVar(line, false)));
              break;
            case "Break":
            {
              var found = false;
              var fcs = FlowControl.ToArray();
              for (var k = 0; k < fcs.Length; k++)
              {
                if (fcs[k].type == 1)
                {
                  for (var j = 0; j <= k; j++)
                  {
                    fcs[j].active = false;
                  }
                  found = true;
                  break;
                }
              }
              if (!found)
              {
                Warn("Unexpected Break");
              }
              break;
            }
            case "Case":
              if (FlowControl.Count == 0 || FlowControl.Peek().type != 1)
              {
                Warn("Unexpected Case");
              }
              break;
            case "Default":
              if (FlowControl.Count == 0 || FlowControl.Peek().type != 1)
              {
                Warn("Unexpected Default");
              }
              break;
            case "EndSelect":
              if (FlowControl.Count != 0 && FlowControl.Peek().type == 1)
              {
                FlowControl.Pop();
              }
              else
              {
                Warn("Unexpected EndSelect");
              }
              break;
            case "For":
            {
              var fc = FunctionFor(line, i);
              FlowControl.Push(fc);
              if (fc.line != -1 && fc.values.Length > 0)
              {
                variables[fc.var] = fc.values[0];
                fc.active = true;
              }
              break;
            }
            case "Continue":
            {
              var found = false;
              var fcs = FlowControl.ToArray();
              for (var k = 0; k < fcs.Length; k++)
              {
                if (fcs[k].type == 2)
                {
                  fcs[k].forCount++;
                  if (fcs[k].forCount == fcs[k].values.Length)
                  {
                    for (var j = 0; j <= k; j++)
                    {
                      fcs[j].active = false;
                    }
                  }
                  else
                  {
                    i = fcs[k].line;
                    variables[fcs[k].var] = fcs[k].values[fcs[k].forCount];
                    for (var j = 0; j < k; j++)
                    {
                      FlowControl.Pop();
                    }
                  }
                  found = true;
                  break;
                }
              }
              if (!found)
              {
                Warn("Unexpected Continue");
              }
              break;
            }
            case "Exit":
            {
              var found = false;
              var fcs = FlowControl.ToArray();
              for (var k = 0; k < fcs.Length; k++)
              {
                if (fcs[k].type == 2)
                {
                  for (var j = 0; j <= k; j++)
                  {
                    FlowControl.Peek().active = false;
                  }
                  found = true;
                  break;
                }
              }
              if (!found)
              {
                Warn("Unexpected Exit");
              }
              break;
            }
            case "EndFor":
              if (FlowControl.Count != 0 && FlowControl.Peek().type == 2)
              {
                var fc = FlowControl.Peek();
                fc.forCount++;
                if (fc.forCount == fc.values.Length)
                {
                  FlowControl.Pop();
                }
                else
                {
                  i = fc.line;
                  variables[fc.var] = fc.values[fc.forCount];
                }
              }
              else
              {
                Warn("Unexpected EndFor");
              }
              break;
              //Functions
            case "Message":
              FunctionMessage(line);
              break;
            case "PerformBasicInstall":
              FunctionPerformBasicInstall(line);
              break;
            case "InstallDataFile":
              FunctionInstallDataFile(line);
              break;
            case "CopyDataFile":
              FunctionCopyDataFile(line);
              break;
            case "FatalError":
              Break = true;
              Fatal = true;
              break;
            case "Return":
              Break = true;
              break;
            case "SetPluginActivation":
              FunctionSetPluginActivation(line);
              break;
            case "EditINI":
              FunctionEditINI(line, IniType.Fallout);
              break;
            case "EditPrefsINI":
              FunctionEditINI(line, IniType.FalloutPrefs);
              break;
            case "EditGeckINI":
              FunctionEditINI(line, IniType.Geck);
              break;
            case "EditGeckPrefsINI":
              FunctionEditINI(line, IniType.GeckPrefs);
              break;
            case "EditSDP":
            case "EditShader":
              FunctionEditShader(line);
              break;
            case "SetVar":
              FunctionSetVar(line);
              break;
            case "GetFolderName":
            case "GetDirectoryName":
              FunctionGetDirectoryName(line);
              break;
            case "GetFileName":
              FunctionGetFileName(line);
              break;
            case "GetFileNameWithoutExtension":
              FunctionGetFileNameWithoutExtension(line);
              break;
            case "CombinePaths":
              FunctionCombinePaths(line);
              break;
            case "Substring":
              FunctionSubstring(line);
              break;
            case "RemoveString":
              FunctionRemoveString(line);
              break;
            case "StringLength":
              FunctionStringLength(line);
              break;
            case "ReadINI": //TODO: Split into 4
              FunctionReadINI(line, IniType.Fallout);
              break;
            case "ReadPrefsINI":
              FunctionReadINI(line, IniType.FalloutPrefs);
              break;
            case "ReadGeckINI":
              FunctionReadINI(line, IniType.Geck);
              break;
            case "ReadGeckPrefsINI":
              FunctionReadINI(line, IniType.GeckPrefs);
              break;
            case "ReadRendererInfo":
              FunctionReadRenderer(line);
              break;
            case "ExecLines":
              FunctionExecLines(line, ExtraLines);
              break;
            case "iSet":
              FunctionSet(line, true);
              break;
            case "fSet":
              FunctionSet(line, false);
              break;
            default:
              Warn("Unknown function '" + line[0] + "'");
              break;
          }
        }
        if (Break)
        {
          break;
        }
      }
      if (SkipTo != null)
      {
        Warn("Expected " + SkipTo);
      }
      variables = null;
      return !Fatal;
    }
  }
}