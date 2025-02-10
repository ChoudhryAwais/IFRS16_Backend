using IFRS16_Backend.Models;
using IFRS16_Backend.Services.LeaseLiabilityAggregation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IFRS16_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaseReportController(ILeaseReportService leaseReportService) : ControllerBase
    {
        ILeaseReportService _leaseReportService = leaseReportService;

        [HttpPost]
        public async Task<ActionResult<LeaseLiabilityResult>> GetLeaseLiabilityForLease([FromBody] LeaseReportRequest request)
        {
            try
            {
                var result = await _leaseReportService.GetLeaseReport(request.StartDate, request.EndDate, request.LeaseIdList);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
