using IFRS16_Backend.Models;

namespace IFRS16_Backend.Services.LeaseLiabilityAggregation
{
    public interface ILeaseReportService
    {
        Task<LeaseReportTable> GetLeaseReport(DateTime startDate, DateTime endDate, string leaseIdList);
    }
}
