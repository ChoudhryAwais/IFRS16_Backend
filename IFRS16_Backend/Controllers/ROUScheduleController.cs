using IFRS16_Backend.Models;
using IFRS16_Backend.Services.InitialRecognition;
using IFRS16_Backend.Services.ROUSchedule;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IFRS16_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ROUScheduleController(IROUScheduleService rouScheduleService) : ControllerBase
    {
        private readonly IROUScheduleService _rouScheduleService = rouScheduleService;

        [HttpPost]
        public ActionResult<ROUScheduleTable> GetROUSchedule([FromBody] ROUScheduleRequest request)
        {
            try
            {
                var rouSchedule = _rouScheduleService.GetROUSchedule(request.TotalNPV, request.LeaseData);
                return Ok(rouSchedule);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
