using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YAP_middle_csharp_Booking.Application.Interfaces.IRepositories;
using YAP_middle_csharp_Booking.Application.Interfaces.IServices;
using YAP_middle_csharp_Booking.Application.Services;
using YAP_middle_csharp_Booking.Infrastructure.DataAccess;
using YAP_middle_csharp_Booking.Infrastructure.Repository;
using YAP_middle_csharp_Booking.Infrastructure.Services;

namespace YAP_middle_csharp_Booking.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, string connectionString)
        {

            services.AddHttpContextAccessor();
            services.AddScoped<IUserContextService, UserContextService>();
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString).LogTo(Console.WriteLine));

            services.AddScoped<IBookingRepository, BookingRepository>();

            services.AddSingleton<IKafkaEventProducer, KafkaEventProducer>();

            return services;
        }
    }
}
