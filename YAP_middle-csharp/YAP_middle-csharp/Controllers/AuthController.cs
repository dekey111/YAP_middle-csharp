using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YAP_middle_csharp.Application.Interfaces.IServices;
using YAP_middle_csharp.Application.Models;
using YAP_middle_csharp.Domain.Models;
using LoginRequest = YAP_middle_csharp.Application.Models.LoginRequest;
using RegisterRequest = YAP_middle_csharp.Application.Models.RegisterRequest;

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
        [HttpPost("registerUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterStandartUserAsync([FromBody] RegisterRequest request)
        {
            _logger.LogInformation("[AuthController] Запрос на регистрацию пользователя: {Login}", request.Login);

            await _userService.RegisterAsync(request.Login, request.Password, UserRoleEnum.User);
            return Ok(new { message = "Регистрация успешно завершена" });
        }


        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("registerAdmin")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterAdminAsync([FromBody] RegisterRequest request)
        {
            _logger.LogInformation("[AuthController] Запрос на регистрацию администратора: {Login}", request.Login);

            await _userService.RegisterAdminAsync(request.Login, request.Password, UserRoleEnum.Admin);
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
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
        {
            _logger.LogInformation("[AuthController] Запрос на вход пользователя: {Login}", request.Login);

            var token = await _userService.LoginAsync(request.Login, request.Password);
            return Ok(new LoginResponse { Token = token });
        }
    }
}
