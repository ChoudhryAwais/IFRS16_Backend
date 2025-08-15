using IFRS16_Backend.Models;

namespace IFRS16_Backend.Services.Report
{
    public interface IReportsService
    {
        Task<IEnumerable<AllLeasesReportTable>> GetAllLeaseReport(DateTime fromDate, DateTime endDate, int companyId);
        Task<IEnumerable<LeaseReportSummaryTable>> GetLeaseReportSummary(DateTime startDate, DateTime endDate, string? leaseIdList, int companyId);
        Task<IEnumerable<JournalEntryReport>> GetJEReport(DateTime startDate, DateTime endDate, int companyId);
        Task<DisclosureTable> GetDisclosure(DateTime startDate, DateTime endDate, int companyId);
        Task<IEnumerable<DisclouserMaturityAnalysisTable>> GetDisclouserMaturityAnalysis(DateTime startDate, DateTime endDate, int companyId);

    }
}
