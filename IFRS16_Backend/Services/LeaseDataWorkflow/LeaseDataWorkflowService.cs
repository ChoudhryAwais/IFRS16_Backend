using Azure.Core;
using IFRS16_Backend.Models;
using IFRS16_Backend.Services.InitialRecognition;
using IFRS16_Backend.Services.JournalEntries;
using IFRS16_Backend.Services.LeaseData;
using IFRS16_Backend.Services.LeaseLiability;
using IFRS16_Backend.Services.ROUSchedule;
using Microsoft.EntityFrameworkCore;

namespace IFRS16_Backend.Services.LeaseDataWorkflow
{
    public class LeaseDataWorkflowService(
        ILeaseDataService leaseFormDataService,
        IInitialRecognitionService initialRecognitionService,
        IROUScheduleService rouScheduleService,
        ILeaseLiabilityService leaseLiabilityService,
        IJournalEntriesService journalEntriesService,
        ApplicationDbContext context
        ) : ILeaseDataWorkflowService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILeaseDataService _leaseFormDataService = leaseFormDataService;
        private readonly IInitialRecognitionService _initialRecognitionService = initialRecognitionService;
        private readonly IROUScheduleService _rouScheduleService = rouScheduleService;
        private readonly ILeaseLiabilityService _leaseLiabilityService = leaseLiabilityService;
        private readonly IJournalEntriesService _journalEntriesService = journalEntriesService;

        public async Task<bool> ProcessLeaseFormDataAsync(LeaseFormData leaseFormData)
        {
            try
            {
                bool result = await _leaseFormDataService.AddLeaseFormDataAsync(leaseFormData);
                var initialRecognitionRes = new InitialRecognitionResult();
                if (leaseFormData.CustomIRTable != null)
                {
                    initialRecognitionRes = await _initialRecognitionService.PostCustomInitialRecognitionForLease(leaseFormData);
                }
                else
                {
                    initialRecognitionRes = await _initialRecognitionService.PostInitialRecognitionForLease(leaseFormData);
                }

                ROUScheduleRequest request = new()
                {
                    TotalNPV = (double)initialRecognitionRes.TotalNPV,
                    LeaseData = leaseFormData
                };

                var (rouSchedule, fc_rouSchedule) = await _rouScheduleService.PostROUSchedule(request.TotalNPV, request.LeaseData, leaseFormData.ReportingCurrencyID);

                var (leaseLiability, fc_leaseLiability) = await _leaseLiabilityService.PostLeaseLiability(
                    request.TotalNPV,
                    initialRecognitionRes.CashFlow,
                    initialRecognitionRes.Dates,
                    leaseFormData,
                    leaseFormData.ReportingCurrencyID
                );

                var journalEntries = await _journalEntriesService.PostJEForLease(leaseFormData, leaseLiability, rouSchedule);
                if (fc_rouSchedule.Count > 0 && fc_leaseLiability.Count > 0)
                {
                    var fc_journalEntries = await _journalEntriesService.PostJEForLeaseforFC(leaseFormData, fc_leaseLiability, fc_rouSchedule);
                }

                return true;
            }
            catch (Exception)
            {
                // Log and handle exceptions appropriately
                throw;
            }
        }
        public async Task<bool> ModificationLeaseFormDataAsync(LeaseFormData leaseModificationData)
        {
            try
            {
                LeaseLiabilityTable? leaseLiabilityObjBackDate = _context.LeaseLiability
                    .Where(item => item.LeaseId == leaseModificationData.LeaseId && item.LeaseLiability_Date < leaseModificationData.LastModifiedDate)
                    .OrderByDescending(item => item.LeaseLiability_Date)
                    .FirstOrDefault();

                ROUScheduleTable? rouObjBackDate = _context.ROUSchedule
                    .Where(item => item.LeaseId == leaseModificationData.LeaseId && item.ROU_Date < leaseModificationData.LastModifiedDate)
                    .OrderByDescending(item => item.ROU_Date)
                    .FirstOrDefault();

                LeaseLiabilityTable? leaseLiabilityObjOnModificationDate = _context.LeaseLiability
                   .FirstOrDefault(item => item.LeaseId == leaseModificationData.LeaseId && item.LeaseLiability_Date == leaseModificationData.LastModifiedDate);

                ROUScheduleTable? rouObj = _context.ROUSchedule
                    .FirstOrDefault(item => item.LeaseId == leaseModificationData.LeaseId && item.ROU_Date == leaseModificationData.LastModifiedDate);

                double ROUWithOutAdjustment = leaseModificationData.RouOpening ?? 0;
                double modificationAdjustmentForJE = 0;

                await _context.ModifyLeaseAsync(leaseModificationData?.LastModifiedDate, leaseModificationData.LeaseId);

                InitialRecognitionResult IRResult = await _initialRecognitionService.PostCustomInitialRecognitionForLease(leaseModificationData);

                if (leaseLiabilityObjBackDate != null)
                {
                    leaseLiabilityObjBackDate.ModificationAdjustment = ((double)IRResult.TotalNPV - leaseLiabilityObjBackDate.Closing);
                    modificationAdjustmentForJE = ((double)IRResult.TotalNPV - (leaseModificationData?.LLOpening ?? leaseLiabilityObjBackDate.Closing));
                    _context.LeaseLiability.Update(leaseLiabilityObjBackDate);
                    await _context.SaveChangesAsync();
                }
                leaseModificationData.RouOpening = 
                    leaseModificationData.RouOpening != null ? 
                    leaseModificationData.RouOpening // + leaseLiabilityObjBackDate?.ModificationAdjustment Comment out as per requirement - 01/09/2025
                    : leaseLiabilityObjBackDate?.ModificationAdjustment + rouObj?.Opening;
                if (rouObjBackDate != null)
                {
                    rouObjBackDate.ModificationAdjustment = ((double)(leaseModificationData?.RouOpening ?? 0) - rouObjBackDate.Closing);
                    _context.ROUSchedule.Update(rouObjBackDate);
                    await _context.SaveChangesAsync();
                }

                // Update the corresponding lease
                LeaseFormData? existingLease = await _context.LeaseData.FirstOrDefaultAsync(item => item.LeaseId == leaseModificationData.LeaseId);
                if (existingLease != null)
                {
                    existingLease.UserID = leaseModificationData.UserID;
                    existingLease.UserName = leaseModificationData.UserName;
                    existingLease.LeaseName = leaseModificationData.LeaseName;
                    existingLease.Rental = leaseModificationData.Rental;
                    existingLease.EndDate = leaseModificationData.EndDate;
                    existingLease.Annuity = leaseModificationData.Annuity;
                    existingLease.IBR = leaseModificationData.IBR;
                    existingLease.Frequency = leaseModificationData.Frequency;
                    existingLease.IDC = leaseModificationData.IDC;
                    existingLease.GRV = leaseModificationData.GRV;
                    existingLease.Increment = leaseModificationData.Increment;
                    existingLease.IncrementalFrequency = leaseModificationData.IncrementalFrequency;
                    existingLease.LastModifiedDate = leaseModificationData.LastModifiedDate;
                    existingLease.CurrencyID = leaseModificationData.CurrencyID;

                    _context.LeaseData.Update(existingLease);
                    await _context.SaveChangesAsync();
                }

                ROUScheduleRequest request = new()
                {
                    TotalNPV = (double)IRResult.TotalNPV,
                    LeaseData = leaseModificationData
                };

                var (rouSchedule, fc_rouSchedule) = await _rouScheduleService.PostROUSchedule(request.TotalNPV, request.LeaseData, leaseModificationData.ReportingCurrencyID);
                var (leaseLiability, fc_leaseLiability) = await _leaseLiabilityService.PostLeaseLiability(
                    request.TotalNPV,
                    IRResult.CashFlow,
                    IRResult.Dates,
                    leaseModificationData,
                    leaseModificationData.ReportingCurrencyID
                );

                var modificationDetails = new ModificationDetails(
                    (leaseModificationData.IsChangeInScope ? leaseLiabilityObjOnModificationDate?.Opening - (leaseModificationData?.LLOpening ?? (double)IRResult.TotalNPV) : 0), //?? (double)IRResult.TotalNPV)  31 August,2025 change due to exluding the leaseLiability opening from UI in case of change in scope
                    (leaseModificationData.IsChangeInScope ? rouObj.Opening - ROUWithOutAdjustment : 0),
                    (modificationAdjustmentForJE)
                );

                var journalEntries = await _journalEntriesService.PostJEForLease(leaseModificationData, leaseLiability, rouSchedule, modificationDetails);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}
