﻿using Microsoft.EntityFrameworkCore;
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
        public DbSet<FC_ROUScheduleTable> FC_ROUSchedule { get; set; }
        public DbSet<LeaseLiabilityTable> LeaseLiability { get; set; }
        public DbSet<FC_LeaseLiabilityTable> FC_LeaseLiability { get; set; }
        public DbSet<CompanyProfile> CompanyProfile { get; set; }
        public DbSet<JournalEntryTable> JournalEntries { get; set; }
        public DbSet<LeaseLiabilityAggregationTable> LeaseLiabilityAggregation { get; set; }
        public DbSet<ROUAggregationTable> ROUAggregation { get; set; }
        public DbSet<CurrenciesTable> Currencies { get; set; }
        public DbSet<ExchangeRateTable> ExchangeRates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExtendedLeaseDataSP>()
                .HasKey(e => e.LeaseId); // Set LeaseId as the primary key
            modelBuilder.Entity<LeaseLiabilityAggregationTable>().HasNoKey();
            modelBuilder.Entity<ROUAggregationTable>().HasNoKey();

            base.OnModelCreating(modelBuilder);
        }

        public async Task<IEnumerable<ExtendedLeaseDataSP>> GetLeaseDataPaginatedAsync(int pageNumber, int pageSize, int CompanyID)
        {
            var leaseData = await this.LeaseDataSP
                .FromSqlRaw("EXEC GetLeaseDataPaginated @PageNumber = {0}, @PageSize = {1}, @CompanyID= {2}", pageNumber, pageSize, CompanyID)
                .ToListAsync();

            return leaseData;
        }
        public async Task<IEnumerable<InitialRecognitionTable>> GetInitialRecognitionPaginatedAsync(int pageNumber, int pageSize, int leaseId)
        {
            var InitialRecognition = await this.InitialRecognition
                .FromSqlRaw("EXEC GetInitialRecognitionPaginated @PageNumber = {0}, @PageSize = {1}, @LeaseId = {2}", pageNumber, pageSize, leaseId)
                .ToListAsync();

            return InitialRecognition;
        }
        public async Task<IEnumerable<ROUScheduleTable>> GetROUSchedulePaginatedAsync(int pageNumber, int pageSize, int leaseId, int fc_lease=0)
        {
            var ROUSchedule = await this.ROUSchedule
                .FromSqlRaw("EXEC GetROUSchedulePaginated @PageNumber = {0}, @PageSize = {1}, @LeaseId = {2}, @FC_Lease = {3}", pageNumber, pageSize, leaseId, fc_lease)
                .ToListAsync();

            return ROUSchedule;
        }
        public async Task<IEnumerable<FC_LeaseLiabilityTable>> GetLeaseLiabilityPaginatedAsync(int pageNumber, int pageSize, int leaseId, int fc_lease)
        {
            var LeaseLiability = await this.FC_LeaseLiability
                .FromSqlRaw("EXEC GetLeaseLiabilityPaginated @PageNumber = {0}, @PageSize = {1}, @LeaseId = {2}, @FC_Lease = {3}", pageNumber, pageSize, leaseId, fc_lease)
                .ToListAsync();

            return LeaseLiability;
        }
        public async Task<IEnumerable<JournalEntryTable>> GetJournalEntriesAsync(int pageNumber, int pageSize, int leaseId)
        {
            var JournalEntries = await this.JournalEntries
                .FromSqlRaw("EXEC GetJournalEntriesPaginated @PageNumber = {0}, @PageSize = {1}, @LeaseId = {2}", pageNumber, pageSize, leaseId)
                .ToListAsync();

            return JournalEntries;
        }
        public async Task<IEnumerable<LeaseLiabilityAggregationTable>> GetLeaseLiabilityAggregationTableAsync(DateTime startDate,DateTime endDate, string leaseIdList)
        {
            var leaseLiabilityAggregation = await this.LeaseLiabilityAggregation
                .FromSqlRaw("EXEC GetLeaseLiabilityAggregation @StartDate  = {0}, @EndDate = {1}, @LeaseIdList = {2}", startDate, endDate, leaseIdList)
                .ToListAsync();

            return leaseLiabilityAggregation;
        }
        public async Task<IEnumerable<ROUAggregationTable>> GetROUAggregationTableAsync(DateTime startDate, DateTime endDate, string leaseIdList)
        {
            var rouAggregation = await this.ROUAggregation
                .FromSqlRaw("EXEC GetROUAggregation @StartDate  = {0}, @EndDate = {1}, @LeaseIdList = {2}", startDate, endDate, leaseIdList)
                .ToListAsync();

            return rouAggregation;
        }
    }
}
