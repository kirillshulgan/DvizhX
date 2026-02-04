using DvizhX.Application.Features.Auth.Commands.GoogleAuth;
using DvizhX.Application.Features.Auth.Commands.Login;
using DvizhX.Application.Features.Auth.Commands.RefreshToken;
using DvizhX.Application.Features.Auth.Commands.Register;
using DvizhX.Application.Features.Auth.Commands.TelegramAuth;
using DvizhX.Application.Features.Auth.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DvizhX.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IMediator mediator) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("google")]
        public async Task<IActionResult> GoogleAuth(GoogleAuthCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("telegram")]
        public async Task<IActionResult> TelegramAuth(TelegramAuthCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            // Извлекаем UserId из Claims токена
            // .NET автоматически парсит JWT и кладет Claims в User
            var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

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
