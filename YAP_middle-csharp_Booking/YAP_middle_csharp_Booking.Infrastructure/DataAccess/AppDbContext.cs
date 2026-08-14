using Microsoft.EntityFrameworkCore;
using YAP_middle_csharp_Booking.Domain.Models;

namespace YAP_middle_csharp_Booking.Infrastructure.DataAccess
{
    public sealed class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<BookingModel> Bookings => Set<BookingModel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
