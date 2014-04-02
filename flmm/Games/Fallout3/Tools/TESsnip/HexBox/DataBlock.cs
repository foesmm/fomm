namespace Fomm.Games.Fallout3.Tools.TESsnip.HexBox
{
  internal abstract class DataBlock
  {
    public abstract long Length { get; }

    public abstract void RemoveBytes(long position, long count);
  }
}