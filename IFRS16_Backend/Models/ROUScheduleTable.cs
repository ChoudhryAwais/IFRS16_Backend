namespace IFRS16_Backend.Models
{
    public class ROUScheduleTable
    {
        public DateTime Date { get; set; }
        public double Opening { get; set; }
        public double Amortization { get; set; }
        public double Closing { get; set; }
    }
    public class ROUScheduleRequest
    {
        public LeaseFormData LeaseData { get; set; }
        public double TotalNPV { get; set; }
    }
}
