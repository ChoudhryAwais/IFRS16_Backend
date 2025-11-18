using Microsoft.EntityFrameworkCore;
using EFCore.BulkExtensions;

namespace IFRS16_Backend.Models
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<LeaseFormData> LeaseData { get; set; }
        public DbSet<ExtendedLeaseDataSP> LeaseDataSP { get; set; }
        public DbSet<InitialRecognitionTable> InitialRecognition { get; set; }
        public DbSet<ROUScheduleTable> ROUSchedule { get; set; }
        public DbSet<FC_ROUScheduleTable> FC_ROUSchedule { get; set; }
        public DbSet<LeaseLiabilityTable> LeaseLiability { get; set; }
        public DbSet<FC_LeaseLiabilityTable> FC_LeaseLiability { get; set; }
        public DbSet<JournalEntryTable> JournalEntries { get; set; }
        public DbSet<FC_JournalEntryTable> FC_JournalEntries { get; set; }
        public DbSet<CurrenciesTable> Currencies { get; set; }
        public DbSet<ExchangeRateTable> ExchangeRates { get; set; }
        public DbSet<AllLeasesReportTable> AllLeasesReport { get; set; }
        public DbSet<JournalEntryReport> JournalEntryReport { get; set; }
        public DbSet<LeaseReportSummaryTable> LeasesReportSummary { get; set; }
        public DbSet<LeaseContract> LeaseDataContracts { get; set; }
        public DbSet<DisclouserMaturityAnalysisTable> DisclouserMaturityAnalysis { get; set; }
        public DbSet<SessionTokenTable> SessionToken { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExtendedLeaseDataSP>()
                .HasKey(e => e.LeaseId); // Set LeaseId as the primary key
            modelBuilder.Entity<AllLeasesReportTable>().HasNoKey();
            modelBuilder.Entity<LeaseReportSummaryTable>().HasNoKey();
            modelBuilder.Entity<JournalEntryReport>().HasNoKey();
            modelBuilder.Entity<DisclouserMaturityAnalysisTable>().HasNoKey();


            base.OnModelCreating(modelBuilder);
        }

        public async Task<IEnumerable<ExtendedLeaseDataSP>> GetLeaseDataPaginatedAsync(int pageNumber, int pageSize, int CompanyID, string leaseID)
        {
            var leaseData = await this.LeaseDataSP
                .FromSqlRaw("EXEC GetLeaseDataPaginated @PageNumber = {0}, @PageSize = {1}, @CompanyID= {2}, @LeaseID={3}", pageNumber, pageSize, CompanyID, leaseID)
                .ToListAsync();

            return leaseData;
        }
        public async Task<IEnumerable<InitialRecognitionTable>> GetInitialRecognitionPaginatedAsync(int pageNumber, int pageSize, int leaseId, DateTime? startDate, DateTime? endDate)
        {
            var InitialRecognition = await this.InitialRecognition
                .FromSqlRaw("EXEC GetInitialRecognitionPaginated @PageNumber = {0}, @PageSize = {1}, @LeaseId = {2}, @StartDate = {3}, @EndDate = {4}", pageNumber, pageSize, leaseId, startDate, endDate)
                .ToListAsync();

            return InitialRecognition;
        }
        public async Task<IEnumerable<ROUScheduleTable>> GetROUSchedulePaginatedAsync(int pageNumber, int pageSize, int leaseId, DateTime? startDate, DateTime? endDate)
        {
            var ROUSchedule = await this.ROUSchedule
                .FromSqlRaw("EXEC GetROUSchedulePaginated @PageNumber = {0}, @PageSize = {1}, @LeaseId = {2}, @StartDate = {3}, @EndDate = {4}", pageNumber, pageSize, leaseId, startDate, endDate)
                .ToListAsync();

            return ROUSchedule;
        }
        public async Task<IEnumerable<LeaseLiabilityTable>> GetLeaseLiabilityPaginatedAsync(int pageNumber, int pageSize, int leaseId, DateTime? startDate, DateTime? endDate)
        {
            var LeaseLiability = await this.LeaseLiability
                .FromSqlRaw("EXEC GetLeaseLiabilityPaginated @PageNumber = {0}, @PageSize = {1}, @LeaseId = {2}, @StartDate = {3}, @EndDate = {4}", pageNumber, pageSize, leaseId, startDate, endDate)
                .ToListAsync();

            return LeaseLiability;
        }
        public async Task<IEnumerable<JournalEntryTable>> GetJournalEntriesAsync(int pageNumber, int pageSize, int leaseId, DateTime? startDate, DateTime? endDate)
        {
            var JournalEntries = await this.JournalEntries
                .FromSqlRaw("EXEC GetJournalEntriesPaginated @PageNumber = {0}, @PageSize = {1}, @LeaseId = {2}, @StartDate = {3}, @EndDate = {4}", pageNumber, pageSize, leaseId, startDate, endDate)
                .ToListAsync();

            return JournalEntries;
        }
        public async Task<IEnumerable<AllLeasesReportTable>> GetAllLeaseReport(DateTime startDate, DateTime endDate, int companyId)
        {
            var allLeaseReport = await this.AllLeasesReport
                .FromSqlRaw("EXEC GetAllLeasesReport @FromDate  = {0}, @EndDate = {1}, @CompanyID = {2}", startDate, endDate, companyId)
                .ToListAsync();

            return allLeaseReport;
        }
        public async Task<IEnumerable<JournalEntryReport>> GetJEReport(DateTime startDate, DateTime endDate, int companyId)
        {
            var jEReport = await this.JournalEntryReport
                .FromSqlRaw("EXEC GetJournalEntryReport @FromDate  = {0}, @EndDate = {1}, @CompanyID = {2}", startDate, endDate, companyId)
                .ToListAsync();

            return jEReport;
        }
        public async Task<IEnumerable<LeaseReportSummaryTable>> GetLeaseReportSummary(DateTime startDate, DateTime endDate, string leaseIdList, int companyId)
        {
            var leaseReportSummary = await this.LeasesReportSummary
                .FromSqlRaw("EXEC GetSummarizeLeasesReport @FromDate  = {0}, @EndDate = {1}, @LeaseIdList = {2}, @CompanyID = {3}", startDate, endDate, leaseIdList, companyId)
                .ToListAsync();

            return leaseReportSummary;
        }
        public async Task<IEnumerable<DisclouserMaturityAnalysisTable>> GetDisclouserMaturityAnalysis(DateTime startDate, DateTime endDate, int companyId)
        {
            var disclouserMaturityAnalysisReport = await this.DisclouserMaturityAnalysis
                .FromSqlRaw("EXEC GetDisclouserForMaturityAnaylsis @FromDate = {0}, @EndDate = {1}, @CompanyID = {2}", startDate, endDate, companyId)
                .ToListAsync();

            return disclouserMaturityAnalysisReport;
        }
        public async Task DeleteLeaseDataAsync(string leaseIds)
        {
            await this.Database.ExecuteSqlRawAsync("EXEC DeleteLeaseWithDependencies @leaseIds = {0}", leaseIds);
        }
        public async Task TerminateLeaseAsync(DateTime terminateDate, int leaseId)
        {
            await this.Database.ExecuteSqlRawAsync("EXEC TerminateLease @TerminateDate = {0}, @LeaseId = {1}", terminateDate, leaseId);
        }
        public async Task ModifyLeaseAsync(DateTime? LastModifiedDate, int leaseId)
        {
            await this.Database.ExecuteSqlRawAsync("EXEC ModifyLease @ModificationDate = {0}, @LeaseId = {1}", LastModifiedDate, leaseId);
        }
    }
}
