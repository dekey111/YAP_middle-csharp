using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using YAP_middle_csharp.Application.Interfaces.IServices;
using YAP_middle_csharp.Application.Services;
using YAP_middle_csharp.Domain.Exceptions;
using YAP_middle_csharp.Domain.Models;
using YAP_middle_csharp.Infrastructure.DataAccess;
using YAP_middle_csharp.Infrastructure.Repository;

namespace EventApi.IntegrationTests
{
    [Collection("PostgresCollection")]
    public class AuthServiceTests : IAsyncLifetime
    {
        private readonly PostgresFixture _fixture;
        private AppDbContext _context = null!;
        private UserRepository _repository = null!;
        private IUserService _userService = null!;

        public AuthServiceTests(PostgresFixture fixture)
        {
            _fixture = fixture;
        }

        public async Task InitializeAsync()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_fixture.Container.GetConnectionString())
                .Options;

            _context = new AppDbContext(options);
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.MigrateAsync();

            _repository = new UserRepository(_context);

            var identityHasher = new PasswordHasher<UserModel>();
            var passwordHasherService = new PasswordHasherService(identityHasher);

            var inMemorySettings = new Dictionary<string, string?>
            {
                { "JwtSettings:SecretKey", "MySuperPuperHighKey3000_SuperSecretKeyinTheFuWorld!" },
                { "JwtSettings:Issuer", "TestIssuer" },
                { "JwtSettings:Audience", "TestAudience" },
                { "JwtSettings:ExpiryMinutes", "60" }
            };

            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var jwtGenerator = new JwtTokenGenerator(config);

            _userService = new UserService(_repository, passwordHasherService, jwtGenerator);
        }

        public async Task DisposeAsync() => await _context.DisposeAsync();


        [Fact]
        public async Task RegisterAndLoginAsync_ValidCredentials_ReturnsJwtToken()
        {
            var login = "SuperUser123";
            var password = "SuperPassword123!";

            await _userService.RegisterAsync(login, password, UserRoleEnum.User);

            var token = await _userService.LoginAsync(login, password);

            Assert.NotNull(token);
            Assert.NotEmpty(token);

            var userInDb = await _repository.FindByLoginAsync(login);
            Assert.NotNull(userInDb);
            Assert.NotEqual(password, userInDb.PasswordHash);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ThrowsValidationExceptionApp()
        {
            var login = "WrongUser";
            var password = "Password123!";

            await _userService.RegisterAsync(login, password, UserRoleEnum.User);

            var ex = await Assert.ThrowsAsync<UnauthorizedException>(() =>  _userService.LoginAsync(login, "InvalidPassword123!"));

            Assert.Equal("Ошибка авторизации", ex.Message);
        }
    }
}
