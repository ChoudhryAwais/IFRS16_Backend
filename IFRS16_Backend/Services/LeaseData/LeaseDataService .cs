using System.Threading.Tasks;
using IFRS16_Backend.Models;
using IFRS16_Backend.Services.LeaseDataService;
using Microsoft.EntityFrameworkCore;

namespace IFRS16_Backend.Services.LeaseData
{
    public class LeaseDataService(ApplicationDbContext context) : ILeaseDataService
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<bool> AddLeaseFormDataAsync(LeaseFormData leaseFormData)
        {
            if (leaseFormData == null)
            {
                return false;
            }

            _context.LeaseData.Add(leaseFormData);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
