using Microsoft.AspNetCore.Http.Features;
using System.Diagnostics;
using YAP_middle_csharp.Interfaces;
using YAP_middle_csharp.Interfaces.IRepositories;
using YAP_middle_csharp.Interfaces.IServices;
using YAP_middle_csharp.Middleware;
using YAP_middle_csharp.Models;
using YAP_middle_csharp.Repository;
using YAP_middle_csharp.Services;
using YAP_middle_csharp.Services.BackgroundServices;
using YAP_middle_csharp.Validator;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHostedService<BackgroundBookingService>();

builder.Services.AddSingleton<IRepository<EventModel>, EventRepository>();
builder.Services.AddSingleton<IBooklngRepository, BookingRepository>(); 

builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IBookingServive, BookingService>();
builder.Services.AddTransient<IValidator<EventModel>, EventValidator>();
builder.Services.AddTransient<IValidator<BookingModel>, BookingValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance =
            $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

        Activity? activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
        context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
    };
});

var app = builder.Build();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
