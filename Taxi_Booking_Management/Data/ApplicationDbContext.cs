using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Xml;
using Taxi_Booking_Management.Models;

namespace Taxi_Booking_Management.Data
{
    public class ApplicationDbContext :IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options) 
        { }
        
        public DbSet<Taxi> taxis { get; set; }
        public DbSet<TaxiOwner> owner { get; set; }
        public DbSet<TaxiDriver> drivers { get; set; }
        public DbSet<Booking> Bookings { get; set;}
        public DbSet<PaymentHistory> PaymentHistories { get; set; }
        public DbSet<AspNetRoles> AspNetRoleses { get; set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            SeedAdminUser(modelBuilder).GetAwaiter().GetResult();
            modelBuilder.Ignore<IdentityUser>();
            modelBuilder.Entity<IdentityRole>().ToTable("AspNetRoles");

        }
        private async Task SeedAdminUser(ModelBuilder modelBuilder)
        {
            var adminUser = new User
            {
                Id = "1",
                UserName = "admin@example.com",
                NormalizedUserName = "ADMIN@EXAMPLE.COM",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                EmailConfirmed = true,
                Name = "Admin" // Provide the value for the 'Name' property
            };

            var passwordHasher = new PasswordHasher<User>();
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin@123");

            modelBuilder.Entity<User>().HasData(adminUser);
        }

    }
}
