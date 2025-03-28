namespace Code.Common.Extensions
{
  public static class NumericExtensions
  {
    public static float ZeroIfNegative(this float value) => value >= 0 ? value : 0;

    public static int ZeroIfNegative(this int value) => value >= 0 ? value : 0;
  }
}