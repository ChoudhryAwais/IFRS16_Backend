using System.Threading.Tasks;
using IFRS16_Backend.Models;

namespace IFRS16_Backend.Services.UserCrud
{
    public interface IUserService
    {
        Task<bool> UpdateUserAsync(UserUpdateDto dto);
        Task<bool> VerifyPasswordAsync(int userId, string password);
    }
}
