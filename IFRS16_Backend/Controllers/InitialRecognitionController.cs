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

        [HttpPost]
        public ActionResult<InitialRecognitionResult> GetInitialRecognitionForLease([FromBody] LeaseFormData leaseData)
        {
            try
            {
                var leases = _intialRecognitionService.GetInitialRecognitionForLease(leaseData);
                return Ok(leases);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
         
        }

    }
}
