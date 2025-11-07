using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IFRS16_Backend.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace IFRS16_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogoutController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return BadRequest(new { error = "Invalid user token." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
            if (user == null)
                return NotFound(new { error = "User not found." });

            user.CurrentSessionToken = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Logged out successfully." });
        }
    }
}
