using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using YAP_middle_csharp.Application.Interfaces.IServices;
using YAP_middle_csharp.Application.Models;
using YAP_middle_csharp.Domain.Models;
using YAP_middle_csharp.Infrastructure.DataAccess;
using static System.Net.Mime.MediaTypeNames;

namespace EventApi.IntegrationTests
{
    [Collection("PostgresCollection")]
    public class AuthAndSecurityIntegrationTests : IAsyncLifetime
    {
        private readonly PostgresFixture _fixture;
        private CustomWebApplicationFactory _factory = null!;
        private HttpClient _client = null!;

        public AuthAndSecurityIntegrationTests(PostgresFixture fixture)
        {
            _fixture = fixture;
        }

        public async Task InitializeAsync()
        {
            _factory = new CustomWebApplicationFactory(_fixture.Container.GetConnectionString());
            _client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            if (_factory != null)
            {
                await _factory.DisposeAsync();
            }
        }

        private async Task<(string Token, Guid UserId)> RegisterAndLoginUserAsync(string login, string password, UserRoleEnum role)
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            await userService.RegisterAsync(login, password, role);

            var user = await dbContext.Users.FirstAsync(u => u.Login == login);
            var token = await userService.LoginAsync(login, password);

            return (token, user.Id);
        }

        [Fact]
        public async Task GetBooking_WithoutToken_Returns401Unauthorized()
        {
            var randomBookingId = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/Booking/{randomBookingId}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetBooking_UserATriesToAccessUserBBooking_Returns404NotFound()
        {
            var (userAToken, _) = await RegisterAndLoginUserAsync("user_a_" + Guid.NewGuid(), "Password123!", UserRoleEnum.User);
            var (_, userBId) = await RegisterAndLoginUserAsync("user_b_" + Guid.NewGuid(), "Password123!", UserRoleEnum.User);

            Guid userBBookingId;
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var testEvent = new EventModel
                {
                    Id = Guid.NewGuid(),
                    Title = "Интеграционный ивент",
                    TotalSeats = 10,
                    AvailableSeats = 10,
                    StartAt = DateTime.UtcNow.AddDays(1),
                    EndAt = DateTime.UtcNow.AddDays(2)
                };

                var userBBooking = new BookingModel(testEvent.Id, userBId);

                await dbContext.Events.AddAsync(testEvent);
                await dbContext.Bookings.AddAsync(userBBooking);
                await dbContext.SaveChangesAsync();

                userBBookingId = userBBooking.Id;
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userAToken);
            var response = await _client.GetAsync($"/api/Booking/{userBBookingId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateEvent_AsStandardUser_Returns403Forbidden()
        {
            var (userToken, _) = await RegisterAndLoginUserAsync("regular_user_" + Guid.NewGuid(), "Password123!", UserRoleEnum.User);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            var createEventRequest = new EventRequest
            {
                Title = "Запрещённое создание ивента",
                Description = "Тестовое описание",
                TotalSeats = 10,
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2)
            };

            var response = await _client.PostAsJsonAsync("/api/Events", createEventRequest);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithInvalidPassword_Returns401Unauthorized()
        {
            var login = "valid_user_" + Guid.NewGuid();
            var correctPassword = "CorrectPassword123!";
            var wrongPassword = "WrongPassword123!";

            await RegisterAndLoginUserAsync(login, correctPassword, UserRoleEnum.User);

            var loginRequest = new LoginRequest
            {
                Login = login,
                Password = wrongPassword
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
