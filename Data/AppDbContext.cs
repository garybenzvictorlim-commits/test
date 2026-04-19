using Microsoft.EntityFrameworkCore;
using SchoolQueueSystem.Models;

namespace SchoolQueueSystem.Data
{
    // AppDbContext is the "bridge" between your C# code and the SQLite database.
    // Every time you want to read or save data, you use this class.
    public class AppDbContext : DbContext
    {
        // Constructor - passes options to the base EntityFrameworkCore class
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet = a "table" in the database
        // You can think of QueueCounters as: SELECT * FROM QueueCounters
        public DbSet<QueueCounter> QueueCounters { get; set; }

        // Stores admin accounts (username + hashed password)
        public DbSet<AdminUser> AdminUsers { get; set; }

        // This method runs when the database is first created
        // It seeds (pre-fills) some default queue counters
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QueueCounter>().HasData(
                new QueueCounter { Id = 1, Name = "Tuition Payment",     CurrentNumber = 0, NextNumber = 1, Status = "Open", LastUpdated = DateTime.Now },
                new QueueCounter { Id = 2, Name = "Registrar / Enrollment", CurrentNumber = 0, NextNumber = 1, Status = "Open", LastUpdated = DateTime.Now },
                new QueueCounter { Id = 3, Name = "Scholarship Office",  CurrentNumber = 0, NextNumber = 1, Status = "Open", LastUpdated = DateTime.Now },
                new QueueCounter { Id = 4, Name = "Cashier",             CurrentNumber = 0, NextNumber = 1, Status = "Open", LastUpdated = DateTime.Now }
            );
        }
    }
}
