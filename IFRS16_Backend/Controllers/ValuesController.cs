using IFRS16_Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IFRS16_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet("getdummyusers")]
        public IActionResult GetDummyUsers()
        {
            var users = new List<User>
            {
                new User
                {
                    UserID = 1,
                    Username = "john_doe",
                    PasswordHash = "hashedpassword1",
                    PhoneNumber = "123-456-7890",
                    UserAddress = "123 Main St, Springfield",
                    Email = "john.doe@example.com",
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    IsActive = true
                },
                new User
                {
                    UserID = 2,
                    Username = "jane_smith",
                    PasswordHash = "hashedpassword2",
                    PhoneNumber = "987-654-3210",
                    UserAddress = "456 Elm St, Springfield",
                    Email = "jane.smith@example.com",
                    CreatedAt = DateTime.UtcNow.AddDays(-60),
                    IsActive = true
                },
                new User
                {
                    UserID = 3,
                    Username = "sam_jones",
                    PasswordHash = "hashedpassword3",
                    PhoneNumber = "555-123-4567",
                    UserAddress = "789 Oak St, Springfield",
                    Email = "sam.jones@example.com",
                    CreatedAt = DateTime.UtcNow.AddDays(-90),
                    IsActive = false
                }
            };

            return Ok(users); // Return the list of dummy users
        }
    }
}
