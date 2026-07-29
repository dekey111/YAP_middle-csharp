using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using YAP_middle_csharp.Domain.Models;
using YAP_middle_csharp.Infrastructure.DataAccess;
using YAP_middle_csharp.Infrastructure.Repository;

namespace EventApi.IntegrationTests
{
    [Collection("PostgresCollection")]
    public class UserRepositoryTest : IAsyncLifetime
    {
        private readonly PostgresFixture _fixture;
        private AppDbContext _context = null!;
        private UserRepository _repository = null!;

        public UserRepositoryTest(PostgresFixture fixture)
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
        }

        public async Task DisposeAsync() => await _context.DisposeAsync();

        [Fact]
        public async Task CreateAsync_SavesUserToDatabase()
        {
            var user = new UserModel("TestUser123", "PasswordHash", UserRoleEnum.User);

            await _repository.CreateAsync(user);

            var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            Assert.NotNull(savedUser);
            Assert.Equal("TestUser123", savedUser.Login);
            Assert.Equal("PasswordHash", savedUser.PasswordHash);
            Assert.Equal(UserRoleEnum.User, savedUser.UserRole);
        }

        [Fact]
        public async Task FindByLoginAsync_CorrectUser()
        {
            var user = new UserModel("TestUser2", "SecretPassword123", UserRoleEnum.Admin);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var foundUser = await _repository.FindByLoginAsync("TestUser2");

            Assert.NotNull(foundUser);
            Assert.Equal(user.Id, foundUser.Id);
            Assert.Equal(UserRoleEnum.Admin, foundUser.UserRole);
        }

        [Fact]
        public async Task FindByLoginAsync_NonExistentUser()
        {
            var result = await _repository.FindByLoginAsync("tbIKto?");
            Assert.Null(result);
        }

        [Fact]
        public async Task FindByIdAsync_CorrectUser()
        {
            var user = new UserModel("SuperSecretUser", "ThisIsPASSSSWORD!", UserRoleEnum.User);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var foundUser = await _repository.FindByIdAsync(user.Id);

            Assert.NotNull(foundUser);
            Assert.Equal("SuperSecretUser", foundUser.Login);
        }

        [Fact]
        public async Task CreateAsync_DuplicateLogin()
        {
            var user1 = new UserModel("Ilya", "Ilya1", UserRoleEnum.User);
            var user2 = new UserModel("Ilya", "Ilya2", UserRoleEnum.User);

            await _repository.CreateAsync(user1);

            await Assert.ThrowsAsync<DbUpdateException>(() => _repository.CreateAsync(user2));
        }
    }
}
