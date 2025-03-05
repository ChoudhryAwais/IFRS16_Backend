using IFRS16_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace IFRS16_Backend.Services.LeaseLiabilityAggregation
{
    public class ReportsService(ApplicationDbContext context) : IReportsService
    {
        private readonly ApplicationDbContext _context = context;
        public async Task<IEnumerable<AllLeasesReportTable>> GetAllLeaseReport(DateTime fromDate, DateTime endDate)
        {
            IEnumerable<AllLeasesReportTable> leasesReport = await _context.GetAllLeaseReport(fromDate, endDate);

            return leasesReport;
        }

        public async Task<IEnumerable<LeaseReportSummaryTable>> GetLeaseReportSummary(DateTime startDate, DateTime endDate, string? leaseIdList)
        {
            IEnumerable<LeaseReportSummaryTable> leasesReportSummary = await _context.GetLeaseReportSummary(startDate, endDate, leaseIdList);

            return leasesReportSummary;
        }
        public async Task<IEnumerable<JournalEntryReport>> GetJEReport(DateTime startDate, DateTime endDate)
        {
            IEnumerable<JournalEntryReport> journalEntryReport = await _context.GetJEReport(startDate, endDate);

            return journalEntryReport;
        }

    }
}
