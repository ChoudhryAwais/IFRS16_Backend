using IFRS16_Backend.Helper;
using IFRS16_Backend.Models;

namespace IFRS16_Backend.Services.LeaseLiability
{
    public class LeaseLiabilityService() : ILeaseLiabilityService
    {
        public IEnumerable<LeaseLiabilityTable> GetLeaseLiability(double totalNPV, List<double> cashFlow, List<DateTime> dates, LeaseFormData leaseData)
        {
            var (_, TotalDays) = CalculateLeaseDuration.GetLeaseDuration(leaseData.CommencementDate, leaseData.EndDate);
            double xirr = XIRR.XIRRCalculation(cashFlow, dates);
            double xirrDaily = Math.Pow(1 + xirr, 1.0 / 365.0) - 1;

            // Initialize variables
            double opening = totalNPV;
            double interest;
            double closing;
            cashFlow.RemoveAt(0); // Remove the first element
            dates.RemoveAt(0);    // Remove the first element

            DateTime currentDate = leaseData.CommencementDate;
            var leaseTable = new List<LeaseLiabilityTable>();

            for (int i = 1; i <= TotalDays; i++)
            {
                // Format the current date
                string formattedDateForXirr = FormatDate.FormatDateSimple(currentDate);
                string formattedDate = FormatDate.FormatDateSimple(currentDate);

                // Determine the rental amount
                double rental = 0;
                if (dates.Contains(currentDate))
                {
                    int indexOfDate = dates.FindIndex(date => FormatDate.FormatDateSimple(date) == formattedDateForXirr);
                    
                        rental = cashFlow[indexOfDate]; 
                }

                // Calculate interest and closing balance
                interest = (opening - rental) * xirrDaily;
                closing = (opening + interest) - rental;

                // Create a new table entry
                var tableObj = new LeaseLiabilityTable
                {
                    Date = formattedDate,
                    Opening = opening,
                    Interest = interest,
                    Payment = rental,
                    Closing = closing
                };

                // Move to the next day
                currentDate = currentDate.AddDays(1);
                opening = closing;

                // Add the entry to the lease table
                leaseTable.Add(tableObj);
            }

            return leaseTable;
        }
    }
}
