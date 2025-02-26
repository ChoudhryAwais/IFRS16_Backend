﻿using IFRS16_Backend.Models;

namespace IFRS16_Backend.Services.JournalEntries
{
    public interface IJournalEntriesService
    {
        Task<List<JournalEntryTable>> PostJEForLease(LeaseFormData leaseSpecificData, List<LeaseLiabilityTable> leaseLiability, List<ROUScheduleTable> rouSchedule);
        Task<List<FC_JournalEntryTable>> PostJEForLeaseforFC(LeaseFormData leaseSpecificData, List<FC_LeaseLiabilityTable> fc_leaseLiability, List<FC_ROUScheduleTable> fc_rouSchedule);
        Task<JournalEntryResult> GetJEForLease(int pageNumber, int pageSize, int leaseId, DateTime? startDate, DateTime? endDate);
    }
}
