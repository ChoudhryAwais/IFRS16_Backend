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

        [HttpGet("{leaseId}")]
        public ActionResult<InitialRecognitionResult> GetInitialRecognitionForLease(int leaseId)
        {
            try
            {
                var leases = _intialRecognitionService.GetInitialRecognitionForLease(leaseId);
                return Ok(leases);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
         
        }

    }
}
