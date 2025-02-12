using IFRS16_Backend.enums;
using IFRS16_Backend.Helper;
using IFRS16_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace IFRS16_Backend.Services.LeaseLiability
{
    public class LeaseLiabilityService(ApplicationDbContext context, GetCurrecyRates getCurrencyRates) : ILeaseLiabilityService
    {
        private readonly GetCurrecyRates _getCurrencyRates = getCurrencyRates;
        private readonly ApplicationDbContext _context = context;
        public async Task<(List<LeaseLiabilityTable>, List<FC_LeaseLiabilityTable>)> PostLeaseLiability(double totalNPV, List<double> cashFlow, List<DateTime> dates, LeaseFormData leaseData)
        {
            var (_, TotalDays) = CalculateLeaseDuration.GetLeaseDuration(leaseData.CommencementDate, leaseData.EndDate);
            double xirr = XIRR.XIRRCalculation(cashFlow, dates);
            double xirrDaily = Math.Pow(1 + xirr, 1.0 / 365.0) - 1;
            List<FC_LeaseLiabilityTable> fc_LeaseLiability = [];
            List<ExchangeRateDTO> exchangeRatesList = _getCurrencyRates.GetListOfExchangeRates(leaseData);

            // Initialize variables
            double opening = totalNPV;
            double fc_opening = 1;
            //Foreign currency exchange
            double fc_interest;
            double fc_payment;
            double fc_closing;
            double fc_ex_gain_loss;

            if (exchangeRatesList.Count > 0)
            {
                decimal exchangeRateForOpening = exchangeRatesList.FirstOrDefault(item => item.ExchangeDate == leaseData.CommencementDate)?.ExchangeRate ?? exchangeRatesList[^1].ExchangeRate;
                fc_opening = opening * (double)exchangeRateForOpening;
            }
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

                // Determine the rental amount
                double rental = 0;
                if (dates.Contains(currentDate))
                {
                    if (i != TotalDays || leaseData.Annuity != AnnuityType.Advance)
                    {
                        int indexOfDate = dates.FindIndex(date => FormatDate.FormatDateSimple(date) == formattedDateForXirr);
                        rental = cashFlow[indexOfDate];
                    }
                }

                // Calculate interest and closing balance
                interest = ((opening - rental) * xirrDaily);
                closing = (opening + interest) - rental;

                // Create a new table entry
                var tableObj = new LeaseLiabilityTable
                {
                    LeaseId = leaseData.LeaseId,
                    LeaseLiability_Date = currentDate,
                    Opening = opening,
                    Interest = interest,
                    Payment = rental,
                    Closing = closing
                };

                if (exchangeRatesList.Count > 0)
                {
                    decimal exchangeRate = exchangeRatesList.FirstOrDefault(item => item.ExchangeDate == currentDate)?.ExchangeRate ?? exchangeRatesList[^1].ExchangeRate;
                    // Create a new table entry
                    fc_interest = interest * (double)exchangeRate;
                    fc_closing = closing * (double)exchangeRate;
                    fc_payment = rental * (double)exchangeRate;
                    fc_ex_gain_loss = fc_opening + fc_interest - fc_payment - fc_closing;

                    fc_LeaseLiability.Add(new FC_LeaseLiabilityTable
                    {
                        LeaseId = leaseData.LeaseId,
                        LeaseLiability_Date = currentDate,
                        Opening = fc_opening,
                        Interest = fc_interest,
                        Payment = fc_payment,
                        Closing = fc_closing,
                        Exchange_Gain_Loss = fc_ex_gain_loss
                    });
                    fc_opening = closing * (double)exchangeRate;
                }

                // Move to the next day
                currentDate = currentDate.AddDays(1);
                opening = closing;

                // Add the entry to the lease table
                leaseTable.Add(tableObj);
            }
            try
            {
                _context.LeaseLiability.AddRange(leaseTable);
                if (exchangeRatesList.Count > 0)
                {
                    _context.FC_LeaseLiability.AddRange(fc_LeaseLiability);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return (leaseTable, fc_LeaseLiability);
        }
        public async Task<LeaseLiabilityResult> GetLeaseLiability(int pageNumber, int pageSize, int leaseId, int fc_lease)
        {
            IEnumerable<FC_LeaseLiabilityTable> leaseLiability = await _context.GetLeaseLiabilityPaginatedAsync(pageNumber, pageSize, leaseId, fc_lease);
            int totalRecord = await _context.LeaseLiability.Where(r => r.LeaseId == leaseId).CountAsync();
            return new()
            {
                Data = leaseLiability,
                TotalRecords = totalRecord,
            };
        }
    }
}
