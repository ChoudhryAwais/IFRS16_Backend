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
        [HttpGet]
        public async Task<ActionResult<JournalEntryResult>> GetJournalEntriesForLease([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] int leaseId = 0)
        {
            try
            {
                var result = await _journalEntriesService.GetJEForLease(pageNumber, pageSize, leaseId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
