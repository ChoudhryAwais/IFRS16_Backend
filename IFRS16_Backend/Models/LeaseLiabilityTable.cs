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
    }
    public class LeaseLiabilityRequest
    {
        public double TotalNPV { get; set; }
        public List<double> CashFlow { get; set; }
        public List<DateTime> Dates { get; set; }
        public LeaseFormData LeaseData { get; set; }
    }
    public class LeaseLiabilityResult
    {
        public IEnumerable<LeaseLiabilityTable> Data { get; set; }
        public int TotalRecords { get; set; }

    }
}
