using Microsoft.EntityFrameworkCore;
using TitleVerification.Api.Models;

namespace TitleVerification.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : base(opts) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Document> Documents { get; set; } = null!;
        public DbSet<LandRecord> LandRecords { get; set; } = null!;
        public DbSet<Report> Reports { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique constraints
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<LandRecord>().HasIndex(l => new { l.LandID }).IsUnique();
            // Optionally, if you want unique coordinates-per-land: add index on lat+lng
            modelBuilder.Entity<LandRecord>().HasIndex(l => new { l.Latitude, l.Longitude }).IsUnique();

            // Seed sample land record
            modelBuilder.Entity<LandRecord>().HasData(new LandRecord
            {
                Id = 1,
                LandID = "LAND123",
                Latitude = 12.9716,
                Longitude = 77.5946,
                Name = "John Doe",
                AadhaarNumber = "111122223333",
                OwnershipType = "Self",
                SiblingApproval = false,
                LoanDisputeStatus = false,
                LandType = "Agriculture"
            });
        }
    }
}
