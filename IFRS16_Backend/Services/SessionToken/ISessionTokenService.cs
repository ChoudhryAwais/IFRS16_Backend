using System.Threading.Tasks;
using IFRS16_Backend.Models;

namespace IFRS16_Backend.Services.SessionToken
{
    public interface ISessionTokenService
    {
        Task UpsertSessionTokenAsync(int userId, string token);
    }
}
