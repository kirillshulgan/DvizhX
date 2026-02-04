using DvizhX.Application.Common.Interfaces;
using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Application.Common.Interfaces.Realtime;
using DvizhX.Application.Features.Kanban.Common;
using DvizhX.Domain.Entities;
using MediatR;

namespace DvizhX.Application.Features.Kanban.Commands.CreateCard
{
    public class CreateCardCommandHandler(
    ICardRepository cardRepository,
    ICurrentUserService currentUserService,
    IKanbanNotifier notifier)
    : IRequestHandler<CreateCardCommand, CardDto>
    {
        public async Task<CardDto> Handle(CreateCardCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

            // 1. Проверяем доступ через метод репозитория
            var column = await cardRepository.GetColumnWithHierarchyAsync(request.ColumnId, cancellationToken);

            if (column == null) throw new Exception("Column not found");

            var isParticipant = column.Board.Event.Participants.Any(p => p.UserId == userId);
            if (!isParticipant) throw new UnauthorizedAccessException("Access denied");

            // 2. Вычисляем OrderIndex через метод репозитория
            var maxIndex = await cardRepository.GetMaxOrderIndexAsync(request.ColumnId, cancellationToken);

            // 3. Создаем карточку
            var card = new Card
            {
                ColumnId = request.ColumnId,
                Title = request.Title,
                Description = request.Description,
                OrderIndex = maxIndex + 1,
                AssignedUserId = null
            };

            // Сохраняем через репозиторий
            await cardRepository.AddAsync(card, cancellationToken);

            // 4. Формируем DTO
            var cardDto = new CardDto(
                card.Id,
                card.Title,
                card.Description,
                card.OrderIndex,
                null,
                null
            );

            // 5. Уведомляем Realtime
            var eventId = column.Board.EventId;
            await notifier.CardCreatedAsync(eventId, cardDto);

            return cardDto;
        }
    }
}
