using Microsoft.EntityFrameworkCore;
using YAP_middle_csharp.Domain.Models;

namespace YAP_middle_csharp.Infrastructure.DataAccess
{
    public sealed class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<EventModel> Events => Set<EventModel>();
        public DbSet<BookingModel> Bookings => Set<BookingModel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
