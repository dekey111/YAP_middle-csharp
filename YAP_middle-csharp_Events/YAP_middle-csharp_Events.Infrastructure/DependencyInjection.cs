using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YAP_middle_csharp_Events.Application.Interfaces.IRepositories;
using YAP_middle_csharp_Events.Infrastructure.DataAccess;
using YAP_middle_csharp_Events.Infrastructure.Repository;

namespace YAP_middle_csharp_Events.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString).LogTo(Console.WriteLine));

            services.AddScoped<IEventRepository, EventRepository>();

            return services;
        }
    }
}
