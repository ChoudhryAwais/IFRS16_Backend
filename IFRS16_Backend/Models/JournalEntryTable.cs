namespace IFRS16_Backend.Models
{
    public class JournalEntryTable
    {
        public int ID { get; set; }
        public int LeaseId { get; set; }
        public DateTime JE_Date { get; set; }
        public string Particular { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }

    }
    public class JournalEntryReport
    {
        public string? Particular { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }

    }

    public class FC_JournalEntryTable
    {
        public int ID { get; set; }
        public int LeaseId { get; set; }
        public DateTime JE_Date { get; set; }
        public string Particular { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }

    }
    public class JournalEntryResult
    {
        public IEnumerable<JournalEntryTable> Data { get; set; }
        public int TotalRecords { get; set; }

    }
}
