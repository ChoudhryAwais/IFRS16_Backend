using IFRS16_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace IFRS16_Backend.Services.UserCrud
{
    public class UserService(ApplicationDbContext context) : IUserService
    {
        private readonly ApplicationDbContext _context = context;
        public async Task<bool> UpdateUserAsync(UserUpdateDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == dto.UserID);
            if (user == null)
                return false;

            user.Username = dto.Username;
            user.PhoneNumber = dto.PhoneNumber;
            user.UserAddress = dto.UserAddress;
            // Only update password if provided
            if (!string.IsNullOrEmpty(dto.PasswordHash))
                user.PasswordHash = dto.PasswordHash;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> VerifyPasswordAsync(int userId, string password)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            // If password is hashed, use hashing verification
            return user.PasswordHash == password; // replace with proper hash verification
        }
    }
}
