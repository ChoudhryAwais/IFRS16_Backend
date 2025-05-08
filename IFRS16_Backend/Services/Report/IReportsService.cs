using IFRS16_Backend.Models;

namespace IFRS16_Backend.Services.LeaseLiabilityAggregation
{
    public interface IReportsService
    {
        Task<IEnumerable<AllLeasesReportTable>> GetAllLeaseReport(DateTime fromDate, DateTime endDate);
        Task<IEnumerable<LeaseReportSummaryTable>> GetLeaseReportSummary(DateTime startDate, DateTime endDate, string? leaseIdList);
        Task<IEnumerable<JournalEntryReport>> GetJEReport(DateTime startDate, DateTime endDate);
        Task<DisclosureTable> GetDisclosure(DateTime startDate, DateTime endDate);

    }
}
