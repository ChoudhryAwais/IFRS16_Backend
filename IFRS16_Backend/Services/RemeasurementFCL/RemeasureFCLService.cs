﻿using IFRS16_Backend.Models;
using IFRS16_Backend.Services.JournalEntries;
using IFRS16_Backend.Services.LeaseLiability;
using Microsoft.EntityFrameworkCore;

namespace IFRS16_Backend.Services.RemeasurementFCL
{
    public class RemeasureFCLService(ApplicationDbContext context, ILeaseLiabilityService leaseLiabilityService, IJournalEntriesService journalEntriesService) : IRemeasureFCLService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILeaseLiabilityService _leaseLiabilityService = leaseLiabilityService;
        private readonly IJournalEntriesService _journalEntriesService = journalEntriesService;

        public async Task<bool> RunBatchRemeasurementAsync(RemeasureFCRequest request)
        {
            // Get all leases with the specified CurrencyID, ordered by LeaseId DESC
            var leases = await _context.LeaseData
                .Where(l => l.CurrencyID == request.CurrencyID)
                .OrderByDescending(l => l.LeaseId)
                .ToListAsync();

            foreach (var lease in leases)
            {
                // Delete lease liability records for each lease where date is greater than or equal to the remeasurement date
                var liabilitiesToDelete = await _context.LeaseLiability
                    .Where(ll => ll.LeaseId == lease.LeaseId &&
                               ll.LeaseLiability_Date >= request.RemeasurementDate)
                    .ToListAsync();

                // Delete journal entry records for each lease where JE_Date is greater than or equal to the remeasurement date
                var journalEntriesToDelete = await _context.JournalEntries
                    .Where(je => je.LeaseId == lease.LeaseId &&
                                 je.JE_Date >= request.RemeasurementDate)
                    .ToListAsync();

                // Get ROU schedule records for each lease where ROU_Date is greater than or equal to the remeasurement date
                var rouSchedules = await _context.ROUSchedule
                    .Where(r => r.LeaseId == lease.LeaseId &&
                                r.ROU_Date >= request.RemeasurementDate)
                    .ToListAsync();

                // Get the last LeaseLiabilityTable before or on the remeasurement date
                var lastLiabilityBeforeRemeasure = await _context.LeaseLiability
                    .Where(ll => ll.LeaseId == lease.LeaseId && ll.LeaseLiability_Date < request.RemeasurementDate)
                    .OrderByDescending(ll => ll.LeaseLiability_Date)
                    .FirstOrDefaultAsync();

                // Get the last FC_LeaseLiabilityTable before or on the remeasurement date
                var originalLastLiabilityBeforeRemeasure = await _context.FC_LeaseLiability
                    .Where(ll => ll.LeaseId == lease.LeaseId && ll.LeaseLiability_Date < request.RemeasurementDate)
                    .OrderByDescending(ll => ll.LeaseLiability_Date)
                    .FirstOrDefaultAsync();

                // Error handling: if any required data is missing, throw and stop the process
                if (liabilitiesToDelete == null)
                    continue;
                if (journalEntriesToDelete == null)
                    continue;
                if (lastLiabilityBeforeRemeasure == null)
                    continue; // Skip this lease and move to the next iteration
                if (originalLastLiabilityBeforeRemeasure == null)
                    continue;

                if (journalEntriesToDelete.Count > 0 && liabilitiesToDelete.Count > 0)
                {
                    _context.LeaseLiability.RemoveRange(liabilitiesToDelete);
                    _context.JournalEntries.RemoveRange(journalEntriesToDelete);

                    // Get InitialRecognition data for the lease
                    var initialRecognitionList = await _context.InitialRecognition
                        .Where(ir => ir.LeaseId == lease.LeaseId)
                        .OrderBy(ir => ir.PaymentDate)
                        .ToListAsync();

                    if (initialRecognitionList == null || initialRecognitionList.Count == 0)
                        throw new Exception($"No InitialRecognition data found for LeaseId {lease.LeaseId}.");

                    // Sum the NPV column
                    var totalNPV = initialRecognitionList.Sum(ir => ir.NPV);

                    // Convert Rental column to cashFlow array and PaymentDate to dateArray
                    var cashFlow = initialRecognitionList.Select(ir => (double)ir.Rental).ToList();
                    var dateArray = initialRecognitionList.Select(ir => ir.PaymentDate).ToList();

                    // Insert -totalNPV and CommencementDate at the start
                    cashFlow.Insert(0, (double)-totalNPV);
                    dateArray.Insert(0, lease.CommencementDate);

                    var leaseLiability = await _leaseLiabilityService.RemeasureLeaseLiability(
                        cashFlow,
                        dateArray,
                        lease,
                        request.RemeasurementDate,
                        originalLastLiabilityBeforeRemeasure.Closing,
                        lastLiabilityBeforeRemeasure.Closing
                    );

                    if (leaseLiability == null || leaseLiability.Count == 0)
                        throw new Exception($"RemeasureLeaseLiability failed for LeaseId {lease.LeaseId}.");

                    var journalEntries = await _journalEntriesService.PostJEForLease(lease, leaseLiability, rouSchedules) ?? throw new Exception($"PostJEForLease failed for LeaseId {lease.LeaseId}.");
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
