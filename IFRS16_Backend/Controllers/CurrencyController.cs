using IFRS16_Backend.Models;
using IFRS16_Backend.Services.Currencies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IFRS16_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController(ICurrenciesService currencyService) : ControllerBase
    {
        private ICurrenciesService _currencyService= currencyService;

        [HttpGet("GetAllCurrencies")]
        public async Task<ActionResult<List<CurrenciesTable>>> GetAllCurrencies()
        {
            try
            {
                var currencies = await _currencyService.GetAllCurrencies();
                return Ok(currencies);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
