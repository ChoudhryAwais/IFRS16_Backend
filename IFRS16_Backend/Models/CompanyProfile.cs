using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IFRS16_Backend.Models
{
    public class CompanyProfile
    {
        [Key]
        public int CompanyID { get; set; } // Auto-incremented unique identifier for the company
        public string Name { get; set; } // Company name
        public string RegistrationNumber { get; set; } // Registration number
        public int ReportingCurrencyId { get; set; } // Reporting currency (e.g., USD, EUR)
        public string ReportingCurrencyCode { get; set; } // Reporting currency (e.g., USD, EUR)
        public DateTime FinancialYearEnd { get; set; } // Financial year-end date
        public string? LeaseTypes { get; set; } // Lease types (e.g., operating, finance)
        public string AssetType { get; set; }
        public string? LicenseKey { get; set; } // Timestamp of when the company profile was created
        [NotMapped]
        public DateTime LicenseExpiry { get; set; } // Timestamp of when the company profile was created
        [NotMapped]
        public int AllowedUsers { get; set; } // Number of users allowed for this company 
    }
}
