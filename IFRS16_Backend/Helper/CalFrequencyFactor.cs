using IFRS16_Backend.enums;

namespace IFRS16_Backend.Helper
{
    public class CalFrequencyFactor
    {
        public static int FrequencyFactor(string frequency)
        {
            int factor = 1;
            if (frequency == "annual")
            {
                factor = (int)Frequencies.Annual;

            }
            else if (frequency == "bi-annual")
            {
                factor = (int)Frequencies.BiAnnual;

            }
            else if (frequency == "quarterly")
            {
                factor = (int)Frequencies.Quarterly;
            }
            else if (frequency == "monthly")
            {
                factor = (int)Frequencies.Monthly;
            }
            return factor;
        }
    }
}
