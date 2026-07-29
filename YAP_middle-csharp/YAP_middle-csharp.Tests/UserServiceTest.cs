using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using YAP_middle_csharp.Application.Interfaces;
using YAP_middle_csharp.Application.Interfaces.IRepositories;
using YAP_middle_csharp.Application.Interfaces.IServices;
using YAP_middle_csharp.Application.Services;
using YAP_middle_csharp.Domain.Exceptions;
using YAP_middle_csharp.Domain.Interface;
using YAP_middle_csharp.Domain.Models;
using YAP_middle_csharp.Infrastructure.DataAccess;
using YAP_middle_csharp.Infrastructure.Repository;

namespace YAP_middle_csharp.Tests
{
    public class UserServiceTest
    {
        private readonly IUserService _userService;

        public UserServiceTest()
        {
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            services.AddLogging();

            services.AddScoped<IPasswordHasher<UserModel>, PasswordHasher<UserModel>>();
            services.AddScoped<IPasswordHasherService, PasswordHasherService>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string?>
                {
                    { "JwtSettings:SecretKey", "MySuperPuperHighKey3000_SuperSecretKeyinTheFuWorld!" },
                    { "JwtSettings:Issuer", "TestIssuer" },
                    { "JwtSettings:Audience", "TestAudience" },
                    { "JwtSettings:ExpiryMinutes", "60" }
                }).Build();

            services.AddSingleton<IConfiguration>(config);

            var provider = services.BuildServiceProvider();
            _userService = provider.CreateScope().ServiceProvider.GetRequiredService<IUserService>();
        }

        [Fact]
        public async Task LoginAsync_ReturnsJwtToken()
        {
            await _userService.RegisterAsync("sussUser", "Password123!", UserRoleEnum.User);

            var token = await _userService.LoginAsync("sussUser", "Password123!");

            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword()
        {
            await _userService.RegisterAsync("UserwronntPass", "CorrectPassword123!", UserRoleEnum.User);

            var ex = await Assert.ThrowsAsync<UnauthorizedException>(() =>  _userService.LoginAsync("UserwronntPass", "WrongPassword123!"));
            Assert.Equal("Ошибка авторизации", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_NotFoundUser()
        {
            var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _userService.LoginAsync("404User", "AnyPassword"));

            Assert.Equal("Ошибка авторизации", ex.Message);
        }
    }
}
