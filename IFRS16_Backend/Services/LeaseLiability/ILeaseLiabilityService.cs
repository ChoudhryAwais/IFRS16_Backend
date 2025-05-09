﻿using IFRS16_Backend.Models;

namespace IFRS16_Backend.Services.LeaseLiability
{
    public interface ILeaseLiabilityService
    {
        Task<(List<LeaseLiabilityTable>, List<FC_LeaseLiabilityTable>)> PostLeaseLiability(double totalNPV, List<double> cashFlow, List<DateTime> dates, LeaseFormData leaseData, double customOpening = 0);

        Task<LeaseLiabilityResult> GetLeaseLiability(int pageNumber, int pageSize, int leaseId, DateTime? startDate, DateTime? endDate);
        Task<List<LeaseLiabilityTable>> GetAllLeaseLiability(int leaseId);
    }
}
