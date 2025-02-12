using IFRS16_Backend.Models;
using IFRS16_Backend.Services.LeaseLiability;
using Microsoft.AspNetCore.Mvc;

namespace IFRS16_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaseLiabilityController(ILeaseLiabilityService leaseLiabilityService) : ControllerBase
    {

        private readonly ILeaseLiabilityService _leaseLiabilityService = leaseLiabilityService;

        [HttpPost("Add")]
        public async Task<ActionResult<LeaseLiabilityTable>> PostLeaseLiability([FromBody] LeaseLiabilityRequest request)
        {
            try
            {
                var leaseLiability = await _leaseLiabilityService.PostLeaseLiability(request.TotalNPV, request.CashFlow, request.Dates, request.LeaseData);
                return Ok(leaseLiability);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet]
        public async Task<ActionResult<LeaseLiabilityResult>> GetLeaseLiabilityForLease([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] int leaseId = 0)
        {
            try
            {
                var result = await _leaseLiabilityService.GetLeaseLiability(pageNumber, pageSize, leaseId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

    }
}
