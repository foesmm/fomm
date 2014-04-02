using System.IO;

namespace Fomm.Util
{
  /// <summary>
  /// Utility functions to work with text.
  /// </summary>
  public class TextUtil
  {
    /// <summary>
    /// Converts the given byte array to a string.
    /// </summary>
    /// <remarks>
    /// This method attempts to detect the text encoding.
    /// </remarks>
    /// <param name="p_bteText">The bytes to convert to a string.</param>
    /// <returns>A string respresented by the given bytes.</returns>
    public static string ByteToString(byte[] p_bteText)
    {
      string strText;
      using (var msmFile = new MemoryStream(p_bteText))
      {
        using (var strReader = new StreamReader(msmFile, true))
        {
          strText = strReader.ReadToEnd();
          strReader.Close();
        }
        msmFile.Close();
      }
      return strText;
    }
  }
}