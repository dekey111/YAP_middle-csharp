using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using YAP_middle_csharp_Auth.Application.Interfaces;
using YAP_middle_csharp_Auth.Application.Interfaces.IServices;
using YAP_middle_csharp_Auth.Application.Models;
using YAP_middle_csharp_Auth.Application.Services;
using YAP_middle_csharp_Auth.Application.Validators;
using YAP_middle_csharp_Auth.Domain.Interface;
using YAP_middle_csharp_Auth.Domain.Models;

namespace YAP_middle_csharp_Auth_Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IPasswordHasherService, PasswordHasherService>();
            services.AddScoped<IPasswordHasher<UserModel>, PasswordHasher<UserModel>>();
            services.AddScoped<IUserContextService, UserContextService>();
            services.AddScoped<IUserService, UserService>();

            services.AddTransient<IValidator<LoginRequest>, LoginValidator>();


            return services;
        }
    }
}
