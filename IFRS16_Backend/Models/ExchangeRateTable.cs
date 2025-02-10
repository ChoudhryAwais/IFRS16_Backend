using System.ComponentModel.DataAnnotations;

namespace IFRS16_Backend.Models
{
    public class ExchangeRateTable
    {

        [Key]
        public int ExchangeRateID { get; set; }
        public int CurrencyID { get; set; }
        public Decimal ExchangeRate { get; set; }
        public DateTime ExchangeDate { get; set; }
    }
    public class ExchangeRateDTO
    {
        public DateTime ExchangeDate { get; set; }
        public decimal ExchangeRate { get; set; }
    }
}
