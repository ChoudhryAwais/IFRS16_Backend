using System.Threading.Tasks;
using System.Collections.Generic;
using IFRS16_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace IFRS16_Backend.Services.ExchangeRate
{
    public class ExchangeRateService(ApplicationDbContext context) : IExchangeRateService
    {
        private readonly ApplicationDbContext _context = context;
        public async Task<List<ExchangeRateDto>> GetAllExchangeRatesByCurrencyIdAsync(int currencyId)
        {
            var exchangeRates = await _context.ExchangeRates
                .Where(x => x.CurrencyID == currencyId)
                .OrderByDescending(x => x.ExchangeDate)
                .ToListAsync();

            if (exchangeRates == null || exchangeRates.Count == 0)
                return new List<ExchangeRateDto>();

            var currency = await _context.Currencies
                .FirstOrDefaultAsync(x => x.CurrencyID == currencyId);

            var currencyName = currency?.CurrencyCode ?? string.Empty;

            return [.. exchangeRates.Select(er => new ExchangeRateDto
            {
                ExchangeRateID = er.ExchangeRateID,
                CurrencyID = er.CurrencyID,
                CurrencyName = currencyName,
                ExchangeRate = er.ExchangeRate,
                ExchangeDate = er.ExchangeDate
            })];
        }

        public async Task<bool> AddExchangeRateAsync(AddExchangeRateDto dto)
        {
            var entity = new ExchangeRateTable
            {
                CurrencyID = dto.CurrencyID,
                ExchangeRate = dto.ExchangeRate,
                ExchangeDate = dto.ExchangeDate
            };

            try
            {
                _context.ExchangeRates.Add(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
        public async Task<bool> DeleteExchangeRatesAsync(List<int> exchangeRateIds)
        {
            var entities = await _context.ExchangeRates
                .Where(x => exchangeRateIds.Contains(x.ExchangeRateID))
                .ToListAsync();

            if (entities.Count == 0)
                return false;

            _context.ExchangeRates.RemoveRange(entities);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
