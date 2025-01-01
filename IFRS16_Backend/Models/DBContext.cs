using Microsoft.EntityFrameworkCore;

namespace IFRS16_Backend.Models
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<LeaseFormData> LeaseData { get; set; }
        //public DbSet<InitialRecognitionTable> InitialRecognition { get; set; }
        //public DbSet<ROUScheduleTable> ROUSchedule { get; set; }
        //public DbSet<LeaseLiabilityTable> LeaseLiability { get; set; }
    }
}
