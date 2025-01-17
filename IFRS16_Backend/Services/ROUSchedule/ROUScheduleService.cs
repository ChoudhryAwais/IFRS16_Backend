using IFRS16_Backend.Helper;
using IFRS16_Backend.Models;
using IFRS16_Backend.Services.InitialRecognition;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace IFRS16_Backend.Services.ROUSchedule
{
    public class ROUScheduleService(ApplicationDbContext context) : IROUScheduleService
    {
        private readonly ApplicationDbContext _context = context;
        public async Task<bool> PostROUSchedule(double totalNPV, LeaseFormData leaseData)
        {
            // Calculate amortization
            var (_, TotalDays) = CalculateLeaseDuration.GetLeaseDuration(leaseData.CommencementDate, leaseData.EndDate);
            double amortization = ((totalNPV / TotalDays) + double.Epsilon) * 100 / 100;
            double opening = totalNPV+ (leaseData.IDC ?? 0);
            double closing = ((totalNPV - amortization) + double.Epsilon) * 100 / 100;

            var rouSchedule = new List<ROUScheduleTable>();
            DateTime currentDate = leaseData.CommencementDate;

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

                // Update values for the next iteration
                currentDate = currentDate.AddDays(1);
                opening = closing;
                closing = ((opening - amortization) + double.Epsilon) * 100 / 100;
            }

            _context.ROUSchedule.AddRange(rouSchedule);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<ROUScheduleResult> GetROUSchedule(int pageNumber, int pageSize, int leaseId)
        {
            IEnumerable<ROUScheduleTable> rouSchedule = await _context.GetROUSchedulePaginatedAsync(pageNumber, pageSize, leaseId);
            int totalRecord = await _context.ROUSchedule.Where(r => r.LeaseId == leaseId).CountAsync();

            return new()
            {
                Data = rouSchedule,
                TotalRecords = totalRecord,
            };

        }
    }
}
