using Microsoft.EntityFrameworkCore;
using YAP_middle_csharp_Auth.Domain.Models;

namespace YAP_middle_csharp_Auth.Infrastructure.DataAccess
{
    public sealed class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<UserModel> Users => Set<UserModel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
