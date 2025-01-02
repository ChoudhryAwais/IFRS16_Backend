using IFRS16_Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace IFRS16_Backend.Services.InitialRecognition
{
    public interface IInitialRecognitionService
    {
        Task<InitialRecognitionResult> PostInitialRecognitionForLease(LeaseFormData leaseSpecificData);
        Task<InitialRecognitionResult> GetInitialRecognitionForLease(int leaseId);

    }
}
