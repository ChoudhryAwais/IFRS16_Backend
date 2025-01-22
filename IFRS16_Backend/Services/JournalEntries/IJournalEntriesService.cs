using IFRS16_Backend.Models;

namespace IFRS16_Backend.Services.JournalEntries
{
    public interface IJournalEntriesService
    {
        Task<List<JournalEntryTable>> PostJEForLease(LeaseFormData leaseSpecificData, List<LeaseLiabilityTable> leaseLiability, List<ROUScheduleTable> rouSchedule);
        Task<JournalEntryResult> GetJEForLease(int pageNumber, int pageSize, int leaseId);
    }
}
