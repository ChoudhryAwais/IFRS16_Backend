using IFRS16_Backend.Models;

namespace IFRS16_Backend.Services.LeaseDataWorkflow
{
    public interface ILeaseDataWorkflowService
    {
        Task<bool> ProcessLeaseFormDataAsync(LeaseFormData leaseFormData);
        Task<bool> ModificationLeaseFormDataAsync(LeaseFormData leaseModificationData);
    }
}
