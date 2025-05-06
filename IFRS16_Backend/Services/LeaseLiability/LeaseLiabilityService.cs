using IFRS16_Backend.enums;
using IFRS16_Backend.Helper;
using IFRS16_Backend.Models;
using Microsoft.EntityFrameworkCore;
using EFCore.BulkExtensions;

namespace IFRS16_Backend.Services.LeaseLiability
{
    public class LeaseLiabilityService(ApplicationDbContext context, GetCurrecyRates getCurrencyRates) : ILeaseLiabilityService
    {
        private readonly GetCurrecyRates _getCurrencyRates = getCurrencyRates;
        private readonly ApplicationDbContext _context = context;
        public async Task<(List<LeaseLiabilityTable>, List<FC_LeaseLiabilityTable>)> PostLeaseLiability(double totalNPV, List<double> cashFlow, List<DateTime> dates, LeaseFormData leaseData, double customOpening = 0)
        {
            var (_, TotalDays, _) = CalculateLeaseDuration.GetLeaseDuration(leaseData.CommencementDate, leaseData.EndDate);
            double xirr = XIRR.XIRRCalculation(cashFlow, dates);
            double xirrDaily = Math.Pow(1 + xirr, 1.0 / 365.0) - 1;
            List<LeaseLiabilityTable> leaseLiability = [];
            List<ExchangeRateDTO> exchangeRatesList = _getCurrencyRates.GetListOfExchangeRates(leaseData);

            decimal exchangeRate = 1;
            // Initialize variables
            double base_opening = customOpening == 0 ? totalNPV : customOpening;
            double fc_opening = customOpening == 0 ? totalNPV : customOpening;
            //Foreign currency exchange
            double interest;
            double payment;
            double closing;
            double fc_ex_gain_loss = 0;

            if (exchangeRatesList.Count > 0)
            {
                decimal exchangeRateForOpening = exchangeRatesList.FirstOrDefault(item => item.ExchangeDate == leaseData.CommencementDate)?.ExchangeRate ?? exchangeRatesList[^1].ExchangeRate;
                fc_opening = base_opening * (double)exchangeRateForOpening;
            }
            double base_interest;
            double base_closing;
            cashFlow.RemoveAt(0); // Remove the first element
            dates.RemoveAt(0);    // Remove the first element

            DateTime currentDate = leaseData.CommencementDate;
            var fc_leaseLiability = new List<FC_LeaseLiabilityTable>();

            for (int i = 1; i <= TotalDays; i++)
            {
                // Format the current date
                string formattedDateForXirr = FormatDate.FormatDateSimple(currentDate);

                // Determine the rental amount
                double rental = 0;
                if (dates.Contains(currentDate))
                {
                    if (i != TotalDays || leaseData.Annuity != AnnuityType.Advance) //This condition is for GRV 
                    {
                        int indexOfDate = dates.FindIndex(date => FormatDate.FormatDateSimple(date) == formattedDateForXirr);
                        rental = cashFlow[indexOfDate];
                    }
                }
                // Calculate interest and closing balance
                base_interest = ((base_opening - rental) * xirrDaily);
                base_closing = (base_opening + base_interest) - rental;



                if (exchangeRatesList.Count > 0)
                {
                    exchangeRate = exchangeRatesList.FirstOrDefault(item => item.ExchangeDate == currentDate)?.ExchangeRate ?? exchangeRatesList[^1].ExchangeRate;
                    // Create a new table entry
                    // Add the entry to the lease table
                    fc_leaseLiability.Add(new FC_LeaseLiabilityTable
                    {
                        LeaseId = leaseData.LeaseId,
                        LeaseLiability_Date = currentDate,
                        Opening = base_opening,
                        Interest = base_interest,
                        Payment = rental,
                        Closing = base_closing
                    });
                }
                // Create a new table entry
                interest = base_interest * (double)exchangeRate;
                closing = base_closing * (double)exchangeRate;
                payment = rental * (double)exchangeRate;
                if (exchangeRatesList.Count > 0)
                {
                    fc_ex_gain_loss = fc_opening + interest - payment - closing;
                }


                leaseLiability.Add(new LeaseLiabilityTable
                {
                    LeaseId = leaseData.LeaseId,
                    LeaseLiability_Date = currentDate,
                    Opening = fc_opening,
                    Interest = interest,
                    Payment = payment,
                    Closing = closing,
                    Exchange_Gain_Loss = fc_ex_gain_loss,
                    ModificationAdjustment = 0
                });
                fc_opening = base_closing * (double)exchangeRate;


                // Move to the next day
                currentDate = currentDate.AddDays(1);
                base_opening = base_closing;


            }
            try
            {
                await _context.BulkInsertAsync(leaseLiability);

                if (exchangeRatesList.Count > 0)
                {
                    await _context.BulkInsertAsync(fc_leaseLiability);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return (leaseLiability, fc_leaseLiability);
        }
        public async Task<LeaseLiabilityResult> GetLeaseLiability(int pageNumber, int pageSize, int leaseId, DateTime? startDate, DateTime? endDate)
        {
            IEnumerable<LeaseLiabilityTable> leaseLiability = await _context.GetLeaseLiabilityPaginatedAsync(pageNumber, pageSize, leaseId, startDate, endDate);
            int totalRecord = await _context.LeaseLiability.Where(r => r.LeaseId == leaseId && (startDate == null || endDate == null || (r.LeaseLiability_Date >= startDate && r.LeaseLiability_Date <= endDate))).CountAsync();
            return new()
            {
                Data = leaseLiability,
                TotalRecords = totalRecord,
            };
        }
        public async Task<List<LeaseLiabilityTable>> GetAllLeaseLiability(int leaseId)
        {
            List<LeaseLiabilityTable> leaseLiability = await _context.LeaseLiability.Where(r => r.LeaseId == leaseId).ToListAsync();
            return leaseLiability;
        }
    }
}
