namespace IFRS16_Backend.Models
{
    public class LeaseReportSummaryTable
    {
        public decimal? OpeningLL { get; set; }
        public decimal? Interest { get; set; }
        public decimal? Payment { get; set; }
        public decimal? ClosingLL { get; set; }
        public decimal? OpeningROU { get; set; }
        public decimal? Amortization { get; set; }
        public decimal? ClosingROU { get; set; }
    }
}
