using IFRS16_Backend.Models;

namespace IFRS16_Backend.Services.LeaseLiability
{
    public interface ILeaseLiabilityService
    {
        Task<List<LeaseLiabilityTable>> PostLeaseLiability(double totalNPV, List<double> cashFlow, List<DateTime> dates, LeaseFormData leaseData);

        Task<LeaseLiabilityResult> GetLeaseLiability(int pageNumber, int pageSize, int leaseId, int fc_lease);
    }
}
