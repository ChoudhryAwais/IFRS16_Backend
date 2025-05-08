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

        public async Task<DisclosureTable> GetDisclosure(DateTime fromDate, DateTime endDate)
        {
            IEnumerable<AllLeasesReportTable> leasesReport = await _context.GetAllLeaseReport(fromDate, endDate);

            // Map and sum values for DisclosureTable
            DisclosureTable aggregatedDisclosure = new()
            {
                OpeningLL = leasesReport.Sum(x => x.OpeningLL ?? 0),
                AdditionsDuringYearLL = leasesReport.Sum(x => x.AdditionsDuringYearLL ?? 0),
                Interest = leasesReport.Sum(x => x.Interest ?? 0),
                Payment = leasesReport.Sum(x => x.Payment ?? 0),
                ClosingLL = leasesReport.Sum(x => x.ClosingLL ?? 0),
                OpeningROU = leasesReport.Sum(x => x.OpeningROU ?? 0),
                AdditionsDuringYearROU = leasesReport.Sum(x => x.AdditionsDuringYearROU ?? 0),
                Amortization = leasesReport.Sum(x => x.Amortization ?? 0),
                ClosingROU = leasesReport.Sum(x => x.ClosingROU ?? 0),
                Exchange_Gain_Loss = leasesReport.Sum(x => x.Exchange_Gain_Loss ?? 0),
                ModificationAdjustmentLL = leasesReport.Sum(x => x.ModificationAdjustmentLL ?? 0),
                ModificationAdjustmentROU = leasesReport.Sum(x => x.ModificationAdjustmentROU ?? 0)
            };

            return aggregatedDisclosure;
        }
    }
}
