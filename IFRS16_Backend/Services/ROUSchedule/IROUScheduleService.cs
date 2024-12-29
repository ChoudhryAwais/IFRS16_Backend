using IFRS16_Backend.Models;

namespace IFRS16_Backend.Services.ROUSchedule
{
    public interface IROUScheduleService
    {
        IEnumerable<ROUScheduleTable> GetROUSchedule(double totalNPV, LeaseFormData leaseData);
    }
}
