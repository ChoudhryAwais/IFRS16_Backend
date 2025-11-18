using System.ComponentModel.DataAnnotations;

namespace IFRS16_Backend.Models
{
    public class LeaseLiabilityTable
    {
        [Key]
        public int ID { get; set; }
        public int LeaseId { get; set; }
        public DateTime LeaseLiability_Date { get; set; }
        public double Opening { get; set; }
        public double Interest { get; set; }
        public double Payment { get; set; }
        public double Closing { get; set; }
        public double? Exchange_Gain_Loss { get; set; }
        public double? ModificationAdjustment { get; set; }
    }
    public class FC_LeaseLiabilityTable
    {
        [Key]
        public int ID { get; set; }
        public int LeaseId { get; set; }
        public DateTime LeaseLiability_Date { get; set; }
        public double Opening { get; set; }
        public double Interest { get; set; }
        public double Payment { get; set; }
        public double Closing { get; set; }

    }
    public class LeaseLiabilityRequest
    {
        public double TotalNPV { get; set; }
        public List<double> CashFlow { get; set; }
        public List<DateTime> Dates { get; set; }
        public LeaseFormData LeaseData { get; set; }
        public int ReportingCurrencyID { get; set; }
    }
    public class LeaseLiabilityResult
    {
        public IEnumerable<LeaseLiabilityTable> Data { get; set; }
        public int TotalRecords { get; set; }

    }
}
