using System;
using System.Security.Policy;
using System.CodeDom.Compiler;
using Assembly = System.Reflection.Assembly;
using sList = System.Collections.Generic.List<string>;
using StringBuilder = System.Text.StringBuilder;
using System.Reflection;

namespace Fomm.PackageManager
{
	static class ScriptCompiler
	{
		private static readonly Microsoft.CSharp.CSharpCodeProvider csCompiler = new Microsoft.CSharp.CSharpCodeProvider();
		private static readonly CompilerParameters cParams;
		private static readonly Evidence evidence;
		private static Assembly fommScriptRunner;
		private static object fommScriptObject;

		private static readonly string ScriptOutputPath = System.IO.Path.Combine(Program.tmpPath, "dotnetscript");
		private static uint ScriptCount;

		static ScriptCompiler()
		{
			cParams = new CompilerParameters();
			cParams.GenerateExecutable = false;
			cParams.GenerateInMemory = false;
			cParams.IncludeDebugInformation = false;
			//cParams.OutputAssembly=ScriptOutputPath;
			cParams.ReferencedAssemblies.Add(System.IO.Path.Combine(Program.ExecutableDirectory, "fomm.Scripting.dll"));
			cParams.ReferencedAssemblies.Add("System.dll");
			cParams.ReferencedAssemblies.Add("System.Drawing.dll");
			cParams.ReferencedAssemblies.Add("System.Windows.Forms.dll");
			cParams.ReferencedAssemblies.Add("System.Xml.dll");

			evidence = new Evidence();
			evidence.AddHost(new Zone(System.Security.SecurityZone.Internet));
		}

		private static void LoadFommScriptObject()
		{
			byte[] data = Compile(@"
using System;
using fomm.Scripting;
using Fomm.PackageManager;

class ScriptRunner {
    public static bool RunScript(string script, ModInstaller p_midInstaller) {
        return fommScript.Execute(script, p_midInstaller);
    }
}");
			fommScriptRunner = AppDomain.CurrentDomain.Load(data, null, evidence);
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
			cParams.OutputAssembly = ScriptOutputPath + (ScriptCount++) + ".dll"; //Compatibility fix for mono, which needs a different assembly name each call
			CompilerResults results = csCompiler.CompileAssemblyFromSource(cParams, code);
			stdout = "";
			for (int i = 0; i < results.Output.Count; i++) stdout += results.Output[i] + Environment.NewLine;
			if (results.Errors.HasErrors)
			{
				sList msgs = new sList();
				foreach (CompilerError ce in results.Errors)
				{
					if (!ce.IsWarning) msgs.Add("Error on Line " + ce.Line + ": " + ce.ErrorText);
				}
				errors = msgs.ToArray();
			}
			else errors = null;
			if (results.Errors.HasWarnings)
			{
				sList msgs = new sList();
				foreach (CompilerError ce in results.Errors)
				{
					if (ce.IsWarning) msgs.Add("Warning on Line " + ce.Line + ": " + ce.ErrorText);
				}
				warnings = msgs.ToArray();
			}
			else warnings = null;
			if (results.Errors.HasErrors)
			{
				return null;
			}
			else
			{
				byte[] data = System.IO.File.ReadAllBytes(results.PathToAssembly);
				System.IO.File.Delete(results.PathToAssembly);
				return data;
			}
		}

		public static string CheckSyntax(string script, out string stdout)
		{
			stdout = null;

			if (script.StartsWith("#fommScript")) return "Cannot syntax check a fomm script";

			string[] errors = null;
			string[] warnings = null;

			Compile(script, out errors, out warnings, out stdout);
			if (errors != null || warnings != null)
			{
				StringBuilder sb = new StringBuilder();
				if (errors != null)
				{
					sb.AppendLine("Errors:");
					for (int i = 0; i < errors.Length; i++) sb.AppendLine(errors[i]);
				}
				if (warnings != null)
				{
					sb.AppendLine("Warnings:");
					for (int i = 0; i < warnings.Length; i++) sb.AppendLine(warnings[i]);
				}
				return sb.ToString();
			}
			return null;
		}

		/// <summary>
		/// Executes a custom install script.
		/// </summary>
		/// <param name="script">The script to run.</param>
		/// <param name="p_midInstaller">The installer script to use the execute the custom script.</param>
		/// <returns><lang cref="true"/> if the script return <lang cref="true"/>;
		/// <lang cref="null"/> otherwise.</returns>
		public static bool Execute(string script, ModInstaller p_midInstaller)
		{
			if (script.StartsWith("#fommScript"))
			{
				if (fommScriptObject == null) LoadFommScriptObject();
				return (bool)fommScriptObject.GetType().GetMethod("RunScript").Invoke(fommScriptObject, new object[] { script, p_midInstaller });
			}
			script = script.Replace(": BaseScript", ": Fallout3BaseScript");
			byte[] data = Compile(script);
			if (data == null)
			{
				System.Windows.Forms.MessageBox.Show("C# script failed to compile", "Error");
				return false;
			}
			Assembly asm = AppDomain.CurrentDomain.Load(data, null, evidence);
			//Assembly asm = AppDomain.CurrentDomain.Load(data);
			object s = asm.CreateInstance("Script");
			if (s == null)
			{
				System.Windows.Forms.MessageBox.Show("C# or vb script did not contain a 'Script' class in the root namespace", "Error");
				return false;
			}
			try
			{	
				MethodInfo mifMethod = null;
				for (Type tpeScriptType = s.GetType(); mifMethod == null; tpeScriptType = tpeScriptType.BaseType)
					mifMethod = tpeScriptType.GetMethod("Setup", new Type[] { typeof(ModInstaller) });
				mifMethod.Invoke(s, new object[] { p_midInstaller });
				return (bool)s.GetType().GetMethod("OnActivate").Invoke(s, null);
			}
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show("An exception occured. The mod may not have been activated completely.\n" +
					"Check 'ScriptException.txt' for full details", "Error");
				string str = ex.ToString();
				while (ex.InnerException != null)
				{
					ex = ex.InnerException;
					str += Environment.NewLine + Environment.NewLine + ex.ToString();
				}
				System.IO.File.WriteAllText(System.IO.Path.Combine(Program.GameMode.InstallInfoDirectory, "ScriptException.txt"), str);
				return false;
			}
		}
	}
}
