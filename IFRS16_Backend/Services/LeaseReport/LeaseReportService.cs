using IFRS16_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace IFRS16_Backend.Services.LeaseLiabilityAggregation
{
    public class LeaseReportService(ApplicationDbContext context) : ILeaseReportService
    {
        private readonly ApplicationDbContext _context = context;
        public async Task<LeaseReportTable> GetLeaseReport(DateTime startDate, DateTime endDate, string leaseIdList)
        {
            IEnumerable<LeaseLiabilityAggregationTable> llAggregationTable = await _context.GetLeaseLiabilityAggregationTableAsync(startDate, endDate, leaseIdList);
            IEnumerable<ROUAggregationTable> rouAggregationTable = await _context.GetROUAggregationTableAsync(startDate, endDate, leaseIdList);

            return new()
            {
                LeaseLiabilityAggregation = llAggregationTable,
                ROUAggregation = rouAggregationTable

            };
        }
    }
}
