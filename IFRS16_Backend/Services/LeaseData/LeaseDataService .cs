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
            try
            {
                _context.LeaseData.Add(leaseFormData);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) { 
                 Console.WriteLine(ex.ToString());
            }
            return true;
        }

        public async Task<LeaseFormDataResult> GetAllLeases(int pageNumber,int pageSize,int companyID)
        {

            IEnumerable<ExtendedLeaseDataSP> leaseData = await _context.GetLeaseDataPaginatedAsync(pageNumber, pageSize, companyID);
            int totalRecord = await _context.LeaseData.CountAsync();

            return new()
            {
                Data = leaseData,
                TotalRecords = totalRecord,
            };
        }

        public async Task<List<LeaseFormData>> GetAllLeasesForCompany(int companyId)
        {

            List<LeaseFormData> leaseData = await _context.LeaseData.Where(item => item.CompanyID==companyId).ToListAsync();
            return leaseData;
        }
    }
}
