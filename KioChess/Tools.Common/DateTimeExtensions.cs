
namespace Tools.Common;

public static class DateTimeExtensions
{
    public static string ToFileName(this DateTime dateTime)
    {
        return dateTime.ToString("yyyy_MM_dd_hh_mm_ss_ffff");
    }
}
