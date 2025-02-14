﻿using IFRS16_Backend.Models;
using IFRS16_Backend.Services.LeaseLiabilityAggregation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IFRS16_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaseReportController(ILeaseReportService leaseReportService) : ControllerBase
    {
        private readonly ILeaseReportService _leaseReportService = leaseReportService;
        [HttpPost("AllLeaseReport")]
        public async Task<ActionResult<AllLeasesReportTable>> GetLeasesReport([FromBody] LeaseReportRequest request)
        {
            try
            {
                var result = await _leaseReportService.GetAllLeaseReport(request.StartDate, request.EndDate);
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
                var result = await _leaseReportService.GetLeaseReportSummary(request.StartDate, request.EndDate, request.LeaseIdList);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
