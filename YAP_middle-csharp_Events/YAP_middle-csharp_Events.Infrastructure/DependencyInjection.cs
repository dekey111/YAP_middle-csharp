using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YAP_middle_csharp_Events.Application.Interfaces.IRepositories;
using YAP_middle_csharp_Events.Infrastructure.DataAccess;
using YAP_middle_csharp_Events.Infrastructure.Repository;
using YAP_middle_csharp_Events.Infrastructure.Services;
using YAP_middle_csharp_Events.Infrastructure.Topic;

namespace YAP_middle_csharp_Events.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOption =>
            {
                npgsqlOption.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
            })
            .LogTo(Console.WriteLine));

            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IProcessedBookingRepository, ProcessedBookingRepository>();

            services.AddHostedService<KafkaTopicInit>();

            services.AddHostedService<BackgroundEventService>();

            return services;
        }
    }
}
