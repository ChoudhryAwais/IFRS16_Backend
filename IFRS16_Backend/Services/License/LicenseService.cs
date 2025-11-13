using IFRS16_Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace IFRS16_Backend.Services.License
{
    public class LicenseData
    {
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int NumberOfUsersAllowed { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
    public class LicenseService(IEncryptionHelper encryption, ApplicationDbContext context)
    {
        private readonly ApplicationDbContext _context = context;

        // Generate License
        public string? GenerateLicense(string companyName, string privateKey, int noOfUsersAllowed, DateTime issueDate, DateTime expiryDate)
        {
            using var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);

            // License information
            var licenseData = new
            {
                CompanyName = companyName,
                LicenseIssueDate = issueDate.ToString("O"),
                LicenseExpiry = expiryDate.ToString("O"),
                NoOfUsersAllowed = noOfUsersAllowed
            };

            // Convert license data to JSON for easier parsing
            var licenseJson = System.Text.Json.JsonSerializer.Serialize(licenseData);
            var dataBytes = Encoding.UTF8.GetBytes(licenseJson);

            // Sign data
            var signature = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            // Combine data + signature
            var combined = new
            {
                LicenseData = licenseData,
                Signature = Convert.ToBase64String(signature)
            };

            // Encode entire license
            var finalLicense = System.Text.Json.JsonSerializer.Serialize(combined);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(finalLicense));
        }

        // Validate License
        public LicenseValidationResult ValidateLicense(string license, int companyId)
        {
            var licenseToken = _context.LicenseToken.FirstOrDefault(x => x.CompanyID == companyId);
            if (licenseToken == null)
                return new LicenseValidationResult { Valid = false, Message = "Company not found" };

            var publicKey = Convert.FromBase64String(licenseToken.ApplicationKey);

            // Decode base64
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(license));

            // Deserialize the license content
            var licenseObj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(decoded);
            if (licenseObj == null || !licenseObj.TryGetValue("LicenseData", out object? value) || !licenseObj.TryGetValue("Signature", out object? value1))
                return new LicenseValidationResult { Valid = false, Message = "Invalid license format" };

            var licenseDataJson = value.ToString();
            var signature = Convert.FromBase64String(value1.ToString());

            var dataBytes = Encoding.UTF8.GetBytes(licenseDataJson);

            // Verify signature
            using var rsa = RSA.Create();
            rsa.ImportRSAPublicKey(publicKey, out _);
            bool isValidSignature = rsa.VerifyData(dataBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            if (!isValidSignature)
                return new LicenseValidationResult { Valid = false, Message = "License signature is invalid" };

            // Deserialize the license data
            var licenseData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(licenseDataJson);
            if (licenseData == null)
                return new LicenseValidationResult { Valid = false, Message = "License data invalid" };

            // Check expiry date
            if (DateTime.TryParse(licenseData["LicenseExpiry"].ToString(), out var expiryDate))
            {
                if (expiryDate < DateTime.UtcNow)
                    return new LicenseValidationResult { Valid = false, Message = "License has expired", LicenseData = licenseData };
            }

            // Everything is valid
            return new LicenseValidationResult
            {
                Valid = true,
                Message = "License is valid",
                LicenseData = licenseData
            };
        }
    }
}
