using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using YAP_middle_csharp.Application.Interfaces;
using YAP_middle_csharp.Application.Interfaces.IRepositories;
using YAP_middle_csharp.Application.Interfaces.IServices;
using YAP_middle_csharp.Application.Services;
using YAP_middle_csharp.Application.Validator;
using YAP_middle_csharp.Domain.Interface;
using YAP_middle_csharp.Domain.Models;
using YAP_middle_csharp.Infrastructure.DataAccess;
using YAP_middle_csharp.Infrastructure.Repository;

namespace YAP_middle_csharp.Tests
{
    public class AuthServiceTest
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IPasswordHasherService _hasher;
        public AuthServiceTest()
        {
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
            services.AddLogging();

            services.AddScoped<IPasswordHasher<UserModel>, PasswordHasher<UserModel>>();
            services.AddScoped<IPasswordHasherService, PasswordHasherService>();

            _serviceProvider = services.BuildServiceProvider();
            var scope = _serviceProvider.CreateScope();
            _hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();
        }


        [Fact]
        public void HashPassword_ReturnsHash()
        {
            var password = "MySuperPuperHighPassword3000!";

            var hash = _hasher.HashPassword(password);

            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
            Assert.NotEqual(password, hash);
        }

        [Fact]
        public void CheckPassword_ReturnsTrue()
        {
            var password = "MySuperPuperHighPassword3000!";
            var hash = _hasher.HashPassword(password);
            var user = new UserModel("testUser", hash, UserRoleEnum.User);

            var result = _hasher.CheckPassword(user, password);

            Assert.True(result);
        }

        [Fact]
        public void CheckPassword_ReturnsFalse()
        {
            var password = "MySuperPuperHighPassword3000!";
            var hash = _hasher.HashPassword(password);
            var user = new UserModel("testUser", hash, UserRoleEnum.User);

            var result = _hasher.CheckPassword(user, "weryDrygoyPassword");

            Assert.False(result);
        }
    }
}
