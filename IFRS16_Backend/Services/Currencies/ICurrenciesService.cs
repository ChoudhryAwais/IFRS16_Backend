using IFRS16_Backend.Models;

namespace IFRS16_Backend.Services.Currencies
{
    public interface ICurrenciesService
    {
        Task<List<CurrenciesTable>> GetAllCurrencies();
    }
}
