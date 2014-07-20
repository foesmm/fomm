using System;
using System.IO;
using ChinhDo.Transactions;

namespace Fomm.Util
{
  /// <summary>
  ///   A delegate for a function that takes 1 parameter.
  /// </summary>
  /// <remarks>
  ///   This duplicates the functionality of the delegate with the same signature
  ///   found in .NET v3.5 and later. It is duplicated here to support pre-3.5 installs.
  /// </remarks>
  /// <typeparam name="T">The type of the parameter of the method that this delegate encapsulates.</typeparam>
  /// <typeparam name="TResult">The type of the return value of the method that this delegate encapsulates.</typeparam>
  /// <param name="p_tValue">The first parameter.</param>
  /// <returns>A value of type <typeparamref name="TResult" />.</returns>
  public delegate TResult Func<T, TResult>(T p_tValue);

  /// <summary>
  ///   Utility functions to work with files.
  /// </summary>
  public class FileUtil
  {
    /// <summary>
    ///   Copies the source to the destination.
    /// </summary>
    /// <remarks>
    ///   If the source is a directory, it is copied recursively.
    /// </remarks>
    /// <param name="p_tfmFileManager">The transactional file manager to use to copy the files.</param>
    /// <param name="p_strSource">The path from which to copy.</param>
    /// <param name="p_strDestination">The path to which to copy.</param>
    /// <param name="p_fncCopyCallback">
    ///   A callback method that notifies the caller when a file has been copied,
    ///   and provides the opportunity to cancel the copy operation.
    /// </param>
    /// <returns><lang langref="true" /> if the copy operation wasn't cancelled; <lang langref="false" /> otherwise.</returns>
    public static bool Copy(TxFileManager p_tfmFileManager, string p_strSource, string p_strDestination,
                            Func<string, bool> p_fncCopyCallback)
    {
      if (File.Exists(p_strSource))
      {
        if (!Directory.Exists(Path.GetDirectoryName(p_strDestination)))
        {
          p_tfmFileManager.CreateDirectory(Path.GetDirectoryName(p_strDestination));
        }
        p_tfmFileManager.Copy(p_strSource, p_strDestination, true);
        if ((p_fncCopyCallback != null) && p_fncCopyCallback(p_strSource))
        {
          return false;
        }
      }
      else if (Directory.Exists(p_strSource))
      {
        if (!Directory.Exists(p_strDestination))
        {
          p_tfmFileManager.CreateDirectory(p_strDestination);
        }
        var strFiles = Directory.GetFiles(p_strSource);
        foreach (var strFile in strFiles)
        {
          p_tfmFileManager.Copy(strFile, Path.Combine(p_strDestination, Path.GetFileName(strFile)), true);
          if ((p_fncCopyCallback != null) && p_fncCopyCallback(strFile))
          {
            return false;
          }
        }
        var strDirectories = Directory.GetDirectories(p_strSource);
        foreach (var strDirectory in strDirectories)
        {
          if (!Copy(strDirectory, Path.Combine(p_strDestination, Path.GetFileName(strDirectory)), p_fncCopyCallback))
          {
            return false;
          }
        }
      }
      return true;
    }

    /// <summary>
    ///   Copies the source to the destination.
    /// </summary>
    /// <remarks>
    ///   If the source is a directory, it is copied recursively.
    /// </remarks>
    /// <param name="p_strSource">The path from which to copy.</param>
    /// <param name="p_strDestination">The path to which to copy.</param>
    /// <param name="p_fncCopyCallback">
    ///   A callback method that notifies the caller when a file has been copied,
    ///   and provides the opportunity to cancel the copy operation.
    /// </param>
    /// <returns><lang langref="true" /> if the copy operation wasn't cancelled; <lang langref="false" /> otherwise.</returns>
    public static bool Copy(string p_strSource, string p_strDestination, Func<string, bool> p_fncCopyCallback)
    {
      if (File.Exists(p_strSource))
      {
        if (!Directory.Exists(Path.GetDirectoryName(p_strDestination)))
        {
          Directory.CreateDirectory(Path.GetDirectoryName(p_strDestination));
        }
        File.Copy(p_strSource, p_strDestination, true);
        if ((p_fncCopyCallback != null) && p_fncCopyCallback(p_strSource))
        {
          return false;
        }
      }
      else if (Directory.Exists(p_strSource))
      {
        if (!Directory.Exists(p_strDestination))
        {
          Directory.CreateDirectory(p_strDestination);
        }
        var strFiles = Directory.GetFiles(p_strSource);
        foreach (var strFile in strFiles)
        {
          File.Copy(strFile, Path.Combine(p_strDestination, Path.GetFileName(strFile)), true);
          if ((p_fncCopyCallback != null) && p_fncCopyCallback(strFile))
          {
            return false;
          }
        }
        var strDirectories = Directory.GetDirectories(p_strSource);
        foreach (var strDirectory in strDirectories)
        {
          if (!Copy(strDirectory, Path.Combine(p_strDestination, Path.GetFileName(strDirectory)), p_fncCopyCallback))
          {
            return false;
          }
        }
      }
      return true;
    }

    /// <summary>
    ///   Forces deletion of the given path.
    /// </summary>
    /// <remarks>
    ///   This method is recursive if the given path is a directory. This method will clear read only/system
    ///   attributes if required to delete the path.
    /// </remarks>
    /// <param name="pStrPath">The path to delete.</param>
    public static void ForceDelete(string pStrPath)
    {
      try
      {
        if (File.Exists(pStrPath))
        {
          File.Delete(pStrPath);
        }
        else if (Directory.Exists(pStrPath))
        {
          Directory.Delete(pStrPath, true);
        }
      }
      catch (Exception e)
      {
        if (!(e is IOException || e is UnauthorizedAccessException))
        {
          throw;
        }
        ClearAttributes(pStrPath, true);
        if (File.Exists(pStrPath))
        {
          File.Delete(pStrPath);
        }
        else if (Directory.Exists(pStrPath))
        {
          Directory.Delete(pStrPath, true);
        }
      }
    }

    /// <summary>
    ///   Clears the attributes of the given path.
    /// </summary>
    /// <remarks>
    ///   This sets the path's attributes to <see cref="FileAttributes.Normal" />. This operation is
    ///   optionally recursive.
    /// </remarks>
    /// <param name="p_strPath">The path whose attributes are to be cleared.</param>
    /// <param name="p_booRecurse">Whether or not to clear the attributes on all children files and folers.</param>
    public static void ClearAttributes(string p_strPath, bool p_booRecurse)
    {
      if (File.Exists(p_strPath))
      {
        var fifFile = new FileInfo(p_strPath);
        fifFile.Attributes = FileAttributes.Normal;
      }
      else if (Directory.Exists(p_strPath))
      {
        ClearAttributes(new DirectoryInfo(p_strPath), p_booRecurse);
      }
    }

    /// <summary>
    ///   Clears the attributes of the given directory.
    /// </summary>
    /// <remarks>
    ///   This sets the directory's attributes to <see cref="FileAttributes.Normal" />. This operation is
    ///   optionally recursive.
    /// </remarks>
    /// <param name="p_difPath">The directory whose attributes are to be cleared.</param>
    /// <param name="p_booRecurse">Whether or not to clear the attributes on all children files and folers.</param>
    public static void ClearAttributes(DirectoryInfo p_difPath, bool p_booRecurse)
    {
      p_difPath.Attributes = FileAttributes.Normal;
      if (p_booRecurse)
      {
        foreach (var difDirectory in p_difPath.GetDirectories())
        {
          ClearAttributes(difDirectory, true);
        }
        foreach (var fifFile in p_difPath.GetFiles())
        {
          fifFile.Attributes = FileAttributes.Normal;
        }
      }
    }
  }
}