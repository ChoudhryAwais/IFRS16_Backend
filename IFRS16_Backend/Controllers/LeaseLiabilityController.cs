using IFRS16_Backend.Models;
using IFRS16_Backend.Services.LeaseLiability;
using IFRS16_Backend.Services.ROUSchedule;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IFRS16_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaseLiabilityController(ILeaseLiabilityService leaseLiabilityService) : ControllerBase
    {

        private readonly ILeaseLiabilityService _leaseLiabilityService = leaseLiabilityService;

        [HttpPost]
        public ActionResult<LeaseLiabilityTable> GetLeaseLiability([FromBody] LeaseLiabilityRequest request)
        {
            try
            {
                var leaseLiability = _leaseLiabilityService.GetLeaseLiability(request.TotalNPV,request.CashFlow,request.Dates, request.LeaseData);
                return Ok(leaseLiability);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

    }
}
