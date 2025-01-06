using System.Threading.Tasks;
using IFRS16_Backend.Models;
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

        public async Task<LeaseFormDataResult> GetAllLeases(int pageNumber,int pageSize)
        {

            IEnumerable<ExtendedLeaseDataSP> leaseData = await _context.GetLeaseDataPaginatedAsync(pageNumber, pageSize);
            int totalRecord = await _context.LeaseData.CountAsync();

            return new()
            {
                Data = leaseData,
                TotalRecords = totalRecord,
            };
        }
    }
}
