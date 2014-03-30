using System;
using System.Text.RegularExpressions;
using System.IO;

namespace Fomm.PackageManager.ModInstallLog
{
  public abstract class InstallLogBase
  {
    /// <summary>
    /// Normalizes the given path.
    /// </summary>
    /// <remarks>
    /// This removes multiple consecutive path separators and makes sure all path
    /// separators are <see cref="Path.DirectorySeparatorChar"/>.
    /// </remarks>
    /// <param name="p_strPath">The path to normalize.</param>
    /// <returns>The normalized path.</returns>
    protected string NormalizePath(string p_strPath)
    {
      Regex re;
      string ret = p_strPath;

      re = new Regex(Regex.Escape(Path.DirectorySeparatorChar.ToString()) + "+");

      ret = ret.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
      ret = re.Replace(ret, Path.DirectorySeparatorChar.ToString());

      return ret;
    }
  }
}
