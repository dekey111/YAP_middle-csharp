using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using YAP_middle_csharp.Application.Interfaces.IServices;
using YAP_middle_csharp.Application.Models;

namespace YAP_middle_csharp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class AuthController(IUserService userService, ILogger<AuthController> logger) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly ILogger<AuthController> _logger = logger;

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterAsync([FromBody] Application.Models.RegisterRequest request)
        {
            _logger.LogInformation("[AuthController] Запрос на регистрацию пользователя: {Login}", request.Login);

            await _userService.RegisterAsync(request.Login, request.Password, request.UserRole);
            return Ok(new { message = "Регистрация успешно завершена" });
        }

        /// <summary>
        /// Аутентификация пользователя и выдача токена
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> LoginAsync([FromBody] Application.Models.LoginRequest request)
        {
            _logger.LogInformation("[AuthController] Запрос на вход пользователя: {Login}", request.Login);

            var token = await _userService.LoginAsync(request.Login, request.Password);
            return Ok(new LoginResponse { Token = token });
        }
    }
}
