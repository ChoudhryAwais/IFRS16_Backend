using IFRS16_Backend.Models;

namespace IFRS16_Backend.Services.ROUSchedule
{
    public interface IROUScheduleService
    {
        Task<(List<ROUScheduleTable>, List<FC_ROUScheduleTable>)> PostROUSchedule(double TotalNPV, LeaseFormData leaseData, int reportingCurrencyID);
        Task<ROUScheduleResult> GetROUSchedule(int pageNumber, int pageSize, int leaseId, DateTime? startDate, DateTime? endDate);
        Task<List<ROUScheduleTable>> GetAllROUSchedule(int leaseId);
    }
}
