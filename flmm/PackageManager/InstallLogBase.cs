using System;
using System.Text.RegularExpressions;
using System.IO;

namespace Fomm.PackageManager
{
	abstract class InstallLogBase
	{
		private static readonly Regex m_rgxCleanPath = new Regex("[" + Path.DirectorySeparatorChar + Path.AltDirectorySeparatorChar + "]{2,}");
		
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
			string strNormalizedPath = m_rgxCleanPath.Replace(p_strPath, Path.DirectorySeparatorChar.ToString());
			strNormalizedPath = strNormalizedPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			return strNormalizedPath;
		}
	}
}
