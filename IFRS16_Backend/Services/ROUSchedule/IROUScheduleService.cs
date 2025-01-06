using IFRS16_Backend.Models;

namespace IFRS16_Backend.Services.ROUSchedule
{
    public interface IROUScheduleService
    {
        Task<bool> PostROUSchedule(double TotalNPV, LeaseFormData leaseData);
        Task<ROUScheduleResult> GetROUSchedule(int pageNumber, int pageSize, int leaseId);
    }
}
