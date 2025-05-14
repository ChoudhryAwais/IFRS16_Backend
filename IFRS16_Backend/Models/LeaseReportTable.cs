namespace IFRS16_Backend.Models
{
    public class LeaseReportTable
    {
        public IEnumerable<LeaseLiabilityAggregationTable> LeaseLiabilityAggregation { get; set; }
        public IEnumerable<ROUAggregationTable> ROUAggregation { get; set; }
    }
    public class LeaseReportRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? LeaseIdList { get; set; }
        public int CompanyId { get; set; }
    }
}
