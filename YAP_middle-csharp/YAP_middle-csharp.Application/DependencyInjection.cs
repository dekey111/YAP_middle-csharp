using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using YAP_middle_csharp.Application.Interfaces.IServices;
using YAP_middle_csharp.Interfaces;
using YAP_middle_csharp.Models;
using YAP_middle_csharp.Services;
using YAP_middle_csharp.Validator;

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
