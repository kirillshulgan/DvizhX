using DvizhX.Application.Common.Interfaces;
using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Application.Common.Interfaces.Realtime;
using MediatR;

namespace DvizhX.Application.Features.Kanban.Commands.DeleteCard
{
    public class DeleteCardCommandHandler(
        ICardRepository cardRepository,
        ICurrentUserService currentUserService,
        IKanbanNotifier notifier)
        : IRequestHandler<DeleteCardCommand>
    {
        public async Task Handle(DeleteCardCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

            var card = await cardRepository.GetByIdWithColumnAsync(request.CardId, cancellationToken);
            if (card == null) throw new KeyNotFoundException("Card not found");

            var isParticipant = card.Column!.Board!.Event!.Participants.Any(p => p.UserId == userId);
            if (!isParticipant) throw new UnauthorizedAccessException();

            var columnId = card.ColumnId;
            var orderIndex = card.OrderIndex;
            var eventId = card.Column.Board.EventId;

            // 1. Удаляем саму карту
            await cardRepository.DeleteAsync(card, cancellationToken);

            // 2. Сдвигаем индексы оставшихся карт вверх (закрываем дырку)
            // Используем наш оптимизированный SQL-метод
            await cardRepository.ShiftIndexesUpAsync(columnId, orderIndex, cancellationToken);

            // 3. Уведомляем
            await notifier.CardDeletedAsync(eventId, request.CardId);
        }
    }
}
