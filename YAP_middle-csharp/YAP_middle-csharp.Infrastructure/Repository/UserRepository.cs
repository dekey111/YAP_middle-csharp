using Microsoft.EntityFrameworkCore;
using YAP_middle_csharp.Application.Interfaces.IRepositories;
using YAP_middle_csharp.Domain.Models;
using YAP_middle_csharp.Infrastructure.DataAccess;

namespace YAP_middle_csharp.Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserModel?> FindByIdAsync(Guid id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<UserModel?> FindByLoginAsync(string login)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Login.Trim().ToLower() == login.Trim().ToLower());
        }

        public async Task CreateAsync(UserModel user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
    }
}
