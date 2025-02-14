using IFRS16_Backend.Models;

namespace IFRS16_Backend.Services.LeaseLiabilityAggregation
{
    public interface ILeaseReportService
    {
        Task<IEnumerable<AllLeasesReportTable>> GetAllLeaseReport(DateTime fromDate, DateTime endDate);
        Task<IEnumerable<LeaseReportSummaryTable>> GetLeaseReportSummary(DateTime startDate, DateTime endDate, string? leaseIdList);
    }
}
