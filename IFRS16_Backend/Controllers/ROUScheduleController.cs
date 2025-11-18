using IFRS16_Backend.Models;
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
                var rouSchedule = await _rouScheduleService.PostROUSchedule(request.TotalNPV, request.LeaseData, request.ReportingCurrencyID);
                return Ok(rouSchedule);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpPost("Get")]
        public async Task<ActionResult<ROUScheduleResult>> GetROUScheduleForLease([FromBody] GetLeaseDetails requestModal)
        {
            try
            {
                var result = await _rouScheduleService.GetROUSchedule(requestModal.PageNumber, requestModal.PageSize, requestModal.LeaseId, requestModal.StartDate, requestModal.EndDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("{leaseId}")]
        public async Task<ActionResult<ROUScheduleTable>> GetROUScheduleForLease(int leaseId)
        {
            try
            {
                var result = await _rouScheduleService.GetAllROUSchedule(leaseId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
