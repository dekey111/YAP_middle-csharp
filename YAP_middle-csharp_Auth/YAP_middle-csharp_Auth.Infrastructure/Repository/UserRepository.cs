using Microsoft.EntityFrameworkCore;
using YAP_middle_csharp_Auth.Application.Interfaces.IRepositories;
using YAP_middle_csharp_Auth.Domain.Exceptions;
using YAP_middle_csharp_Auth.Domain.Models;
using YAP_middle_csharp_Auth.Infrastructure.DataAccess;

namespace YAP_middle_csharp_Auth.Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserModel> FindByIdAsync(Guid id)
        {
            var findUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (findUser == null)
                throw new NotFoundExceptionApp("Пользователь не найден!");

            return findUser;
        }

        public async Task<UserModel?> FindByLoginAsync(string login)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Login.Trim().ToLower() == login.Trim().ToLower());
        }

        public async Task CreateAsync(UserModel user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
    }
}
