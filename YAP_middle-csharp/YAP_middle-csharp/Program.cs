using YAP_middle_csharp.Interfaces;
using YAP_middle_csharp.Middleware;
using YAP_middle_csharp.Models;
using YAP_middle_csharp.Services;
using YAP_middle_csharp.Validator;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IEventService, EventService>(); 
builder.Services.AddTransient<IValidator<EventResponse>, EventValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();



var app = builder.Build();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseExceptionHandler();


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
