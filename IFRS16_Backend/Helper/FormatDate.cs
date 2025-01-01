namespace IFRS16_Backend.Helper
{
    public class FormatDate
    {
        public static string FormatDateSimple(DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }
    }
}
