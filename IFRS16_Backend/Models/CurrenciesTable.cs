using System.ComponentModel.DataAnnotations;

namespace IFRS16_Backend.Models
{
    public class CurrenciesTable
    {
        [Key]
        public int CurrencyID { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
    }
}
