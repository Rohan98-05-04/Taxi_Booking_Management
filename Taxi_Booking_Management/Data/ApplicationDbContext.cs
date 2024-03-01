using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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
        
    }
}
