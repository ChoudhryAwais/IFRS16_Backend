using IFRS16_Backend.Models;
using IFRS16_Backend.Services.SessionToken;
using Microsoft.AspNetCore.Mvc;

namespace IFRS16_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionTokenController(ISessionTokenService service) : ControllerBase
    {
        private readonly ISessionTokenService _service = service;

        [HttpPost("Upsert")]
        public async Task<IActionResult> Upsert([FromBody] UpsertRequest req)
        {
            if (req == null) return BadRequest("Invalid payload");
            if (req.UserId <= 0) return BadRequest("Invalid UserId");
            if (string.IsNullOrWhiteSpace(req.Token)) return BadRequest("Token required");

            await _service.UpsertSessionTokenAsync(req.UserId, req.Token);
            return Ok(new { message = "Session token upserted" });
        }
    }
}
