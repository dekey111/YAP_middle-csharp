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
            var findUser = await _userRepository.FindByLoginAsync(login);
            if (findUser != null)
            {
                throw new ValidationExceptionApp("Пользователь с таким логином уже существует");
            }

            var passwordHash = _passwordHasher.HashPassword(password);
            var newUser = new UserModel(login, passwordHash, role);
            await _userRepository.CreateAsync(newUser);
        }

        public async Task<string> LoginAsync(string login, string password)
        {
            var findUser = await _userRepository.FindByLoginAsync(login);
            if (findUser == null)
            {
                throw new NotFoundExceptionApp("Неверный логин или пароль");
            }

            bool isPasswordValid = _passwordHasher.CheckPassword(findUser, password);
            if (!isPasswordValid)
            {
                throw new NotFoundExceptionApp("Неверный логин или пароль");
            }

            return _jwtTokenGenerator.GenerateToken(findUser);
        }
    }
}
