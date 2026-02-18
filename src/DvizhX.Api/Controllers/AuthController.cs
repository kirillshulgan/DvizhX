using DvizhX.Application.Features.Auth.Commands.GoogleAuth;
using DvizhX.Application.Features.Auth.Commands.Login;
using DvizhX.Application.Features.Auth.Commands.RefreshToken;
using DvizhX.Application.Features.Auth.Commands.Register;
using DvizhX.Application.Features.Auth.Commands.TelegramAuth;
using DvizhX.Application.Features.Auth.Common;
using DvizhX.Application.Features.Auth.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DvizhX.Api.Controllers
{
    /// <summary> Аутентификация и управление токенами </summary>
    /// <remarks> Методы для регистрации, входа (Email, Google, Telegram) и обновления JWT токенов. </remarks>
    [Tags("1. Аутентификация")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IMediator mediator) : ControllerBase
    {
        /// <summary> Регистрация нового пользователя </summary>
        /// <remarks>
        /// Создает нового пользователя с Email и паролем. 
        /// Возвращает пару Access + Refresh токенов для немедленного входа.
        /// </remarks>
        /// <param name="command">Данные для регистрации</param>
        /// <response code="200">Успешная регистрация, токены выданы</response>
        /// <response code="400">Ошибка валидации или Email уже занят</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(RegisterCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }

        /// <summary> Вход по Email и паролю </summary>
        /// <remarks> Проверяет учетные данные и возвращает JWT токены. </remarks>
        /// <response code="200">Вход выполнен успешно</response>
        /// <response code="400">Пользователь не найден или неверный пароль</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login(LoginCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }

        /// <summary> Вход через Google </summary>
        /// <remarks>
        /// Принимает `id_token` от Google Sign-In. 
        /// Если пользователя нет — регистрирует. Если есть — входит.
        /// </remarks>
        [HttpPost("google")]
        [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GoogleAuth(GoogleAuthCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }

        /// <summary> Вход через Telegram Widget </summary>
        /// <remarks>
        /// Принимает данные, полученные от Telegram Login Widget.
        /// Проверяет подпись HMAC-SHA256.
        /// </remarks>
        [HttpPost("telegram")]
        [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> TelegramAuth(TelegramAuthCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }

        /// <summary> Обновление Access токена </summary>
        /// <remarks>
        /// Принимает истекший AccessToken и валидный RefreshToken.
        /// Если всё ок — возвращает новую пару. Старый RefreshToken аннулируется (Rotation).
        /// </remarks>
        /// <response code="200">Токены обновлены</response>
        /// <response code="401">Токен невалиден, истек или использован повторно</response>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken(RefreshTokenCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }

        /// <summary> Получить профиль текущего пользователя </summary>
        /// <remarks>
        /// Требует заголовок `Authorization: Bearer {token}`.
        /// Возвращает ID, имя, почту и аватарку.
        /// </remarks>
        /// <response code="200">Профиль пользователя</response>
        /// <response code="401">Токен не передан или невалиден</response>
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Me()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { Message = "Invalid token" });
            }

            var query = new GetCurrentUserQuery(userId);
            var result = await mediator.Send(query);

            return Ok(result);
        }
    }
}
