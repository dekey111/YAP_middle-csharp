using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using YAP_middle_csharp_Events.Application.Interfaces;
using YAP_middle_csharp_Events.Application.Interfaces.IServices;
using YAP_middle_csharp_Events.Application.Services;
using YAP_middle_csharp_Events.Application.Validator;
using YAP_middle_csharp_Events.Domain.Models;

namespace YAP_middle_csharp_Events.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IEventService, EventService>();

            services.AddTransient<IValidator<EventModel>, EventValidator>();

            return services;
        }
    }

}
