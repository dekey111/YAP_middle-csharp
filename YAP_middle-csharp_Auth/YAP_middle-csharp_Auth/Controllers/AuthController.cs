using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YAP_middle_csharp_Auth.Application.Interfaces;
using YAP_middle_csharp_Auth.Application.Interfaces.IServices;
using YAP_middle_csharp_Auth.Application.Models;
using YAP_middle_csharp_Auth.Domain.Exceptions;
using YAP_middle_csharp_Auth.Domain.Models;
using LoginRequest = YAP_middle_csharp_Auth.Application.Models.LoginRequest;

namespace YAP_middle_csharp_Auth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController(IUserService userService, ILogger<AuthController> logger, IValidator<LoginRequest> validator) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly ILogger<AuthController> _logger = logger;
        private readonly IValidator<LoginRequest> _validator = validator;

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        /// <param name="request">Принимает данные регистрации пользователя</param>
        [HttpPost("register-user")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterUserAsync([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[AuthController] [RegisterUserAsync] Запрос на регистрацию пользователя: {Login}", request.Login);

            if (!_validator.IsValid(request))
            {
                _logger.LogDebug("[AuthController] [RegisterUserAsync] неуспешная регистрация пользователя: {login}", request.Login);
                throw new ValidationExceptionApp("Проверьте правильность заполненных данных!");
            }

            await _userService.RegisterAsync(request.Login, request.Password, UserRoleEnum.User, cancellationToken);
            return NoContent();
        }


        /// <summary>
        /// Регистрация нового администратора
        /// </summary>
        /// <param name="request">Принимает данные регистрации пользователя</param>
        [HttpPost("register-admin")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterAdminAsync([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[AuthController] [RegisterAdminAsync] Запрос на регистрацию администратора: {Login}", request.Login);


            if (!_validator.IsValid(request))
            {
                _logger.LogDebug("[AuthController] [RegisterAdminAsync] неуспешная регистрация администратора: {login}", request.Login);
                throw new ValidationExceptionApp("Проверьте правильность заполненных данных!");
            }

            await _userService.RegisterAsync(request.Login, request.Password, UserRoleEnum.Admin, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Аутентификация пользователя и выдача токена
        /// </summary>
        /// <param name="request">Принимает данные авторизации</param>
        /// <returns>Возвращает JWT токен</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginRequest), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[AuthController] Запрос на вход пользователя: {Login}", request.Login);

            if (!_validator.IsValid(request))
            {
                _logger.LogDebug("[AuthController] [RegisterAdminAsync] неуспешная авториация. Login: {login}", request.Login);
                throw new ValidationExceptionApp("Проверьте правильность заполненных данных!");
            }


            var token = await _userService.LoginAsync(request.Login, request.Password, cancellationToken);
            return Ok(new LoginResponse { Token = token });
        }

    }
}
