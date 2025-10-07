using System.Threading.Tasks;
using System.Collections.Generic;
using IFRS16_Backend.Models;

namespace IFRS16_Backend.Services.ExchangeRate
{
    public interface IExchangeRateService
    {
        Task<List<ExchangeRateDto>> GetAllExchangeRatesByCurrencyIdAsync(int currencyId);
        // Add this method for adding a new exchange rate
        Task<bool> AddExchangeRateAsync(AddExchangeRateDto dto);
        // Add this method for batch deleting exchange rates by their IDs
        Task<bool> DeleteExchangeRatesAsync(List<int> exchangeRateIds);
    }
}
