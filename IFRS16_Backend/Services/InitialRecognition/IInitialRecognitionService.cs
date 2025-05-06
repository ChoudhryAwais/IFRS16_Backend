using IFRS16_Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace IFRS16_Backend.Services.InitialRecognition
{
    public interface IInitialRecognitionService
    {
        Task<InitialRecognitionResult> PostInitialRecognitionForLease(LeaseFormData leaseSpecificData);
        Task<InitialRecognitionResult> PostCustomInitialRecognitionForLease(LeaseFormData leaseSpecificData);

        Task<InitialRecognitionResult> ModifyInitialRecognitionForLease(LeaseFormModification leaseSpecificData);
        Task<InitialRecognitionResult> GetInitialRecognitionForLease(int pageNumber, int pageSize, int leaseId, DateTime? startDate, DateTime? endDate);
        Task<List<InitialRecognitionTable>> GetAllInitialRecognitionForLease(int leaseId);

    }
}
