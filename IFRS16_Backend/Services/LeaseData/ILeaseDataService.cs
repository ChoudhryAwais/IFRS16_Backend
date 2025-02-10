using IFRS16_Backend.Models;

namespace IFRS16_Backend.Services.LeaseData
{
    public interface ILeaseDataService
    {
        Task<bool> AddLeaseFormDataAsync(LeaseFormData leaseFormData);
        Task<LeaseFormDataResult> GetAllLeases(int pageNumber, int pageSize, int companyID);
        Task<List<LeaseFormData>> GetAllLeasesForCompany(int companyID);
    }
}
