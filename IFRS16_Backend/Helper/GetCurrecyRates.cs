using IFRS16_Backend.enums;
using IFRS16_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace IFRS16_Backend.Helper
{

    public class GetCurrecyRates(ApplicationDbContext context)
    {
        private readonly ApplicationDbContext _context = context;
        public List<ExchangeRateDTO> GetListOfExchangeRates(LeaseFormData leaseData)
        {
            CompanyProfile companyProfile = _context.CompanyProfile.FirstOrDefault(profile => profile.CompanyID == leaseData.CompanyID);
            if (companyProfile?.ReportingCurrencyId != leaseData?.CurrencyID)
            {
                List<ExchangeRateDTO> exchangeRates = [.. _context.ExchangeRates
                                        .Where(rate => rate.CurrencyID == leaseData.CurrencyID && rate.ExchangeDate >= leaseData.CommencementDate && rate.ExchangeDate <= leaseData.EndDate)
                                        .Select(rate => new ExchangeRateDTO { ExchangeDate = rate.ExchangeDate, ExchangeRate = rate.ExchangeRate })];
                if (exchangeRates.Count == 0)
                {
                    exchangeRates = [.. _context.ExchangeRates
                                        .Where(rate => rate.CurrencyID == leaseData.CurrencyID)
                                        .Select(rate => new ExchangeRateDTO { ExchangeDate = rate.ExchangeDate, ExchangeRate = rate.ExchangeRate })
                                        .OrderBy(rate => rate.ExchangeDate)
                                        ];
                    return exchangeRates;
                }

                return exchangeRates;
            }
            return [];
        }
    }
}
