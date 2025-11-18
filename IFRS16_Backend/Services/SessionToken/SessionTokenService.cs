using IFRS16_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace IFRS16_Backend.Services.SessionToken
{
    public class SessionTokenService(ApplicationDbContext db) : ISessionTokenService
    {
        private readonly ApplicationDbContext _db = db;

        public async Task UpsertSessionTokenAsync(int userId, string token)
        {
            // Try to find existing record for the user
            var existing = await _db.SessionToken.FirstOrDefaultAsync(s => s.UserId == userId);
            if (existing == null)
            {
                var newEntry = new SessionTokenTable
                {
                    UserId = userId,
                    Token = token
                };

                _db.SessionToken.Add(newEntry);
            }
            else
            {
                existing.Token = token;
                _db.SessionToken.Update(existing);
            }

            await _db.SaveChangesAsync();
        }
    }
}
