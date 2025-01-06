using Microsoft.EntityFrameworkCore;
using System.Data;

namespace IFRS16_Backend.Models
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<LeaseFormData> LeaseData { get; set; }
        public DbSet<ExtendedLeaseDataSP> LeaseDataSP { get; set; }
        public DbSet<InitialRecognitionTable> InitialRecognition { get; set; }
        public DbSet<ROUScheduleTable> ROUSchedule { get; set; }
        public DbSet<LeaseLiabilityTable> LeaseLiability { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExtendedLeaseDataSP>()
                .HasKey(e => e.LeaseId); // Set LeaseId as the primary key

            base.OnModelCreating(modelBuilder);
        }

        public async Task<IEnumerable<ExtendedLeaseDataSP>> GetLeaseDataPaginatedAsync(int pageNumber, int pageSize)
        {
            // Execute the stored procedure and get the lease data
            var leaseData = await this.LeaseDataSP
                .FromSqlRaw("EXEC GetLeaseDataPaginated @PageNumber = {0}, @PageSize = {1}", pageNumber, pageSize)
                .ToListAsync();

            return leaseData;
        }
        public async Task<IEnumerable<InitialRecognitionTable>> GetInitialRecognitionPaginatedAsync(int pageNumber, int pageSize,int leaseId)
        {
            // Execute the stored procedure and get the lease data
            var InitialRecognition = await this.InitialRecognition
                .FromSqlRaw("EXEC GetInitialRecognitionPaginated @PageNumber = {0}, @PageSize = {1}, @LeaseId = {2}", pageNumber, pageSize,leaseId)
                .ToListAsync();

            return InitialRecognition;
        }

        public async Task<IEnumerable<ROUScheduleTable>> GetROUSchedulePaginatedAsync(int pageNumber, int pageSize, int leaseId)
        {
            // Execute the stored procedure and get the lease data
            var ROUSchedule = await this.ROUSchedule
                .FromSqlRaw("EXEC GetROUSchedulePaginated @PageNumber = {0}, @PageSize = {1}, @LeaseId = {2}", pageNumber, pageSize, leaseId)
                .ToListAsync();

            return ROUSchedule;
        }

        public async Task<IEnumerable<LeaseLiabilityTable>> GetLeaseLiabilityPaginatedAsync(int pageNumber, int pageSize, int leaseId)
        {
            // Execute the stored procedure and get the lease data
            var LeaseLiability = await this.LeaseLiability
                .FromSqlRaw("EXEC GetLeaseLiabilityPaginated @PageNumber = {0}, @PageSize = {1}, @LeaseId = {2}", pageNumber, pageSize, leaseId)
                .ToListAsync();

            return LeaseLiability;
        }
    }
}
