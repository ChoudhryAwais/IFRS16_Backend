using System.ComponentModel.DataAnnotations;

namespace IFRS16_Backend.Models
{
    public class InitialRecognitionTable
    {
        [Key]
        public int ID { get; set; }
        public int LeaseId { get; set; }
        public int SerialNo { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Rental { get; set; }
        public decimal NPV { get; set; }
        public bool IsActive { get; set; }
    }

    public class CustomInitialRecognitionTable
    {
        [Key]
        public int SerialNo { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Rental { get; set; }
    }

    public class InitialRecognitionResult
    {
        public decimal TotalNPV { get; set; }
        public IEnumerable<InitialRecognitionTable> InitialRecognition { get; set; }
        public List<double>? CashFlow { get; set; }
        public List<DateTime>? Dates { get; set; }
        public int TotalRecords { get; set; }

    }
}
