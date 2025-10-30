using Microsoft.EntityFrameworkCore;
using SportsTournament.Models;
using SportsTournament.ViewModels;

namespace SportsTournament.Models
{
    public class ManagementSystemDbContext : DbContext
    {
        public ManagementSystemDbContext(DbContextOptions<ManagementSystemDbContext> options) : base(options) { }
     
        public DbSet<User> Users { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Records> TournamentRecords { get; set; } = null!;
        public DbSet<Records> UserTable { get; set; } = null!;
        public DbSet<AdminRequest> MainAdmin{ get; set; }
        public DbSet<UserDetail> UserDetails { get; set; }
        public DbSet<AdminDetail> AdminDetails { get; set; }



        // Seed admin account automatically
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Default admin
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Password = "admin123", Role = "Admin" }
            );
        }
    }
}