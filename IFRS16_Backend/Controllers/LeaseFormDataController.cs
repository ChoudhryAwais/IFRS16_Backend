using IFRS16_Backend.Models;
using IFRS16_Backend.Services.LeaseDataService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IFRS16_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaseFormDataController(ILeaseDataService leaseFormDataService) : ControllerBase
    {
        private readonly ILeaseDataService _leaseFormDataService = leaseFormDataService;


        [HttpGet("GetAllLeases")]
        public ActionResult<IEnumerable<LeaseFormData>> GetProducts()
        {
            var leases = _leaseFormDataService.GetAllLeases();
            return Ok(leases);
        }


        [HttpPost]
        public async Task<IActionResult> PostLeaseFormData([FromBody] LeaseFormData leaseFormData)
        {
            if (leaseFormData == null)
            {
                return BadRequest("LeaseFormData cannot be null.");
            }

            bool result = await _leaseFormDataService.AddLeaseFormDataAsync(leaseFormData);

            if (!result)
            {
                return StatusCode(500, "Internal server error while adding LeaseFormData.");
            }

            return CreatedAtAction(nameof(PostLeaseFormData), new { id = leaseFormData.LeaseId }, leaseFormData);
        }

    }
}
