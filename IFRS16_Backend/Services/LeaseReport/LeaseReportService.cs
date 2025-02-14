using IFRS16_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace IFRS16_Backend.Services.LeaseLiabilityAggregation
{
    public class LeaseReportService(ApplicationDbContext context) : ILeaseReportService
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

    }
}
