using IFRS16_Backend.Models;

namespace IFRS16_Backend.Services.InitialRecognition
{
    public interface IInitialRecognitionService
    {
        InitialRecognitionResult GetInitialRecognitionForLease(int leaseId);
    }
}
