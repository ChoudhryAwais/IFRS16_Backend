﻿using Azure.Core;
using IFRS16_Backend.Models;
using IFRS16_Backend.Services.LeaseData;
using IFRS16_Backend.Services.LeaseDataWorkflow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IFRS16_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaseFormDataController(
        ILeaseDataService leaseFormDataService,
        ILeaseDataWorkflowService leaseDataWorkflowService,
         ApplicationDbContext context
        ) : ControllerBase
    {
        private readonly ILeaseDataService _leaseFormDataService = leaseFormDataService;
        private readonly ILeaseDataWorkflowService _leaseDataWorkflowService = leaseDataWorkflowService;
        private readonly ApplicationDbContext _context = context;


        [HttpGet("GetAllLeasesForCompany")]
        public async Task<ActionResult<List<LeaseFormData>>> GetAllLeasesForCompany([FromQuery] int companyId = 1)
        {
            try
            {
                var leases = await _leaseFormDataService.GetAllLeasesForCompany(companyId);
                return Ok(leases);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GetAllLeases")]
        public async Task<ActionResult<IEnumerable<ExtendedLeaseDataSP>>> GetAllLeases([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] int companyID = 1)
        {
            try
            {
                var leases = await _leaseFormDataService.GetAllLeases(pageNumber, pageSize, companyID);
                return Ok(leases);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GetLeaseById/{leaseId}")]
        public async Task<ActionResult<LeaseFormData>> GetLeaseyId(int leaseId)
        {
            try
            {
                var leases = await _leaseFormDataService.GetLeaseById(leaseId);
                return Ok(leases);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> PostLeaseFormData([FromBody] LeaseFormData leaseFormData)
        {
            try
            {
                await _leaseDataWorkflowService.ProcessLeaseFormDataAsync(leaseFormData);
                return CreatedAtAction(nameof(PostLeaseFormData), new { id = leaseFormData.LeaseId }, leaseFormData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("BulkImport")]
        public async Task<IActionResult> PostBulkLeaseFormData([FromBody] List<LeaseFormData> leaseFormData)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var lease in leaseFormData)
                {
                    try
                    {
                        await _leaseDataWorkflowService.ProcessLeaseFormDataAsync(lease);
                    }
                    catch (Exception ex)
                    {
                        // Log the error for the specific lease
                        Console.WriteLine($"Error processing lease {lease.LeaseId}: {ex.Message}");
                        throw; // Optionally stop further processing
                    }
                }

                await transaction.CommitAsync();
                return CreatedAtAction(nameof(PostLeaseFormData), new { count = leaseFormData.Count }, leaseFormData);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("Delete")]
        public async Task<IActionResult> DeleteLeaseData([FromBody] DeleteLeaseData deleteReq)
        {
            try
            {
                await _leaseFormDataService.DeleteLeases(deleteReq.LeaseIds);
                return Ok(new { status = 200 }); // 200 OK with a message
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("TerminateLease")]
        public async Task<IActionResult> TerminateLeaseData([FromBody] TerminateLease leaseTerminate)
        {

            var result = await _leaseFormDataService.TerminateLease(leaseTerminate);
            if (result == true)
            {
                return Ok(new { status = 200 }); // 200 OK with a message
            }
            else
            {
                return BadRequest();
            }

        }
        [HttpPost("ModifyLease")]
        public async Task<IActionResult> ModifyLeaseData([FromBody] LeaseFormData leaseModification)
        {
            try
            {
                await _leaseDataWorkflowService.ModificationLeaseFormDataAsync(leaseModification);
                return CreatedAtAction(nameof(PostLeaseFormData), new { id = leaseModification.LeaseId }, leaseModification);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost]
        [Route("UploadLeaseContract")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadLeaseContract([FromForm] LeaseContractDto dto)
        {
            try
            {
                await _leaseFormDataService.UploadLeaseContractAsync(dto.LeaseId, dto.ContractDoc);
                return Ok(new { message = "Lease contract uploaded successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("GetLeaseContract/{leaseId}")]
        public async Task<IActionResult> GetLeaseContract(int leaseId)
        {
            var leaseContract = await _leaseFormDataService.GetLeaseContractByLeaseIdAsync(leaseId);

            if (leaseContract == null || leaseContract.ContractDoc == null)
            {
                return NotFound();
            }

            return File(leaseContract.ContractDoc, "application/pdf", $"LeaseContract_{leaseId}.pdf");
        }



    }
}
