using System.ComponentModel.DataAnnotations;

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
    }
}
