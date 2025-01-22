using IFRS16_Backend.Models;
using IFRS16_Backend.Services.InitialRecognition;
using IFRS16_Backend.Services.JournalEntries;
using IFRS16_Backend.Services.LeaseData;
using IFRS16_Backend.Services.LeaseLiability;
using IFRS16_Backend.Services.ROUSchedule;

namespace IFRS16_Backend.Services.LeaseDataWorkflow
{
    public class LeaseDataWorkflowService(
        ILeaseDataService leaseFormDataService,
        IInitialRecognitionService initialRecognitionService,
        IROUScheduleService rouScheduleService,
        ILeaseLiabilityService leaseLiabilityService,
        IJournalEntriesService journalEntriesService
        ) : ILeaseDataWorkflowService
    {
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

                var initialRecognitionRes = await _initialRecognitionService.PostInitialRecognitionForLease(leaseFormData);

                ROUScheduleRequest request = new()
                {
                    TotalNPV = (double)initialRecognitionRes.TotalNPV,
                    LeaseData = leaseFormData
                };

                var rouSchedule = await _rouScheduleService.PostROUSchedule(request.TotalNPV, request.LeaseData);

                var leaseLiability = await _leaseLiabilityService.PostLeaseLiability(
                    request.TotalNPV,
                    initialRecognitionRes.CashFlow,
                    initialRecognitionRes.Dates,
                    leaseFormData
                );

                var journalEntries = await _journalEntriesService.PostJEForLease(leaseFormData, leaseLiability, rouSchedule);
                return true;
            }
            catch (Exception ex)
            {
                // Log and handle exceptions appropriately
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}
