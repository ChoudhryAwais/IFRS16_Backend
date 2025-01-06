using Azure.Core;
using IFRS16_Backend.Models;
using IFRS16_Backend.Services.InitialRecognition;
using IFRS16_Backend.Services.LeaseData;
using IFRS16_Backend.Services.LeaseLiability;
using IFRS16_Backend.Services.ROUSchedule;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IFRS16_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaseFormDataController(
        ILeaseDataService leaseFormDataService,
        IInitialRecognitionService initialRecognitionService,
        ILeaseLiabilityService leaseLiabilityService,
        IROUScheduleService rOUScheduleService
        ) : ControllerBase
    {
        private readonly ILeaseDataService _leaseFormDataService = leaseFormDataService;
        private readonly IInitialRecognitionService _intialRecognitionService = initialRecognitionService;
        private readonly ILeaseLiabilityService _leaseLiabilityService = leaseLiabilityService;
        private readonly IROUScheduleService _rouScheduleService = rOUScheduleService;


        [HttpGet("GetAllLeases")]
        public async Task<ActionResult<IEnumerable<ExtendedLeaseDataSP>>> GetAllLeases([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var leases = await _leaseFormDataService.GetAllLeases(pageNumber, pageSize);
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
                bool result = await _leaseFormDataService.AddLeaseFormDataAsync(leaseFormData);
                try
                {
                    var initialRecognitionRes = await _intialRecognitionService.PostInitialRecognitionForLease(leaseFormData);
                    try
                    {
                        ROUScheduleRequest request = new()
                        {
                            TotalNPV = (double)initialRecognitionRes.TotalNPV,
                            LeaseData = leaseFormData

                        };
                        var rouSchedule = await _rouScheduleService.PostROUSchedule(request.TotalNPV, request.LeaseData);
                        try
                        {
                            var leaseLiability = await _leaseLiabilityService.PostLeaseLiability(request.TotalNPV, initialRecognitionRes.CashFlow, initialRecognitionRes.Dates, leaseFormData);
                            return CreatedAtAction(nameof(PostLeaseFormData), new { id = leaseFormData.LeaseId }, leaseFormData);

                        }
                        catch (Exception ex)
                        {
                            return BadRequest(ex.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.InnerException);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }


        }
    }
}
