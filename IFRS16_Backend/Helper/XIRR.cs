namespace IFRS16_Backend.Helper
{
    public class XIRR
    {
        public static double XIRRCalculation(List<double> cashFlows, List<DateTime> dates, double guess = 0.1)
        {
            if (cashFlows.Count != dates.Count)
            {
                throw new ArgumentException("Cash flows and dates must have the same length.");
            }

            // Convert dates to timestamps for calculation
            var times = dates.ConvertAll(date => date.Ticks);

            // Helper function to calculate the XNPV
            double XNPV(double rate)
            {
                var t0 = times[0]; // First date as the base date
                return cashFlows.Zip(times, (cf, t) =>
                {
                    var diffInYears = (t - t0) / (double)TimeSpan.TicksPerDay / 365.0; // Difference in years
                    return cf / Math.Pow(1 + rate, diffInYears);
                }).Sum();
            }

            // Helper function to calculate the derivative of XNPV (used for Newton-Raphson)
            double dXNPV(double rate)
            {
                var t0 = times[0];
                return cashFlows.Zip(times, (cf, t) =>
                {
                    var diffInYears = (t - t0) / (double)TimeSpan.TicksPerDay / 365.0; // Difference in years
                    return -(diffInYears * cf) / Math.Pow(1 + rate, diffInYears + 1);
                }).Sum();
            }

            // Newton-Raphson method to find the root (XIRR)
            double rate = guess;
            const int maxIterations = 1000;
            const double tolerance = 1e-6;

            for (int i = 0; i < maxIterations; i++)
            {
                double npv = XNPV(rate);
                double derivative = dXNPV(rate);

                if (Math.Abs(npv) < tolerance)
                {
                    return rate; // Return the result as a decimal
                }

                if (Math.Abs(derivative) < tolerance)
                {
                    throw new InvalidOperationException("Derivative too small; Newton-Raphson method failed.");
                }

                rate -= npv / derivative;
            }

            throw new InvalidOperationException("Newton-Raphson method did not converge.");
        }
    }
}
