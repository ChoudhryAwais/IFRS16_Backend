namespace IFRS16_Backend.Helper
{
    public class CalculateLeaseDuration
    {
        public static (double TotalYears, int TotalDays, decimal TotalInitialDurationWithDecimal) GetLeaseDuration(DateTime commencementDate, DateTime endDate, string frequency = "annual", bool fromIR = false)
        {
            if (frequency == "irregular")
            {
                frequency = "annual";
            }
            // Validate the dates
            if (commencementDate == default || endDate == default)
            {
                throw new ArgumentException("Invalid date format. Provide valid DateTime values.");
            }

            // Ensure the end date is after the commencement date
            if (endDate < commencementDate)
            {
                throw new ArgumentException("End date must be after commencement date.");
            }

            // Calculate the difference in days
            int totalDays = (endDate - commencementDate).Days + (fromIR ? 0 : 1);

            // Calculate the difference in years
            int frequecnyFactor = 12 / CalFrequencyFactor.FrequencyFactor(frequency);
            decimal totalYears = (decimal)totalDays / 365;

            int TotalInitialDuration = (int)Math.Round(totalYears * frequecnyFactor);
            decimal TotalInitialDurationWithDecimal = totalYears;

            return (TotalInitialDuration, totalDays, TotalInitialDurationWithDecimal);
        }
    }
}
