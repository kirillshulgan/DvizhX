using DvizhX.Application.Common.Interfaces;
using DvizhX.Application.Common.Interfaces.Persistence;
using DvizhX.Application.Common.Interfaces.Realtime;
using MediatR;

namespace DvizhX.Application.Features.Kanban.Commands.DeleteCard
{
    public class DeleteCardCommandHandler(
        ICardRepository cardRepository,
        ICurrentUserService currentUserService,
        IKanbanNotifier notifier,
        INotificationService firebaseService,
        IDeviceTokenRepository deviceTokenRepository)
        : IRequestHandler<DeleteCardCommand>
    {
        public async Task Handle(DeleteCardCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

            var card = await cardRepository.GetByIdWithColumnAsync(request.CardId, cancellationToken);
            if (card == null) throw new KeyNotFoundException("Card not found");

            var isParticipant = card.Column!.Board!.Event!.Participants.Any(p => p.UserId == userId);
            if (!isParticipant) throw new UnauthorizedAccessException();

            // Сохраняем данные для уведомления ДО удаления
            var cardTitle = card.Title;
            var recipientIds = card.Column.Board.Event.Participants
                .Where(p => p.UserId != userId)
                .Select(p => p.UserId)
                .ToList();

            var columnId = card.ColumnId;
            var orderIndex = card.OrderIndex;
            var eventId = card.Column.Board.EventId;

            // 1. Удаляем саму карту
            await cardRepository.DeleteAsync(card, cancellationToken);

            // 2. Сдвигаем индексы оставшихся карт вверх (закрываем дырку)
            await cardRepository.ShiftIndexesUpAsync(columnId, orderIndex, cancellationToken);

            // 3. Уведомляем фронтенд (SignalR)
            await notifier.CardDeletedAsync(eventId, request.CardId);

            // 4. 🔥 Уведомляем Push (Firebase) 🔥
            try
            {
                if (recipientIds.Any())
                {
                    // Достаем токены
                    var tokens = await deviceTokenRepository.GetTokensByUserIdsAsync(recipientIds);

                    if (tokens.Any())
                    {
                        await firebaseService.SendMulticastAsync(
                            tokens,
                            "Задача удалена 🗑️",
                            $"Задача '{cardTitle}' была удалена"
                        );
                    }
                }
            }
            catch (Exception)
            {
                // Игнорируем ошибки пушей
            }
        }
    }
}
