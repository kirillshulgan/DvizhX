using DvizhX.Application.Common.Interfaces;
using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Application.Features.Kanban.Common;
using DvizhX.Domain.Entities;
using MediatR;

namespace DvizhX.Application.Features.Kanban.Queries.GetBoard
{
    public class GetBoardQueryHandler(
    IBoardRepository boardRepository,
    IEventRepository eventRepository, // <--- Нужно для проверки участия
    ICurrentUserService currentUserService) // <--- Нужно для получения ID юзера
    : IRequestHandler<GetBoardQuery, BoardDto>
    {
        public async Task<BoardDto> Handle(GetBoardQuery request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

            // 1. Сначала проверяем доступ к событию!
            // Используем метод GetByIdWithParticipantsAsync, который мы писали ранее
            var eventEntity = await eventRepository.GetByIdWithParticipantsAsync(request.EventId, cancellationToken);

            if (eventEntity == null)
            {
                throw new Exception("Event not found"); // Или NotFoundException
            }

            // Является ли юзер участником?
            var isParticipant = eventEntity.Participants.Any(p => p.UserId == userId);
            if (!isParticipant)
            {
                throw new Exception("Access denied"); // Или ForbiddenException
            }

            // 2. Теперь безопасно ищем доску
            var board = await boardRepository.GetByEventIdAsync(request.EventId, cancellationToken);

            // 3. Если нет - создаем дефолтную (Lazy initialization)
            if (board == null)
            {
                board = new Board
                {
                    EventId = request.EventId,
                    Title = "Main Board",
                    Columns = new List<BoardColumn>
                {
                    new() { Title = "To Do", OrderIndex = 0 },
                    new() { Title = "In Progress", OrderIndex = 1 },
                    new() { Title = "Done", OrderIndex = 2 }
                }
                };
                await boardRepository.AddAsync(board, cancellationToken);

                // Важно: После AddAsync EF Core может не подгрузить вложенные коллекции, если мы не перечитаем или не инициализируем их вручную.
                // Но в данном случае мы сами создали объект в памяти, так что коллекции в нем есть.
            }

            // 4. Маппим
            return new BoardDto(
                board.Id,
                board.EventId,
                board.Columns.Select(c => new ColumnDto(
                    c.Id,
                    c.Title,
                    c.OrderIndex,
                    c.Cards.Select(card => new CardDto(
                        card.Id,
                        card.Title,
                        card.Description,
                        card.OrderIndex,
                        card.AssignedUserId,
                        card.AssignedUser?.AvatarUrl
                    )).ToList()
                )).ToList()
            );
        }
    }
}
