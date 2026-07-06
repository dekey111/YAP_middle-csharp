using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using YAP_middle_csharp.DataAccess;
using YAP_middle_csharp.Interfaces.IRepositories;
using YAP_middle_csharp.Repository;
using YAP_middle_csharp.Services.BackgroundServices;

namespace YAP_middle_csharp.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString).LogTo(Console.WriteLine));

            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddHostedService<BackgroundBookingService>();

            return services;
        }
    }
}