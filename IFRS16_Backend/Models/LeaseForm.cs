using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IFRS16_Backend.Models
{
    public class LeaseFormData
    {
        [Key]
        public int LeaseId { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string LeaseName { get; set; }
        public double Rental { get; set; }
        public DateTime CommencementDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Annuity { get; set; }
        public double IBR { get; set; }
        public string Frequency { get; set; }
        public double? IDC { get; set; }
        public double? GRV { get; set; }
        public double? Increment { get; set; }
        public string? IncrementalFrequency { get; set; }
        public int CompanyID { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string AssetType { get; set; }
        public int CurrencyID { get; set; }
        [NotMapped]
        public double? RouOpening { get; set; }
        [NotMapped]
        public double? LLOpening { get; set; }
        [NotMapped]
        public decimal? RouExRate { get; set; }
        [NotMapped]
        public List<CustomInitialRecognitionTable>? CustomIRTable { get; set; }
        [NotMapped]
        public bool IsChangeInScope { get; set; }
        [NotMapped]
        public int ReportingCurrencyID { get; set; }

    }

    public class LeaseFormModification
    {
        [Key]
        public int LeaseId { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string LeaseName { get; set; }
        public double Rental { get; set; }
        public DateTime ModificationDate { get; set; }
        public DateTime CommencementDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Annuity { get; set; }
        public double IBR { get; set; }
        public string Frequency { get; set; }
        public double? IDC { get; set; }
        public double? GRV { get; set; }
        public double? Increment { get; set; }
        public string? IncrementalFrequency { get; set; }
        public int CompanyID { get; set; }
        public int CurrencyID { get; set; }
        [NotMapped]
        public double? RouOpening { get; set; }
        [NotMapped]
        public decimal? RouExRate { get; set; }

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
        public double Rental { get; set; }
        public DateTime CommencementDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Annuity { get; set; }
        public double IBR { get; set; }
        public string Frequency { get; set; }
        public string Username { get; set; }
        public double? IDC { get; set; }
        public double? GRV { get; set; }
        public double? Increment { get; set; }
        public int CompanyID { get; set; }
        public string CurrencyCode { get; set; }
        public int CurrencyID { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string AssetType { get; set; }

    }
    public class DeleteLeaseData
    {
        public string LeaseIds { get; set; }
    }
    public class GetLeaseDetails
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int LeaseId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    public class TerminateLease
    {
        public DateTime TerminateDate { get; set; }
        public decimal? Penalty { get; set; }
        public int LeaseId { get; set; }

    }
    public class GetLeaseFormData
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int CompanyId { get; set; }
        public string? LeaseName { get; set; }
    }
    public class LeaseContract
    {
        [Key]
        public int LeaseContractId { get; set; }
        public int LeaseId { get; set; }
        public byte[] ContractDoc { get; set; }
        public string DocFileName { get; set; }
        public string ContentType { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
    public class LeaseContractDto
    {
        public int LeaseId { get; set; }
        public IFormFile ContractDoc { get; set; } // file from frontend
    }
}
