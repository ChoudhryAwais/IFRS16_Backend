using System.ComponentModel.DataAnnotations;

namespace IFRS16_Backend.Models
{
    public class ROUScheduleTable
    {
        [Key]
        public int ID { get; set; }
        public int LeaseId { get; set; }
        public DateTime ROU_Date { get; set; }
        public double Opening { get; set; }
        public double Amortization { get; set; }
        public double Closing { get; set; }
        public double? ModificationAdjustment { get; set; }
    }
    public class FC_ROUScheduleTable
    {
        [Key]
        public int ID { get; set; }
        public int LeaseId { get; set; }
        public DateTime ROU_Date { get; set; }
        public double Opening { get; set; }
        public double Amortization { get; set; }
        public double Closing { get; set; }
    }
    public class ROUScheduleRequest
    {
        public LeaseFormData LeaseData { get; set; }
        public double TotalNPV { get; set; }
        public int ReportingCurrencyID { get; set; }
    }

    public class ROUScheduleResult
    {
        public IEnumerable<ROUScheduleTable> Data { get; set; }
        public int TotalRecords { get; set; }

    }
}
