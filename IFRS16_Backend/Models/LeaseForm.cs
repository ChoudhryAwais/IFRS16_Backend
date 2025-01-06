using System.ComponentModel.DataAnnotations;

namespace IFRS16_Backend.Models
{
    public class LeaseFormData
    {
        [Key]
        public int LeaseId { get; set; }
        public int UserID { get; set; }
        public string LeaseName { get; set; }
        public long Rental { get; set; }
        public DateTime CommencementDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Annuity { get; set; }
        public int IBR { get; set; }
        public string Frequency { get; set; }
    }
    public class LeaseFormDataResult
    {
        public IEnumerable<ExtendedLeaseDataSP> Data { get; set; }
        public int TotalRecords { get; set; }

    }
    public class ExtendedLeaseDataSP
    {
        public int LeaseId { get; set; }
        public int UserID { get; set; }
        public string LeaseName { get; set; }
        public long Rental { get; set; }
        public DateTime CommencementDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Annuity { get; set; }
        public int IBR { get; set; }
        public string Frequency { get; set; }
        public string Username { get; set; }
    }
}
