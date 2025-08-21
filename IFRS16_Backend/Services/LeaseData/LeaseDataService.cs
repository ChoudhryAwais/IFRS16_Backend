using System.Threading.Tasks;
using IFRS16_Backend.Models;
using IFRS16_Backend.Services.InitialRecognition;
using IFRS16_Backend.Services.JournalEntries;
using Microsoft.EntityFrameworkCore;

namespace IFRS16_Backend.Services.LeaseData
{
    public class LeaseDataService(ApplicationDbContext context, IJournalEntriesService journalEntriesService) : ILeaseDataService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IJournalEntriesService _journalEntriesService = journalEntriesService;

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

        public async Task<LeaseFormDataResult> GetAllLeases(int pageNumber, int pageSize, int companyID, string leaseName)
        {

            IEnumerable<ExtendedLeaseDataSP> leaseData = await _context.GetLeaseDataPaginatedAsync(pageNumber, pageSize, companyID, leaseName);
            int totalRecord = await _context.LeaseData.CountAsync();
            List<LeaseFormData> leaseDataFiltered = await _context.LeaseData.Where(item => item.CompanyID == companyID && (leaseName == null || leaseName == "" || (item.LeaseName == leaseName))).ToListAsync();

            return new()
            {
                Data = leaseData,
                TotalRecords = leaseDataFiltered.Count,
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
        public async Task UploadLeaseContractAsync(int leaseId, IFormFile contractDoc)
        {
            if (contractDoc == null || contractDoc.Length == 0)
                throw new ArgumentException("Invalid contract document.");

            byte[] fileData;
            using (var memoryStream = new MemoryStream())
            {
                await contractDoc.CopyToAsync(memoryStream);
                fileData = memoryStream.ToArray();
            }

            var leaseContract = new LeaseContract
            {
                LeaseId = leaseId,
                ContractDoc = fileData,
                DocFileName = contractDoc.FileName,
                ContentType = contractDoc.ContentType,
                CreatedDate = DateTime.UtcNow
            };

            _context.LeaseDataContracts.Add(leaseContract);
            await _context.SaveChangesAsync();
        }
        public async Task<LeaseContract> GetLeaseContractByLeaseIdAsync(int leaseId)
        {
            var leaseContract = await _context.LeaseDataContracts
                .FirstOrDefaultAsync(lc => lc.LeaseId == leaseId);

            return leaseContract;
        }

        public async Task<bool> UpdateLeaseFormDataAsync(int leaseId, LeaseFormData updatedLease)
        {
            var existingLease = await _context.LeaseData.FirstOrDefaultAsync(l => l.LeaseId == leaseId);
            if (existingLease == null)
                return false;

            // Update properties
            existingLease.UserID = updatedLease.UserID;
            existingLease.LeaseName = updatedLease.LeaseName;
            existingLease.Rental = updatedLease.Rental;
            existingLease.CommencementDate = updatedLease.CommencementDate;
            existingLease.EndDate = updatedLease.EndDate;
            existingLease.Annuity = updatedLease.Annuity;
            existingLease.IBR = updatedLease.IBR;
            existingLease.Frequency = updatedLease.Frequency;
            existingLease.IDC = updatedLease.IDC;
            existingLease.GRV = updatedLease.GRV;
            existingLease.Increment = updatedLease.Increment;
            existingLease.IncrementalFrequency = updatedLease.IncrementalFrequency;
            existingLease.CompanyID = updatedLease.CompanyID;
            existingLease.IsActive = updatedLease.IsActive;
            existingLease.LastModifiedDate = updatedLease.LastModifiedDate;
            existingLease.CurrencyID = updatedLease.CurrencyID;
            // Add any additional fields as needed

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public async Task<bool> UpdateLeaseContractAsync(int leaseId, IFormFile contractDoc)
        {
            if (contractDoc == null || contractDoc.Length == 0)
                return false;

            var leaseContract = await _context.LeaseDataContracts.FirstOrDefaultAsync(lc => lc.LeaseId == leaseId);
            if (leaseContract == null)
            {
                byte[] fileData;
                using (var memoryStream = new MemoryStream())
                {
                    await contractDoc.CopyToAsync(memoryStream);
                    fileData = memoryStream.ToArray();
                }

                leaseContract = new LeaseContract
                {
                    LeaseId = leaseId,
                    ContractDoc = fileData,
                    DocFileName = contractDoc.FileName,
                    ContentType = contractDoc.ContentType,
                    CreatedDate = DateTime.UtcNow
                };

                _context.LeaseDataContracts.Add(leaseContract);
                try
                {
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                using (var memoryStream = new MemoryStream())
                {
                    await contractDoc.CopyToAsync(memoryStream);
                    leaseContract.ContractDoc = memoryStream.ToArray();
                }
                leaseContract.DocFileName = contractDoc.FileName;
                leaseContract.ContentType = contractDoc.ContentType;
                leaseContract.CreatedDate = DateTime.UtcNow;

                try
                {
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }


               
        }
    }
}
