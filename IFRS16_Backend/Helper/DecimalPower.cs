namespace IFRS16_Backend.Helper
{
    public class DecimalPower
    {
        public static decimal DecimalPowerCal(decimal baseValue, int exponent)
        {
            decimal result = 1m;
            for (int j = 0; j < exponent; j++)
            {
                result *= baseValue;
            }
            return result;
        }
        public static bool IsDecimal(string value)
        {
            return decimal.TryParse(value, out _);
        }
    }
}
