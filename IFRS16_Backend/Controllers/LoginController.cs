using IFRS16_Backend.Models;
using IFRS16_Backend.Services.License;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IFRS16_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController(ApplicationDbContext context, IConfiguration configuration, LicenseService licenseService) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IConfiguration _configuration = configuration;
        private readonly LicenseService _licenseService = licenseService;

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] Login loginRequest)
        {
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.PasswordHash))
                return BadRequest("Invalid login request.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email && u.IsActive);
            if (user == null) return Unauthorized("Invalid credentials.");
            var companyProfile = await _context.CompanyProfile.FirstOrDefaultAsync(c => c.CompanyID == user.CompanyID);

            dynamic result = _licenseService.ValidateLicense(companyProfile.LicenseKey, companyProfile.CompanyID);

            if (result == null)
                return BadRequest(new { message = "License validation failed." });

            if (result.Valid)
            {
                if (user == null || user.PasswordHash != loginRequest.PasswordHash)
                    return Unauthorized("Invalid credentials.");

                // ✅ Step 1: If user already logged in, invalidate old token
                if (!string.IsNullOrEmpty(user.CurrentSessionToken))
                {
                    user.CurrentSessionToken = string.Empty; // force logout previous session
                    await _context.SaveChangesAsync();
                }

                var token = GenerateJwtToken(user);

                // ✅ Step 3: Save new session token
                user.CurrentSessionToken = token;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Token = token,
                    User = new
                    {
                        user.UserID,
                        user.Username,
                        user.Email,
                        user.PhoneNumber,
                        user.UserAddress,
                        user.CompanyID,
                        user.Role
                    },
                    CompanyProfile = companyProfile
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

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                // unique identifier for the token so each login produces a different token
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                // issued at time
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
