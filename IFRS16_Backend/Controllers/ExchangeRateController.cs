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

        [HttpDelete("batch/{ids}")]
        public async Task<IActionResult> DeleteExchangeRates(string ids)
        {
            var idList = ids.Split(',')
                .Select(id => int.TryParse(id, out var val) ? val : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToList();

            if (idList.Count == 0)
                return BadRequest(new { error = "No valid IDs provided." });

            var result = await _exchangeRateService.DeleteExchangeRatesAsync(idList);
            if (!result)
                return NotFound(new { error = "No exchange rates found or could not be deleted." });

            return Ok(new { message = "Exchange rates deleted successfully." });
        }
    }
}
