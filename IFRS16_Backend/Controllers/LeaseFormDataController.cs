using Azure.Core;
using IFRS16_Backend.Models;
using IFRS16_Backend.Services.LeaseData;
using IFRS16_Backend.Services.LeaseDataWorkflow;
using Microsoft.AspNetCore.Mvc;

namespace IFRS16_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaseFormDataController(
        ILeaseDataService leaseFormDataService,
        ILeaseDataWorkflowService leaseDataWorkflowService
        ) : ControllerBase
    {
        private readonly ILeaseDataService _leaseFormDataService = leaseFormDataService;
        private readonly ILeaseDataWorkflowService _leaseDataWorkflowService = leaseDataWorkflowService;


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
            try
            {
                foreach (var lease in leaseFormData)
                {
                    await _leaseDataWorkflowService.ProcessLeaseFormDataAsync(lease);
                }

                return CreatedAtAction(nameof(PostLeaseFormData), new { count = leaseFormData.Count }, leaseFormData);
            }
            catch (Exception ex)
            {
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
    }
}
