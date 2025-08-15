using IFRS16_Backend.Models;
using IFRS16_Backend.Services.Report;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IFRS16_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisclosureController(IReportsService leaseReportService) : ControllerBase
    {
        private readonly IReportsService _leaseReportService = leaseReportService;
        [HttpPost("Get")]
        public async Task<ActionResult<DisclosureTable>> GetDisclosure([FromBody] LeaseReportRequest request)
        {
            try
            {
                var result = await _leaseReportService.GetDisclosure(request.StartDate, request.EndDate, request.CompanyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost("GetDisclouserMaturityAnalysis")]
        public async Task<ActionResult<DisclouserMaturityAnalysisTable>> GetDisclouserMaturityAnalysis([FromBody] LeaseReportRequest request)
        {
            try
            {
                var result = await _leaseReportService.GetDisclouserMaturityAnalysis(request.StartDate, request.EndDate, request.CompanyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
