using IFRS16_Backend.Models;
using IFRS16_Backend.Services.JournalEntries;
using IFRS16_Backend.Services.LeaseLiability;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IFRS16_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JournalEntriesController(IJournalEntriesService journalEntriesService) : ControllerBase
    {
        private readonly IJournalEntriesService _journalEntriesService = journalEntriesService;
        [HttpPost("Get")]
        public async Task<ActionResult<JournalEntryResult>> GetJournalEntriesForLease([FromBody] GetLeaseDetails requestModal)
        {
            try
            {
                var result = await _journalEntriesService.GetJEForLease(requestModal.PageNumber, requestModal.PageSize, requestModal.LeaseId, requestModal.StartDate, requestModal.EndDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("{leaseId}")]
        public async Task<ActionResult<JournalEntryTable>> GetAllJournalEntriesForLease(int leaseId)
        {
            try
            {
                var result = await _journalEntriesService.GetAllJEForLease(leaseId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }


    }
}
