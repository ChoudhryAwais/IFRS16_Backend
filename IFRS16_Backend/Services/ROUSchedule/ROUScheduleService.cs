using EFCore.BulkExtensions;
using IFRS16_Backend.Helper;
using IFRS16_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace IFRS16_Backend.Services.ROUSchedule
{
    public class ROUScheduleService(ApplicationDbContext context, GetCurrecyRates getCurrencyRates) : IROUScheduleService
    {
        private readonly GetCurrecyRates _getCurrencyRates = getCurrencyRates;
        private readonly ApplicationDbContext _context = context;
        public async Task<(List<ROUScheduleTable>, List<FC_ROUScheduleTable>)> PostROUSchedule(double totalNPV, LeaseFormData leaseData, int reportingCurrencyID)
        {
            // Calculate amortization
            var (_, TotalDays, _) = CalculateLeaseDuration.GetLeaseDuration(leaseData.CommencementDate, leaseData.EndDate);
            List<FC_ROUScheduleTable> fc_RouSchedule = [];
            List<ExchangeRateDTO> exchangeRatesList = _getCurrencyRates.GetListOfExchangeRates(leaseData, reportingCurrencyID);
            decimal exchangeRate = 1;
            double opening = (double)((leaseData?.RouOpening != null ? leaseData.RouOpening : totalNPV) + (leaseData.IDC ?? 0));
            double GRV = leaseData.GRV ?? 0;
            double amortization = (((opening - GRV) / TotalDays) + double.Epsilon) * 100 / 100;
            double closing = ((opening - amortization) + double.Epsilon) * 100 / 100;

            var rouSchedule = new List<ROUScheduleTable>();
            DateTime currentDate = leaseData.CommencementDate;
            if (exchangeRatesList.Count > 0)
            {
                if (leaseData.RouExRate != null)
                {
                    exchangeRate = (decimal)leaseData.RouExRate;
                }
                else
                {
                    exchangeRate = exchangeRatesList.FirstOrDefault(item => item.ExchangeDate == leaseData.CommencementDate)?.ExchangeRate ?? exchangeRatesList[^1].ExchangeRate;
                }
            }

            for (int i = 1; i <= TotalDays; i++)
            {
                // Add the ROU schedule entry
                rouSchedule.Add(new ROUScheduleTable
                {
                    LeaseId = leaseData.LeaseId,
                    ROU_Date = currentDate,
                    Opening = opening * (double)exchangeRate,
                    Amortization = amortization * (double)exchangeRate,
                    Closing = closing * (double)exchangeRate,
                });
                if (exchangeRatesList.Count > 0)
                {
                    // Add the ROU schedule entry
                    fc_RouSchedule.Add(new FC_ROUScheduleTable
                    {
                        LeaseId = leaseData.LeaseId,
                        ROU_Date = currentDate,
                        Opening = opening,
                        Amortization = amortization,
                        Closing = closing
                    });
                }
                // Update values for the next iteration
                currentDate = currentDate.AddDays(1);
                opening = closing;
                closing = ((opening - amortization) + double.Epsilon) * 100 / 100;
            }

            try
            {
                await _context.BulkInsertAsync(rouSchedule);
                if (exchangeRatesList.Count > 0)
                {
                    await _context.BulkInsertAsync(fc_RouSchedule);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }



            return (rouSchedule, fc_RouSchedule);
        }
        public async Task<ROUScheduleResult> GetROUSchedule(int pageNumber, int pageSize, int leaseId, DateTime? startDate, DateTime? endDate)
        {
            IEnumerable<ROUScheduleTable> rouSchedule = await _context.GetROUSchedulePaginatedAsync(pageNumber, pageSize, leaseId, startDate, endDate);
            int totalRecord = await _context.ROUSchedule.Where(r => r.LeaseId == leaseId && (startDate == null || endDate == null || (r.ROU_Date >= startDate && r.ROU_Date <= endDate))).CountAsync();

            return new()
            {
                Data = rouSchedule,
                TotalRecords = totalRecord,
            };

        }
        public async Task<List<ROUScheduleTable>> GetAllROUSchedule(int leaseId)
        {
            List<ROUScheduleTable> ROUSchedule = await _context.ROUSchedule.Where(r => r.LeaseId == leaseId).ToListAsync();
            return ROUSchedule;

        }
    }
}
