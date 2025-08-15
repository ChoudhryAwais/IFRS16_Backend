using IFRS16_Backend.Models;
using IFRS16_Backend.Services.Report;
using Microsoft.AspNetCore.Mvc;

namespace IFRS16_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController(IReportsService leaseReportService) : ControllerBase
    {
        private readonly IReportsService _leaseReportService = leaseReportService;
        [HttpPost("AllLeaseReport")]
        public async Task<ActionResult<AllLeasesReportTable>> GetLeasesReport([FromBody] LeaseReportRequest request)
        {
            try
            {
                var result = await _leaseReportService.GetAllLeaseReport(request.StartDate, request.EndDate, request.CompanyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpPost("LeaseReportSummary")]
        public async Task<ActionResult<AllLeasesReportTable>> GetLeasesReportSummary([FromBody] LeaseReportRequest request)
        {
            try
            {
                var result = await _leaseReportService.GetLeaseReportSummary(request.StartDate, request.EndDate, request.LeaseIdList, request.CompanyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpPost("JournalEntryReport")]
        public async Task<ActionResult<JournalEntryReport>> GetJEReport([FromBody] LeaseReportRequest request)
        {
            try
            {
                var result = await _leaseReportService.GetJEReport(request.StartDate, request.EndDate, request.CompanyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
