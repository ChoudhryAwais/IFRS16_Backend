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
                var leaseLiability = await _leaseLiabilityService.PostLeaseLiability(request.TotalNPV,request.CashFlow,request.Dates, request.LeaseData);
                return Ok(leaseLiability);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("{leaseId}")]
        public ActionResult<LeaseLiabilityTable> GetLeaseLiabilityForLease(int leaseId)
        {
            try
            {
                var result = _leaseLiabilityService.GetLeaseLiability(leaseId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

    }
}
