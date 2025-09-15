using IFRS16_Backend.Models;
using IFRS16_Backend.Services.UserCrud;
using Microsoft.AspNetCore.Mvc;

namespace IFRS16_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        [HttpPut("Update")]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDto dto)
        {
            var result = await _userService.UpdateUserAsync(dto);
            if (!result)
                return NotFound(new { error = "User not found or update failed." });

            return Ok(new {status=true, message = "User updated successfully." });
        }

        [HttpPost("VerifyPassword")]
        public async Task<IActionResult> VerifyPassword([FromBody] UserPasswordDto dto)
        {
            var isValid = await _userService.VerifyPasswordAsync(dto.UserId, dto.Password);

            return Ok(new { isValid });
        }
    }
}
