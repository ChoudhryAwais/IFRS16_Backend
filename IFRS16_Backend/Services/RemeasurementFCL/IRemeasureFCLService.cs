using IFRS16_Backend.Models;

namespace IFRS16_Backend.Services.RemeasurementFCL
{
    public interface IRemeasureFCLService
    {
        Task<bool> RunBatchRemeasurementAsync(RemeasureFCRequest request);
    }
}
