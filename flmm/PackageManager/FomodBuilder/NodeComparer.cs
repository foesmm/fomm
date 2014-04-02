using System.Collections;

namespace Fomm.PackageManager.FomodBuilder
{
  /// <summary>
  /// Compares two <see cref="FileSystemTreeNode"/>s.
  /// </summary>
  public class NodeComparer : IComparer
  {
    #region IComparer Members

    /// <summary>
    /// Compares the given <see cref="FileSystemTreeNode"/>s using their
    /// <see cref="FileSystemTreeNode.CompareTo"/> method.
    /// </summary>
    /// <param name="x">A <see cref="FileSystemTreeNode"/> to compare to another node.</param>
    /// <param name="y">A <see cref="FileSystemTreeNode"/> to compare to another node.</param>
    /// <returns>A value less than 0 if <paramref name="x"/> is less than <paramref name="y"/>.
    /// 0 if this node is equal to the other.
    /// A value greater than 0 if <paramref name="x"/> is greater than <paramref name="y"/>.</returns>
    public int Compare(object x, object y)
    {
      return ((FileSystemTreeNode) x).CompareTo((FileSystemTreeNode) y);
    }

    #endregion
  }
}