using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Policy;
using System.Text;
using System.Windows.Forms;
using Microsoft.CSharp;
using sList = System.Collections.Generic.List<string>;

namespace Fomm.PackageManager
{
  internal static class ScriptCompiler
  {
    private static readonly CSharpCodeProvider csCompiler = new CSharpCodeProvider();
    private static readonly CompilerParameters cParams;
    private static readonly Evidence evidence;
    private static Assembly fommScriptRunner;
    private static object fommScriptObject;

    private static readonly string ScriptOutputPath = Path.Combine(Program.tmpPath, "dotnetscript");
    private static uint ScriptCount;

    static ScriptCompiler()
    {
      cParams = new CompilerParameters();
      cParams.GenerateExecutable = false;
      cParams.GenerateInMemory = false;
      cParams.IncludeDebugInformation = false;
      cParams.ReferencedAssemblies.Add(Path.Combine(Program.ExecutableDirectory, "fomm.Scripting.dll"));
      cParams.ReferencedAssemblies.Add("System.dll");
      cParams.ReferencedAssemblies.Add("System.Drawing.dll");
      cParams.ReferencedAssemblies.Add("System.Windows.Forms.dll");
      cParams.ReferencedAssemblies.Add("System.Xml.dll");

      evidence = new Evidence();
      evidence.AddHostEvidence(new Zone(SecurityZone.Internet));
    }

    private static void LoadFommScriptObject()
    {
      var data = Compile(@"
using System;
using fomm.Scripting;
using Fomm.PackageManager;

class ScriptRunner {
    public static bool RunScript(string script, ModInstaller p_midInstaller) {
        return fommScript.Execute(script, p_midInstaller);
    }
}");
      fommScriptRunner = AppDomain.CurrentDomain.Load(data);
      fommScriptObject = fommScriptRunner.CreateInstance("ScriptRunner");
    }

    private static byte[] Compile(string code)
    {
      string[] errors, warnings;
      string stdout;
      return Compile(code, out errors, out warnings, out stdout);
    }

    private static byte[] Compile(string code, out string[] errors, out string[] warnings, out string stdout)
    {
      cParams.OutputAssembly = ScriptOutputPath + (ScriptCount++) + ".dll";
      cParams.TempFiles = new TempFileCollection(Environment.GetEnvironmentVariable("TEMP"), true);
      cParams.IncludeDebugInformation = false;

      //Compatibility fix for mono, which needs a different assembly name each call
      var results = csCompiler.CompileAssemblyFromSource(cParams, code);
      stdout = "";
      for (var i = 0; i < results.Output.Count; i++)
      {
        stdout += results.Output[i] + Environment.NewLine;
      }
      if (results.Errors.HasErrors)
      {
        var msgs = new sList();
        foreach (CompilerError ce in results.Errors)
        {
          if (!ce.IsWarning)
          {
            msgs.Add("Error on Line " + ce.Line + ": " + ce.ErrorText);
          }
        }
        errors = msgs.ToArray();
      }
      else
      {
        errors = null;
      }
      if (results.Errors.HasWarnings)
      {
        var msgs = new sList();
        foreach (CompilerError ce in results.Errors)
        {
          if (ce.IsWarning)
          {
            msgs.Add("Warning on Line " + ce.Line + ": " + ce.ErrorText);
          }
        }
        warnings = msgs.ToArray();
      }
      else
      {
        warnings = null;
      }
      if (results.Errors.HasErrors)
      {
        return null;
      }

      var data = File.ReadAllBytes(results.PathToAssembly);
      
      File.Delete(results.PathToAssembly);
      return data;
    }

    public static string CheckSyntax(string script, out string stdout)
    {
      stdout = null;

      if (script.StartsWith("#fommScript"))
      {
        return "Cannot syntax check a fomm script";
      }

      string[] errors;
      string[] warnings;

      Compile(script, out errors, out warnings, out stdout);
      if (errors != null || warnings != null)
      {
        var sb = new StringBuilder();
        if (errors != null)
        {
          sb.AppendLine("Errors:");
          foreach (var error in errors)
          {
            sb.AppendLine(error);
          }
        }
        if (warnings != null)
        {
          sb.AppendLine("Warnings:");
          foreach (var warning in warnings)
          {
            sb.AppendLine(warning);
          }
        }
        return sb.ToString();
      }
      return null;
    }

    /// <summary>
    ///   Executes a custom install script.
    /// </summary>
    /// <param name="script">The script to run.</param>
    /// <param name="p_midInstaller">The installer script to use the execute the custom script.</param>
    /// <returns>
    ///   <lang langref="true" /> if the script return <lang langref="true" />;
    ///   <lang langref="null" /> otherwise.
    /// </returns>
    public static bool Execute(string script, ModInstaller p_midInstaller)
    {
      if (script.StartsWith("#fommScript"))
      {
        if (fommScriptObject == null)
        {
          LoadFommScriptObject();
        }
        return (bool) fommScriptObject.GetType().GetMethod("RunScript").Invoke(fommScriptObject, new object[]
        {
          script, p_midInstaller
        });
      }
      var data = Compile(script);
      if (data == null)
      {
        MessageBox.Show("C# script failed to compile", "Error");
        return false;
      }

      var asm = AppDomain.CurrentDomain.Load(data);
      var s = asm.CreateInstance("Script");
      if (s == null)
      {
        MessageBox.Show("C# or vb script did not contain a 'Script' class in the root namespace",
                        "Error");
        return false;
      }
      try
      {
        MethodInfo mifMethod = null;
        for (var tpeScriptType = s.GetType(); mifMethod == null; tpeScriptType = tpeScriptType.BaseType)
        {
          mifMethod = tpeScriptType.GetMethod("Setup", new[]
          {
            typeof (ModInstaller)
          });
        }
        mifMethod.Invoke(s, new object[]
        {
          p_midInstaller
        });
        return (bool) s.GetType().GetMethod("OnActivate").Invoke(s, null);
      }
      catch (Exception ex)
      {
        MessageBox.Show("An exception occured. The mod may not have been activated completely.\n" +
                        "Check" + Environment.NewLine +
                        Path.Combine(Program.GameMode.InstallInfoDirectory,
                                     "ScriptException.txt") + Environment.NewLine +
                        "for full details", "Error");
        var str = ex.ToString();
        while (ex.InnerException != null)
        {
          ex = ex.InnerException;
          str += Environment.NewLine + Environment.NewLine + ex;
        }
        File.WriteAllText(
          Path.Combine(Program.GameMode.InstallInfoDirectory, "ScriptException.txt"), str);
        return false;
      }
    }
  }
}