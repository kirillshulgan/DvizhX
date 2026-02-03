using DvizhX.Application.Features.Auth.Commands.Login;
using DvizhX.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DvizhX.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IMediator mediator) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterCommand command)
        {
            // MediatR сам найдет нужный хендлер
            var token = await mediator.Send(command);
            return Ok(new { Token = token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginCommand command)
        {
            var token = await mediator.Send(command);
            return Ok(new { Token = token });
        }
    }
}
