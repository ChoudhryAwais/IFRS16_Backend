namespace IFRS16_Backend.Models
{
    public class LicenseTokenTable
    {
        public int Id { get; set; }          // Primary Key
        public int CompanyID { get; set; }
        public string ApplicationKey { get; set; }
        public string SecretKey { get; set; }
    }
    public class LicenseSettings
    {
        public string SecretKey { get; set; }
    }

    public class LicenseValidationResult
    {
        public bool Valid { get; set; }
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, object>? LicenseData { get; set; }
    }
}
