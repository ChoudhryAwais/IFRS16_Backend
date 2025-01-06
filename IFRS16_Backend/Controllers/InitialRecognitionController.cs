using IFRS16_Backend.Models;
using IFRS16_Backend.Services.InitialRecognition;
using Microsoft.AspNetCore.Mvc;

namespace IFRS16_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InitialRecognitionController(IInitialRecognitionService intialRecognitionService) : ControllerBase
    {
        private readonly IInitialRecognitionService _intialRecognitionService = intialRecognitionService;

        [HttpPost("Add")]
        public async Task<ActionResult<InitialRecognitionResult>> PostInitialRecognitionForLease([FromBody] LeaseFormData leaseData)
        {
            try
            {
                var result = await _intialRecognitionService.PostInitialRecognitionForLease(leaseData);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet]
        public async Task<ActionResult<InitialRecognitionResult>> GetInitialRecognitionForLease([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] int leaseId = 0)
        {
            try
            {
                var result = await _intialRecognitionService.GetInitialRecognitionForLease(pageNumber, pageSize, leaseId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

    }
}
