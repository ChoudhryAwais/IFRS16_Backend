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
                var initialRecognitionRes = new InitialRecognitionResult();
                if (leaseFormData.CustomIRTable!=null)
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

                var (rouSchedule, fc_rouSchedule) = await _rouScheduleService.PostROUSchedule(request.TotalNPV, request.LeaseData);

                var (leaseLiability, fc_leaseLiability) = await _leaseLiabilityService.PostLeaseLiability(
                    request.TotalNPV,
                    initialRecognitionRes.CashFlow,
                    initialRecognitionRes.Dates,
                    leaseFormData
                );

                var journalEntries = await _journalEntriesService.PostJEForLease(leaseFormData, leaseLiability, rouSchedule);
                if(fc_rouSchedule.Count>0 && fc_leaseLiability.Count > 0)
                {
                    var fc_journalEntries = await _journalEntriesService.PostJEForLeaseforFC(leaseFormData, fc_leaseLiability, fc_rouSchedule);
                }
               

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
