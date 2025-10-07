using IFRS16_Backend.Models;

namespace IFRS16_Backend.Services.JournalEntries
{
    public interface IJournalEntriesService
    {
        Task<List<JournalEntryTable>> PostJEForLease(LeaseFormData leaseSpecificData, List<LeaseLiabilityTable> leaseLiability, List<ROUScheduleTable> rouSchedule, ModificationDetails? modificationDetails = null, bool fromRemeasure = false);
        Task<List<FC_JournalEntryTable>> PostJEForLeaseforFC(LeaseFormData leaseSpecificData, List<FC_LeaseLiabilityTable> fc_leaseLiability, List<FC_ROUScheduleTable> fc_rouSchedule);
        Task<JournalEntryResult> GetJEForLease(int pageNumber, int pageSize, int leaseId, DateTime? startDate, DateTime? endDate);
        Task<List<JournalEntryTable>> GetAllJEForLease(int leaseId);
        Task<IEnumerable<JournalEntryTable>> EnterJEOnTermination(decimal LLClosing, decimal ROUClosing, decimal? Penalty, DateTime terminationDate, int leaseId);
    }
}
