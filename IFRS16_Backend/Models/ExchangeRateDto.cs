namespace IFRS16_Backend.Models
{
    public class ExchangeRateDto
    {
        public int ExchangeRateID { get; set; }
        public int CurrencyID { get; set; }
        public string CurrencyName { get; set; }
        public decimal ExchangeRate { get; set; }
        public DateTime ExchangeDate { get; set; }

    }

    public class AddExchangeRateDto
    {
        public int CurrencyID { get; set; }
        public decimal ExchangeRate { get; set; }
        public DateTime ExchangeDate { get; set; }
    }

}

