using IFRS16_Backend.Models;

namespace IFRS16_Backend.Services.LeaseDataService
{
    public interface ILeaseDataService
    {
        Task<bool> AddLeaseFormDataAsync(LeaseFormData leaseFormData);
    }
}
