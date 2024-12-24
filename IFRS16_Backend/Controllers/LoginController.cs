using IFRS16_Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IFRS16_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController(ApplicationDbContext context, IConfiguration configuration) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IConfiguration _configuration = configuration;

        [HttpPost]
        public IActionResult Login([FromBody] Login loginRequest)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == loginRequest.Email && u.IsActive==true);
            if (user == null || user.PasswordHash != loginRequest.PasswordHash)
                return Unauthorized("   ");

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                Token = token,
                User = (new
                {
                    user.UserID,
                    user.Username,
                    user.Email,
                    user.PhoneNumber,
                    user.UserAddress,
                })
            });
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity([
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString())
                ]),
                Expires = DateTime.UtcNow.AddHours(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
