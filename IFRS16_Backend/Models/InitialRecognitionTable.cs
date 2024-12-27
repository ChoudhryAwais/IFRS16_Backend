namespace IFRS16_Backend.Models
{
    public class InitialRecognitionTable
    {
        public int SerialNo { get; set; }
        public string PaymentDate { get; set; }
        public decimal Rental { get; set; }
        public decimal NPV { get; set; }
    }

    public class InitialRecognitionResult
    {
        public decimal TotalNPV { get; set; }
        public List<InitialRecognitionTable> InitialRecognition { get; set; }
        public List<double> CashFlow { get; set; }
        public List<string> Dates { get; set; }
    }
}
