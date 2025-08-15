namespace IFRS16_Backend.Models
{
    public class DisclosureTable
    {
        public double? OpeningLL { get; set; }  // Nullable in case there's no record
        public double? Interest { get; set; }
        public double? Payment { get; set; }
        public double? ClosingLL { get; set; }
        public double? OpeningROU { get; set; }
        public double? Amortization { get; set; }
        public double? ClosingROU { get; set; }
        public double? Exchange_Gain_Loss { get; set; }
        public double? ModificationAdjustmentLL { get; set; }
        public double? ModificationAdjustmentROU { get; set; }
        public double? AdditionsDuringYearLL { get; set; }
        public double? AdditionsDuringYearROU { get; set; }

    }
    public class DisclouserMaturityAnalysisTable
    {
        public decimal? LessThanOneYear { get; set; }
        public decimal? BetweenOneAndFiveYears { get; set; }
        public decimal? AfterFiveYears { get; set; }
    }

}
