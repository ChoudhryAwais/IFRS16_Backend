using IFRS16_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace IFRS16_Backend.Services.Currencies
{
    public class CurrenciesService(ApplicationDbContext context) : ICurrenciesService
    {
        private readonly ApplicationDbContext _context = context;
        public async Task<List<CurrenciesTable>> GetAllCurrencies()
        {

            List<CurrenciesTable> currencies = await _context.Currencies.ToListAsync();
            return currencies;
        }
    }
}
