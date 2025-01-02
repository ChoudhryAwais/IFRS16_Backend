﻿using IFRS16_Backend.Models;
using IFRS16_Backend.Services.ROUSchedule;
using Microsoft.AspNetCore.Mvc;

namespace IFRS16_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ROUScheduleController(IROUScheduleService rouScheduleService) : ControllerBase
    {
        private readonly IROUScheduleService _rouScheduleService = rouScheduleService;

        [HttpPost("Add")]
        public async Task<ActionResult<ROUScheduleTable>> PostROUScheduleForLease([FromBody] ROUScheduleRequest request)
        {
            try
            {
                var rouSchedule = await _rouScheduleService.PostROUSchedule(request.TotalNPV,request.LeaseData);
                return Ok(rouSchedule);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpGet("{leaseId}")]
        public ActionResult<InitialRecognitionResult> GetROUScheduleForLease(int leaseId)
        {
            try
            {
                var result = _rouScheduleService.GetROUSchedule(leaseId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
