using IFRS16_Backend.Models;

namespace IFRS16_Backend.Services.LeaseData
{
    public interface ILeaseDataService
    {
        Task<bool> AddLeaseFormDataAsync(LeaseFormData leaseFormData);
        Task<LeaseFormDataResult> GetAllLeases(int pageNumber, int pageSize, int companyID, string leaseID);
        Task<LeaseFormData> GetLeaseById(int leaseId);
        Task<List<LeaseFormData>> GetAllLeasesForCompany(int companyID);
        Task<bool> DeleteLeases(string leaseIds);
        Task<bool> TerminateLease(TerminateLease termination);
        Task UploadLeaseContractAsync(int leaseId, IFormFile contractDoc);
        Task<LeaseContract> GetLeaseContractByLeaseIdAsync(int leaseId);
        Task<bool> UpdateLeaseFormDataAsync(int leaseId, LeaseFormData updatedLease);
        Task<bool> UpdateLeaseContractAsync(int leaseId, IFormFile contractDoc);
    }
}
