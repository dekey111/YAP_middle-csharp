using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YAP_middle_csharp_Auth.Application.Interfaces.IRepositories;
using YAP_middle_csharp_Auth.Domain.Interface;
using YAP_middle_csharp_Auth.Infrastructure.DataAccess;
using YAP_middle_csharp_Auth.Infrastructure.Repository;

namespace YAP_middle_csharp_Auth.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString).LogTo(Console.WriteLine));
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            services.AddScoped<IUserRepository, UserRepository>();
            return services;
        }
    }
}
