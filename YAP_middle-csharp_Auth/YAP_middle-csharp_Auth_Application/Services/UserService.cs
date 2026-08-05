using YAP_middle_csharp_Auth.Application.Interfaces.IRepositories;
using YAP_middle_csharp_Auth.Application.Interfaces.IServices;
using YAP_middle_csharp_Auth.Domain.Exceptions;
using YAP_middle_csharp_Auth.Domain.Interface;
using YAP_middle_csharp_Auth.Domain.Models;

namespace YAP_middle_csharp_Auth.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasherService _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public UserService(IUserRepository userRepository, IPasswordHasherService passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        /// <param name="login">Принимает логин</param>
        /// <param name="password">Принимает пароль</param>
        /// <param name="role">Принимает роль</param>
        /// <returns>Возвращает нового пользователя</returns>
        /// <exception cref="ValidationExceptionApp">В случае если такой логин уже существует</exception>
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

        /// <summary>
        /// Метод авторизации пользователя
        /// </summary>
        /// <param name="login">Принимает логин </param>
        /// <param name="password">Принимает пароль</param>
        /// <returns>Возвращает JWT токен</returns>
        /// <exception cref="UnauthorizedException">В случае если логин или пароль не совпадает</exception>
        public async Task<string> LoginAsync(string login, string password)
        {
            var findUser = await _userRepository.FindByLoginAsync(login);
            if (findUser == null)
            {
                throw new UnauthorizedException();
            }

            bool isPasswordValid = _passwordHasher.CheckPassword(findUser, password);
            if (!isPasswordValid)
            {
                throw new UnauthorizedException();
            }

            return _jwtTokenGenerator.GenerateToken(findUser);
        }
    }

}
