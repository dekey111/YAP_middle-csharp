using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using YAP_middle_csharp.Application.Interfaces;
using YAP_middle_csharp.Application.Interfaces.IServices;
using YAP_middle_csharp.Application.Services;
using YAP_middle_csharp.Application.Validator;
using YAP_middle_csharp.Domain.Interface;
using YAP_middle_csharp.Domain.Models;


namespace YAP_middle_csharp.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IPasswordHasherService, PasswordHasherService>();
            services.AddScoped<IPasswordHasher<UserModel>, PasswordHasher<UserModel>>();
            services.AddScoped<IUserContextService, UserContextService>();
            services.AddScoped<IUserService, UserService>();

            services.AddTransient<IValidator<EventModel>, EventValidator>();
            services.AddTransient<IValidator<BookingModel>, BookingValidator>();

            return services;
        }
    }
}
