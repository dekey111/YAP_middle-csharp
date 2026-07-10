using Microsoft.Extensions.DependencyInjection;
using YAP_middle_csharp.Application.Interfaces.IServices;
using YAP_middle_csharp.Application.Services;
using YAP_middle_csharp.Application.Validator;
using YAP_middle_csharp.Interfaces;
using YAP_middle_csharp.Models;

namespace YAP_middle_csharp.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IBookingService, BookingService>();

            services.AddTransient<IValidator<EventModel>, EventValidator>();
            services.AddTransient<IValidator<BookingModel>, BookingValidator>();

            return services;
        }
    }
}
