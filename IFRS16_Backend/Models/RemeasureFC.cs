namespace IFRS16_Backend.Models
{
    public class RemeasureFCRequest
    {
        public int CurrencyID { get; set; }
        public DateTime RemeasurementDate { get; set; }
        public int ReportingCurrencyID { get; set; }
    }
}
