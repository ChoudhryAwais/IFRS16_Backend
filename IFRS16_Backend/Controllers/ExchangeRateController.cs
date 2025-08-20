using System.Threading.Tasks;
using IFRS16_Backend.Models;
using IFRS16_Backend.Services.ExchangeRate;
using Microsoft.AspNetCore.Mvc;

namespace IFRS16_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExchangeRateController(IExchangeRateService exchangeRateService) : ControllerBase
    {
        private readonly IExchangeRateService _exchangeRateService = exchangeRateService;

        [HttpGet("all/{currencyId}")]
        public async Task<IActionResult> GetAllExchangeRates(int currencyId)
        {
            var result = await _exchangeRateService.GetAllExchangeRatesByCurrencyIdAsync(currencyId);
            if (result == null || result.Count == 0)
                return NotFound(new { error = "No exchange rates found for the specified currency." });

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddExchangeRate([FromBody] AddExchangeRateDto dto)
        {
            var result = await _exchangeRateService.AddExchangeRateAsync(dto);
            if (!result)
                return BadRequest(new { error = "Failed to add exchange rate." });

            return Ok(new { message = "Exchange rate added successfully." });
        }
    }
}
