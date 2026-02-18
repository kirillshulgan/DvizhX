using DvizhX.Api.Services;
using DvizhX.Application.Common.Interfaces;
using DvizhX.Application.Features.Auth.Queries.GetCurrentUser;
using DvizhX.Application.Features.Kanban.Common;
using DvizhX.Application.Features.Kanban.Queries.GetBoard;
using DvizhX.Application.Features.Notifications.Commands.SubscribeDevice;
using DvizhX.Application.Features.Notifications.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DvizhX.Api.Controllers
{
    /// <summary> Управление уведомлениями </summary>
    /// <remarks>
    /// Методы для работы с уведомлениями.
    /// <para>
    /// ⚡ **Real-time:** Методы уведомлений
    /// </para>
    /// </remarks>
    [Tags("4. Уведомления")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController(IMediator mediator, ICurrentUserService currentUserService) : ControllerBase
    {
        /// <summary> Подписаться на уведомления </summary>
        /// <param name="dto">DTO подписи на уведомления</param>
        [HttpPost("subscribe")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Subscribe(SubscribeDto dto)
        {
            // Вызываем команду MediatR, которая сохранит токен в БД для текущего юзера
            //var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            //{
            //    return Unauthorized(new { Message = "Invalid token" });
            //}

            //var query = new GetCurrentUserQuery(userId);
            //var currentUser = await mediator.Send(query);

            var userId = currentUserService.UserId ?? Guid.Empty;

            await mediator.Send(new SubscribeDeviceCommand(userId, dto.Token, dto.DeviceType));
            return Ok();
        }
    }
}
