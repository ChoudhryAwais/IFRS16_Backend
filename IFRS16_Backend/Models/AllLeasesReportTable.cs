namespace IFRS16_Backend.Models
{
    public class AllLeasesReportTable
    {
        public int LeaseId { get; set; }
        public string LeaseName { get; set; }
        public double Rental { get; set; }
        public string Frequency { get; set; }
        public DateTime CommencementDate { get; set; }
        public DateTime EndDate { get; set; }

        public double? OpeningLL { get; set; }  // Nullable in case there's no record
        public double? Interest { get; set; }
        public double? Payment { get; set; }
        public double? ClosingLL { get; set; }

        public double? OpeningROU { get; set; }
        public double? Amortization { get; set; }
        public double? ClosingROU { get; set; }
        public double? Exchange_Gain_Loss { get; set; }
        public double? ModificationAdjustment { get; set; }
        public int? CurrencyID { get; set; }
        public string? CurrencyCode { get; set; }



    }
}
