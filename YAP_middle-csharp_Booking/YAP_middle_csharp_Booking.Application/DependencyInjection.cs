using Microsoft.Extensions.DependencyInjection;
using YAP_middle_csharp.Application.Interfaces;
using YAP_middle_csharp_Booking.Application.Interfaces.IServices;
using YAP_middle_csharp_Booking.Application.Services;
using YAP_middle_csharp_Booking.Application.Services.BackgroundServices;
using YAP_middle_csharp_Booking.Application.Validator;
using YAP_middle_csharp_Booking.Domain.Models;

namespace YAP_middle_csharp_Booking.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IBookingService, BookingService>();
            services.AddTransient<IValidator<BookingModel>, BookingValidator>();
            services.AddHostedService<BackgroundBookingService>();
            return services;
        }
    }
}
