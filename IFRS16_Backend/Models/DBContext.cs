using Microsoft.EntityFrameworkCore;

namespace IFRS16_Backend.Models
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<LeaseFormData> LeaseData { get; set; }
       
    }
}
