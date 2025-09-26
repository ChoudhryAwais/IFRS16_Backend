using IFRS16_Backend.Models;
using IFRS16_Backend.Services.RemeasurementFCL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IFRS16_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RemeasureFCLeasesController(IRemeasureFCLService remeasureService) : ControllerBase
    {
        private readonly IRemeasureFCLService _remeasureService = remeasureService;

        [HttpPost]
        public async Task<IActionResult> RemeasureFC([FromBody] RemeasureFCRequest request)
        {
            try
            {
                if (request.RemeasurementDate == default)
                    return BadRequest(new { error = "Remeasurement Date is required." });

                var results = await _remeasureService.RunBatchRemeasurementAsync(request);
                return Ok(new
                {
                    message = "Batch remeasurement completed.",
                    status = results
                });

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
