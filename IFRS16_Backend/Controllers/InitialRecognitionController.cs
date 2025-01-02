﻿using IFRS16_Backend.Models;
using IFRS16_Backend.Services.InitialRecognition;
using Microsoft.AspNetCore.Mvc;

namespace IFRS16_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InitialRecognitionController(IInitialRecognitionService intialRecognitionService) : ControllerBase
    {
        private readonly IInitialRecognitionService _intialRecognitionService = intialRecognitionService;

        [HttpPost("Add")]
        public async Task<ActionResult<InitialRecognitionResult>> PostInitialRecognitionForLease([FromBody] LeaseFormData leaseData)
        {
            try
            {
                var result = await _intialRecognitionService.PostInitialRecognitionForLease(leaseData);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
         
        }

        [HttpGet("{leaseId}")]
        public async Task<ActionResult<InitialRecognitionResult>> GetInitialRecognitionForLease(int leaseId)
        {
            try
            {
                var result = await _intialRecognitionService.GetInitialRecognitionForLease(leaseId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

    }
}
