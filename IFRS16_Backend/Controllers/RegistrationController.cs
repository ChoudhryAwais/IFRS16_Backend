using IFRS16_Backend.Models;
using IFRS16_Backend.Services.License;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IFRS16_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController(ApplicationDbContext context, LicenseService licenseService, IEncryptionHelper encryption) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IEncryptionHelper _encryption = encryption;
        private readonly LicenseService _licenseService = licenseService;

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Register(User user)
        {
            if (_context.Users.Any(u => u.Email == user.Email))
                return BadRequest("Username already exists.");

            user.CreatedAt = DateTime.UtcNow;
            _context.Users.Add(user);
            _context.SaveChanges();

            return CreatedAtAction(nameof(Register), new { id = user.UserID }, user);
        }

        [HttpPost("company")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterCompany(CompanyProfile company)
        {
            // create keys
            var (pub, priv) = KeyGenerator.GenerateRsaKeys();
            var encryptedPrivate = _encryption.EncryptBase64(priv);
            DateTime startdate = DateTime.Now;
            DateTime enddate = company.LicenseExpiry;

            string? license = _licenseService.GenerateLicense(company.Name, priv, company.AllowedUsers, startdate, enddate);
            //return license != null ? Ok(license) : NotFound("Company not found");
            CompanyProfile companyProfile = new()
            {
                Name = company.Name,
                RegistrationNumber = company.RegistrationNumber,
                ReportingCurrencyId = company.ReportingCurrencyId,
                ReportingCurrencyCode = company.ReportingCurrencyCode,
                FinancialYearEnd = company.FinancialYearEnd,
                LeaseTypes = company.LeaseTypes,
                AssetType = company.AssetType,
                LicenseKey = license ?? string.Empty, // Fix: assign empty string if license is null
            };

            await _context.CompanyProfile.AddAsync(companyProfile);
            await _context.SaveChangesAsync();

            ////Create LicenseToken record for this company
            var licenseToken = new LicenseTokenTable
            {
                CompanyID = companyProfile.CompanyID,
                ApplicationKey = pub,          // Public key
                SecretKey = encryptedPrivate   // Encrypted private key
            };

            //Add LicenseToken entry
            await _context.LicenseToken.AddAsync(licenseToken);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Register), new { id = company.CompanyID, licenseKey = pub }, company);
        }

        [HttpPost("validate-license/{companyId}")]
        [AllowAnonymous]
        public IActionResult ValidateLicense(int companyId, [FromBody] string license)
        {
            dynamic result = _licenseService.ValidateLicense(license, companyId);

            if (result == null)
                return BadRequest(new { message = "License validation failed." });

            if (result.Valid)
            {
                return Ok(new
                {
                    message = result.Message,
                    data = result.LicenseData
                });
            }
            else
            {
                return BadRequest(new
                {
                    message = result.Message
                });
            }
        }
    }
}
