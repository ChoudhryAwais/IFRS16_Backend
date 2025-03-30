using System.Threading.Tasks;
using IFRS16_Backend.Models;
using IFRS16_Backend.Services.InitialRecognition;
using IFRS16_Backend.Services.JournalEntries;
using Microsoft.EntityFrameworkCore;

namespace IFRS16_Backend.Services.LeaseData
{
    public class LeaseDataService(ApplicationDbContext context, IJournalEntriesService journalEntriesService, IInitialRecognitionService initialRecognitionService) : ILeaseDataService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IJournalEntriesService _journalEntriesService = journalEntriesService;
        private readonly IInitialRecognitionService _initialRecognitionService = initialRecognitionService;

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
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return true;
        }

        public async Task<LeaseFormDataResult> GetAllLeases(int pageNumber, int pageSize, int companyID)
        {

            IEnumerable<ExtendedLeaseDataSP> leaseData = await _context.GetLeaseDataPaginatedAsync(pageNumber, pageSize, companyID);
            int totalRecord = await _context.LeaseData.CountAsync();

            return new()
            {
                Data = leaseData,
                TotalRecords = totalRecord,
            };
        }
        public async Task<LeaseFormData> GetLeaseById(int leaseId)
        {
            LeaseFormData leaseData = _context.LeaseData.FirstOrDefault(item => item.LeaseId == leaseId);
            return leaseData;
        }
        public async Task<List<LeaseFormData>> GetAllLeasesForCompany(int companyId)
        {

            List<LeaseFormData> leaseData = await _context.LeaseData.Where(item => item.CompanyID == companyId).ToListAsync();
            return leaseData;
        }

        public async Task<bool> DeleteLeases(string leaseIds)
        {
            try
            {
                await _context.DeleteLeaseDataAsync(leaseIds);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return true;
            }

        }

        public async Task<bool> TerminateLease(TerminateLease termination)
        {
            try
            {
                LeaseLiabilityTable? leaseLiability = _context.LeaseLiability.FirstOrDefault(item => item.LeaseId == termination.LeaseId && item.LeaseLiability_Date == termination.TerminateDate);
                ROUScheduleTable? rouSchedule = _context.ROUSchedule.FirstOrDefault(item => item.LeaseId == termination.LeaseId && item.ROU_Date == termination.TerminateDate);

                // Check if both leaseLiability and rouSchedule are empty
                if (leaseLiability == null && rouSchedule == null)
                {
                    return false;
                }

                await _context.TerminateLeaseAsync(termination.TerminateDate, termination.LeaseId);
                var result = await _journalEntriesService.EnterJEOnTermination((decimal)leaseLiability.Closing, (decimal)rouSchedule.Closing, termination.Penalty, termination.TerminateDate, termination.LeaseId);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return true;
            }
        }
    }
}
