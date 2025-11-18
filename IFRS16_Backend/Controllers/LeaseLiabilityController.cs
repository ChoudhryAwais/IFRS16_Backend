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
                var leaseLiability = await _leaseLiabilityService.PostLeaseLiability(request.TotalNPV, request.CashFlow, request.Dates, request.LeaseData, request.ReportingCurrencyID);
                return Ok(leaseLiability);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost("Get")]
        public async Task<ActionResult<LeaseLiabilityResult>> GetLeaseLiabilityForLease([FromBody] GetLeaseDetails requestModal)
        {
            try
            {
                var result = await _leaseLiabilityService.GetLeaseLiability(requestModal.PageNumber, requestModal.PageSize, requestModal.LeaseId, requestModal.StartDate, requestModal.EndDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("{leaseId}")]
        public async Task<ActionResult<LeaseLiabilityTable>> GetAllLeaseLiabilityForLease(int leaseId)
        {
            try
            {
                var result = await _leaseLiabilityService.GetAllLeaseLiability(leaseId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

    }
}
