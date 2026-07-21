using YAP_middle_csharp.Application.Interfaces;
using YAP_middle_csharp.Application.Interfaces.IRepositories;
using YAP_middle_csharp.Application.Interfaces.IServices;
using YAP_middle_csharp.Domain.Exceptions;
using YAP_middle_csharp.Domain.Models;

namespace YAP_middle_csharp.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasherService _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public UserService(
            IUserRepository userRepository,
            IPasswordHasherService passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task RegisterAsync(string login, string password, UserRoleEnum role)
        {
            var existingUser = await _userRepository.FindByLoginAsync(login);
            if (existingUser != null)
            {
                throw new ValidationExceptionApp("Пользователь с таким логином уже существует");
            }

            var passwordHash = _passwordHasher.HasPassword(password);

            var newUser = new UserModel(login, passwordHash, role);
            await _userRepository.CreateAsync(newUser);
        }

        public async Task<string> LoginAsync(string login, string password)
        {
            var user = await _userRepository.FindByLoginAsync(login);
            if (user == null)
            {
                throw new ValidationExceptionApp("Неверный логин или пароль");
            }

            bool isPasswordValid = _passwordHasher.CheckPassword(password, user.PasswordHash);
            if (!isPasswordValid)
            {
                throw new ValidationExceptionApp("Неверный логин или пароль");
            }

            return _jwtTokenGenerator.GenerateToken(user);
        }
    }
}
