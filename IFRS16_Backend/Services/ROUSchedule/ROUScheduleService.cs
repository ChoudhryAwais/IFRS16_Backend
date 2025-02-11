using IFRS16_Backend.Helper;
using IFRS16_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace IFRS16_Backend.Services.ROUSchedule
{
    public class ROUScheduleService(ApplicationDbContext context, GetCurrecyRates getCurrencyRates) : IROUScheduleService
    {
        private readonly GetCurrecyRates _getCurrencyRates = getCurrencyRates;
        private readonly ApplicationDbContext _context = context;
        public async Task<(List<ROUScheduleTable>, List<FC_ROUScheduleTable>)> PostROUSchedule(double totalNPV, LeaseFormData leaseData)
        {
            // Calculate amortization
            var (_, TotalDays) = CalculateLeaseDuration.GetLeaseDuration(leaseData.CommencementDate, leaseData.EndDate);
            List<FC_ROUScheduleTable> fc_RouSchedule = [];
            List<ExchangeRateDTO> exchangeRatesList = _getCurrencyRates.GetListOfExchangeRates(leaseData);

            double amortization = ((totalNPV / TotalDays) + double.Epsilon) * 100 / 100;
            double opening = totalNPV + (leaseData.IDC ?? 0);
            double closing = ((totalNPV - amortization) + double.Epsilon) * 100 / 100;

            var rouSchedule = new List<ROUScheduleTable>();
            DateTime currentDate = leaseData.CommencementDate;
            decimal exchangeRate = exchangeRatesList.FirstOrDefault(item => item.ExchangeDate == leaseData.CommencementDate)?.ExchangeRate ?? 1;
            for (int i = 1; i <= TotalDays; i++)
            {
                // Add the ROU schedule entry
                rouSchedule.Add(new ROUScheduleTable
                {
                    LeaseId = leaseData.LeaseId,
                    ROU_Date = currentDate,
                    Opening = opening,
                    Amortization = amortization,
                    Closing = closing
                });
                if (exchangeRatesList.Count > 0)
                {
                    // Add the ROU schedule entry
                    fc_RouSchedule.Add(new FC_ROUScheduleTable
                    {
                        LeaseId = leaseData.LeaseId,
                        ROU_Date = currentDate,
                        Opening = opening * (double)exchangeRate,
                        Amortization = amortization * (double)exchangeRate,
                        Closing = closing * (double)exchangeRate
                    });
                }
                // Update values for the next iteration
                currentDate = currentDate.AddDays(1);
                opening = closing;
                closing = ((opening - amortization) + double.Epsilon) * 100 / 100;
            }

            try
            {
                _context.ROUSchedule.AddRange(rouSchedule);
                if (exchangeRatesList.Count > 0)
                {
                    _context.FC_ROUSchedule.AddRange(fc_RouSchedule);
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }



            return (rouSchedule, fc_RouSchedule);
        }
        public async Task<ROUScheduleResult> GetROUSchedule(int pageNumber, int pageSize, int leaseId, int fc_lease)
        {
            IEnumerable<ROUScheduleTable> rouSchedule = await _context.GetROUSchedulePaginatedAsync(pageNumber, pageSize, leaseId, fc_lease);
            int totalRecord = await _context.ROUSchedule.Where(r => r.LeaseId == leaseId).CountAsync();

            return new()
            {
                Data = rouSchedule,
                TotalRecords = totalRecord,
            };

        }
    }
}
