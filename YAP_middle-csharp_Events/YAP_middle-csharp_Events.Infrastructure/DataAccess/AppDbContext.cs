using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using YAP_middle_csharp_Events.Domain.Models;

namespace YAP_middle_csharp_Events.Infrastructure.DataAccess
{
    public sealed class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<EventModel> Events => Set<EventModel>();
        public DbSet<ProcessedBookingsModel> ProcessedBookings => Set<ProcessedBookingsModel>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
